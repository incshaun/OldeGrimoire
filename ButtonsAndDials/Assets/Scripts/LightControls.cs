using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightControls : MonoBehaviour
{
    public void setColour (Color col)
    {
        GetComponent <MeshRenderer> ().material.color = col;
    }

    public void lightOff ()
    {
        setColour (new Color (0, 0, 0)); 
    }

    public void lightOn ()
    {
        setColour (new Color (1, 1, 0.8f)); 
    }
}
