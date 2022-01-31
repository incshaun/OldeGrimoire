using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;
using UnityEngine.XR.Management;

// For the moment, switch the project settings at the same time as toggling this.
[ExecuteAlways]
public class SwitchARVR : MonoBehaviour
{
    public enum Modality { VR, AR };
    
    public Modality applicationMode = Modality.VR;
    
    [System.Serializable]
    public class GameObjectList
    {
        public List <GameObject> objects;
    }
    
    public GameObjectList VR;
    public GameObjectList AR;
    
    private void switchSelection ()
    {
        // Some custom changes, based on each modality.
        switch (applicationMode)
        {
            case Modality.VR:
            {
#if UNITY_EDITOR      
//                XRGeneralSettingsPerBuildTarget buildTargetSettings = null;          
//                 var buildTargetSettings = XRGeneralSettingsPerBuildTarget.SettingsForBuildTarget (3);
#endif            
//                 XRGeneralSettingsPerBuildTarget buildTargetSettings = null;
// EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings);
// XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);      
// XRPackageMetadataStore.RemoveLoader(settings.Manager, "Unity.XR.Oculus.OculusLoader", BuildTargetGroup.Android);

//                 XRGeneralSettings settings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget (BuildTargetGroup.Android);
            }
            break;
            case Modality.AR:
                break;
        }
        
        // Enable/disable objects associated with each modality.
        Dictionary <Modality, GameObjectList> selections = new Dictionary <Modality, GameObjectList> ();
        selections[Modality.VR] = VR;
        selections[Modality.AR] = AR;
        
        foreach (KeyValuePair <Modality, GameObjectList> item in selections)
        {
            foreach (GameObject g in item.Value.objects)
            {
                g.SetActive (applicationMode == item.Key);
            }
        }
    }  
    
    public IEnumerator StartXRCoroutine()
    {
        Debug.Log("Initializing XR...");
        yield return XRGeneralSettings.Instance.Manager.InitializeLoader();
        
        if (XRGeneralSettings.Instance.Manager.activeLoader == null)
        {
            Debug.LogError("Initializing XR Failed. Check Editor or Player log for details.");
        }
        else
        {
            Debug.Log("Starting XR...");
            XRGeneralSettings.Instance.Manager.StartSubsystems();
        }
    }
    
    void Start ()
    {
        if (Application.isPlaying)
        {
           StartCoroutine (StartXRCoroutine ());    
        }
    }
    
    void Update ()
    {
        if (!Application.isPlaying)
        {
          Debug.Log ("Updating");
          switchSelection ();
        }
    }
}
