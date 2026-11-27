using System.Collections;
using Freyja.Narrative;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Yarn.Unity;

namespace Freyja.Narrative.Tests
{
    public sealed class NarrativeYarnFlowTests
    {
        [UnityTest]
        public IEnumerator RunnerPresentsContextualOptionOnlyAfterBothExplicitShares()
        {
            var fixture = SceneManager.CreateScene("Narrative_PlayMode_Fixture");
            var runtimeObject = new GameObject("Narrative fixture runner");
            SceneManager.MoveGameObjectToScene(runtimeObject, fixture);
            var runner = runtimeObject.AddComponent<DialogueRunner>();
            var reference = Resources.Load<DemoYarnProjectReference>("Narrative/DemoYarnProjectReference");
            Assert.IsNotNull(reference);
            runner.SetProject(reference.Project);
            var presenter = runtimeObject.AddComponent<NarrativeYarnPresenter>();
            presenter.AutoAdvanceForTests = true;
            runner.DialoguePresenters = new DialoguePresenterBase[] { presenter };
            var progress = new NarrativeProgress();
            progress.Discover("demo.clue.boat_schedule");
            progress.Discover("demo.clue.helena_birth");
            var bridge = new NarrativeYarnBridge(runner, new NarrativeRuntime(progress, new FixedNarrativeClock(8), false));

            presenter.AutoSelectLineId = "line:demo.ivo.share_boat";
            bridge.StartNpc("Demo_Ivo", "demo.ivo");
            yield return WaitFor(() => progress.Knows("demo.ivo", "demo.clue.boat_schedule"), runner, presenter);
            Assert.IsFalse(progress.Knows("demo.ivo", "demo.clue.helena_birth"));

            presenter.AutoSelectLineId = "line:demo.ivo.share_helena";
            bridge.StartNpc("Demo_Ivo", "demo.ivo");
            yield return WaitFor(() => progress.Knows("demo.ivo", "demo.clue.helena_birth"), runner, presenter);
            Assert.IsTrue(progress.Knows("demo.ivo", "demo.clue.boat_schedule"));

            presenter.AutoSelectLineId = "line:demo.ivo.ask_context";
            bridge.StartNpc("Demo_Ivo", "demo.ivo");
            yield return WaitFor(() => progress.effects.Contains("demo.ivo.departure_question"), runner, presenter);
            Assert.IsTrue(presenter.PresentedLineIds.Contains("line:demo.ivo.context_1"));
            Assert.IsTrue(progress.history.Exists(x => x.storyletId == "demo.ivo.departure_revealed"));

            Object.Destroy(runtimeObject);
            yield return SceneManager.UnloadSceneAsync(fixture);
        }

        [UnityTest]
        public IEnumerator ConfirmedYarnLineRecordsTypedDiscoveryAndNpcSource()
        {
            var fixture = SceneManager.CreateScene("Narrative_Discovery_PlayMode_Fixture");
            var runtimeObject = new GameObject("Narrative discovery fixture runner");
            SceneManager.MoveGameObjectToScene(runtimeObject, fixture);
            var runner = runtimeObject.AddComponent<DialogueRunner>();
            var reference = Resources.Load<DemoYarnProjectReference>("Narrative/DemoYarnProjectReference");
            Assert.IsNotNull(reference);
            runner.SetProject(reference.Project);
            var presenter = runtimeObject.AddComponent<NarrativeYarnPresenter>();
            presenter.AutoAdvanceForTests = true;
            presenter.AutoSelectLineId = "line:demo.nuno.ask_schedule";
            runner.DialoguePresenters = new DialoguePresenterBase[] { presenter };
            var progress = new NarrativeProgress();
            var bridge = new NarrativeYarnBridge(runner, new NarrativeRuntime(progress, new FixedNarrativeClock(8), false));
            bridge.StartNpc("Demo_Nuno", "demo.nuno");

            yield return WaitFor(() => progress.discoveryDetails.Count == 1, runner, presenter);
            Assert.AreEqual("demo.clue.boat_schedule", progress.discoveryDetails[0].clueId);
            Assert.AreEqual("depoimento", progress.discoveryDetails[0].kind);
            Assert.AreEqual("Nuno Pires", progress.discoveryDetails[0].source);
            Assert.AreEqual(1, progress.discovered.Count);

            Object.Destroy(runtimeObject);
            yield return SceneManager.UnloadSceneAsync(fixture);
        }


        private static IEnumerator WaitForPresenter(System.Func<bool> condition, NarrativeYarnPresenter presenter)
        {
            for (int i = 0; i < 600 && !condition(); i++) yield return null;
            Assert.IsTrue(condition(), "Presenter state not reached. lines=" + string.Join("|", presenter.PresentedLineIds) + " options=" + string.Join("|", presenter.PresentedOptionIds));
        }


        [UnityTest]
        public IEnumerator BuiltInLocaleSwitchChangesTextWithoutChangingLineId()
        {
            var fixture = SceneManager.CreateScene("Narrative_Locale_PlayMode_Fixture");
            var runtimeObject = new GameObject("Narrative locale fixture runner");
            SceneManager.MoveGameObjectToScene(runtimeObject, fixture);
            var runner = runtimeObject.AddComponent<DialogueRunner>();
            var reference = Resources.Load<DemoYarnProjectReference>("Narrative/DemoYarnProjectReference");
            Assert.IsNotNull(reference);
            runner.SetProject(reference.Project);
            var presenter = runtimeObject.AddComponent<NarrativeYarnPresenter>();
            presenter.AutoAdvanceForTests = true;
            runner.DialoguePresenters = new DialoguePresenterBase[] { presenter };
            var progress = new NarrativeProgress();
            var bridge = new NarrativeYarnBridge(runner, new NarrativeRuntime(progress, new FixedNarrativeClock(8), false));
            bridge.SetLocale("en-US");
            bridge.StartNpc("Demo_Nuno", "demo.nuno");
            yield return WaitForPresenter(() => presenter.PresentedLineIds.Contains("line:demo.nuno.greeting"), presenter);
            Assert.AreEqual("line:demo.nuno.greeting", presenter.PresentedLineIds[0]);
            StringAssert.Contains("If this is about the boat", presenter.PresentedLineTexts[0]);
            Assert.AreEqual(0, progress.discovered.Count);
            runner.Stop().Forget();
            Object.Destroy(runtimeObject);
            yield return SceneManager.UnloadSceneAsync(fixture);
        }

        private static IEnumerator WaitFor(System.Func<bool> condition, DialogueRunner runner, NarrativeYarnPresenter presenter)
        {
            for (int i = 0; i < 600 && !condition(); i++) yield return null;
            Assert.IsTrue(condition(), "Yarn dialogue did not reach expected state. lines=" + string.Join("|", presenter.PresentedLineIds) + " options=" + string.Join("|", presenter.PresentedOptionIds));
            for (int i = 0; i < 120 && runner.IsDialogueRunning; i++) yield return null;
            Assert.IsFalse(runner.IsDialogueRunning, "Yarn dialogue did not complete after the selected option.");
        }
    }
}
