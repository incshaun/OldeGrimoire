using System.Collections;
using System.Collections.Generic;
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
        phase = new float[inputHeight, inputWidth];
        distance = new float[inputHeight, inputWidth];
        depth = new float[inputHeight, inputWidth];
        mask = new bool[inputHeight, inputWidth];
        process = new bool[inputHeight, inputWidth];
        colors = new Color[inputHeight, inputWidth];
        names = new int[inputHeight, inputWidth];
            phaseWrap();
            phaseUnwrap();
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
    
    
}
