using UnityEngine;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
namespace Freyja.WorldBuilder.Tests
{
    [InitializeOnLoad] public sealed class ResultRecorder:ICallbacks
    {
        static TestRunnerApi api;
        static ResultRecorder()
        {
            api=ScriptableObject.CreateInstance<TestRunnerApi>();
            api.RegisterCallbacks(new ResultRecorder());
        }
        public static string Run(bool play)
        {
            EditorSceneManagementSave();
            SessionState.SetString("WB021Result",play?"playmode-results.xml":"editmode-results.xml");
            return api.Execute(new ExecutionSettings(new Filter
            {
                testMode=play?TestMode.PlayMode:TestMode.EditMode,assemblyNames=new[]
                {
                    play?"Freyja.WorldBuilder.PlayTests":"Freyja.WorldBuilder.Tests"
                }
            }
            ));
        }
        static void EditorSceneManagementSave()=>UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        public void RunStarted(ITestAdaptor t)
        {
        }
        public void TestStarted(ITestAdaptor t)
        {
        }
        public void TestFinished(ITestResultAdaptor r)
        {
        }
        public void RunFinished(ITestResultAdaptor r)
        {
            var file=SessionState.GetString("WB021Result","");
            if(string.IsNullOrEmpty(file))return;
            TestRunnerApi.SaveResultToFile(r,System.IO.Path.GetFullPath("../Artifacts/WorldBuilder/"+file));
            SessionState.EraseString("WB021Result");
        }
    }
}
