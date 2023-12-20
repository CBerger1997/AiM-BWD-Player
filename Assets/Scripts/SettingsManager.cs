using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.Windows.WebCam;
using System.Linq;

public class SettingsManager : MonoBehaviour {

    //Attahced to Settings Manager GameObject

    //Configures web cam, video resolution etc, num of screenings
    #region ENUMS

    public enum AnalysisOptions {
        Valence_Baseline,
        Arousal_Baseline,
        Arousal_Over_Valence,
        Valence_Over_Arousal
    }

    public enum ResolutionOptions {
        _2K
    }

    #endregion

    #region CORE VARIABLES

    public WebCamTexture webcam { get; set; }
    public AnalysisOptions analysis { get; set; }
    public ResolutionOptions resolution { get; set; }
    public int displayDevice { get; set; }
    public int numOfScreenings { get; set; }

    #endregion

    #region UI VARIABLES

    [SerializeField] private TMP_Dropdown cameraDropdown;
    [SerializeField] private RawImage cameraImage;
    [SerializeField] private Button SaveButton;
    [SerializeField] private ViewManager viewManager;    
    [SerializeField] private GameObject NoWebcamWarning;

    #endregion

    #region DEVICE VARIABLES

    private WebCamDevice[] cameraDevices;
    [SerializeField] private int webcamNumSelected;

    #endregion

    #region BOOLEANS

    private bool isCameraSet;

    #endregion

    public readonly int textureRequestedWidth = 1280;
    public readonly int textureRequestedHeight = 720;
    private readonly int wTextureRequestedFPS = 30;
    //private int maxScreenings = 10;


    private void Start()
    {
        SaveButton.onClick.AddListener(delegate { OnContinueButtonClicked(); });
        cameraDropdown.onValueChanged.AddListener(OnCameraOptionChanged);

        InitSettings();
    }

    public void InitSettings()
    {
        numOfScreenings = 2;
        displayDevice = 0;

        webcam = new WebCamTexture();
        webcam.requestedFPS = 30;

        NoWebcamWarning.SetActive(false);

        GetWebcamDevices();
        DefaultToFirstWebcam();
        SetWebcam();

        /*
       //Trying to set the size of the dropdown
       GameObject[] objectsWithName = GameObject.FindObjectsOfType<GameObject>()
           .Where(obj => obj.name == "Item Label")
           .ToArray();

       foreach (GameObject obj in objectsWithName)
       {
           obj.GetComponent<TextMeshPro>().fontSize = 34;
           Debug.Log("Found object: " + obj.name);
       }
       */
    }

    #region CAMERA SETTINGS

    private void DefaultToFirstWebcam()
    {
        //If we can, auto-select the first webcam make the Continue button available
        if (cameraDevices.Length > 0)
        {
            webcamNumSelected = 1;
            SaveButton.interactable = true;
        }
        else
        {
            SaveButton.interactable = false;
        }
    }


    //FIX CORRECT WEBCAM BEING SELECTED 
    //FIX MAC PERMISSIONS FOR WEBCAM
    private void OnCameraOptionChanged (int index) {
        webcamNumSelected = cameraDropdown.value+1;
        //Debug.Log($"[{GetType().Name}] webcamNumSelected : " + webcamNumSelected);
        SetWebcam();
    }

    

    private void SetWebcam()
    {
        //Debug.Log($"[{GetType().Name}] SetWebcam...");

        if (webcamNumSelected > 0)
        {
            Debug.Log($"[{GetType().Name}] SetWebcam : Webcam selected :  "+ webcamNumSelected);
            //Application.RequestUserAuthorization(UserAuthorization.WebCam);

            if (webcam != null)
            {
                
                webcam.Stop();
                //Debug.Log($"[{GetType().Name}] SetWebcam : webcam Stopped");
            }

           /* if (!webcam) Debug.LogError("webcam : IS NULL");
            if (cameraDevices.Length == 0) Debug.LogError("cameraDevices : EMPTY LIST");
            if (webcamNumSelected == 0) Debug.LogError("webcamNumSelected : IS 0");
            if (cameraDevices[webcamNumSelected - 1].name != "") Debug.LogError("cameraDevices[webcamNumSelected - 1].name : IS EMPTY");
            if (textureRequestedWidth == 0) Debug.LogError("textureRequestedWidth : IS 0");
            if (textureRequestedHeight == 0) Debug.LogError("textureRequestedHeight : IS 0");
            if (wTextureRequestedFPS == 0) Debug.LogError("wTextureRequestedFPS : IS 0");*/

            webcam = new WebCamTexture(
                cameraDevices[webcamNumSelected - 1].name,
                textureRequestedWidth,
                textureRequestedHeight,
                wTextureRequestedFPS
            );
           
            //Debug.Log($"[{GetType().Name}] SetWebcam : created new webcam texture");

            webcam.name = cameraDevices[webcamNumSelected - 1].name;

            cameraImage.texture = webcam;

            //Debug.Log($"[{GetType().Name}] SetWebcam : Set image texture to Webcam texture");

            if (webcam.isReadable)
            {
                Debug.Log($"[{GetType().Name}] SetWebcam : Play Webcam : "+ webcam.name);
                webcam.Play();
            }

            isCameraSet = true;
            SaveButton.interactable = true;
        }
        else
        {
            isCameraSet = false;
        }
    }
    private void GetWebcamDevices () 
    {
        //Init
        cameraDropdown.ClearOptions ();
        webcamNumSelected = 0;

        //Populate list of devices
        cameraDevices = WebCamTexture.devices;

        List<string> deviceNames = new List<string> ();

        foreach ( WebCamDevice device in cameraDevices ) {
            deviceNames.Add ( device.name );
            Debug.Log($"[{GetType().Name}] Webcam available  : "+device.name);
        }

        cameraDropdown.AddOptions ( deviceNames );


        //Warn if none found
        if (deviceNames.Count == 0)
        {            
            NoWebcamWarning.SetActive(true);
            Debug.LogError("No camera devices found!");
        } 
    }

    #endregion


    /// <summary>
    /// Function triggered when the button is clicked
    /// Stops the webcam and goes to the main menu
    /// </summary>
    private void OnContinueButtonClicked () {

        webcam.Stop();

        Debug.Log($"[{GetType().Name}] Button Clicked : Go To Video View");

        viewManager.GoToVideoView();
    }
}
