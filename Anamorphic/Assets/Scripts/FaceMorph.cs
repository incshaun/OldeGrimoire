using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class FaceMorph : MonoBehaviour
{
  [Tooltip ("Text element for status and debugging information")]
  public TextMeshProUGUI morphStatusText;
  
  // Handle multiple objects linked to the face morph. Useful
  // for avatar objects that have several pieces.
  [System.Serializable]
  public class FaceMorphElement
  {
    public GameObject morphSourceMesh;
    public GameObject morphTargetObject;
    public Transform morphTransformObjectToOrigin;
    
    // Internal element, used to map vertices on
    // the avatar to vertices on the canonical face.
    [System.NonSerialized]
    public int [] [] morphCanonicalToFace;
  }
  
  [SerializeField]
  [Tooltip ("Provide a list of objects that contain: original mesh, the destination mesh, and any offsets required")]
  public FaceMorphElement [] morphs;
  
  [Tooltip ("This is the object with the FaceProperties component, that connects to the face tracker from AR Foundation")]
  public FaceProperties faceSource;
  
  [Tooltip ("This is the reference face, positioned and aligned to the avatar object")]
  public GameObject canonicalObject;
  
  private Mesh canonicalMesh;
  
  void Start()
  {
    canonicalMesh = canonicalObject.GetComponent <SkinnedMeshRenderer> ().sharedMesh;
    
    createMapping ();
    
    faceSource.faceHandler += drawFace;
  }
  
  // Presently only using the first neighbour found, but with the
  // intention that smoother animation could be achieved by using
  // several close neighbours from the control mesh to drive the
  // deformation.
  private int [] findWeightsAndBasis (Vector3 p, Vector3 [] source)
  {
    int numNeighbours = 4; // How many points to use as the basis.
    
    List <(int, float)> best = new List <(int, float)> ();
    
    for (int i = 0; i < source.Length; i++)
    {
      float distance = Vector3.Distance (p, source[i]);
      
      best.Add ((i, distance));
    }
    
    best.Sort ((x, y) => x.Item2.CompareTo (y.Item2));
    
    int [] basisIndices = new int [numNeighbours];
    float [] basisWeights = new float [numNeighbours];
    for (int i = 0; i < numNeighbours; i++)
    {
      basisIndices[i] = best[i].Item1;
    }      
    
    return basisIndices;
  }
  
  private void createMapping ()
  {
    Vector3 [] refVerts = new Vector3 [canonicalMesh.vertices.Length];
    for (int i = 0; i < canonicalMesh.vertices.Length; i++)
    {
      refVerts[i] = canonicalObject.transform.TransformPoint (canonicalMesh.vertices[i]);
    }
    
    foreach (FaceMorphElement fme in morphs)
    {
      Mesh sourceMesh = fme.morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh;
      // Note that retrieving vertices is a time consuming operation. Do this
      // once, outside the loop.
      Vector3 [] morphSourceVertices = sourceMesh.vertices;
      fme.morphCanonicalToFace = new int [morphSourceVertices.Length][];
      for (int i = 0; i < morphSourceVertices.Length; i++)
      {
        // Get the position of the vertex in world coordinates,
        // so we can match the two aligned objects.
        Vector3 fvw = fme.morphSourceMesh.transform.TransformPoint (morphSourceVertices[i]);
        fme.morphCanonicalToFace[i] = findWeightsAndBasis (fvw, refVerts);
        //         Debug.Log ("fv " + fvw.ToString ("F4") + " " + canonicalToFace[i][0]);
      }
    }
  }
  
  public void drawFace (ARFace face)
  {
    // Step 1: Just use the mesh provided by the ARFace directly.
    //         target.mesh.SetVertices (face.vertices);
    //         target.mesh.SetIndices (face.indices, MeshTopology.Triangles, 0, false);
    //         target.mesh.RecalculateBounds ();
    //         target.mesh.SetNormals (face.normals);
    //         target.mesh.RecalculateNormals ();
    
    // Step 2: Use the face Mesh from the avatar.
    // FIXME: tweak transform.
    //         target.mesh.SetVertices (faceMesh.vertices);
    //         target.mesh.SetIndices (faceMesh.GetIndices (0, false), MeshTopology.Triangles, 0, false);
    //         target.mesh.RecalculateBounds ();
    //         target.mesh.SetNormals (faceMesh.normals);
    //         target.mesh.RecalculateNormals ();
    
    // Step 3: Deform avatar to match deformation on ARFace mesh.
    Vector3 [] canVertices = canonicalMesh.vertices;
    NativeArray<Vector3> faceVertices = face.vertices;
    foreach (FaceMorphElement fme in morphs)
    {
      Mesh sourceMesh = fme.morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh;
      Vector3 [] morphSourceVertices = sourceMesh.vertices;
      Vector3 [] morphTargetVertices = new Vector3 [morphSourceVertices.Length];
      for (int i = 0; i < morphSourceVertices.Length; i++)
      {
        int cIndex = fme.morphCanonicalToFace[i][0];
        Vector3 disp = faceVertices[cIndex] - canVertices[cIndex];
        disp = fme.morphSourceMesh.transform.InverseTransformVector (canonicalObject.transform.TransformVector (disp));
        morphTargetVertices[i] = fme.morphTransformObjectToOrigin.TransformPoint (morphSourceVertices[i] + disp);
      }
      
      // FIXME: This doesn't seem to work right unless the meshfilter is assigned a
      // copy of the correct mesh. Colours or uvs are not being copied correctly.
      Mesh mesh = fme.morphTargetObject.GetComponent <MeshFilter> ().mesh;
      //           mesh.Clear ();
      mesh.triangles = null;
      mesh.vertices = morphTargetVertices;
      mesh.triangles = sourceMesh.triangles;
      mesh.RecalculateBounds ();          
      mesh.RecalculateNormals ();
      mesh.uv = sourceMesh.uv;
      
      fme.morphTargetObject.transform.position = face.transform.position;
      fme.morphTargetObject.transform.rotation = face.transform.rotation;
      fme.morphTargetObject.transform.localScale = new Vector3 (1, 1, 1);
    }
    
  }
}
