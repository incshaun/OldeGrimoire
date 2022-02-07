using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Single;

// Developed thanks to the following resources:
// ThreePhase-2-source.zip at https://code.google.com/archive/p/structured-light/downloads, referenced from:
// https://www.instructables.com/Structured-Light-3D-Scanning/. Tracked to: Kyle McDonald, http://kylemcdonald.net/
// Fast phase unwrapping: https://github.com/mfkasim91/unwrap_phase/
public class ThreePhase
{
    // Step (how many vertices to skip over) when generating mesh.
    public int renderDetail = 2;
    
    public void findMatrix (Vector3 [] reg, Vector2 [] map, MeshFilter scanObject)
    {
        Debug.Log ("Finding matrix");
        
        //        Vector3 [] xx = { new Vector3 (1, 0, 0), new Vector3 (-1, 0, 0), new Vector3 (0, 0, 1), new Vector3 (0, 1, 0), new Vector3 (0, 1, -1), new Vector3 (1, 1, 0) };
        //        Vector3 [] yy = { new Vector3 (100, 77, 3), new Vector3 (70, 20, 5), new Vector3 (94, 127, 1.8f), new Vector3 (50, 78, 2.3f) };
        Vector3 [] xx = reg;
        
        // Want to find A such that
        // Ayi = xi
        // A 4x4 heterogenous transformation matrix.
        // Ayi = [ A1yi, A2yi, A3yi, A4yi] = [xi1w, xi2w, xi3w, w]
        // or
        // [ A1yi, A2yi, A3yi ] = [xi1 A4yi, xi2 A4yi, xi3 A4yi]
        // provided A4yi != 0
        // [                ] [Aij] = 0
        
        // [                         ] [ a11]
        // [                         ] [ a12]
        // [                         ] [ a13]
        // [                         ] [ a14]
        // [                         ] [ a21]
        // [                         ] [ a22]
        // [                         ] [ a23]
        // [                         ] [ a24]
        // [                         ] [ a31]
        // [                         ] [ a32]
        // [                         ] [ a33]
        // [                         ] [ a34]
        // [                         ] [ a41]
        // [                         ] [ a42]
        // [                         ] [ a43]
        // [                         ] [ a44]
        
        //         DenseMatrix T = new DenseMatrix (4, 4, new float [] { 10, 23, 42, 12, 
        //                                                               23, 23, 43, 12, 
        //                                                               42, 33, 13, 21,
        //                                                               42, 93, 12, 95 });
        Vector [] zz = new Vector[xx.Length];
        for (int i = 0; i < xx.Length; i++)
        {
            Vector xxx = new DenseVector (new float [] { xx[i].x, xx[i].y, xx[i].z, 1 });
            //             zz[i] = (Vector) T.Inverse ().Multiply (xxx);
            
            float v = phase[(int) (map[i].y * phaseHeight), (int) (map[i].x * phaseWidth)];
            zz[i] = new DenseVector (new float [] { map[i].x, map[i].y, v, 1 });
            Debug.Log ("V : " + xxx + " " + zz[i]);
        }
        
        // zz[3][2] = 7;
        
        Matrix M = new DenseMatrix (xx.Length * 4, 16);
        Vector bb = new DenseVector (xx.Length * 4);
        for (int i = 0; i < xx.Length; i++)
        {
            M[i * 4 + 0, 0] = zz[i][0];
            M[i * 4 + 0, 1] = zz[i][1];
            M[i * 4 + 0, 2] = zz[i][2];
            M[i * 4 + 0, 3] = zz[i][3];
            
            M[i * 4 + 1, 4] = zz[i][0];
            M[i * 4 + 1, 5] = zz[i][1];
            M[i * 4 + 1, 6] = zz[i][2];
            M[i * 4 + 1, 7] = zz[i][3];
            
            M[i * 4 + 2, 8] = zz[i][0];
            M[i * 4 + 2, 9] = zz[i][1];
            M[i * 4 + 2, 10] = zz[i][2];
            M[i * 4 + 2, 11] = zz[i][3];
            
            M[i * 4 + 3, 12] = zz[i][0];
            M[i * 4 + 3, 13] = zz[i][1];
            M[i * 4 + 3, 14] = zz[i][2];
            M[i * 4 + 3, 15] = zz[i][3];
            
            bb[i * 4 + 0] = xx[i].x;
            bb[i * 4 + 1] = xx[i].y;
            bb[i * 4 + 2] = xx[i].z;
            bb[i * 4 + 3] = 1.0f;
            
            //             M[i * 4 + 0, 0] = xx[i][0];
            //             M[i * 4 + 0, 1] = xx[i][1];
            //             M[i * 4 + 0, 2] = xx[i][2];
            //             M[i * 4 + 0, 3] = 1.0f;
            //             
            //             M[i * 4 + 1, 4] = xx[i][0];
            //             M[i * 4 + 1, 5] = xx[i][1];
            //             M[i * 4 + 1, 6] = xx[i][2];
            //             M[i * 4 + 1, 7] = 1.0f;
            // 
            //             M[i * 4 + 2, 8] = xx[i][0];
            //             M[i * 4 + 2, 9] = xx[i][1];
            //             M[i * 4 + 2, 10] = xx[i][2];
            //             M[i * 4 + 2, 11] = 1.0f;
            // 
            //             M[i * 4 + 3, 12] = xx[i][0];
            //             M[i * 4 + 3, 13] = xx[i][1];
            //             M[i * 4 + 3, 14] = xx[i][2];
            //             M[i * 4 + 3, 15] = 1.0f;
            // 
            //             bb[i * 4 + 0] = zz[i][0];
            //             bb[i * 4 + 1] = zz[i][1];
            //             bb[i * 4 + 2] = zz[i][2];
            //             bb[i * 4 + 3] = zz[i][3];
        }
        // https://christoph.ruegg.name/blog/linear-regression-mathnet-numerics.html
        Vector pp = (Vector) M.QR().Solve(bb);
        Debug.Log ("Results: " + pp + "M = " + M + "b = " + bb);
        
        Matrix A = (Matrix) new DenseMatrix (4, 4, new float [] { pp[0], pp[1], pp[2], pp[3],
            pp[4], pp[5], pp[6], pp[7],
            pp[8], pp[9], pp[10], pp[11],
            pp[12], pp[13], pp[14], pp[15]}).Transpose ();
            for (int i = 0; i < xx.Length; i++)
            {
                Vector mapv = zz[i];
                Vector o = (Vector) A.Multiply (mapv);
                
                Debug.Log ("Outs : " + zz[i] + " == " + o + " " + xx[i]);
            }
            
            makeMesh (renderDetail, scanObject, A);
            //  // data points
            // var xdata = new float[] { 10, 20, 30 };
            // var ydata = new float[] { 15, 20, 25 };
            // 
            // // build matrices
            // var X = DenseMatrix.OfColumns(new[] {DenseVector.Create (xdata.Length, i => 1), new DenseVector(xdata)});
            // var y = new DenseVector(ydata);
            // 
            // // solve
            // var p = X.QR().Solve(y);
            // var a = p[0];
            // var b = p[1];
            //         Debug.Log ("Results: " + p + " " + a + " " + b);
    }
    
    // Internal phase arrays.
    private int phaseWidth;
    private int phaseHeight;
    private float [,] phase;
    private float [,] unwrappedphase;
    
    // Colour from original images, to be used for texture.
    private Color[,] colours;

    // Convert the 3 images provided into an unwrapped phase image.      
    public void unwrapPhase (RenderTexture p1, RenderTexture p2, RenderTexture p3) 
    {
        Texture2D phase1Image, phase2Image, phase3Image;

        phase1Image = accessTexture (p1);
        phase2Image = accessTexture (p2);
        phase3Image = accessTexture (p3);
        
        phaseWidth = phase1Image.width;
        phaseHeight = phase1Image.height;
        phase = new float[phaseHeight, phaseWidth];
        unwrappedphase = new float[phaseHeight, phaseWidth];
        colours = new Color[phaseHeight, phaseWidth];

        initializePhase (phase1Image, phase2Image, phase3Image);
        exportPhase (phase, "Phase");
        unwrap_phase ();
        exportPhase (unwrappedphase, "UnwrappedPhase");
        phase = unwrappedphase;
    }
    
    // A single texture, to which the colour data is extracted.
    private Texture2D meshTex;
    
    private void makeMesh (int step, MeshFilter scanObject, Matrix A)
    {
        if (meshTex == null)
        {
            meshTex = new Texture2D(2, 2);
        }
        
        Vector3[] vertices = new Vector3[(phaseWidth / step) * (phaseHeight / step)];
        Vector2[] uvs = new Vector2[(phaseWidth / step) * (phaseHeight / step)];
        int[] triangles = new int[6 * ((phaseWidth / step) - 1) * ((phaseHeight / step) - 1)];
        
        meshTex.Resize (phaseWidth / step, phaseHeight / step);
        
        int triangleIndex = 0;
        for (int y = 0; y < phaseHeight; y += step)
        {
            for (int x = 0; x < phaseWidth; x += step)
            {
                float xc = (float)x / phaseWidth;
                float zc = (float)y / phaseHeight;
                float yc = 0.0f;

                float v = phase[y, x];
                Vector inv = new DenseVector (new float [] { xc, zc, v, 1 });
                Vector o = (Vector) A.Multiply (inv);
                
                meshTex.SetPixel (x / step, y / step, colours[y, x]);
                xc = o[0] + 0.5f;
                yc = o[1];
                zc = o[2] + 0.5f;
                
                vertices[(y / step) * (phaseWidth / step) + (x / step)] = new Vector3(xc - 0.5f, yc, zc - 0.5f);
                uvs[(y / step) * (phaseWidth / step) + (x / step)] = new Vector2( (float)x / phaseWidth, (float)y / phaseHeight);
                
                // Skip the last row/col
                if ((x < phaseWidth - step) && (y < phaseHeight - step))
                {
                    int topLeft = (y / step) * (phaseWidth / step) + (x / step);
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + (phaseWidth / step);
                    int bottomRight = bottomLeft + 1;
                    
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomLeft;

                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomRight;
                }
            }
        }
        
        meshTex.Apply ();
        
        Mesh m = new Mesh();
        m.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        m.vertices = vertices;
        m.uv = uvs;
        m.triangles = triangles;
        m.RecalculateNormals();
        scanObject.mesh = m;
        
        scanObject.gameObject.GetComponent <MeshRenderer> ().material.mainTexture = meshTex;
    }
    
    // Convert a render texture, to a texture with easier pixel access.
    Texture2D accessTexture (RenderTexture originalTexture)
    {
        Texture2D tex = new Texture2D(originalTexture.width, originalTexture.height);
        RenderTexture.active = originalTexture;
        tex.ReadPixels (new Rect (0, 0, originalTexture.width, originalTexture.height), 0, 0);
        tex.Apply();
        return tex;
    }
        
    void initializePhase (Texture2D phase1Image, Texture2D phase2Image, Texture2D phase3Image) 
    {
        float sqrt3 = Mathf.Sqrt (3.0f);
        for (int y = 0; y < phaseHeight; y++) {
            for (int x = 0; x < phaseWidth; x++) {     
                
                Color color1 = phase1Image.GetPixel (x, y);
                Color color2 = phase2Image.GetPixel (x, y);
                Color color3 = phase3Image.GetPixel (x, y);
                
                float phase1 = averageBrightness(color1);
                float phase2 = averageBrightness(color2);
                float phase3 = averageBrightness(color3);
                
                // Fundamental expression of phase shift - recover phase from image intensities.
                phase[y, x] = Mathf.Atan2(sqrt3 * (phase1 - phase3), 2 * phase2 - phase1 - phase3);// + Mathf.PI;
                
                colours[y, x] = blendColorLightest(blendColorLightest(color1, color2), color3);
            }
        }
    }
    
    // A support function, to show the phase maps.
    void exportPhase (float [,] phase, string fn)
    {
        Texture2D tex = new Texture2D(phaseWidth, phaseHeight);
        for (int y = 0; y < phaseHeight; y++) {
            for (int x = 0; x < phaseWidth; x++) {     
                float p = (phase[y, x] / (2.0f * Mathf.PI)) + 0.5f;
                tex.SetPixel (x, y, new Color (p, p, p));
            }
        }
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        var dirPath = Application.dataPath;
        File.WriteAllBytes(dirPath + "/" + fn + ".png", bytes);
    }
    
    // https://github.com/processing/processing/blob/a6e0e227a948e7e2dc042c04504d6f5b8cf0c1a6/core/src/processing/core/PImage.java
    Color blendColorLightest (Color dst, Color src)
    {
        float a = src.a;
        
        float s_a = a;
        float d_a = 1.0f - s_a;
        
        return new Color (dst.r * d_a + Mathf.Max (src.r, dst.r) * s_a,
                          dst.g * d_a + Mathf.Max (src.g, dst.g) * s_a,
                          dst.b * d_a + Mathf.Max (src.b, dst.b) * s_a, 
                          Mathf.Min(dst.a + a, 1.0f));
    }
    
    float averageBrightness(Color c) {
        return (c.r + c.g + c.b) / (3.0f);
    }
    
    float gamma (float x)
    {
        return Mathf.Sign(x) * (Mathf.Abs(x) % Mathf.PI);
    }
    
    float [,] get_reliability (float [,] img)
    {
        float [,] rel = new float [phaseHeight, phaseWidth];
        
        for(int y = 0; y < phaseHeight; y ++) {
            for(int x = 0; x < phaseWidth; x ++) {
                float img_im1_jm1 = ((x > 0) && (y > 0)) ? img[y - 1, x - 1] : 0.0f;
                float img_i_jm1   = (y > 0) ? img[y - 1, x] : 0.0f;
                float img_ip1_jm1 = ((x < phaseWidth - 1) && (y > 0)) ? img[y - 1, x + 1] : 0.0f;
                float img_im1_j   = (x > 0) ? img[y, x - 1] : 0.0f;
                float img_i_j     = img[y, x];
                float img_ip1_j   = (x < phaseWidth - 1) ? img[y, x + 1] : 0.0f;
                float img_im1_jp1 = ((x > 0) && (y < phaseHeight - 1)) ? img[y + 1, x - 1] : 0.0f;
                float img_i_jp1   = (y < phaseHeight - 1) ? img[y + 1, x] : 0.0f;
                float img_ip1_jp1 = ((x < phaseWidth - 1) && (y < phaseHeight - 1)) ? img[y + 1, x + 1] : 0.0f;
                float H  = gamma (img_im1_j  - img_i_j) - gamma(img_i_j - img_ip1_j  );
                float V  = gamma(img_i_jm1   - img_i_j) - gamma(img_i_j - img_i_jp1  );
                float D1 = gamma(img_im1_jm1 - img_i_j) - gamma(img_i_j - img_ip1_jp1);
                float D2 = gamma(img_im1_jp1 - img_i_j) - gamma(img_i_j - img_ip1_jm1);
                
                float D = Mathf.Sqrt (H * H + V * V + D1 * D1 + D2 * D2);
                
                if (D != 0.0f)
                {
                    rel[y, x] = 1.0f / D;
                }
                else
                {
                    rel[y, x] = 0.0f;
                }                
            }
        }
        return rel;
    }
    
    void unwrap_phase ()
    {
        int Nx = phaseWidth;
        int Ny = phaseHeight;
                
        float [,] reliability = get_reliability(phase);
        
        float [,] h_edges = new float [phaseHeight, phaseWidth];
        float [,] v_edges = new float [phaseHeight, phaseWidth];
        for(int y = 0; y < phaseHeight; y ++) {
            for(int x = 0; x < phaseWidth; x ++) {
                unwrappedphase[y, x] = phase[y, x];
                
                h_edges[y, x] = (x < phaseWidth - 1) ? reliability[y, x] + reliability[y, x + 1] : float.NaN;
                v_edges[y, x] = (y < phaseHeight - 1) ? reliability[y, x] + reliability[y + 1, x] : float.NaN;
            }
        }
        
        List <(float, int)> edges = new List <(float, int)> ();
        for(int x = 0; x < phaseWidth; x ++) {
            for(int y = 0; y < phaseHeight; y ++) {
                edges.Add ((h_edges[y, x], edges.Count));
            }
        }
        for(int x = 0; x < phaseWidth; x ++) {
            for(int y = 0; y < phaseHeight; y ++) {
                edges.Add ((v_edges[y, x], edges.Count));
            }
        }
        
        int edge_bound_idx = Ny * Nx;
        edges.Sort ((a, b) => b.Item1.CompareTo (a.Item1));
                
        int [] idxs1 = new int [edges.Count];
        int [] idxs2 = new int [edges.Count];
        for (int i = 0; i < edges.Count; i++)
        {
            idxs1[i] = edges[i].Item2 % edge_bound_idx;
            idxs2[i] = idxs1[i] + 1 + (edges[i].Item2 < edge_bound_idx ? Ny - 1 : 0);
        }
        
        int [] group = new int [Ny * Nx];
        bool [] is_grouped = new bool [Ny * Nx];
        List <int> [] group_members = new List <int> [Ny * Nx];
        int [] num_members_group = new int [Ny * Nx];
        for (int i = 0; i < Ny * Nx; i++)
        {
            group[i] = i;
            is_grouped[i] = false;
            group_members[i] = new List <int> ();
            group_members[i].Add (i);
            num_members_group[i] = 1;
        }
        
        for (int i = 0; i < edges.Count; i++)
        {
            int idx1 = idxs1[i];
            int idx2 = idxs2[i];
            
            if ((idx1 != -1) && (idx2 != -1) && (idx2 < edge_bound_idx) && (!float.IsNaN (edges[idx1].Item1)) && (!float.IsNaN (edges[idx2].Item1)))
            {
                //               Debug.Log (idx1 + " " + idx2);
                if (!(group[idx1] == group[idx2]))
                {
                    bool all_grouped = false;
                    if (is_grouped[idx1])
                    {
                        if (!is_grouped[idx2])
                        {
                            int idxt = idx1;
                            idx1 = idx2;
                            idx2 = idxt;
                        }
                        else if (num_members_group[group[idx1]] > num_members_group[group[idx2]])
                        {
                            int idxt = idx1;
                            idx1 = idx2;
                            idx2 = idxt;
                            all_grouped = true;
                        }
                        else
                        {
                            all_grouped = true;
                        }
                    }
                    
                    // At this point, either all grouped, or idx1 is the not grouped.
                    float dval = Mathf.Floor((unwrappedphase[idx2 % phaseHeight, idx2 / phaseHeight] - unwrappedphase[idx1 % phaseHeight, idx1 / phaseHeight] + Mathf.PI) / (2.0f * Mathf.PI)) * (2.0f * Mathf.PI);
                    
                    int g1 = group[idx1];
                    int g2 = group[idx2];
                    List <int> pix_idxs;
                    if (all_grouped)
                    {
                        pix_idxs = group_members[g1];
                    }
                    else
                    {
                        pix_idxs = new List <int> ();
                        pix_idxs.Add (idx1);
                    }
                    
                    if (dval != 0.0f)
                    {
                        for (int j = 0; j < pix_idxs.Count; j++)
                        {
                            unwrappedphase[pix_idxs[j] % phaseHeight, pix_idxs[j] / phaseHeight] += dval;
                        }
                    }
                    
                    group_members[g2].AddRange (pix_idxs);
                    for (int j = 0; j < pix_idxs.Count; j++)
                    {
                        group[pix_idxs[j]] = g2;
                    }
                    num_members_group[g2] += pix_idxs.Count;
                    is_grouped[idx1] = true;
                    is_grouped[idx2] = true;
                }
            }
        }
        
        for(int y = 0; y < phaseHeight; y ++) {
            for(int x = 0; x < phaseWidth; x ++) {
                // Arbitrary scaling, to make the exported version visible - remap to range 0 to 1 just
                // for the test data used.
                unwrappedphase[y, x] = 0.2f * unwrappedphase[y, x] + 2.0f;
            }
        }
        
    }
}
