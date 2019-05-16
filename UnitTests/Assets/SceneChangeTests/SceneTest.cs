using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneTest {

    [Test]
    // Make sure the scene change returns an 
    // error if the name of the destination scene
    // is invalid.
    public void SceneChangeChecksDestination() {
      bool result = SceneChanger.changeScene ("ThisSceneShouldNotExist");
      Assert.AreEqual (result, false, "Changing to a non existent scene should fail - instead succeeded");
    }

    [UnityTest]
    // Make sure that the scene change can actually
    // change scenes, and end up in a different scene.
    public IEnumerator SceneChangeResultsInNewScene() {
      Debug.Log (SceneManager.GetActiveScene ().name + " " + SceneManager.sceneCount + " " + SceneManager.sceneCountInBuildSettings);
        string targetScene = "TestDestScene";
      //SceneManager.LoadScene (targetScene);
        Assert.AreNotEqual (SceneManager.GetActiveScene ().name, targetScene, "Test is starting in the wrong scene.");
        yield return null;
        
        bool result = SceneChanger.changeScene (targetScene);
        Assert.AreEqual (result, true, "Attempt to change scene to " + targetScene + "failed.");

        // Give a few frames for new scene to load.
        yield return null;
        yield return null;
        yield return null;

        Assert.AreEqual (SceneManager.GetActiveScene ().name, targetScene, "Destination scene not reached after scene change");        
    }
}
