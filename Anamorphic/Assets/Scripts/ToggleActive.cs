using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ToggleActive : MonoBehaviour
{
  [System.Serializable]
  public class GameObjectList
  {
    public List <GameObject> objects;
  }
  
  public List <GameObjectList> selections;
  
  private int currentSelection = 0;
  
  private void switchSelection ()
  {
    if (selections.Count > 0)
    {
      currentSelection = (currentSelection + 1) % selections.Count;
      for (int i = 0; i < selections.Count; i++)
      {
        foreach (GameObject g in selections[i].objects)
        {
          g.SetActive (currentSelection == i);
        }
      }
    }
  }
  
  void Update()
  {
    if (Input.GetMouseButtonDown (0))
    {
      switchSelection ();
    }
  }
}
