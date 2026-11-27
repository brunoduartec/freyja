using System;
using System.Threading;
using System.Linq;
using UnityEngine;
using Yarn.Unity;

namespace Freyja.Narrative
{
    public class NarrativeYarnPresenter : DialoguePresenterBase
    {
        private string speaker;
        private string line;
        private DialogueOption[] options = Array.Empty<DialogueOption>();
        private YarnTaskCompletionSource lineDone;
        private YarnTaskCompletionSource<DialogueOption> choiceDone;
        private bool visible;
        public event Action Completed;
        [NonSerialized] public bool AutoAdvanceForTests;
        [NonSerialized] public string AutoSelectLineId;
        public string LocaleLabel = "pt-BR";
        public string CurrentSpeaker => speaker;
        public string CurrentLine => line;
        public DialogueOption[] CurrentOptions => options;
        public System.Collections.Generic.List<string> PresentedLineIds { get; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> PresentedLineTexts { get; } = new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<string> PresentedOptionIds { get; } = new System.Collections.Generic.List<string>();

        public override YarnTask OnDialogueStartedAsync()
        {
            visible = true;
            return YarnTask.CompletedTask;
        }
        public override YarnTask OnDialogueCompleteAsync()
        {
            visible = false;
            lineDone?.TrySetCanceled();
            choiceDone?.TrySetCanceled();
            Completed?.Invoke();
            return YarnTask.CompletedTask;
        }
        public override async YarnTask RunLineAsync(LocalizedLine localized, LineCancellationToken token)
        {
            speaker = localized.CharacterName ?? "";
            line = localized.Text.Text;
            PresentedLineIds.Add(localized.TextID);
            PresentedLineTexts.Add(localized.RawText ?? "");
            if (AutoAdvanceForTests) return;
            options = Array.Empty<DialogueOption>();
            lineDone = new YarnTaskCompletionSource();
            using (token.NextContentToken.Register(() => lineDone.TrySetCanceled()))
            {
                try { await lineDone.Task; }
                catch (OperationCanceledException) { }
            }
        }
        public override async YarnTask<DialogueOption> RunOptionsAsync(DialogueOption[] dialogueOptions, LineCancellationToken token)
        {
            options = dialogueOptions;
            PresentedOptionIds.AddRange(dialogueOptions.Select(o => o.TextID));
            line = null;
            if (AutoAdvanceForTests)
            {
                foreach (var option in dialogueOptions)
                    if (option.IsAvailable && option.TextID == AutoSelectLineId)
                    { AutoSelectLineId = null; return option; }
                return null;
            }
            choiceDone = new YarnTaskCompletionSource<DialogueOption>();
            using (token.NextContentToken.Register(() => choiceDone.TrySetCanceled()))
            {
                try { return await choiceDone.Task; }
                catch (OperationCanceledException) { return null; }
            }
        }
        public void AdvanceLine() => lineDone?.TrySetResult();
        public void SelectOption(DialogueOption option) => choiceDone?.TrySetResult(option);
        public bool SelectOptionByTextId(string textId)
        {
            var option = options.FirstOrDefault(o => o.IsAvailable && o.TextID == textId);
            if (option == null) return false;
            SelectOption(option);
            return true;
        }
        public bool IsShowingLine => visible && !string.IsNullOrEmpty(line);
        public bool IsShowingOptions => visible && options.Length > 0;

        private void OnDisable()
        {
            visible = false;
            lineDone?.TrySetCanceled();
            choiceDone?.TrySetCanceled();
        }
    }
}
