using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;

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
    public string videoFilePath { get; set; }

    #endregion

    #region UI VARIABLES

    [SerializeField] private TMP_Dropdown cameraDropdown;
    [SerializeField] private RawImage cameraImage;
    //[SerializeField] private TMP_Dropdown screeningsDropdown;

    [SerializeField] private Button SaveButton;
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private TMP_Text VideoFilePathText;

    #endregion

    #region DEVICE VARIABLES

    private WebCamDevice[] cameraDevices;
    private int webcamNumSelected = 0;

    #endregion

    #region BOOLEANS

    private bool isCameraSet;
    //private bool isScreeningsSet

    #endregion

    public readonly int textureRequestedWidth = 1280;
    public readonly int textureRequestedHeight = 720;
    private readonly int wTextureRequestedFPS = 30;
    private int maxScreenings = 10;

    private void Awake () {
        
        //screeningsDropdown.onValueChanged.AddListener ( delegate { OnScreeningsValueChanged (); } );
        SaveButton.onClick.AddListener ( delegate { OnSavebuttonClicked (); } );
        cameraDropdown.onValueChanged.AddListener(delegate { OnCameraOptionChanged(); });

        GetWebcamDevices ();
        //GetScreeningOptions ();
        numOfScreenings = 2;
        displayDevice = 0;
        videoFilePath = Application.streamingAssetsPath + "/BWD 2K/";

        webcam = new WebCamTexture ();
        webcam.requestedFPS = 30;

        if (cameraDevices.Length > 0) webcamNumSelected = 1;
        SetWebcam();
    }

    private void Update () {
        if (isCameraSet)
        {
            SaveButton.interactable = true;
            Debug.Log("Webcam Playing ? : " + webcam.isPlaying);
        }
        else
        {
            SaveButton.interactable = false;
        }
    }

    #region CAMERA SETTINGS

    //FIX CORRECT WEBCAM BEING SELECTED 
    //FIX MAC PERMISSIONS FOR WEBCAM
    private void OnCameraOptionChanged () {
        Debug.Log("OnCameraOptionChanged...");
        webcamNumSelected = cameraDropdown.value;
        SetWebcam();
    }

    private void SetWebcam()
    {
        Debug.Log("SetWebcam...");

        if (webcamNumSelected > 0)
        {
            Debug.Log("SetWebcam : webcamNumSelected :  "+ webcamNumSelected);
            //Application.RequestUserAuthorization(UserAuthorization.WebCam);

            if (webcam != null)
            {
                webcam.Stop();
            }

            webcam = new WebCamTexture(
                cameraDevices[webcamNumSelected - 1].name,
                textureRequestedWidth,
                textureRequestedHeight,
                wTextureRequestedFPS
            );
            Debug.Log("SetWebcam : created new webcam texture");

            webcam.name = cameraDevices[webcamNumSelected - 1].name;

            cameraImage.texture = webcam;

            Debug.Log("SetWebcam : Set image texture to Webcam texture");

            if (webcam.isReadable)
            {
                Debug.Log("SetWebcam : webcam.isReadable, Play webcam : "+ webcam.name);
                webcam.Play();
            }

            isCameraSet = true;
        }
        else
        {
            isCameraSet = false;
        }
    }
    private void GetWebcamDevices () {
        cameraDropdown.ClearOptions ();

        cameraDevices = WebCamTexture.devices;

        List<string> deviceNames = new List<string> ();

        foreach ( WebCamDevice device in cameraDevices ) {
            deviceNames.Add ( device.name );
            Debug.Log("Webcam available  : "+device.name);
        }

        cameraDropdown.AddOptions ( deviceNames );


        if (deviceNames.Count == 0) 
        {
            //NO WEBCAMS ATTACHED!!
            Debug.LogError("No camera devices found!"); //WARN THE USER !!!!!!!!!!!!!!!!!!
        } 
    }

    #endregion

    #region SCREENING SETTINGS

    /*private void OnScreeningsValueChanged () {
        if ( screeningsDropdown.value > 0 ) {
            analysis = ( AnalysisOptions ) screeningsDropdown.value - 1;
            numOfScreenings = screeningsDropdown.value;
            isScreeningsSet = true;
        } else {
            isScreeningsSet = false;
        }
    }*/

    /*private void GetScreeningOptions () {
        screeningsDropdown.ClearOptions ();

        List<string> options = new List<string> ();

        for ( int i = 1; i <= maxScreenings; i++ ) {
            options.Add ( i.ToString () );
        }

        screeningsDropdown.options.Add ( blankTempData );

        screeningsDropdown.AddOptions ( options );
    }*/

    #endregion


    /// <summary>
    /// Function triggered when the save button is clicked
    /// Sets the output filename, stops the webcam and goes to the main menu
    /// </summary>
    private void OnSavebuttonClicked () {

        webcam.Stop ();
        Debug.Log("OnSavebuttonClicked, webcam.Stopped, go to MainMenu");

        viewManager.GoToMainMenu ();
    }
}
