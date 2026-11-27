using System;
using Yarn.Unity;

namespace Freyja.Narrative
{
    public sealed class NarrativeYarnBridge
    {
        private readonly DialogueRunner runner;
        private readonly NarrativeRuntime runtime;
        private string currentNpc;
        private bool revealed;

        public NarrativeYarnBridge(DialogueRunner runner, NarrativeRuntime runtime)
        {
            this.runner = runner ?? throw new ArgumentNullException(nameof(runner));
            this.runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
            runner.AddCommandHandler("discover", (Action<string, string>)Discover);
            runner.AddCommandHandler("communicate", (Action<string, string>)Communicate);
            runner.AddCommandHandler("effect_once", (Action<string>)EffectOnce);
            runner.AddCommandHandler("checkpoint", (Action<string>)Checkpoint);
            SetLocale("pt-BR");
        }
        public string Locale { get; private set; } = "pt-BR";
        public void SetLocale(string locale)
        {
            if (locale != "pt-BR" && locale != "en-US") throw new ArgumentOutOfRangeException(nameof(locale));
            Locale = locale;
            var provider = runner.GetComponent<LineProviderBehaviour>();
            if (provider == null) { _ = runner.LineProvider; provider = runner.GetComponent<LineProviderBehaviour>(); }
            if (provider != null) provider.LocaleCode = locale;
        }
        public void ToggleLocale() => SetLocale(Locale == "pt-BR" ? "en-US" : "pt-BR");

        public void StartNpc(string nodeName, string npcId)
        {
            currentNpc = npcId;
            var storage = runner.VariableStorage;
            if (storage != null)
            {
                storage.SetValue("$demo_has_boat", runtime.Progress.discovered.Contains("demo.clue.boat_schedule"));
                storage.SetValue("$demo_has_helena", runtime.Progress.discovered.Contains("demo.clue.helena_birth"));
                storage.SetValue("$demo_ivo_knows_boat", runtime.Knows("demo.ivo", "demo.clue.boat_schedule"));
                storage.SetValue("$demo_ivo_knows_helena", runtime.Knows("demo.ivo", "demo.clue.helena_birth"));
                storage.SetValue("$demo_ivo_departure_revealed", runtime.Progress.effects.Contains("demo.ivo.departure_question"));
                int hourOfDay = ((runtime.Clock.Hour % 24) + 24) % 24;
                bool clockWindow = hourOfDay < 9 || hourOfDay >= 18;
                storage.SetValue("$demo_clock_topic_available", clockWindow && runtime.Progress.IsAvailable("demo.ivo.clock_talk", RepeatPolicy.Cooldown(6), runtime.Clock.Hour));
            }
            runner.StartDialogue(nodeName);
        }
        private void Discover(string clueId, string kind)
        {
            if (!IsAuthorizedDiscovery(currentNpc, clueId, kind))
                throw new InvalidOperationException("Descoberta fora da allow-list Demo: " + currentNpc + "/" + clueId + "/" + kind);
            var source = currentNpc switch
            {
                "demo.nuno" => "Nuno Pires",
                "demo.luana" => "Luana Batista",
                "demo.ivo" => "Ivo Batista",
                _ => currentNpc
            };
            if (runtime.Discover(clueId, kind, source)) runtime.Save();
        }
        private void Communicate(string npcId, string clueId)
        {
            if (npcId != currentNpc) throw new InvalidOperationException("NPC fora do destinatário ativo.");
            if (runtime.Communicate(npcId, clueId)) runtime.Save();
            if (npcId == "demo.ivo" && clueId == "demo.clue.boat_schedule") runner.VariableStorage.SetValue("$demo_ivo_knows_boat", true);
            if (npcId == "demo.ivo" && clueId == "demo.clue.helena_birth") runner.VariableStorage.SetValue("$demo_ivo_knows_helena", true);
        }
        private void EffectOnce(string effectId)
        {
            if (currentNpc != "demo.ivo" || effectId != "demo.ivo.departure_question")
                throw new InvalidOperationException("Efeito fora da allow-list Demo: " + effectId);
            if (runtime.Progress.ApplyEffectOnce(effectId)) runtime.Save();
        }
        private void Checkpoint(string checkpointId)
        {
            if (currentNpc != "demo.ivo" || (checkpointId != "demo.ivo.clock_talk" && checkpointId != "demo.ivo.departure_revealed"))
                throw new InvalidOperationException("Checkpoint fora da allow-list Demo: " + checkpointId);
            if (checkpointId == "demo.ivo.clock_talk")
            {
                if (runtime.Progress.Confirm(checkpointId, RepeatPolicy.Cooldown(6), runtime.Clock.Hour)) runtime.Save();
                runner.VariableStorage.SetValue("$demo_clock_topic_available", false);
                return;
            }
            if (checkpointId != "demo.ivo.departure_revealed") return;
            runtime.Progress.Confirm(checkpointId, RepeatPolicy.Once, runtime.Clock.Hour);
            runtime.Save();
            runner.VariableStorage.SetValue("$demo_ivo_departure_revealed", true);
            revealed = true;
        }

        private static bool IsAuthorizedDiscovery(string npc, string clueId, string kind)
        {
            string expectedKind = clueId switch
            {
                "demo.clue.boat_schedule" => "depoimento",
                "demo.clue.helena_birth" => "depoimento",
                "demo.clue.village_decision" => "depoimento",
                "demo.clue.stair_repair" => "depoimento",
                "demo.clue.blue_ribbon" => "observacao",
                _ => null
            };
            if (kind != expectedKind) return false;
            return npc switch
            {
                "demo.nuno" => clueId == "demo.clue.boat_schedule" || clueId == "demo.clue.helena_birth",
                "demo.luana" => clueId == "demo.clue.boat_schedule" || clueId == "demo.clue.helena_birth" || clueId == "demo.clue.village_decision",
                "demo.ivo" => clueId == "demo.clue.boat_schedule" || clueId == "demo.clue.village_decision" ||
                              clueId == "demo.clue.stair_repair" || clueId == "demo.clue.blue_ribbon",
                _ => false
            };
        }
        public bool HasRevealedDeparture => revealed;
    }
}
