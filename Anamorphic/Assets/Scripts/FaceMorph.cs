using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class FaceMorph : MonoBehaviour
{
  public TextMeshProUGUI morphStatusText;
  
  [System.Serializable]
  public class FaceMorphElement
  {
    public GameObject morphSourceMesh;
    public GameObject morphTargetObject;
    public Transform morphTransformObjectToOrigin;
    
    [System.NonSerialized]
    public int [] [] morphCanonicalToFace;
  }
  
  [SerializeField]
  public FaceMorphElement [] morphs;
  
  public FaceProperties faceSource;
  
//   public GameObject faceObject;
  public GameObject canonicalObject;
//   public GameObject targetObject;
  
//   private Mesh faceMesh;
  private Mesh canonicalMesh;
  
//   int [] [] canonicalToFace;
  
//   public Transform objectsToOrigin;
  
//   private static FaceMorph fm;
  
    void Start()
    {
//       faceMesh = faceObject.GetComponent <SkinnedMeshRenderer> ().sharedMesh;
      canonicalMesh = canonicalObject.GetComponent <SkinnedMeshRenderer> ().sharedMesh;
      
      morphStatusText.text = "Size " + canonicalMesh.vertices.Length;
//       fm = this;
      
      createMapping ();
      
      faceSource.faceHandler += drawFace;
//       morphStatusText.text = "TF " + canonicalMesh.triangles[0] + " " +
//                                        canonicalMesh.triangles[1] + " " +
//                                        canonicalMesh.triangles[2] + " " +
//                                        canonicalMesh.triangles[3] + " " +
//                                        canonicalMesh.triangles[4] + " ";
      Debug.Log ("AAA " + morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.vertices.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.uv.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.uv2.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.uv3.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.uv4.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.uv5.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.uv6.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.uv7.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.uv8.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.colors.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.colors32.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.bindposes.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.boneWeights.Length + " " +
        morphs[5].morphSourceMesh.GetComponent <SkinnedMeshRenderer> ().sharedMesh.normals.Length + " " +
      "");
      Debug.Log ("BBB " + morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.vertices.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.uv.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.uv2.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.uv3.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.uv4.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.uv5.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.uv6.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.uv7.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.uv8.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.colors.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.colors32.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.bindposes.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.boneWeights.Length + " " +
        morphs[5].morphTargetObject.GetComponent <MeshFilter> ().mesh.normals.Length + " " +
      "");
    }

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
      
//       Debug.Log ("aa " + best[0] + " " + best[1]);
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
//           mesh.bindposes = null;
//           mesh.boneWeights = null;
//           mesh.uv2 = sourceMesh.uv2;
//           mesh.uv3 = sourceMesh.uv3;
//           mesh.uv4 = sourceMesh.uv4;
//           mesh.uv5 = sourceMesh.uv5;
//           mesh.uv6 = sourceMesh.uv6;
//           mesh.uv7 = sourceMesh.uv7;
//           mesh.colors = sourceMesh.colors;
//           mesh.RecalculateBounds ();
//           mesh.normals = sourceMesh.normals;
//           fme.morphTargetObject.GetComponent <MeshFilter> ().sharedMesh = mesh;
//          fme.morphTargetObject.GetComponent <MeshFilter> ().mesh = mesh;
          
          fme.morphTargetObject.transform.position = face.transform.position;
          fme.morphTargetObject.transform.rotation = face.transform.rotation;
          fme.morphTargetObject.transform.localScale = new Vector3 (1, 1, 1);
//        morphStatusText.text = "Po " + targetObject.transform.position.ToString ("F4");
        }
      
//       morphStatusText.text += "Verts " + canonicalMesh.vertices.Length + " " + faceMesh.vertices.Length;
//       morphStatusText.text = "T " + canonicalMesh.triangles[0] + " " + face.indices[0] + " " +
//                                        canonicalMesh.triangles[1] + " " + face.indices[1] + " " +
//                                        canonicalMesh.triangles[2] + " " + face.indices[2] + " " +
//                                        canonicalMesh.triangles[3] + " " + face.indices[3] + " " +
//                                        canonicalMesh.triangles[4] + " " + face.indices[4] + " ";
//         Vector3 [] oldVertices = faceMesh.vertices;
//         Vector3 [] canVertices = canonicalMesh.vertices;
//         NativeArray<Vector3> faceVertices = face.vertices;
//         Vector3 [] vertices = new Vector3 [oldVertices.Length];
//         for (int i = 0; i < oldVertices.Length; i++)
//         {
//           int cIndex = canonicalToFace[i][0];
//           Vector3 disp = faceVertices[cIndex] - canVertices[cIndex];
// //           Vector3 disp = canonicalMesh.vertices[cIndex] - canonicalMesh.vertices[cIndex];
//           disp = faceObject.transform.InverseTransformVector (canonicalObject.transform.TransformVector (disp));
// //           vertices[i] = Quaternion.AngleAxis (-90.0f, Vector3.right) * Quaternion.AngleAxis (180.0f, Vector3.forward) * (oldVertices[i] + disp + new Vector3 (0.0f, 0.0f, -0.0061f));
//           vertices[i] = objectsToOrigin.TransformPoint (oldVertices[i] + disp);
//         }
// //         target.mesh.SetVertices (vertices);
//         Mesh mesh = targetObject.GetComponent <MeshFilter> ().mesh;
//         mesh.vertices = vertices;
//         mesh.triangles = faceMesh.triangles;
//         mesh.RecalculateBounds ();
//         mesh.SetNormals (faceMesh.normals);
//         mesh.RecalculateNormals ();
//         
//         targetObject.transform.position = face.transform.position;
//         targetObject.transform.rotation = face.transform.rotation;
//         targetObject.transform.localScale = new Vector3 (1, 1, 1);
//       morphStatusText.text = "Po " + targetObject.transform.position.ToString ("F4");
//         
      }
}
