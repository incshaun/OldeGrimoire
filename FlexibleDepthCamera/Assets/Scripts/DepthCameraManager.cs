using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using Unity.Sentis;

public class DepthCameraManager : MonoBehaviour
{
    [Tooltip ("For debugging, a material to show the camera image")]
    public Material colourImageMaterial;
    
    [Tooltip ("For debugging, a material to show the depth image")]
    public Material depthImageMaterial;
    
    [Tooltip ("The depth estimation model")]
    public ModelAsset estimationModel;
    
    private WebCamTexture webcamTexture;
    private Worker depthCameraWorker;
    private Tensor<float> inputTensor;
    private RenderTexture depthTexture;
    
    private void updateWebCam ()
    {
        if (webcamTexture == null)
        {
            webcamTexture = new WebCamTexture ();
            #if UNITY_ANDROID         
            if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
            {
                webcamTexture = null;
            }
            #endif              
        }
        if (!webcamTexture.isPlaying)
        {
            if (colourImageMaterial != null)
            {
                colourImageMaterial.mainTexture = webcamTexture;
            }
            webcamTexture.Play ();
        }
    }
    
    public Texture getColourTexture ()
    {
        return webcamTexture;
    }
    
    public Texture getDepthTexture ()
    {
        return depthTexture;
    }
    
    // Thanks: https://github.com/Unity-Technologies/sentis-samples/tree/main/DepthEstimationSample
    private void loadModel ()
    {
        var model = ModelLoader.Load(estimationModel);
        
        // Extend the graph with some post processing to normalize the depth map.
        var graph = new FunctionalGraph();
        var inputs = graph.AddInputs (model);
        FunctionalTensor[] outputs = Functional.Forward(model, inputs);
        var output = outputs[0];
        FunctionalTensor max0 = Functional.ReduceMax(output, new int [] { 0, 1, 2 }, false);
        FunctionalTensor min0 = Functional.ReduceMin(output, new int [] { 0, 1, 2 }, false);
        FunctionalTensor maxmmin = Functional.Sub(max0, min0);
        FunctionalTensor outputmmin = Functional.Sub(output, min0);
        FunctionalTensor output2 = Functional.Div(outputmmin, maxmmin);
        model = graph.Compile(output2);
        
        depthCameraWorker = new Worker(model, BackendType.GPUCompute);

        depthTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGBFloat);
        inputTensor = new Tensor<float>(new TensorShape(1, 3, 256, 256), true);        
    }
    
    private void getDepth ()
    {
        TextureConverter.ToTensor (webcamTexture, inputTensor, new TextureTransform());
        depthCameraWorker.Schedule (inputTensor);

        var output = depthCameraWorker.PeekOutput() as Tensor<float>;
        output.Reshape(output.shape.Unsqueeze(0));
        TextureConverter.RenderToTexture(output as Tensor<float>, depthTexture, new TextureTransform().SetCoordOrigin(CoordOrigin.TopLeft));
        
        if (depthImageMaterial != null)
        {
            depthImageMaterial.mainTexture = depthTexture;
        }
    }
    
    void Start()
    {
        loadModel ();
    }
    
    void Update()
    {
        updateWebCam (); 
        
        getDepth ();
    }

    private void OnDestroy()
    {
        depthCameraWorker.Dispose();
        inputTensor.Dispose();
        depthTexture.Release();
    }
    
}
