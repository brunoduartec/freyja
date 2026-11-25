using UnityEngine;using UnityEditor.TestTools.TestRunner.Api;
public class RunWorldBuilderTests:ICallbacks {
 static TestRunnerApi api;public static string Main(){UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();api=ScriptableObject.CreateInstance<TestRunnerApi>();api.RegisterCallbacks(new RunWorldBuilderTests());return api.Execute(new ExecutionSettings(new Filter{testMode=TestMode.EditMode,assemblyNames=new[]{"Freyja.WorldBuilder.Tests"}}));}
 public void RunStarted(ITestAdaptor t){}public void TestStarted(ITestAdaptor t){}public void TestFinished(ITestResultAdaptor r){}
 public void RunFinished(ITestResultAdaptor r){TestRunnerApi.SaveResultToFile(r,"/Users/bruno.duartec/Documents/Projects/Freyja/Artifacts/WorldBuilder/editmode-results.xml");}
}
