using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.Networking;
using System.IO;

public class SettingsManager : MonoBehaviour {

    #region ENUMS

    public enum AnalysisOptions {
        Valence_Baseline,
        Arousal_Baseline,
        Arousal_Over_Valence,
        Valence_Over_Arousal
    }

    public enum ResolutionOptions {
        _2K,
        _4K
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
    //[SerializeField] private TMP_Dropdown resolutionDropdown;
    //[SerializeField] private TMP_Dropdown displayDeviceDropdown;
    [SerializeField] private Button SaveButton;
    [SerializeField] private Button DisplayTestButton;
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private TMP_Text VideoFilePathText;
    private TMP_Dropdown.OptionData blankTempData;

    #endregion 

    #region DEVICE VARIABLES

    private WebCamDevice[] cameraDevices;
    //private Display[] displayDevices;

    #endregion

    #region BOOLEANS

    private bool isCameraSet;
    //private bool isScreeningsSet
    //private bool isResolutionSet;
    //private bool isDisplaySet;

    #endregion

    public readonly int textureRequestedWidth = 1280;
    public readonly int textureRequestedHeight = 720;
    private readonly int wTextureRequestedFPS = 30;
    private int maxScreenings = 10;

    private void Awake () {
        cameraDropdown.onValueChanged.AddListener ( delegate { OnCameraOptionChanged (); } );
        //screeningsDropdown.onValueChanged.AddListener ( delegate { OnScreeningsValueChanged (); } );
        //resolutionDropdown.onValueChanged.AddListener ( delegate { OnResolutionOptionChanged (); } );
        //displayDeviceDropdown.onValueChanged.AddListener ( delegate { OnDisplayDeviceChanged (); } );
        SaveButton.onClick.AddListener ( delegate { OnSavebuttonClicked (); } );
        //DisplayTestButton.onClick.AddListener ( delegate { OnDisplayTestClicked (); } );

        blankTempData = new TMP_Dropdown.OptionData ( "-" );

        GetWebcamDevices ();
        //GetScreeningOptions ();
        numOfScreenings = 2;
        //GetDisplayDevices ();
        displayDevice = 0;
        //GetResolutionOptions ();
        SaveButton.interactable = false;
        webcam = new WebCamTexture ();
        webcam.requestedFPS = 30;

        SetFilePathDestinations ();

        

        
    }

    private void Update () {
        if ( isCameraSet)//  && isDisplaySet )
        { //&& isScreeningsSet && isResolutionSet
            SaveButton.interactable = true;
        } else {
            SaveButton.interactable = false;
        }
    }

    #region CAMERA SETTINGS

    //FIX CORRECT WEBCAM BEING SELECTED 
    //FIX MAC PERMISSIONS FOR WEBCAM
    private void OnCameraOptionChanged () {
        if ( cameraDropdown.value > 0 ) {

            //Application.RequestUserAuthorization(UserAuthorization.WebCam);

            if ( webcam != null ) {
                webcam.Stop ();
            }

            webcam = new WebCamTexture (
                cameraDevices[ cameraDropdown.value - 1 ].name,
                textureRequestedWidth,
                textureRequestedHeight,
                wTextureRequestedFPS
            );

            webcam.name = cameraDevices[ cameraDropdown.value - 1 ].name;

            cameraImage.texture = webcam;

            if ( webcam.isReadable ) {
                webcam.Play ();
            }

            isCameraSet = true;
        } else {
            isCameraSet = false;
        }
    }

    private void GetWebcamDevices () {
        cameraDropdown.ClearOptions ();

        cameraDevices = WebCamTexture.devices;

        List<string> deviceNames = new List<string> ();

        foreach ( WebCamDevice device in cameraDevices ) {
            deviceNames.Add ( device.name );
        }

        cameraDropdown.options.Add ( blankTempData );

        cameraDropdown.AddOptions ( deviceNames );
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

    #region RESOLUTION SETTINGS

    /*private void OnResolutionOptionChanged () {
        if ( resolutionDropdown.value > 0 ) {
            resolution = ( ResolutionOptions ) resolutionDropdown.value - 1;
            isResolutionSet = true;
            if ( resolutionDropdown.value == 1 ) {
                CheckVideosFromFolder ( true );
            } else {
                CheckVideosFromFolder ( false );
            }
            VideoFilePathText.text = videoFilePath;
        } else {
            isResolutionSet = false;
        }
    }*/

    /*private void GetResolutionOptions () {
        resolutionDropdown.ClearOptions ();

        List<string> options = new List<string> ();

        foreach ( string option in System.Enum.GetNames ( typeof ( ResolutionOptions ) ) ) {
            options.Add ( option );
        }

        resolutionDropdown.options.Add ( blankTempData );

        resolutionDropdown.AddOptions ( options );
    }*/

    /*private void CheckVideosFromFolder ( bool is2K ) {
        string folder = is2K == true ? "BWD 2K" : "BWD 4K";
        videoFilePath = folder;
    }*/

    #endregion

    #region DISPLAY SETTINGS

    /*private void OnDisplayDeviceChanged () {
        displayDevice = displayDeviceDropdown.value - 1;
        isDisplaySet = true;
    }*/

    /*private void GetDisplayDevices () {
        displayDeviceDropdown.ClearOptions ();

        displayDevices = Display.displays;

        List<string> deviceNames = new List<string> ();

        foreach ( Display device in displayDevices ) {
            //Find way for actual device name
            deviceNames.Add ( device.ToString () );
        }

        displayDeviceDropdown.options.Add ( blankTempData );

        displayDeviceDropdown.AddOptions ( deviceNames );
    }*/

    /*private void OnDisplayTestClicked () {
        Display.displays[ displayDevice ].Activate ();
    }*/

    #endregion

    #region FILENAME SETTINGS

    /// <summary>
    /// Function to define the file path for the videos and output files
    /// Change the path variable to string to define the location of the videos
    /// The output files are located within a folder called Data Output, if this doesn't exist within the video folder, please create it
    /// </summary>
    private void SetFilePathDestinations () {
        videoFilePath = Application.streamingAssetsPath + "/BWD 2K/";

        //VideoFilePathText.text = videoFilePath;
    }

    #endregion

    /// <summary>
    /// Function triggered when the save button is clicked
    /// Sets the output filename, stops the webcam and goes to the main menu
    /// </summary>
    private void OnSavebuttonClicked () {
        webcam.Stop ();

        viewManager.GoToMainMenu ();
    }
}
