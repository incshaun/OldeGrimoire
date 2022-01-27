using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Single;

// Derived from: https://github.com/ClayFlannigan/icp
// For: Iterative Closest Point
public class ICP
{
  public Matrix4x4 BestFit (Vector3 [] A, Vector3 [] B)
  {
    int n = A.Length;
    int m = 3;
    Matrix mA = (Matrix) Matrix.Build.Dense (n, m);
    Matrix mB = (Matrix) Matrix.Build.Dense (n, m);
    for (int i = 0; i < n; i++)
    {
        mA[i, 0] = A[i].x; 
        mA[i, 1] = A[i].y; 
        mA[i, 2] = A[i].z; 
        mB[i, 0] = B[i].x; 
        mB[i, 1] = B[i].y; 
        mB[i, 2] = B[i].z; 
    }
    
    Matrix mT = best_fit_transform (mA, mB);
    Matrix4x4 T = new Matrix4x4 ();
    for (int i = 0; i < 4; i++)
    {
        for (int j = 0; j < 4; j++)
        {
          T[i, j] = mT[i, j];
        }
    }
    T[m, m] = 1.0f;
    return T;
  }
  
  private Vector findCentroid (Matrix A)
  {
      return (Vector) (A.ColumnSums () / A.RowCount);
  }
  
  private Matrix offset (Matrix A, Vector os)
  {
    Matrix AA = (Matrix) A.Clone ();
    Vector rowbfr = (Vector) Vector.Build.Dense (AA.ColumnCount);
    for (int i = 0; i < AA.RowCount; i++)
    {
      AA.Row (i, rowbfr);
      rowbfr.Subtract (os, rowbfr);
      AA.SetRow (i, rowbfr);
    }
    return AA;
  }
  
  private Matrix best_fit_transform (Matrix A, Matrix B)
  {
//     Calculates the least-squares best-fit transform that maps corresponding points A to B in 3 spatial dimensions
//     Input:
//       A: Nxm numpy array of corresponding points
//       B: Nxm numpy array of corresponding points
//     Returns:
//       T: (m+1)x(m+1) homogeneous transformation matrix that maps A on to B
//       R: mxm rotation matrix
//       t: mx1 translation vector

    Debug.Assert ((A.RowCount == B.RowCount) && (A.ColumnCount == B.ColumnCount));

    int m = A.ColumnCount;
    Debug.Log ("M " + m + " " + A);
    
    // translate points to their centroids
    Vector centroid_A = findCentroid (A);
    Vector centroid_B = findCentroid (B);
    Matrix AA = offset (A, centroid_A);
    Matrix BB = offset (B, centroid_B);

    Debug.Log ("Off " + A + " " + AA + " " + centroid_A);
    Debug.Log ("OffB " + B + " " + BB + " " + centroid_B);
    // rotation matrix
    DenseMatrix H = (DenseMatrix) AA.Transpose ().Multiply (BB);
    Debug.Log ("Den H " + H);
    MathNet.Numerics.LinearAlgebra.Factorization.Svd<float> svdResult = H.Svd ();
    Matrix U = (Matrix) svdResult.U;
    Vector S = (Vector) svdResult.S;
    Matrix Vt = (Matrix) svdResult.VT;
    Matrix R = (Matrix) Vt.Transpose ().Multiply (U.Transpose ());
    Debug.Log ("SVD " + U + " " + S + " " + Vt + " " + R);
    
    // special reflection case
    if (R.Determinant () < 0)
    {
      Debug.Log ("Before " + Vt + " " + R);
       Vector row = (Vector) Vector.Build.Dense (Vt.ColumnCount);
       Vt.Row (m - 2, row);
       row.Multiply (-1, row);
       Vt.SetRow (m - 2, row);
       R = (Matrix) Vt.Transpose ().Multiply (U.Transpose ());
      Debug.Log ("After " + Vt + " " + R);
    }

    // translation
//     Vector t = centroid_B.Transpose () - R.Multiply (centroid_A.Transpose ());
    Vector t = (Vector) (centroid_B - R.Multiply (centroid_A));
    Debug.Log ("T " + t);
    
    // homogeneous transformation
    Matrix T = new DenseMatrix (m + 1, m + 1);
    T.SetSubMatrix (0, m, 0, m, R);
    T.SetSubMatrix (0, m, m, 1, Matrix.Build.Dense (m, 1).InsertColumn (0, t));
//    T[:m, :m] = R
//    T[:m, m] = t
    Debug.Log ("Re " + R + " " + t);

//     return T, R, t
    return T;

//    return null;
  }
/*
  void nearest_neighbor(Vector3 [] src, Vector3 [] dst)
  {
//     Find the nearest (Euclidean) neighbor in dst for each point in src
//     Input:
//         src: N array of points
//         dst: N array of points
//     Output:
//         distances: Euclidean distances of the nearest neighbor
//         indices: dst indices of the nearest neighbor

    Debug.Assert (src.shape == dst.Count);

    neigh = NearestNeighbors(n_neighbors=1);
    neigh.fit(dst);
    distances, indices = neigh.kneighbors(src, return_distance=True);
    return distances.ravel(), indices.ravel();
  }

  void icp(A, B, init_pose=None, max_iterations=20, tolerance=0.001)
  {
//     The Iterative Closest Point method: finds best-fit transform that maps points A on to points B
//     Input:
//         A: Nxm numpy array of source mD points
//         B: Nxm numpy array of destination mD point
//         init_pose: (m+1)x(m+1) homogeneous transformation
//         max_iterations: exit algorithm after max_iterations
//         tolerance: convergence criteria
//     Output:
//         T: final homogeneous transformation that maps A on to B
//         distances: Euclidean distances (errors) of the nearest neighbor
//         i: number of iterations to converge

    assert A.shape == B.shape

    // get number of dimensions
    m = A.shape[1]

    // make points homogeneous, copy them to maintain the originals
    src = np.ones((m+1,A.shape[0]))
    dst = np.ones((m+1,B.shape[0]))
    src[:m,:] = np.copy(A.T)
    dst[:m,:] = np.copy(B.T)

    // apply the initial pose estimation
    if init_pose is not None:
        src = np.dot(init_pose, src)

    prev_error = 0

    for i in range(max_iterations):
        // find the nearest neighbors between the current source and destination points
        distances, indices = nearest_neighbor(src[:m,:].T, dst[:m,:].T)

        // compute the transformation between the current source and nearest destination points
        T,_,_ = best_fit_transform(src[:m,:].T, dst[:m,indices].T)

        // update the current source
        src = np.dot(T, src)

        // check error
        mean_error = np.mean(distances)
        if np.abs(prev_error - mean_error) < tolerance:
            break
        prev_error = mean_error

    // calculate final transformation
    T,_,_ = best_fit_transform(A, src[:m,:].T)

    return T, distances, i
  } 
  
  */
}
