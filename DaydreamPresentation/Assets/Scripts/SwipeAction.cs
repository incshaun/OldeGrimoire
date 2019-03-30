using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipeAction : MonoBehaviour {
  
  // State machine states.
  enum State { Touched, NotTouched };
  private State state;
  // Access the presentation to change slides.
  private UpdatePresentation presentationControls;
  // Access the controller to sense touchpad.
  private GvrControllerInputDevice device;
  // Remember horizontal position at start of swipe.
  private float touchx;
  // Determine length of movement to qualify as a swipe.
  private float swipeThreshold = 0.3f;
  
  void Start () {
    state = State.NotTouched;
    presentationControls = GetComponent <UpdatePresentation> ();
    device = GvrControllerInput.GetDevice (GvrControllerHand.Dominant);
  }
  
  void Update () {
    // Does not check for collisions with the screen, so swipe
    // works at any time.
    switch (state)
    {
      case State.Touched:
        // Event touch up
        if (device.GetButtonUp (GvrControllerButton.TouchPadTouch))
        {
          state = State.NotTouched;
          if (device.TouchPos.x - touchx > swipeThreshold)
          {
            presentationControls.nextSlide ();
          }
          if (device.TouchPos.x - touchx < -swipeThreshold)
          {
            presentationControls.previousSlide ();
          }
          // else just touched and released without moving.
        }
        break;
      case State.NotTouched:
        // Event touch down
        if (device.GetButtonDown (GvrControllerButton.TouchPadTouch))
        {
          touchx = device.TouchPos.x;
          state = State.Touched;
        }
        break;
    }
  }
}
