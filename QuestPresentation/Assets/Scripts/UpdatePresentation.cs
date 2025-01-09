using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdatePresentation : MonoBehaviour {

  public List<Texture> slides;
  
  private int currentSlide;
  
  private void updateScreen ()
  {
    GetComponent <MeshRenderer> ().material.mainTexture = slides[currentSlide];
  }
  
  public void Start ()
  {
    currentSlide = 0;
    updateScreen ();
  }
  
  public void nextSlide ()
  {
    currentSlide = (currentSlide + slides.Count + 1) % slides.Count;
    updateScreen ();
  }

  public void previousSlide ()
  {
    currentSlide = (currentSlide + slides.Count - 1) % slides.Count;
    updateScreen ();
  }
}
