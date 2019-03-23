using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.UI;

public class AccessOpenCV : MonoBehaviour
{
	  public Text text;

    [DllImport("VisualRecognition")]
    private static extern float Foopluginmethod();

    void Start ()
    {
        // This Line should display "Foopluginmethod: 10"
        text.text = "Foopluginmethod: " + Foopluginmethod();
    }
}
