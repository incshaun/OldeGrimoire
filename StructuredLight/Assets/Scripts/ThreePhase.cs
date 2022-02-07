using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Single;

// Based on ThreePhase-2-source.zip at https://code.google.com/archive/p/structured-light/downloads, referenced from:
// https://www.instructables.com/Structured-Light-3D-Scanning/. Tracked to: Kyle McDonald, http://kylemcdonald.net/

// Also: https://github.com/danielorf/Transform3DBestFit
public class ThreePhase
{
    
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
            
            float v = phase[(int) (map[i].y * inputHeight), (int) (map[i].x * inputWidth)];
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
    
    void Start()
    {
//        setup ();
    }
    
    void Update()
    {
//        draw ();
    }
    
//    import peasy.*;
    
//    PeasyCam cam;
    
    int inputWidth, inputHeight;
    float[,] phase, distance, depth;
    float[,] unwrappedphase;
    bool[, ] mask, process;
    Color[,] colors;
    int[, ] names;
    
    bool update, exportMesh, exportCloud;
    
//     void setup() {
// //        size(480, 640, P3D);
//         
//         loadImages();
//         inputWidth = phase1Image.width;
//         inputHeight = phase1Image.height;
//         phase = new float[inputHeight, inputWidth];
//         distance = new float[inputHeight, inputWidth];
//         depth = new float[inputHeight, inputWidth];
//         mask = new bool[inputHeight, inputWidth];
//         process = new bool[inputHeight, inputWidth];
//         colors = new Color[inputHeight, inputWidth];
//         names = new int[inputHeight, inputWidth];
//         
// //        cam = new PeasyCam(this, width);  
//         setupControls();
//         
//         update = true;
//     }
    
    public void generate (MeshFilter scanObject)
    {
        inputWidth = phase1Image.width;
        inputHeight = phase1Image.height;
//         inputWidth = 8;
//         inputHeight = 8;
        phase = new float[inputHeight, inputWidth];
        unwrappedphase = new float[inputHeight, inputWidth];
        distance = new float[inputHeight, inputWidth];
        depth = new float[inputHeight, inputWidth];
        mask = new bool[inputHeight, inputWidth];
        process = new bool[inputHeight, inputWidth];
        colors = new Color[inputHeight, inputWidth];
        names = new int[inputHeight, inputWidth];
            phaseWrap();
            exportPhase (phase, "Phase");
//             phaseUnwrap();
            unwrap_phase ();
            exportPhase (unwrappedphase, "UnwrappedPhase");
            phase=unwrappedphase;
            makeDepth();
//         makeMesh (renderDetail, scanObject);
        
//         fitplane ();
    }
    
    static private Vector findCentroid (Matrix A)
    {
      return (Vector) (A.RowSums () / A.ColumnCount);
    }

    // https://www.ltu.se/cms_fs/1.51590!/svd-fitting.pdf
    private void fitplane ()
    {
        int n = 0;
        for (int y = 0; y < inputHeight; y += renderDetail) {
            for (int x = 0; x < inputWidth; x += renderDetail) {
                if (!mask[y, x]) {
                    n++;
                }
            }
        }
        Matrix pi = (Matrix) Matrix.Build.Dense (3, n);

        int i = 0;
        for (int y = 0; y < inputHeight; y += renderDetail) {
            for (int x = 0; x < inputWidth; x += renderDetail) {
                if (!mask[y, x]) {
                    pi[0, i] = x;
                    pi[1, i] = y;
                    pi[2, i] = phase[y, x];
                    i++;
                }
            }
        }
        
        Vector c = findCentroid (pi);
        Debug.Log ("Centroid " + c);
        Matrix A = (Matrix) Matrix.Build.Dense (3, n);
        for (i = 0; i < n; i++)
        {
            A[0, i] = pi[0, i] - c[0];
            A[1, i] = pi[1, i] - c[1];
            A[2, i] = pi[2, i] - c[2];
        }

        MathNet.Numerics.LinearAlgebra.Factorization.Svd<float> svdResult = A.Svd ();
        Matrix U = (Matrix) svdResult.U;
        Debug.Log ("Norm " + U.Column (2) + " " + svdResult.S);
    }
    
    void draw () {
//         background(0);
//         translate(-width / 2, -height / 2);
//         
        if(update) {
            phaseWrap();
            phaseUnwrap();
            update = false;
        }
        
        makeDepth();
//         
//         noFill();
        Debug.Log ("Draw");
//        makeMesh (renderDetail);
        for (int y = 0; y < inputHeight; y += renderDetail)
            for (int x = 0; x < inputWidth; x += renderDetail)
                if (!mask[y, x]) {
//                     stroke(colors[y, x], 255);
//                     point(x, y, depth[y, x]);
                    Debug.Log ("Depth: " + x + " " + y + " " + depth[y, x]);
                }
                
                if(takeScreenshot) {
//                     saveFrame(getTimestamp() + ".png");
                    takeScreenshot = false;
                }
                if(exportMesh) {
                    doExportMesh();
                    exportMesh = false;
                }
                if(exportCloud) {
                    doExportCloud();
                    exportCloud = false;
                }
    }
    
    private Texture2D meshTex;
    
    private void makeMesh (int step, MeshFilter scanObject, Matrix A)
    {
        if (meshTex == null)
        {
          meshTex = new Texture2D(2, 2);
        }
        
        Vector3[] vertices = new Vector3[(inputWidth / step) * (inputHeight / step)];
        Vector2[] uvs = new Vector2[(inputWidth / step) * (inputHeight / step)];
        int[] triangles = new int[6 * ((inputWidth / step) - 1) * ((inputHeight / step) - 1)];

        meshTex.Resize (inputWidth / step, inputHeight / step);
        
        int triangleIndex = 0;
        for (int y = 0; y < inputHeight; y += step)
        {
            for (int x = 0; x < inputWidth; x += step)
            {
                float xc = (float)x / inputWidth;
                float zc = (float)y / inputHeight;
                float yc = 0.0f;
                if (!mask[y, x]) {
//                  yc = zscale * depth[y, x];
                  
                  // points on the plane satisfy (p-c).n = 0, 
                  // p.n - c.n = 0
                  // pxnx + pyny + pznz - c.n = 0
                  // pz = -(pxnx  pyny - c.n)/nz
                  
//                   float d = 0.0f * 127.153f + 0.0f * 44.9479f + 1.0f * 0.00157377f;
// 
//                   float planez = -(-0.0f * x + 0.0f * y - d) / 1.0f;
// 
//                   yc = 1.0f * (phase[y, x] - planez); 
                  //yc = planez;
                  //yc = phase[y, x];

//                   Debug.Log ("Diff " + phase[y, x] + " " + planez + " " + x + " " + y);
            float v = phase[y, x];
            Vector inv = new DenseVector (new float [] { xc, zc, v, 1 });
            Vector o = (Vector) A.Multiply (inv);
                  
                  meshTex.SetPixel (x / step, y / step, colors[y, x]);
                  xc = o[0] + 0.5f;
                  yc = o[1];
                  zc = o[2] + 0.5f;
                }
//                 vertices[(y / step) * (inputWidth / step) + (x / step)] = new Vector3(xc - 0.5f, yc, zc - 0.5f);
                vertices[(y / step) * (inputWidth / step) + (x / step)] = new Vector3(xc - 0.5f, yc, zc - 0.5f);
                uvs[(y / step) * (inputWidth / step) + (x / step)] = new Vector2( (float)x / inputWidth, (float)y / inputHeight);

                // Skip the last row/col
                if ((x < inputWidth - step) && (y < inputHeight - step))
                {
                    int topLeft = (y / step) * (inputWidth / step) + (x / step);
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + (inputWidth / step);
                    int bottomRight = bottomLeft + 1;

                    if (!mask[y, x] && !mask[y, x + step] && !mask[y + step, x])
                    {
                      triangles[triangleIndex++] = topRight;
                      triangles[triangleIndex++] = topLeft;
                      triangles[triangleIndex++] = bottomLeft;
                    }
                    if (!mask[y + step, x] && !mask[y, x + step] && !mask[y + step, x + step])
                    {
                      triangles[triangleIndex++] = topRight;
                      triangles[triangleIndex++] = bottomLeft;
                      triangles[triangleIndex++] = bottomRight;
                    }
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
    
    /*
     *  Use the wrapped phase information,  and propagate it across the boundaries.
     *  This implementation uses a priority-based propagation algorithm.
     *  
     *  Because the algorithm starts in the center and propagates outwards,
     *  so if you have noise (e.g.: a black background, a shadow) in
     *  the center, then it may not reconstruct your image.
     */
    
    List <WrappedPixel> toProcess;
    long position;
    
    void phaseUnwrap() {
        int startX = inputWidth / 2;
        int startY = inputHeight / 2;
        
        toProcess = new List <WrappedPixel> ();
        toProcess.Add(new WrappedPixel(startX, startY, 0, phase[startY, startX]));
        
        while(toProcess.Count > 0) {
            WrappedPixel cur = (WrappedPixel) toProcess[0];
            toProcess.RemoveAt (0);
            Debug.Log ("Proce " + toProcess.Count);
            int x = cur.x;
            int y = cur.y;
            if(process[y, x]) {
                phase[y, x] = cur.phase;
                process[y, x] = false;
                float d = cur.distance;
                float r = phase[y, x];
                if (y > 0)
                    phaseUnwrap(x, y-1, d, r);
                if (y < inputHeight-1)
                    phaseUnwrap(x, y+1, d, r);
                if (x > 0)
                    phaseUnwrap(x-1, y, d, r);
                if (x < inputWidth-1)
                    phaseUnwrap(x+1, y, d, r);
            }
        }
    }
    
    void phaseUnwrap(int x, int y, float d, float r) {
        if(process[y, x]) {
            float diff = phase[y, x] - (r - (int) r);
            if (diff > .5)
                diff--;
            if (diff < -.5)
                diff++;
            toProcess.Add(new WrappedPixel(x, y, d + distance[y, x], r + diff));
            toProcess.Sort (delegate(WrappedPixel x, WrappedPixel y)
                {
                  return x.distance.CompareTo(y.distance);
                }
            );
        }
    }
    
    void makeDepth() {
        for (int y = 0; y < inputHeight; y += renderDetail) {
            float planephase = 0.5f - (y - (inputHeight / 2)) / zskew;
            for (int x = 0; x < inputWidth; x += renderDetail)
                if (!mask[y, x])
                    depth[y, x] = (phase[y, x] - planephase) * zscale;
        }
    }
    
    class WrappedPixel { //implements Comparable {
        public int x, y;
        public float distance, phase;
        public WrappedPixel(int x, int y, float distance, float phase) {
            this.x = x;
            this.y = y;
            this.distance = distance;
            this.phase = phase;
        }
//         int compareTo(System.Object o) {
//             if(o is WrappedPixel) {
//                 WrappedPixel w = (WrappedPixel) o;
//                 if(w.distance == distance)
//                     return 0;
//                 if(w.distance < distance)
//                     return 1;
//                 else
//                     return -1;
//             } else
//                 return 0;
//         }
    }
    
    
    /*
     *  Go through all the pixels in the three phase images,
     *  and determine their wrapped phase. Ignore noisy pixels.
     */

    public Texture2D phase1ImageOrg, phase2ImageOrg, phase3ImageOrg;
    private Texture2D phase1Image, phase2Image, phase3Image;
    
/*    PImage fitToScreen(PImage img) {
        if(img.width > width)
            img.resize(width, (img.height * width) / img.width);
        else if(img.height > height)
            img.resize((img.width * height) / img.height, height);
        return img;
    }*/
    
    // https://stackoverflow.com/questions/62161868/copy-texture2d-to-leave-original-unchanged
    Texture2D copyTexture (RenderTexture originalTexture)
    {
        Texture2D copyTexture = new Texture2D(originalTexture.width, originalTexture.height);
        RenderTexture.active = originalTexture;
        //copyTexture.SetPixels(originalTexture.GetPixels());
        copyTexture.ReadPixels (new Rect (0, 0, originalTexture.width, originalTexture.height), 0, 0);
        copyTexture.Apply();
        return copyTexture;
    }
//     Texture2D copyTexture (Texture2D originalTexture)
//     {
//         Texture2D copyTexture = new Texture2D(originalTexture.width, originalTexture.height);
//         copyTexture.SetPixels(originalTexture.GetPixels());
//         copyTexture.Apply();
//         return copyTexture;
//     }

    public void loadImages(RenderTexture p1, RenderTexture p2, RenderTexture p3) {
        phase1Image = copyTexture (p1);
        phase2Image = copyTexture (p2);
        phase3Image = copyTexture (p3);
//         phase1Image = copyTexture (phase1ImageOrg);
//         phase2Image = copyTexture (phase2ImageOrg);
//         phase3Image = copyTexture (phase3ImageOrg);
        
/*        phase1Image = fitToScreen(loadImage("img/phase1.jpg"));
        phase2Image = fitToScreen(loadImage("img/phase2.jpg"));
        phase3Image = fitToScreen(loadImage("img/phase3.jpg"));*/
    }
    
    void phaseWrap() {
        float sqrt3 = Mathf.Sqrt(3);
        for (int y = 0; y < inputHeight; y++) {
            for (int x = 0; x < inputWidth; x++) {     
//                 int i = x + y * inputWidth;  
                
                Color color1 = phase1Image.GetPixel (x, y);
                Color color2 = phase2Image.GetPixel (x, y);
                Color color3 = phase3Image.GetPixel (x, y);
                
//                 Color color1 = phase1Image.pixels[i];
//                 Color color2 = phase2Image.pixels[i];
//                 Color color3 = phase3Image.pixels[i];
//                 
                float phase1 = averageBrightness(color1);
                float phase2 = averageBrightness(color2);
                float phase3 = averageBrightness(color3);
                
                float phaseRange = Mathf.Max(phase1, phase2, phase3) - Mathf.Min(phase1, phase2, phase3);
                
                mask[y, x] = phaseRange <= noiseThreshold;
                process[y, x] = !mask[y, x];
                distance[y, x] = phaseRange;
                
                // this equation can be found in Song Zhang's
                // "Recent progresses on real-time 3D shape measurement..."
                // and it is the "bottleneck" of the algorithm
                // it can be sped up with a look up table, which has the benefit
                // of allowing for simultaneous gamma correction.
                phase[y, x] = Mathf.Atan2(sqrt3 * (phase1 - phase3), 2 * phase2 - phase1 - phase3) / (2.0f * Mathf.PI);
                phase[y, x] = Mathf.Atan2(sqrt3 * (phase1 - phase3), 2 * phase2 - phase1 - phase3) + Mathf.PI;
//                 phase[y, x] += 0.5f;
                
                // build color based on the lightest channels from all three images
//                 colors[y, x] = blendColor(blendColor(color1, color2, LIGHTEST), color3, LIGHTEST);
                colors[y, x] = blendColorLightest(blendColorLightest(color1, color2), color3);
            }
        }
        
        for (int y = 1; y < inputHeight - 1; y++) {
            for (int x = 1; x < inputWidth - 1; x++) {
                if(!mask[y, x]) {
                    distance[y, x] = (
                        diff(phase[y, x], phase[y, x - 1]) +
                        diff(phase[y, x], phase[y, x + 1]) +
                        diff(phase[y, x], phase[y - 1, x]) +
                        diff(phase[y, x], phase[y + 1, x])) / distance[y, x];
                }
            }
        }
    }
    
    void exportPhase (float [,] phase, string fn)
    {
        Texture2D copyTexture = new Texture2D(inputWidth, inputHeight);
        for (int y = 0; y < inputHeight; y++) {
            for (int x = 0; x < inputWidth; x++) {     
                float p = phase[y, x] + 0.5f;
                copyTexture.SetPixel (x, y, new Color (p, p, p));
            }
        }
        copyTexture.Apply();
        byte[] bytes = copyTexture.EncodeToPNG();
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
//        return (c.r + c.g + c.b) / (255 * 3);
        return (c.r + c.g + c.b) / (3);
    }
    
    float diff(float a, float b) {
        float d = a < b ? b - a : a - b;
        return d < .5 ? d : 1 - d;
    }
    
//    import controlP5.*;
    
    /*
     * These three variables are the main "settings".
     * 
     * zscale corresponds to how much "depth" the image has,
     * zskew is how "skewed" the imaging plane is.
     * 
     * These two variables are dependent on both the angle
     * between the projector and camera, and the number of stripes.
     * The sign on both is based on the direction of the stripes
     * (whether they're moving up vs down)
     * as well as the orientation of the camera and projector
     * (which one is above the other).
     * 
     * noiseThreshold can significantly change whether an image
     * can be reconstructed or not. Start with it small, and work
     * up until you start losing important parts of the image.
     */
    
    public float noiseThreshold = 0.001f;
    public float zscale = 0.1f;
    public float zskew = 24; 
    public int renderDetail = 2;
    
//    ControlWindow controlWindow;
//    ControlP5 control;
    
    bool takeScreenshot;
    void screenshot() {
        takeScreenshot = true;
    }
    
    void setupControls() {
//         control = new ControlP5(this);
//         controlWindow = control.addControlWindow("controlWindow", 10, 10, 350, 128);
//         controlWindow.hideCoordinates();
//         controlWindow.setTitle("Decoding Parameters");
//         
//         int y = 20;
//         control.addSlider("noiseThreshold", 0, 1, noiseThreshold, 10, y += 10, 256, 9).setWindow(controlWindow);
//         control.addSlider("zscale", -256, 256, zscale, 10, y += 10, 256, 9).setWindow(controlWindow);
//         control.addSlider("zskew", -64, 64, zskew, 10, y += 10, 256, 9).setWindow(controlWindow);
//         control.addSlider("renderDetail", 1, 4, renderDetail, 10, y += 10, 256, 9).setWindow(controlWindow);
//         control.addBang("screenshot", 10, y += 10, 9, 9).setWindow(controlWindow);
//         control.addBang("exportCloud", 80, y, 9, 9).setWindow(controlWindow);
//         control.addBang("exportMesh", 160, y, 9, 9).setWindow(controlWindow);
    }
    
    void setNoiseThreshold(float newThreshold) {
        if(newThreshold != noiseThreshold) {
            noiseThreshold = newThreshold;
            update = true;
        }
    }
    
    string getTimestamp() {
        return "";
//         return day() + " " + hour() + " " + minute() + " " + second();
    }
    
    // Export uses the PLY format. More information is available at:
    // http://local.wasp.uwa.edu.au/~pbourke/dataformats/ply/
    
    int vertexCount() {
        int total = 0;
        for (int y = 0; y < inputHeight; y += renderDetail)
            for (int x = 0; x < inputWidth; x += renderDetail)
                if(!mask[y, x])
                    names[y, x] = total++;
                return total;
    }
    
//     void writeVertices(PrintWriter file) {
//         for (int y = 0; y < inputHeight; y += renderDetail)
//             for (int x = 0; x < inputWidth; x += renderDetail)
//                 if (!mask[y, x]) {
//                     Color cur = (Color) colors[y, x];
//                     file.println(
//                         x + " " +
//                         (inputHeight - y) + " " +
//                         depth[y, x] + " " +
//                         (int) red(cur) + " " + 
//                         (int) green(cur) + " " + 
//                         (int) blue(cur));
//                 }
//     }
    
    void doExportCloud() {
//         PrintWriter ply = createWriter(getTimestamp() + " cloud.ply");
//         ply.println("ply");
//         ply.println("format ascii 1.0");
//         ply.println("element vertex " + vertexCount());
//         ply.println("property float x");
//         ply.println("property float y");
//         ply.println("property float z");
//         ply.println("property uchar red");
//         ply.println("property uchar green");
//         ply.println("property uchar blue");
//         ply.println("end_header");
//         writeVertices(ply); 
//         ply.flush();
//         ply.close();
    }
    
    int faceCount() {
        int total = 0;
        int r = renderDetail;
        for(int y = 0; y < inputHeight - r; y += r)
            for(int x = 0; x < inputWidth - r; x += r) {
                if(!mask[y, x] && !mask[y + r, x + r]) {
                    if(!mask[y, x + r])
                        total++;
                    if(!mask[y + r, x])
                        total++;
                } 
                else if(!mask[y, x + r] && !mask[y + r, x]) {
                    if(!mask[y, x])
                        total++;
                    if(!mask[y + r, x + r])
                        total++;
                }
            }
            return total;
    }
    
//     void writeFace(PrintWriter file, int a, int b, int c) {
//         file.println("3 " + a + " " + b + " " + c);
//     }
    
//     void writeFaces(PrintWriter file) {
//         int r = renderDetail;
//         int total = 0;
//         for(int y = 0; y < inputHeight - r; y += r)
//             for(int x = 0; x < inputWidth - r; x += r) {
//                 if(!mask[y, x] && !mask[y + r, x + r]) {
//                     if(!mask[y, x + r]) {
//                         writeFace(file, names[y + r, x + r], names[y, x + r], names[y, x]);
//                     }
//                     if(!mask[y + r, x]) {
//                         writeFace(file, names[y + r, x], names[y + r, x + r], names[y, x]);
//                     }
//                 } 
//                 else if(!mask[y, x + r] && !mask[y + r, x]) {
//                     if(!mask[y, x]) {
//                         writeFace(file, names[y + r, x], names[y, x + r], names[y, x]);
//                     }
//                     if(!mask[y + r, x + r]) {
//                         writeFace(file, names[y + r, x], names[y + r, x + r], names[y, x + r]);
//                     }
//                 }
//             }
//     }
    
    void doExportMesh() {
//         PrintWriter ply = createWriter(getTimestamp() + " mesh.ply");
//         ply.println("ply");
//         ply.println("format ascii 1.0");
//         ply.println("element vertex " + vertexCount());
//         ply.println("property float x");
//         ply.println("property float y");
//         ply.println("property float z");
//         ply.println("property uchar red");
//         ply.println("property uchar green");
//         ply.println("property uchar blue");
//         ply.println("element face " + faceCount());
//         ply.println("property list uchar uint vertex_indices");
//         ply.println("end_header");
//         writeVertices(ply); 
//         writeFaces(ply);
//         ply.flush();
//         ply.close();
    }

    float gamma (float x)
    {
         return Mathf.Sign(x) * (Mathf.Abs(x) % Mathf.PI);
//         return x;
//         return Mathf.Sign(x) * (Mathf.Abs(x) % 0.5f);
    }

    float [,] get_reliability (float [,] img)
    {
        float [,] rel = new float [inputHeight, inputWidth];
        
        for(int y = 0; y < inputHeight; y ++) {
            for(int x = 0; x < inputWidth; x ++) {
                float img_im1_jm1 = ((x > 0) && (y > 0)) ? img[y - 1, x - 1] : 0.0f;
                float img_i_jm1   = (y > 0) ? img[y - 1, x] : 0.0f;
                float img_ip1_jm1 = ((x < inputWidth - 1) && (y > 0)) ? img[y - 1, x + 1] : 0.0f;
                float img_im1_j   = (x > 0) ? img[y, x - 1] : 0.0f;
                float img_i_j     = img[y, x];
                float img_ip1_j   = (x < inputWidth - 1) ? img[y, x + 1] : 0.0f;
                float img_im1_jp1 = ((x > 0) && (y < inputHeight - 1)) ? img[y + 1, x - 1] : 0.0f;
                float img_i_jp1   = (y < inputHeight - 1) ? img[y + 1, x] : 0.0f;
                float img_ip1_jp1 = ((x < inputWidth - 1) && (y < inputHeight - 1)) ? img[y + 1, x + 1] : 0.0f;
                float H  = gamma (img_im1_j  - img_i_j) - gamma(img_i_j - img_ip1_j  );
                float V  = gamma(img_i_jm1   - img_i_j) - gamma(img_i_j - img_i_jp1  );
                float D1 = gamma(img_im1_jm1 - img_i_j) - gamma(img_i_j - img_ip1_jp1);
                float D2 = gamma(img_im1_jp1 - img_i_j) - gamma(img_i_j - img_ip1_jm1);
                
                float D = Mathf.Sqrt (H * H + V * V + D1 * D1 + D2 * D2);
                
                if (D != 0.0f)
                {
                  rel[y, x] = 1.0f / D;
//                   rel[y, x] = 0.001f * 1.0f / D;
                  
                }
                else
                {
                    rel[y, x] = 0.0f;
                }
                
                // Consistency with matlab version. May not need.
                if ((x == 0) || (y == 0) || (x == inputWidth - 1) || (y == inputHeight - 1))
                {
                    rel[y,x] = 0.0f;
                }
            }
        }
        return rel;
    }

    void unwrap_phase ()
    {
      int Nx = inputWidth;
      int Ny = inputHeight;
      
//       Debug.Log ("Phase ");
//       phase = new float [,] 
//       { 
//           {2.0944f, 2.1437f, 2.2176f, 2.3162f, 2.4394f, 2.5872f, 2.7597f, 2.9568f},
//           {2.0698f, 2.1437f, 2.2176f, 2.3162f, 2.4147f, 2.5626f, 2.7104f, 2.9075f},
//           {2.0451f, 2.1190f, 2.1930f, 2.2915f, 2.3901f, 2.5379f, 2.6858f, 2.8582f},
//           {2.0205f, 2.1190f, 2.1930f, 2.2669f, 2.3901f, 2.5133f, 2.6611f, 2.8336f},
//           {1.9958f, 2.0944f, 2.1930f, 2.2669f, 2.3654f, 2.4886f, 2.6365f, 2.8090f},
//           {1.9712f, 2.0698f, 2.1683f, 2.2669f, 2.3408f, 2.4640f, 2.6118f, 2.7843f},
//           {1.9466f, 2.0451f, 2.1683f, 2.2422f, 2.3408f, 2.4394f, 2.5872f, 2.7350f},
//           {1.9219f, 2.0205f, 2.1437f, 2.2422f, 2.3162f, 2.4394f, 2.5626f, 2.7104f}
//           
//       };
      
//       string s = "";
//       for(int y = 0; y < inputHeight; y ++) {
//           for(int x = 0; x < inputWidth; x ++) {
//               s += phase[y,x].ToString ("F4") + " ";
//           }
//           s += "\n";
//       }
//       Debug.Log ("Phase " + s);
      
      float [,] reliability = get_reliability(phase);
      
//       s = "";
//       for(int y = 0; y < inputHeight; y ++) {
//           for(int x = 0; x < inputWidth; x ++) {
//               s += reliability[y,x].ToString ("F4") + " ";
//           }
//           s += "\n";
//       }
//       Debug.Log ("Reliability " + s);

      float [,] h_edges = new float [inputHeight, inputWidth];
      float [,] v_edges = new float [inputHeight, inputWidth];
      for(int y = 0; y < inputHeight; y ++) {
          for(int x = 0; x < inputWidth; x ++) {
              unwrappedphase[y, x] = phase[y, x];
              
              h_edges[y, x] = (x < inputWidth - 1) ? reliability[y, x] + reliability[y, x + 1] : float.NaN;
              v_edges[y, x] = (y < inputHeight - 1) ? reliability[y, x] + reliability[y + 1, x] : float.NaN;
          }
      }

      List <(float, int)> edges = new List <(float, int)> ();
          for(int x = 0; x < inputWidth; x ++) {
      for(int y = 0; y < inputHeight; y ++) {
              edges.Add ((h_edges[y, x], edges.Count));
          }
      }
          for(int x = 0; x < inputWidth; x ++) {
      for(int y = 0; y < inputHeight; y ++) {
              edges.Add ((v_edges[y, x], edges.Count));
          }
      }

      int edge_bound_idx = Ny * Nx;
      edges.Sort ((a, b) => b.Item1.CompareTo (a.Item1));

//       s="";
//       for(int y = 0; y < inputHeight; y ++) {
//           for(int x = 0; x < inputWidth; x ++) {
//               s += h_edges[y,x].ToString ("F4") + " ";
//           }
//           s += "\n";
//       }
//       Debug.Log ("H edges: " + s);
//       s="";
//       for(int y = 0; y < inputHeight; y ++) {
//           for(int x = 0; x < inputWidth; x ++) {
//               s += v_edges[y,x].ToString ("F4") + " ";
//           }
//           s += "\n";
//       }
//       Debug.Log ("V edges: " + s);
// 
//       s="";
//       for (int i = 0; i < edges.Count; i++)
//       {
//           s += edges[i].Item1 + " - " + edges[i].Item2 + "      ";
//       }
//       Debug.Log ("Sort edges: " + s);
      
      int [] idxs1 = new int [edges.Count];
      int [] idxs2 = new int [edges.Count];
      for (int i = 0; i < edges.Count; i++)
      {
//           idxs1[i] = (edges[i].Item2 % inputWidth < inputWidth - 1) ? ((edges[i].Item2 + 1 + edge_bound_idx) % edge_bound_idx) : -1;
          idxs1[i] = edges[i].Item2 % edge_bound_idx;
          idxs2[i] = idxs1[i] + 1 + (edges[i].Item2 < edge_bound_idx ? Ny - 1 : 0);
//           Debug.Log ("ID " + i + " " + idxs2[i] + " " + (idxs2[i] - edges[i].Item2) + " " + edges[i].Item2);
//     idxs2 = idxs1 + 1 + (Ny - 1) .* (edge_sort_idx <= edge_bound_idx);
      }
//     idxs1 = mod(edge_sort_idx - 1, edge_bound_idx) + 1;
//     idxs2 = idxs1 + 1 + (Ny - 1) .* (edge_sort_idx <= edge_bound_idx);
      
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
                float dval = Mathf.Floor((unwrappedphase[idx2 % inputHeight, idx2 / inputHeight] - unwrappedphase[idx1 % inputHeight, idx1 / inputHeight] + Mathf.PI/* + 0.5f*/) / (2.0f*Mathf.PI)) * (2.0f*Mathf.PI);
                
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
                    unwrappedphase[pix_idxs[j] % inputHeight, pix_idxs[j] / inputHeight] += dval;
//                     unwrappedphase[pix_idxs[j] % inputHeight, pix_idxs[j] / inputHeight] = -99.0f;
                    Debug.Log ("Setting " + pix_idxs[j] + " " + dval + "  - " + j + " of " + pix_idxs.Count + " " + group[pix_idxs[j]]);
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
                Debug.Log ("Group: " + g2 + " (" + g1 + ") " + num_members_group[g2] + " "+ pix_idxs.Count + " " + idx1 + " " + idx2 + "  -- " + num_members_group[g1]);
            }
          }
      }

//       s="";
//       for(int y = 0; y < inputHeight; y ++) {
//           for(int x = 0; x < inputWidth; x ++) {
//               s += unwrappedphase[y,x].ToString ("F4") + " ";
//           }
//           s += "\n";
//       }
//       Debug.Log ("Unwrapped: " + s);
      
      for(int y = 0; y < inputHeight; y ++) {
          for(int x = 0; x < inputWidth; x ++) {
              unwrappedphase[y, x] = 0.025f * unwrappedphase[y, x] + 0.2f;
//               unwrappedphase[y, x] = 0.001f * reliability[y, x];
          }
      }
            
    }
    
// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
// % Fast unwrapping 2D phase image using the algorithm given in:                 %
// %     M. A. Herráez, D. R. Burton, M. J. Lalor, and M. A. Gdeisat,             %
// %     "Fast two-dimensional phase-unwrapping algorithm based on sorting by     %
// %     reliability following a noncontinuous path", Applied Optics, Vol. 41,    %
// %     Issue 35, pp. 7437-7444 (2002).                                          %
// %                                                                              %
// % If using this code for publication, please kindly cite the following:        %
// % * M. A. Herraez, D. R. Burton, M. J. Lalor, and M. A. Gdeisat, "Fast         %
// %   two-dimensional phase-unwrapping algorithm based on sorting by reliability %
// %   following a noncontinuous path", Applied Optics, Vol. 41, Issue 35,        %
// %   pp. 7437-7444 (2002).                                                      %
// % * M. F. Kasim, "Fast 2D phase unwrapping implementation in MATLAB",          %
// %   https://github.com/mfkasim91/unwrap_phase/ (2017).                         %
// %                                                                              %
// % Input:                                                                       %
// % * img: The wrapped phase image either from -pi to pi or from 0 to 2*pi.      %
// %        If there are unwanted regions, it should be filled with NaNs.         %
// %                                                                              %
// % Output:                                                                      %
// % * res_img: The unwrapped phase with arbitrary offset.                        %
// %                                                                              %
// % Author:                                                                      %
// %     Muhammad F. Kasim, University of Oxford (2017)                           %
// %     Email: firman.kasim@gmail.com                                            %
// %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%
// 
// function res_img = unwrap_phase(img)
//     [Ny, Nx] = size(img);
// 
//     % get the reliability
//     reliability = get_reliability(img); % (Ny,Nx)
// 
//     % get the edges
//     [h_edges, v_edges] = get_edges(reliability); % (Ny,Nx) and (Ny,Nx)
// 
//     % combine all edges and sort it
//     edges = [h_edges(:); v_edges(:)];
//     edge_bound_idx = Ny * Nx; % if i <= edge_bound_idx, it is h_edges
//     [~, edge_sort_idx] = sort(edges, 'descend');
// 
//     % get the indices of pixels adjacent to the edges
//     idxs1 = mod(edge_sort_idx - 1, edge_bound_idx) + 1;
//     idxs2 = idxs1 + 1 + (Ny - 1) .* (edge_sort_idx <= edge_bound_idx);
// 
//     % label the group
//     group = reshape([1:numel(img)], Ny*Nx, 1);
//     is_grouped = zeros(Ny*Nx,1);
//     group_members = cell(Ny*Nx,1);
//     for i = 1:size(is_grouped,1)
//         group_members{i} = i;
//     end
//     num_members_group = ones(Ny*Nx,1);
// 
//     % propagate the unwrapping
//     res_img = img;
//     num_nan = sum(isnan(edges)); % count how many nan-s and skip them
//     for i = num_nan+1 : length(edge_sort_idx)
//         % get the indices of the adjacent pixels
//         idx1 = idxs1(i);
//         idx2 = idxs2(i);
// 
//         % skip if they belong to the same group
//         if (group(idx1) == group(idx2)) continue; end
// 
//         % idx1 should be ungrouped (swap if idx2 ungrouped and idx1 grouped)
//         % otherwise, activate the flag all_grouped.
//         % The group in idx1 must be smaller than in idx2. If initially
//         % group(idx1) is larger than group(idx2), then swap it.
//         all_grouped = 0;
//         if is_grouped(idx1)
//             if ~is_grouped(idx2)
//                 idxt = idx1;
//                 idx1 = idx2;
//                 idx2 = idxt;
//             elseif num_members_group(group(idx1)) > num_members_group(group(idx2))
//                 idxt = idx1;
//                 idx1 = idx2;
//                 idx2 = idxt;
//                 all_grouped = 1;
//             else
//                 all_grouped = 1;
//             end
//         end
// 
//         % calculate how much we should add to the idx1 and group
//         dval = floor((res_img(idx2) - res_img(idx1) + pi) / (2*pi)) * 2*pi;
// 
//         % which pixel should be changed
//         g1 = group(idx1);
//         g2 = group(idx2);
//         if all_grouped
//             pix_idxs = group_members{g1};
//         else
//             pix_idxs = idx1;
//         end
// 
//         % add the pixel value
//         if dval ~= 0
//             res_img(pix_idxs) = res_img(pix_idxs) + dval;
//         end
// 
//         % change the group
//         len_g1 = num_members_group(g1);
//         len_g2 = num_members_group(g2);
//         group_members{g2}(len_g2+1:len_g2+len_g1) = pix_idxs;
//         group(pix_idxs) = g2; % assign the pixels to the new group
//         num_members_group(g2) = num_members_group(g2) + len_g1;
// 
//         % mark idx1 and idx2 as already being grouped
//         is_grouped(idx1) = 1;
//         is_grouped(idx2) = 1;
//     end
// end
// 
// function rel = get_reliability(img)
//     rel = zeros(size(img));
// 
//     % get the shifted images (N-2, N-2)
//     img_im1_jm1 = img(1:end-2, 1:end-2);
//     img_i_jm1   = img(2:end-1, 1:end-2);
//     img_ip1_jm1 = img(3:end  , 1:end-2);
//     img_im1_j   = img(1:end-2, 2:end-1);
//     img_i_j     = img(2:end-1, 2:end-1);
//     img_ip1_j   = img(3:end  , 2:end-1);
//     img_im1_jp1 = img(1:end-2, 3:end  );
//     img_i_jp1   = img(2:end-1, 3:end  );
//     img_ip1_jp1 = img(3:end  , 3:end  );
// 
//     % calculate the difference
//     gamma = @(x) sign(x) .* mod(abs(x), pi);
//     H  = gamma(img_im1_j   - img_i_j) - gamma(img_i_j - img_ip1_j  );
//     V  = gamma(img_i_jm1   - img_i_j) - gamma(img_i_j - img_i_jp1  );
//     D1 = gamma(img_im1_jm1 - img_i_j) - gamma(img_i_j - img_ip1_jp1);
//     D2 = gamma(img_im1_jp1 - img_i_j) - gamma(img_i_j - img_ip1_jm1);
// 
//     % calculate the second derivative
//     D = sqrt(H.*H + V.*V + D1.*D1 + D2.*D2);
// 
//     % assign the reliability as 1 / D
//     rel(2:end-1, 2:end-1) = 1./D;
// 
//     % assign all nan's in rel with non-nan in img to 0
//     % also assign the nan's in img to nan
//     rel(isnan(rel) & ~isnan(img)) = 0;
//     rel(isnan(img)) = nan;
// end
// 
// function [h_edges, v_edges] = get_edges(rel)
//     [Ny, Nx] = size(rel);
//     h_edges = [rel(1:end, 2:end) + rel(1:end, 1:end-1), nan(Ny, 1)];
//     v_edges = [rel(2:end, 1:end) + rel(1:end-1, 1:end); nan(1, Nx)];
// end

    
}
