using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Provide a simulated location tracking facility for use in test
// applications.
public class PrototypeLocationISP : MonoBehaviour {
  
  public float moveSpeed = 10.0f;
  public float turnSpeed = 100.0f;
  public float pathSpeed = 1.0f;
  
  [Tooltip ("If checked, step size depends on rate at which time passes, otherwise step size is constant and repeatable per call to getLocation")]
  public bool realTime = true;
  public bool manualMove = true;
  public bool manualRotate = false;
  public bool autoFollowPath = false;
  
  public float pathRadius = 10.0f;
  public int pathSides = 4;
  
  private Vector3 lastPosition;
  private Vector3 velocity;
  
  private InputSystem_Actions controls;
  
  void Start () {
    controls = new InputSystem_Actions ();
    // controls.Player.Move.performed += ctx => Move(ctx.ReadValue<Vector2>());
    controls.Enable ();
  }
  
  // private void Move (Vector2 direction)
  // {
  //   Debug.Log ("Move " + direction);
  //   processPositionControls (direction.x, direction.y, 0.0f);
  // }
  
  private void processPositionControls ()
  {
    // Handle change in position using input axes.
    float h = 0.0f;
    float v = 0.0f;
    float d = 0.0f;
    try
    {
      h = controls.Player.Move.ReadValue<Vector2>().x;
      v = controls.Player.Move.ReadValue<Vector2>().y;
      d = controls.Player.Depth.ReadValue<float>();
    }
    catch (UnityException)
    {
      Debug.Log ("Unable to read from one of input actions: Move, Depth");
    }
    
    float step = moveSpeed;
    if (realTime)
    {
      step *= Time.deltaTime;
    }
    transform.position += step * (h * transform.right + d * transform.up + v * transform.forward);
  }
  
  private void processOrientationControls ()
  {
    float mx = 0.0f;
    float my = 0.0f;
    float fire = 0.0f;
    try
    {
      mx = controls.Player.Look.ReadValue<Vector2>().x;
      my = controls.Player.Look.ReadValue<Vector2>().y;
      fire = controls.Player.Attack.ReadValue<float>();
    }
    catch (UnityException)
    {
      Debug.Log ("Unable to read from one of input actions: Look, Attack");
    }
    
    float step = turnSpeed;
    if (realTime)
    {
      step *= Time.deltaTime;
    }
    if (fire > 0.0f)
    {
      transform.rotation *= Quaternion.AngleAxis (step * my, Vector3.right) * Quaternion.AngleAxis (step * mx, Vector3.up);
    }
  }
  
  private void processControls ()
  {
    if (manualMove)
    {
      processPositionControls ();
    }
    if (manualRotate)
    {
      processOrientationControls ();
    }
  }
  
  // Wander in a circular (polygonal with n sides) horizontal path
  private void followPath ()
  {
    // Work around the angular position, based on angular speed * time.
    float angle = pathSpeed * Time.time / pathRadius;
    
    // Quantize the angle according to the number of sides available.
    float quantAngle = (2.0f * Mathf.PI / pathSides) * Mathf.Floor (angle * pathSides / (2.0f * Mathf.PI));
    
    // Calculate the length of an edge of the inscribed polygon.
    float edgeLength = 2.0f * pathRadius * Mathf.Sin (Mathf.PI / pathSides);
    
    // Work out how far the edge, assuming constant speed.
    float distanceAlongEdge = edgeLength * (angle - quantAngle) / (2.0f * Mathf.PI / pathSides);
    
    // Find the position of the initial point on the edge (start vertex)          
    float x = pathRadius * Mathf.Sin (quantAngle);
    float z = pathRadius * Mathf.Cos (quantAngle);
    // Find direction of edge, based on the circle tangent at the midpoint.
    float dx = pathRadius * Mathf.Sin (quantAngle + Mathf.PI / pathSides);
    float dz = pathRadius * Mathf.Cos (quantAngle + Mathf.PI / pathSides);
    
    // Set position and direction.
    Vector3 forward = Vector3.Normalize (new Vector3 (dz, 0, -dx));
    transform.position = new Vector3 (x, 0, z) + distanceAlongEdge * forward;
    transform.up = Vector3.up;
    transform.forward = forward;
  }
  
  void Update () {
    lastPosition = transform.position;
    
    processControls ();
    
    if (autoFollowPath)
    {
      followPath ();
    }
    
    velocity = 1.0f / Time.deltaTime * (transform.position - lastPosition);
  }
  
  public Transform getLocation ()
  {
    return transform;
  }
  
  public Vector3 getVelocity ()
  {
    return velocity;
  }
}
