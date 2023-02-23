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
        Range,
        Average,
        Baseline
    }

    public enum ResolutionOptions {
        _1920x960,
        _4096x2160
    }

    #endregion

    #region CORE VARIABLES

    public WebCamTexture webcam { get; set; }
    public AnalysisOptions analysis { get; set; }
    public ResolutionOptions resolution { get; set; }
    public int displayDevice { get; set; }
    public string outputFilename { get; set; }
    public string outputFilePath { get; set; }
    public string videoFilePath { get; set; }

    #endregion

    #region UI VARIABLES

    [SerializeField] private TMP_Dropdown cameraDropdown;
    [SerializeField] private RawImage cameraImage;
    [SerializeField] private TMP_Dropdown analysisDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown displayDeviceDropdown;
    [SerializeField] private TMP_InputField outputFilenameInputField;
    [SerializeField] private Button SaveButton;
    [SerializeField] private Button DisplayTestButton;
    [SerializeField] private Button OpenExplorerButtonOutputFilePath;
    [SerializeField] private Button OpenExplorerButtonVideoFilePath;
    [SerializeField] private ViewManager viewManager;
    [SerializeField] private TMP_Text OutputFilePathText;
    [SerializeField] private TMP_Text VideoFilePathText;
    private TMP_Dropdown.OptionData blankTempData;

    #endregion 

    #region DEVICE VARIABLES

    private WebCamDevice[] cameraDevices;
    private Display[] displayDevices;

    #endregion

    #region BOOLEANS

    private bool isCameraSet;
    private bool isAnalysisSet;
    private bool isResolutionSet;
    private bool isDisplaySet;
    private bool isOutputFilenameSet;

    #endregion

    public readonly int textureRequestedWidth = 1280;
    public readonly int textureRequestedHeight = 720;
    private readonly int wTextureRequestedFPS = 30;

    private void Awake() {
        cameraDropdown.onValueChanged.AddListener(delegate { OnCameraOptionChanged(); });
        analysisDropdown.onValueChanged.AddListener(delegate { OnAnalysisOptionChanged(); });
        resolutionDropdown.onValueChanged.AddListener(delegate { OnResolutionOptionChanged(); });
        displayDeviceDropdown.onValueChanged.AddListener(delegate { OnDisplayDeviceChanged(); });
        outputFilenameInputField.onValueChanged.AddListener(delegate { OnOutputFileNameChanged(); });
        SaveButton.onClick.AddListener(delegate { OnSavebuttonClicked(); });
        DisplayTestButton.onClick.AddListener(delegate { OnDisplayTestClicked(); });

        blankTempData = new TMP_Dropdown.OptionData("-");

        GetWebcamDevices();
        GetAnalysisOptions();
        GetDisplayDevices();
        GetResolutionOptions();
        SaveButton.interactable = false;
        webcam = new WebCamTexture();
        webcam.requestedFPS = 30;

        SetFilePathDestinations();
    }

    private void Update() {
        if (isCameraSet && isAnalysisSet && isResolutionSet && isDisplaySet && isOutputFilenameSet) {
            SaveButton.interactable = true;
        } else {
            SaveButton.interactable = false;
        }
    }

    #region CAMERA SETTINGS

    //FIX CORRECT WEBCAM BEING SELECTED 
    //FIX MAC PERMISSIONS FOR WEBCAM
    private void OnCameraOptionChanged() {
        if (cameraDropdown.value > 0) {
            
            //Application.RequestUserAuthorization(UserAuthorization.WebCam);

            if (webcam != null) {
                webcam.Stop();
            }

            webcam = new WebCamTexture(
                cameraDevices[cameraDropdown.value - 1].name,
                textureRequestedWidth,
                textureRequestedHeight,
                wTextureRequestedFPS
            );

            cameraImage.texture = webcam;

            if (webcam.isReadable) {
                webcam.Play();
            }

            isCameraSet = true;
        } else {
            isCameraSet = false;
        }
    }

    private void GetWebcamDevices() {
        cameraDropdown.ClearOptions();

        cameraDevices = WebCamTexture.devices;

        List<string> deviceNames = new List<string>();

        foreach (WebCamDevice device in cameraDevices) {
            deviceNames.Add(device.name);
        }

        cameraDropdown.options.Add(blankTempData);

        cameraDropdown.AddOptions(deviceNames);
    }

    #endregion

    #region ANALYSIS SETTINGS

    private void OnAnalysisOptionChanged() {
        if (analysisDropdown.value > 0) {
            analysis = (AnalysisOptions)analysisDropdown.value - 1;
            isAnalysisSet = true;
        } else {
            isAnalysisSet = false;
        }
    }

    private void GetAnalysisOptions() {
        analysisDropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (string option in System.Enum.GetNames(typeof(AnalysisOptions))) {
            options.Add(option);
        }

        analysisDropdown.options.Add(blankTempData);

        analysisDropdown.AddOptions(options);
    }

    #endregion

    #region RESOLUTION SETTINGS

    private void OnResolutionOptionChanged() {
        if (resolutionDropdown.value > 0) {
            resolution = (ResolutionOptions)resolutionDropdown.value - 1;
            isResolutionSet = true;
            if(resolutionDropdown.value == 1) {
                CheckVideosFromFolder(true);
            } else {
                CheckVideosFromFolder(false);
            }
            VideoFilePathText.text = videoFilePath;
        } else {
            isResolutionSet = false;
        }
    }

    private void GetResolutionOptions() {
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (string option in System.Enum.GetNames(typeof(ResolutionOptions))) {
            options.Add(option);
        }

        resolutionDropdown.options.Add(blankTempData);

        resolutionDropdown.AddOptions(options);
    }

    private void CheckVideosFromFolder(bool is2K) {
        string folder = is2K == true ? "BWD 2K" : "BWD 4K";
        videoFilePath = folder;
        //var info = new DirectoryInfo(Application.streamingAssetsPath + "/" + folder + "/");
        //var fileInfo = info.GetFiles();
        //int counter = 0;
        //foreach (var file in fileInfo) {
        //    if (file.Extension == ".mp4") {
        //        counter++;
        //    }
        //}

    }

    #endregion

    #region DISPLAY SETTINGS

    private void OnDisplayDeviceChanged() {
        displayDevice = displayDeviceDropdown.value - 1;
        isDisplaySet = true;
    }

    private void GetDisplayDevices() {
        displayDeviceDropdown.ClearOptions();

        displayDevices = Display.displays;

        List<string> deviceNames = new List<string>();

        foreach (Display device in displayDevices) {
            //Find way for actual device name
            deviceNames.Add(device.ToString());
        }

        displayDeviceDropdown.options.Add(blankTempData);

        displayDeviceDropdown.AddOptions(deviceNames);
    }

    private void OnDisplayTestClicked() {
        Display.displays[displayDevice].Activate();
    }

    #endregion

    #region FILENAME SETTINGS

    /// <summary>
    /// Function to define the file path for the videos and output files
    /// Change the path variable to string to define the location of the videos
    /// The output files are located within a folder called Data Output, if this doesn't exist within the video folder, please create it
    /// </summary>
    private void SetFilePathDestinations() {
        outputFilePath = Application.streamingAssetsPath + @"\" + "Data Output" + @"\";
        videoFilePath = Application.streamingAssetsPath + "/BWD 2K/";

        OutputFilePathText.text = outputFilePath;
        VideoFilePathText.text = videoFilePath;
    }

    /// <summary>
    /// Function to check that the output filename contains text
    /// Save button is disabled if there is no text
    /// </summary>
    private void OnOutputFileNameChanged() {
        if (outputFilenameInputField.text != "") {
            isOutputFilenameSet = true;
        } else {
            isOutputFilenameSet = false;
        }
    }

    /// <summary>
    /// Stores the output filename
    /// </summary>
    private void SetOutputFilename() {
        outputFilename = outputFilenameInputField.text;
    }

    #endregion

    /// <summary>
    /// Function triggered when the save button is clicked
    /// Sets the output filename, stops the webcam and goes to the main menu
    /// </summary>
    private void OnSavebuttonClicked() {
        SetOutputFilename();

        webcam.Stop();

        viewManager.GoToMainMenu();
    }
}
