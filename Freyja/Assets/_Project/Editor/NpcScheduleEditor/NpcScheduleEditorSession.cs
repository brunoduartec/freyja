using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEditor;

namespace Freyja.NpcScheduling.Editor
{
    internal sealed class NpcScheduleEditorSession
    {
        private sealed class Snapshot { public string Text; public string EntryId; }
        private const int HistoryLimit = 30;
        private readonly Stack<Snapshot> undo = new Stack<Snapshot>();
        private readonly Stack<Snapshot> redo = new Stack<Snapshot>();
        public NpcScheduleDocument Document { get; private set; }
        public string OriginalText { get; private set; }
        public string OriginalFingerprint { get; private set; }
        public string Draft { get; private set; }
        public string SelectedEntryId { get; set; }
        public bool Dirty { get; private set; }
        public string Error { get; private set; }
        public int Revision { get; private set; }

        public void Select(NpcScheduleDocument document, string readOnlyError = null)
        {
            Document = document;
            OriginalText = document.Text;
            OriginalFingerprint = document.Fingerprint;
            Draft = document.Text;
            SelectedEntryId = null;
            Error = document.Error ?? readOnlyError;
            Document.ReadOnly = !string.IsNullOrEmpty(readOnlyError);
            undo.Clear(); redo.Clear();
            Dirty = false;
            Revision++;
            try { JObject.Parse(Draft); }
            catch { Error = "JSON inválido. O arquivo está em modo somente leitura."; }
        }

        public void ReplaceDraft(string text)
        {
            if (Document == null || Document.ReadOnly || EditorApplication.isPlaying) return;
            Push(undo, new Snapshot { Text = Draft, EntryId = SelectedEntryId });
            Draft = text;
            Dirty = Draft != OriginalText;
            redo.Clear();
            Error = null;
            Revision++;
        }

        public bool Undo()
        {
            if (undo.Count == 0) return false;
            Push(redo, new Snapshot { Text = Draft, EntryId = SelectedEntryId });
            Snapshot snapshot = undo.Pop(); Draft = snapshot.Text; Dirty = Draft != OriginalText; SelectedEntryId = snapshot.EntryId; Revision++; return true;
        }

        public bool Redo()
        {
            if (redo.Count == 0) return false;
            Push(undo, new Snapshot { Text = Draft, EntryId = SelectedEntryId });
            Snapshot snapshot = redo.Pop(); Draft = snapshot.Text; Dirty = Draft != OriginalText; SelectedEntryId = snapshot.EntryId; Revision++; return true;
        }

        public void Revert()
        {
            Draft = OriginalText; Dirty = false; SelectedEntryId = null; undo.Clear(); redo.Clear(); Error = null; Revision++;
        }

        public void Saved(NpcScheduleDocument document)
        {
            Select(document);
        }

        private static void Push(Stack<Snapshot> stack, Snapshot snapshot)
        {
            if (stack.Count == HistoryLimit)
            {
                Snapshot[] keep = stack.ToArray(); stack.Clear();
                for (int i = keep.Length - 2; i >= 0; i--) stack.Push(keep[i]);
            }
            stack.Push(snapshot);
        }
    }
}
