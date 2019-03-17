using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleARCore;
using System;
using System.Runtime.InteropServices;

public class PointClouder : MonoBehaviour {

  [Tooltip ("An output string for debugging.")]
  public TextMesh logger;
  
  [Tooltip ("Shape to place around each target point.")]
  public GameObject nodeShape;
  
  [Tooltip ("An object to collect all voxel game objects under.")]
  public GameObject voxelParent;
  
  [Tooltip ("The label on the capture button - changes to enable/disable.")]
  public Text captureMessage;
  
  [Tooltip ("The camera, source of colours for the marker points.")]
  public Camera arCamera;
  
  [Tooltip ("Switch off view of the background to see the octtree more easily.")]
  public ARCoreBackgroundRenderer background;
  
  [Tooltip ("Session manager allows choice of camera configurations.")]
  public ARCoreSession ARSessionManager;

  // A tree for progressively mapping out the scene.
  private OctTree tree;
  
  // Only add points when true.
  private bool addVoxels = false;
    
  void Start () {
    tree = new OctTree (nodeShape);
    
    ARSessionManager.RegisterChooseCameraConfigurationCallback (chooseCameraConfiguration);
    ARSessionManager.enabled = true;
  }

  private int chooseCameraConfiguration(List<CameraConfig> supportedConfigurations)
  {
    // Pick the lowest grade camera.
    return 0;
  }
  
  // Used by external controls to switch the mode of this component
  // between capturing and adding to the octtree, or to just
  // displaying the octtree as it stands.
  public void toggleCapture ()
  {
    addVoxels = !addVoxels;
    if (background != null)
    {
      background.enabled = addVoxels;
    }
    if (addVoxels)
    {
      captureMessage.text = "Disable capture";
    }
    else
    {
      captureMessage.text = "Enable capture";
    }
  }

  // Derived from: https://github.com/DavidSM64/N64-YUV2RGB-Library
  private Color YUV2RGB (byte Y, byte U, byte V)
  {
    return new Color (Mathf.Clamp ((1.164f * (Y - 16) + 1.596f * (V - 128)) / 255.0f, 0.0f, 1.0f),
                      Mathf.Clamp ((1.164f * (Y - 16) - 0.813f * (V - 128) - 0.391f * (U - 128)) / 255.0f, 0.0f, 1.0f),
                      Mathf.Clamp ((1.164f * (Y - 16) + 2.018f * (U - 128)) / 255.0f, 0.0f, 1.0f));
  }

  // Find the colour at the given coordinates. The camera image format
  // requires some care. It uses the YUV colour space so colour values
  // need to be converted from this space to RGB for display. The Y plane
  // uses twice the resolution of the other two, so calculating offets to
  // particular pixel resolutions needs to take this into account.
  private Color getColourAt (CameraImageBytes cim, int cx, int cy, out bool foundColour)
  {
    Color colour = new Color (0, 0, 0);
    foundColour = false;
    
    if ((cx >= 0) && (cy >= 0) && (cx < cim.Width) && (cy < cim.Height))
      {
        byte [] buf = new byte [1];
        byte y;
        byte u;
        byte v;
        Marshal.Copy (new IntPtr (cim.Y.ToInt64 () + cy * cim.YRowStride + cx * 1), buf, 0, 1);
        y = buf[0];
        // Watch out - the UV plane is a quarter of the resolution.
        Marshal.Copy (new IntPtr (cim.U.ToInt64 () + (cy / 2) * cim.UVRowStride + (cx / 2) * cim.UVPixelStride), buf, 0, 1);
        u = buf[0];
        Marshal.Copy (new IntPtr (cim.V.ToInt64 () + (cy / 2) * cim.UVRowStride + (cx / 2) * cim.UVPixelStride), buf, 0, 1);
        v = buf[0];
        colour = YUV2RGB (y, u, v);
        foundColour = true;
      }

    return colour;
  }
  
  void Update () {
    if (Frame.PointCloud.IsUpdatedThisFrame)
    {
      if (logger != null)
      {
        logger.text = "Have Points";
      }
      if (addVoxels)
      {
        // Take a snapshot of the camera image at this time.
        CameraImageBytes cim = Frame.CameraImage.AcquireCameraImageBytes();
        
        // Copy the point cloud points for mesh vertices.
        for (int i = 0; i < Frame.PointCloud.PointCount; i++)
        {
          Color colour = new Color (0, 0, 0);
          bool foundColour = false;
          
          // Get a point.
          PointCloudPoint p = Frame.PointCloud.GetPointAsStruct (i);
          
          // Work out where in the camera image this point would be.
          Vector3 cameraCoordinates = arCamera.WorldToViewportPoint (p);

          // Get the colour from the image, corresponding to this point.
          if (cim.IsAvailable)
          {
            var uvQuad = Frame.CameraImage.DisplayUvCoords;
            // FIXME : take frame display into account - see uvQuad. This should involve just
            // interpolation, as done in the background shader.
            int cx = (int) (cameraCoordinates.x * cim.Width);
            int cy = (int) ((1.0f - cameraCoordinates.y) * cim.Height);
            colour = getColourAt (cim, cx, cy, out foundColour);
          }
          
          // Add the point to the Oct Tree.
          if (foundColour)
          {
            tree.addPoint (p, colour);
          }
        }
        
        cim.Release ();
      }
      // Update the scene description of the Oct Tree.
      tree.renderOctTree (voxelParent);
    }	
    else
    {
      if (logger != null)
      {
        logger.text = "No Points";
      }
    }
  }
}
