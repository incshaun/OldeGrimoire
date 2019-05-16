using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class SceneChanger : MonoBehaviour {

  // Check if the scene name is in the build settings. Return
  // the index if so, otherwise return -1.
  private static int findSceneNameInBuild (string name)
  {
    for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
    {
      if (Path.GetFileNameWithoutExtension (SceneUtility.GetScenePathByBuildIndex (i)).Equals (name))
      {
        return i;
      }
    }
    return -1;
  }
  
  // Switch to the scene named. Return true if the operation
  // succeeds.
  public static bool changeScene (string destSceneName)
  {
    // Checks to see if the destination scene can be invoked.
    if (findSceneNameInBuild (destSceneName) < 0)
    {
      return false;
    }
    SceneManager.LoadScene (destSceneName);
    return true;
  }
}
