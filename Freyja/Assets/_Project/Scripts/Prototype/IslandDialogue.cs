using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Freyja.Narrative;
using Yarn.Unity;

namespace Freyja.Prototype
{
    public sealed class IslandDialogue : MonoBehaviour
    {
        [SerializeField] private CaisSessionController session;
        [SerializeField] private Transform player;
        [SerializeField] private IslandInventory inventory;
        [SerializeField] private Transform nuno;
        [SerializeField] private Transform luana;
        [SerializeField] private Transform ivo;
        [SerializeField, Min(1f)] private float talkDistance = 2.5f;

        private enum EntryId { None, NextBoat, Helena, BlueRibbon, IvoDeparture }
        private enum EntryKind { Depoimento, Observação }

        private sealed class Topic
        {
            public readonly string Question;
            public readonly string Answer;
            public readonly EntryId Entry;
            public readonly EntryKind Kind;

            public Topic(string question, string answer, EntryId entry = EntryId.None,
                EntryKind kind = EntryKind.Depoimento)
            {
                Question = question;
                Answer = answer;
                Entry = entry;
                Kind = kind;
            }
        }

        private sealed class Resident
        {
            public readonly Transform Anchor;
            public readonly string Name;
            public readonly string Greeting;
            public readonly Topic[] Topics;

            public Resident(Transform anchor, string name, string greeting, params Topic[] topics)
            {
                Anchor = anchor;
                Name = name;
                Greeting = greeting;
                Topics = topics;
            }
        }

        private readonly List<(EntryId Id, EntryKind Kind, string Source, string Text)> notebook = new();
        private Resident[] residents;
        private Resident nearby;
        private Resident speaker;
        private ConversationFraming framing;
        private Topic reply;
        private bool readingNotebook;
        private bool readingInventory;
        private Vector2 notebookScroll;
        private Vector2 inventoryScroll;
        private InventoryPickup nearbyPickup;
        private string pickupMessage;
        private float pickupMessageUntil;
        private GUIStyle panelStyle, nameStyle, lineStyle, buttonStyle, promptStyle;
        private GUIStyle subtitleName, subtitleLine, subtitleChoice, subtitleFocus, subtitleExit;
        private Texture2D subtitleGradient, choiceBackground, focusBackground;
        private int focusedChoice = -1;
        private NarrativeRuntime narrative;
        private DialogueRunner yarnRunner;
        private NarrativeYarnPresenter yarnPresenter;
        private NarrativeYarnBridge yarnBridge;
        private bool yarnConversation;
        private bool narrativeDemoMode;
        private GameObject ownedRunner;
        private CoastalNarrativeClock narrativeClock;

        private void Awake()
        {
            var dayNight = FindFirstObjectByType<Freyja.Environment.CoastalDayNight>();
            narrativeClock = dayNight != null ? new CoastalNarrativeClock(dayNight) : null;
            narrative = new NarrativeRuntime(NarrativeProgress.LoadDemo(),
                (INarrativeClock)narrativeClock ?? new FixedNarrativeClock(9));
            var projectReference = Resources.Load<DemoYarnProjectReference>("Narrative/DemoYarnProjectReference");
            yarnRunner = FindFirstObjectByType<DialogueRunner>();
            if (projectReference != null && projectReference.Project != null &&
                (yarnRunner == null || yarnRunner.YarnProject != projectReference.Project))
            {
                ownedRunner = new GameObject("Narrative Dialogue Runtime");
                yarnRunner = ownedRunner.AddComponent<DialogueRunner>();
                yarnRunner.SetProject(projectReference.Project);
            }
            if (yarnRunner != null)
            {
                yarnPresenter = gameObject.AddComponent<NarrativeYarnPresenter>();
                var presenters = new List<DialoguePresenterBase>(yarnRunner.DialoguePresenters) { yarnPresenter };
                yarnRunner.DialoguePresenters = presenters;
                yarnPresenter.Completed += OnYarnDialogueComplete;
                yarnBridge = new NarrativeYarnBridge(yarnRunner, narrative);
                yarnPresenter.LocaleLabel = yarnBridge.Locale;
            }
            residents = new[]
            {
                new Resident(nuno, "Nuno Pires", "Se é sobre o barco, fale baixo, por favor.",
                    new Topic("Quando sai o próximo barco?",
                        "O barco do continente volta antes do nascimento. Artur e eu combinamos de pôr Ivo a bordo nessa viagem.",
                        EntryId.NextBoat)),
                new Resident(luana, "Luana Batista", "Se está procurando meu pai, deve estar nos degraus.",
                    new Topic("Como está Helena?",
                        "Minha mãe está grávida. Ela queria que meu pai estivesse aqui quando o bebê nascer. Agora todo mundo só fala de quando ele vai embora.",
                        EntryId.Helena)),
                new Resident(ivo, "Ivo Batista", "Cuidado com a beirada. Ainda falta firmar umas pedras.",
                    new Topic("O que você está fazendo?",
                        "Consertando os degraus. Quero deixar o caminho em ordem para Helena e Luana."),
                    new Topic("Observar a manga",
                        "Uma tira de pano azul está presa à manga de Ivo.", EntryId.BlueRibbon, EntryKind.Observação),
                    new Topic("Por que você precisa partir antes do nascimento?",
                        "O bebê passa a contar quando nasce. A vila já me escolheu para partir antes. Queria estar aqui com Helena e Luana quando ele chegasse.",
                        EntryId.IvoDeparture))
            };
        }

        private void Update()
        {
            narrativeClock?.Tick();
            if (session == null || player == null || !session.IsInitialized) return;
            var keyboard = Keyboard.current;
            if (speaker != null || readingNotebook || readingInventory)
            {
                if (yarnConversation)
                {
                    if (keyboard?.lKey.wasPressedThisFrame == true)
                    {
                        yarnBridge.ToggleLocale();
                        yarnPresenter.LocaleLabel = yarnBridge.Locale;
                    }
                    if (keyboard?.enterKey.wasPressedThisFrame == true || keyboard?.numpadEnterKey.wasPressedThisFrame == true)
                    {
                        if (yarnPresenter.IsShowingLine) yarnPresenter.AdvanceLine();
                        else if (yarnPresenter.IsShowingOptions)
                        {
                            var options = yarnPresenter.CurrentOptions;
                            int availableIndex = 0;
                            for (int i = 0; i < options.Length; i++)
                                if (options[i].IsAvailable)
                                {
                                    if (availableIndex++ == Mathf.Max(0, focusedChoice))
                                    { yarnPresenter.SelectOption(options[i]); break; }
                                }
                        }
                    }
                    if (yarnPresenter.IsShowingOptions)
                    {
                        var options = yarnPresenter.CurrentOptions;
                        int count = 0;
                        foreach (var option in options) if (option.IsAvailable) count++;
                        if (count > 0 && keyboard?.downArrowKey.wasPressedThisFrame == true)
                            focusedChoice = (focusedChoice + 1) % count;
                        if (count > 0 && keyboard?.upArrowKey.wasPressedThisFrame == true)
                            focusedChoice = focusedChoice < 0 ? count - 1 : (focusedChoice + count - 1) % count;
                    }
                    if (keyboard?.escapeKey.wasPressedThisFrame == true || !InReach(speaker))
                    { if (yarnRunner != null && yarnRunner.IsDialogueRunning) yarnRunner.Stop().Forget(); else ClosePanel(); }
                    return;
                }
                if (keyboard?.escapeKey.wasPressedThisFrame == true ||
                    (readingNotebook && keyboard?.tabKey.wasPressedThisFrame == true) ||
                    (readingInventory && keyboard?.iKey.wasPressedThisFrame == true) ||
                    (speaker != null && !InReach(speaker)))
                    ClosePanel();
                else if (speaker != null && keyboard != null)
                    HandleChoiceInput(keyboard);
                return;
            }

            nearby = session.IsCaptured ? FindNearby() : null;
            float residentDistance = nearby == null ? float.PositiveInfinity :
                (nearby.Anchor.position - player.position).sqrMagnitude;
            nearbyPickup = session.IsCaptured && inventory != null
                ? inventory.FindNearby(player.position, residentDistance)
                : null;
            if (!session.IsCaptured || keyboard == null) return;
            if (keyboard.f2Key.wasPressedThisFrame && yarnRunner != null)
                narrativeDemoMode = !narrativeDemoMode;
            if (keyboard.tabKey.wasPressedThisFrame)
            {
                readingNotebook = true;
                notebookScroll = Vector2.zero;
                session.SetModalOpen(true);
            }
            else if (inventory != null && keyboard.iKey.wasPressedThisFrame)
            {
                readingInventory = true;
                inventoryScroll = Vector2.zero;
                session.SetModalOpen(true);
            }
            else if (keyboard.eKey.wasPressedThisFrame)
            {
                if (nearbyPickup != null)
                {
                    if (inventory.TryCollect(nearbyPickup))
                    {
                        pickupMessage = "Guardado: " + nearbyPickup.DisplayName;
                        pickupMessageUntil = Time.unscaledTime + 2.5f;
                    }
                    nearbyPickup = null;
                    nearby = null;
                }
                else if (nearby != null)
                {
                    OpenConversation(nearby);
                }
            }
        }

        private bool InReach(Resident resident) =>
            resident.Anchor != null && resident.Anchor.gameObject.activeInHierarchy &&
            (resident.Anchor.position - player.position).sqrMagnitude <= talkDistance * talkDistance;

        private Resident FindNearby()
        {
            Resident result = null;
            float closest = float.PositiveInfinity;
            foreach (var resident in residents)
            {
                if (!InReach(resident)) continue;
                float distance = (resident.Anchor.position - player.position).sqrMagnitude;
                if (distance >= closest) continue;
                result = resident;
                closest = distance;
            }
            return result;
        }

        private void OpenConversation(Resident resident)
        {
            speaker = resident;
            reply = null;
            focusedChoice = -1;
            session.SetModalOpen(true);
            framing = ConversationFraming.Begin(session.CameraRig, player, resident.Anchor);
            string node = resident == residents[0] ? "Demo_Nuno" : resident == residents[1] ? "Demo_Luana" : "Demo_Ivo";
            if (narrativeDemoMode && yarnRunner != null && yarnBridge != null && yarnPresenter != null)
            {
                yarnConversation = true;
                string npcId = resident == residents[0] ? "demo.nuno" : resident == residents[1] ? "demo.luana" : "demo.ivo";
                yarnBridge.StartNpc(node, npcId);
            }
        }

        private void ClosePanel()
        {
            framing?.End();
            framing = null;
            bool stopYarn = yarnConversation;
            yarnConversation = false;
            if (stopYarn && yarnRunner != null && yarnRunner.IsDialogueRunning) yarnRunner.Stop().Forget();
            speaker = nearby = null;
            reply = null;
            readingNotebook = readingInventory = false;
            nearbyPickup = null;
            session.SetModalOpen(false);
        }

        private static string ClueId(EntryId id) => id switch
        {
            EntryId.NextBoat => "demo.clue.boat_schedule",
            EntryId.Helena => "demo.clue.helena_birth",
            EntryId.BlueRibbon => "demo.clue.blue_ribbon",
            EntryId.IvoDeparture => "demo.clue.departure_choice",
            _ => "demo.clue.unknown"
        };

        private static string ClueLabel(string clueId) => clueId switch
        {
            "demo.clue.boat_schedule" => "Escala do barco",
            "demo.clue.helena_birth" => "Conversa sobre o nascimento",
            "demo.clue.village_decision" => "Decisão da vila",
            "demo.clue.stair_repair" => "Reparo dos degraus",
            "demo.clue.blue_ribbon" => "Tira azul na manga de Ivo",
            _ => clueId
        };

        private bool Knows(EntryId id) => notebook.Exists(entry => entry.Id == id);

        private bool IsAvailable(Topic topic) => topic.Entry != EntryId.IvoDeparture ||
            (Knows(EntryId.NextBoat) && Knows(EntryId.Helena));

        private void SelectTopic(Topic topic)
        {
            if (speaker == null || !IsAvailable(topic)) return;
            reply = topic;
            focusedChoice = -1;
            if (topic.Entry == EntryId.None || Knows(topic.Entry)) return;
            string source = topic.Kind == EntryKind.Observação ? "Manga de Ivo Batista" : speaker.Name;
            notebook.Add((topic.Entry, topic.Kind, source, topic.Answer));
            narrative.Discover(ClueId(topic.Entry),
                topic.Kind == EntryKind.Observação ? "observacao" : "depoimento", source);
            narrative.Save();
        }

        private void HandleChoiceInput(Keyboard keyboard)
        {
            if (reply != null)
            {
                if (keyboard.enterKey.wasPressedThisFrame)
                    reply = null;
                return;
            }

            int count = CountChoices();
            if (count == 0) return;
            if (keyboard.downArrowKey.wasPressedThisFrame)
                focusedChoice = (focusedChoice + 1) % count;
            if (keyboard.upArrowKey.wasPressedThisFrame)
                focusedChoice = focusedChoice < 0 ? count - 1 : (focusedChoice + count - 1) % count;

            if (!keyboard.enterKey.wasPressedThisFrame) return;
            int chosen = Mathf.Max(0, focusedChoice);
            foreach (var topic in speaker.Topics)
            {
                if (IsAvailable(topic) && chosen-- == 0)
                {
                    SelectTopic(topic);
                    return;
                }
            }
        }

        private int CountChoices()
        {
            int count = 0;
            foreach (var topic in speaker.Topics)
                if (IsAvailable(topic)) count++;
            return count;
        }

        private void OnDisable()
        {
            if ((speaker != null || readingNotebook || readingInventory) && session != null)
                ClosePanel();
        }

        private void OnYarnDialogueComplete()
        {
            if (speaker != null)
            {
                yarnConversation = false;
                ClosePanel();
            }
        }

        private void OnDestroy()
        {
            if (yarnPresenter != null) yarnPresenter.Completed -= OnYarnDialogueComplete;
            narrative?.Dispose();
            if (ownedRunner != null) Destroy(ownedRunner);
            Destroy(subtitleGradient);
            Destroy(choiceBackground);
            Destroy(focusBackground);
        }

        private void OnGUI()
        {
            bool exploring = session != null && session.IsCaptured;
            if (!exploring && speaker == null && !readingNotebook && !readingInventory) return;

            InitializeStyles();
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 900f, Screen.height / 560f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float width = Screen.width / scale;
            float height = Screen.height / scale;

            if (readingNotebook)
                DrawNotebookPanel(width, height);
            else if (readingInventory)
                DrawInventoryPanel(width, height);
            else if (speaker != null)
                DrawConversation(width, height);
            else if (exploring)
                DrawExplorationPrompts(width, height);

            GUI.matrix = previousMatrix;
        }

        private void DrawExplorationPrompts(float width, float height)
        {
            float left = (width - 380f) * 0.5f;
            if (inventory != null)
                GUI.Label(new Rect(width - 168f, height - 44f, 150f, 28f), "[I] Inventário", promptStyle);
            if (yarnRunner != null)
                GUI.Label(new Rect(width - 230f, height - 74f, 210f, 26f),
                    "[F2] Demo Yarn: " + (narrativeDemoMode ? "ativo" : "inativo"), promptStyle);

            if (nearbyPickup != null)
                GUI.Label(new Rect(left, height - 75f, 380f, 38f),
                    "[E]  Pegar " + nearbyPickup.DisplayName, promptStyle);
            else if (nearby != null)
                GUI.Label(new Rect(left, height - 75f, 380f, 38f),
                    "[E]  Conversar com " + nearby.Name, promptStyle);

            if (Time.unscaledTime < pickupMessageUntil)
                GUI.Label(new Rect(left, height - 118f, 380f, 32f), pickupMessage, promptStyle);
        }

        private void DrawInventoryPanel(float width, float height)
        {
            const float panelWidth = 390f;
            const float panelHeight = 250f;
            var bounds = new Rect(width - panelWidth - 22f, height - panelHeight - 22f,
                panelWidth, panelHeight);
            DrawPanelBackground(bounds);
            GUILayout.BeginArea(bounds, panelStyle);
            GUILayout.Label("Inventário", nameStyle);
            GUILayout.Space(10);
            inventoryScroll = GUILayout.BeginScrollView(inventoryScroll);
            if (inventory.IsEmpty)
                GUILayout.Label("Nenhum item guardado.", lineStyle);
            foreach (var item in inventory.Items)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(item.Name, lineStyle);
                GUILayout.FlexibleSpace();
                GUILayout.Label("×" + item.Quantity, nameStyle);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();

            if (GUILayout.Button("Fechar inventário  ·  I / Esc", buttonStyle))
                ClosePanel();
            GUILayout.EndArea();
        }

        private void DrawNotebookPanel(float width, float height)
        {
            const float panelHeight = 300f;
            var bounds = new Rect((width - 680) * 0.5f, height - panelHeight - 22, 680, panelHeight);
            DrawPanelBackground(bounds);
            GUILayout.BeginArea(bounds, panelStyle);
            DrawNotebook();

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Fechar caderno", buttonStyle))
                ClosePanel();
            GUILayout.EndArea();
        }

        private static void DrawPanelBackground(Rect bounds)
        {
            Color previousColor = GUI.color;
            GUI.color = new Color(0.055f, 0.08f, 0.075f, 0.96f);
            GUI.DrawTexture(bounds, Texture2D.whiteTexture);
            GUI.color = previousColor;
        }

        private void DrawConversation(float width, float height)
        {
            if (yarnConversation)
            {
                DrawYarnConversation(width, height);
                return;
            }
            const float rowHeight = 26f;
            const float rowPitch = 30f;
            const float actionHeight = 24f;
            const float bottom = 18f;

            float textWidth = Mathf.Min(640f, width - 70f);
            float left = (width - textWidth) * 0.5f;
            string line = reply?.Answer ?? speaker.Greeting;
            float lineHeight = subtitleLine.CalcHeight(new GUIContent(line), textWidth);
            bool hasReply = reply != null;
            int choiceCount = hasReply ? 0 : CountChoices();
            float actionsY = height - bottom - actionHeight;
            float choicesY = actionsY - choiceCount * rowPitch;
            float lineY = choicesY - lineHeight - 9f;
            float nameY = lineY - 23f;

            GUI.DrawTexture(new Rect(0, nameY - 42f, width, height - nameY + 42f), subtitleGradient);
            GUI.Label(new Rect(left, nameY, textWidth, 21f),
                reply?.Kind == EntryKind.Observação ? "Observação" : speaker.Name, subtitleName);
            GUI.Label(new Rect(left, lineY, textWidth, lineHeight), line, subtitleLine);

            if (GUI.Button(new Rect(left + textWidth - 156f, actionsY, 156f, actionHeight),
                    "Encerrar conversa  ·  Esc", subtitleExit))
            {
                ClosePanel();
                return;
            }

            if (hasReply)
            {
                if (GUI.Button(new Rect(left, actionsY, 180f, actionHeight),
                    "‹  Voltar aos assuntos", subtitleChoice))
                    reply = null;
                return;
            }

            int index = 0;
            foreach (var topic in speaker.Topics)
            {
                if (!IsAvailable(topic)) continue;
                var style = index == focusedChoice ? subtitleFocus : subtitleChoice;
                var row = new Rect(left, choicesY + index * rowPitch, textWidth, rowHeight);
                if (GUI.Button(row, topic.Question, style))
                {
                    SelectTopic(topic);
                    return;
                }
                index++;
            }
        }

        private void DrawYarnConversation(float width, float height)
        {
            const float bottom = 18f;
            float textWidth = Mathf.Min(640f, width - 70f);
            float left = (width - textWidth) * 0.5f;
            var options = yarnPresenter.CurrentOptions;
            int count = 0;
            foreach (var option in options) if (option.IsAvailable) count++;
            const float rowHeight = 26f;
            const float rowPitch = 30f;
            float actionsY = height - bottom - 24f;
            float choicesY = actionsY - count * rowPitch;
            string line = yarnPresenter.CurrentLine ?? "";
            float lineHeight = subtitleLine.CalcHeight(new GUIContent(line), textWidth);
            float lineY = choicesY - lineHeight - 9f;
            float nameY = lineY - 23f;
            GUI.DrawTexture(new Rect(0, nameY - 42f, width, height - nameY + 42f), subtitleGradient);
            GUI.Label(new Rect(left, nameY, textWidth, 21f),
                yarnPresenter.CurrentSpeaker + "  ·  " + yarnPresenter.LocaleLabel, subtitleName);
            if (!string.IsNullOrEmpty(line))
                GUI.Label(new Rect(left, lineY, textWidth, lineHeight), line, subtitleLine);
            int index = 0;
            foreach (var option in options)
            {
                if (!option.IsAvailable) continue;
                var style = index == focusedChoice ? subtitleFocus : subtitleChoice;
                if (GUI.Button(new Rect(left, choicesY + index * rowPitch, textWidth, rowHeight), option.Line.Text.Text, style))
                {
                    if (yarnPresenter.IsShowingLine) yarnPresenter.AdvanceLine();
                    else yarnPresenter.SelectOption(option);
                }
                index++;
            }
            if (!string.IsNullOrEmpty(line) && GUI.Button(new Rect(left + textWidth - 156f, actionsY, 156f, 24f), "Avançar  ·  Enter", subtitleExit))
                yarnPresenter.AdvanceLine();
            else if (count == 0 && GUI.Button(new Rect(left + textWidth - 156f, actionsY, 156f, 24f), "Encerrar conversa  ·  Esc", subtitleExit))
                ClosePanel();
        }

        private void DrawNotebook()
        {
            GUILayout.Label("Caderno", nameStyle);
            notebookScroll = GUILayout.BeginScrollView(notebookScroll);
            if (notebook.Count == 0)
                GUILayout.Label("Nenhuma descoberta registrada.", lineStyle);

            foreach (var entry in notebook)
            {
                GUILayout.Label(entry.Kind + " · " + entry.Source, nameStyle);
                GUILayout.Label(entry.Text, lineStyle);
            }
            if (narrative != null)
            {
                foreach (var detail in narrative.Progress.discoveryDetails)
                {
                    if (notebook.Exists(entry => ClueId(entry.Id) == detail.clueId)) continue;
                    string kind = detail.kind == "observacao" ? "Observação" : "Depoimento";
                    GUILayout.Label(kind + " · " + detail.source, nameStyle);
                    GUILayout.Label(ClueLabel(detail.clueId), lineStyle);
                }
                foreach (var clueId in narrative.Progress.discovered)
                    if (!narrative.Progress.discoveryDetails.Exists(detail => detail.clueId == clueId))
                        GUILayout.Label("Descoberta · " + ClueLabel(clueId), lineStyle);
            }
            GUILayout.EndScrollView();
        }

        private sealed class CoastalNarrativeClock : INarrativeClock
        {
            private readonly Freyja.Environment.CoastalDayNight source;
            private float lastHour;
            private float accumulated;
            public CoastalNarrativeClock(Freyja.Environment.CoastalDayNight source)
            {
                this.source = source;
                lastHour = source.Hour;
                accumulated = source.Hour;
            }
            public int Hour => Mathf.FloorToInt(accumulated);
            public void Restore(int hour) { accumulated = Mathf.Max(0, hour); lastHour = source.Hour; }
            public void Tick()
            {
                float now = source.Hour;
                accumulated += Mathf.Repeat(now - lastHour, 24f);
                lastHour = now;
            }
        }

        private void InitializeStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle { padding = new RectOffset(22, 22, 15, 15) };
            var ink = new Color(0.92f, 0.93f, 0.87f);
            nameStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, fontStyle = FontStyle.Bold };
            nameStyle.normal.textColor = new Color(0.79f, 0.88f, 0.75f);
            lineStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
            lineStyle.normal.textColor = ink;
            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16, alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(14, 14, 5, 5), fixedHeight = 30
            };
            promptStyle = new GUIStyle(GUI.skin.box) { fontSize = 17, alignment = TextAnchor.MiddleCenter };
            promptStyle.normal.textColor = ink;

            subtitleGradient = CreateGradient();
            choiceBackground = CreateSwatch(new Color(0.025f, 0.045f, 0.055f, 0.42f));
            focusBackground = CreateSwatch(new Color(0.17f, 0.32f, 0.40f, 0.94f));
            subtitleName = new GUIStyle(GUI.skin.label) { fontSize = 15, fontStyle = FontStyle.Bold };
            subtitleName.normal.textColor = new Color(0.68f, 0.84f, 0.92f);
            subtitleLine = new GUIStyle(GUI.skin.label) { fontSize = 21, wordWrap = true };
            subtitleLine.normal.textColor = new Color(0.98f, 0.975f, 0.94f);
            subtitleChoice = new GUIStyle
            {
                font = GUI.skin.label.font,
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft,
                padding = new RectOffset(10, 10, 0, 0)
            };
            subtitleChoice.normal.background = choiceBackground;
            subtitleChoice.normal.textColor = new Color(0.91f, 0.94f, 0.95f);
            subtitleChoice.hover.background = subtitleChoice.active.background = focusBackground;
            subtitleChoice.hover.textColor = subtitleChoice.active.textColor = Color.white;
            subtitleFocus = new GUIStyle(subtitleChoice);
            subtitleFocus.normal.background = focusBackground;
            subtitleFocus.normal.textColor = Color.white;
            subtitleExit = new GUIStyle(subtitleChoice) { fontSize = 13, alignment = TextAnchor.MiddleRight };
            subtitleExit.normal.background = null;
            subtitleExit.normal.textColor = new Color(0.77f, 0.83f, 0.85f);
        }

        private static Texture2D CreateGradient()
        {
            const int height = 64;
            var gradient = new Texture2D(1, height, TextureFormat.RGBA32, false)
            {
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            for (int y = 0; y < height; y++)
            {
                float opacity = 0.98f * Mathf.SmoothStep(0f, 1f, (1f - y / (height - 1f)) * 1.17f);
                gradient.SetPixel(0, y, new Color(0.018f, 0.029f, 0.036f, opacity));
            }
            gradient.Apply(false, true);
            return gradient;
        }

        private static Texture2D CreateSwatch(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, color);
            texture.Apply(false, true);
            return texture;
        }
    }
}
