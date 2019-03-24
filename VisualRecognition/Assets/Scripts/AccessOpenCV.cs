using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using System.IO;
using System;

public class AccessOpenCV : MonoBehaviour
{
	  public Text text;

    [DllImport("VisualRecognition")]
    private static extern float Foopluginmethod();

    [DllImport("VisualRecognition")]
    private static extern void prepareModel (string dirname);

    void Start ()
    {
      StartCoroutine (prepareModel ());
    }
    
  IEnumerator prepareModel ()
  {  
    yield return StartCoroutine(extractFile ("", "MobileNetSSD_deploy.caffemodel"));
    yield return StartCoroutine(extractFile ("", "MobileNetSSD_deploy.prototxt.txt"));
    yield return StartCoroutine(extractFile ("", "example_01.jpg"));
    
    // This Line should display "Foopluginmethod: 10"
    text.text = "Foopluginmethod: " + Foopluginmethod();
       
    prepareModel (Application.persistentDataPath);
    
    yield return null;
  }
  
  // Copy file from the android package to a readable/writeable region of the host file system.
  IEnumerator extractFile (string assetPath, string assetFile)
  {
    // Source is the streaming assets path.
    string sourcePath = Application.streamingAssetsPath + "/" + assetPath + assetFile;
    if ((sourcePath.Length > 0) && (sourcePath[0] == '/'))
    {
      sourcePath = "file://" + sourcePath;
    }
    string destinationPath = Application.persistentDataPath + "/" + assetFile;
    
    // Files must be handled via a WWW to extract.
    WWW w = new WWW (sourcePath);
    yield return w;
    try
    {
      File.WriteAllBytes (destinationPath, w.bytes);
    }
    catch (Exception e)
    { 
      Debug.Log ("Issue writing " + destinationPath + " " + e);
    }
    Debug.Log (sourcePath + " -> " + destinationPath + " " + w.bytes.Length);
  }
}
