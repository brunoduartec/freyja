#if UNITY_EDITOR
using System.Linq;
using Freyja.Narrative;
using UnityEditor;
using UnityEngine;

namespace Freyja.Narrative.Editor
{
    public sealed class NarrativeDebuggerWindow : EditorWindow
    {
        private string npcId = "demo.ivo";
        private NarrativeProgress snapshot;
        [MenuItem("Freyja/Narrative/Debugger (read-only)")]
        private static void Open() => GetWindow<NarrativeDebuggerWindow>("Narrative Debugger");
        private void OnEnable() => snapshot = NarrativeProgress.LoadDemo();
        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Somente leitura: estado Demo isolado; nenhum save é alterado.", MessageType.Info);
            npcId = EditorGUILayout.TextField("NPC", npcId);
            if (GUILayout.Button("Recarregar snapshot")) snapshot = NarrativeProgress.LoadDemo();
            if (snapshot == null) return;
            EditorGUILayout.LabelField("Hora salva", snapshot.clockHour.ToString());
            EditorGUILayout.LabelField("Pistas do jogador", string.Join(", ", snapshot.discovered));
            EditorGUILayout.LabelField("Conhecimento do NPC", string.Join(", ", snapshot.npcKnowledge.Where(k => k.npcId == npcId).Select(k => k.clueId)));
            bool contextual = snapshot.Knows("demo.ivo", "demo.clue.boat_schedule") && snapshot.Knows("demo.ivo", "demo.clue.helena_birth");
            EditorGUILayout.LabelField("Pergunta contextual de Ivo", contextual ? "elegível" : "bloqueada: requer comunicação de ambas as pistas a Ivo");
            EditorGUILayout.LabelField("Histórico Once/Cooldown", snapshot.history.Count.ToString());
            EditorGUILayout.LabelField("Efeitos", string.Join(", ", snapshot.effects));
        }
    }
}
#endif
