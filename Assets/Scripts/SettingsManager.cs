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

    //Attached to Settings Manager GameObject

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
    [SerializeField] private int webcamNumSelected = 0;

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
        SaveButton.onClick.AddListener(delegate { OnButtonClicked(); });
        cameraDropdown.onValueChanged.AddListener(OnCameraOptionChanged);

        InitSettings();
    }

    private void Update()
    {
        if (GameObject.Find("Dropdown List")) //Dummy way to set the height, but can't work out to fix otherwise!
        {
            RectTransform myRectTransform = GameObject.Find("Dropdown List").GetComponent<RectTransform>();
            myRectTransform.sizeDelta = new Vector2(myRectTransform.sizeDelta.x, 100);
        }
    }

    public void InitSettings()
    {
        numOfScreenings = 2; // HARDCODED IN A 2 SCREENING NUMBER HERE
        displayDevice = 0;

        GetWebcamDevices();
        DefaultToFirstWebcam();
        SetWebcam();
    }

    #region CAMERA SETTINGS

    private void DefaultToFirstWebcam()
    {
        //If we can, auto-select the first webcam make the Continue button available
        if (cameraDevices.Length > 0)
        {
            if (webcamNumSelected == 0) webcamNumSelected = 1;

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
            //Init camera
            if (webcam != null)
            {
                webcam.Stop();
                //Destroy(webcam);
            }

            webcam = new WebCamTexture();
            webcam.requestedFPS = 30;

            Debug.Log($"[{GetType().Name}] SetWebcam : Webcam selected :  "+ webcamNumSelected);
            //Application.RequestUserAuthorization(UserAuthorization.WebCam);

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
        //Reset
        cameraDropdown.ClearOptions ();
        NoWebcamWarning.SetActive(false);

        //Create list of devices
        cameraDevices = WebCamTexture.devices;
        List<string> deviceNames = new List<string> ();
        foreach ( WebCamDevice device in cameraDevices ) {
            deviceNames.Add ( device.name );
            Debug.Log($"[{GetType().Name}] Webcam available  : "+device.name);
        }

        //Populate the dropdown
        cameraDropdown.AddOptions ( deviceNames );

        //Warn if none found
        if (deviceNames.Count == 0)
        {            
            NoWebcamWarning.SetActive(true);
            Debug.LogError("No camera devices found!");
        }

        //Set dropdown to one chosen if set previously
        cameraDropdown.value = webcamNumSelected-1;
    }

    #endregion


    /// <summary>
    /// Function triggered when the button is clicked
    /// Stops the webcam and goes to the main menu
    /// </summary>
    private void OnButtonClicked () 
    {
        Debug.Log($"[{GetType().Name}] Button Clicked : Go To Tracking View");

        viewManager.TrackingView();
    }
}
