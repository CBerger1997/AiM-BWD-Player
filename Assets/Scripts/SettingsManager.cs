using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour {

    public enum AnalysisOptions {
        Range,
        Average,
        Baseline
    }

    public enum ResolutionOptions {
        _2048x1080,
        _4096x2160
    }

    [SerializeField] private TMP_Dropdown cameraDropdown;
    [SerializeField] private RawImage cameraImage;
    [SerializeField] private TMP_Dropdown analysisDropdown;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown audioDeviceDropdown;
    [SerializeField] private TMP_InputField outputFilenameInputField;
    [SerializeField] private Button SaveButton;
    [SerializeField] private ViewManager viewManager;

    public WebCamDevice webcam;
    public AnalysisOptions analysis;
    public ResolutionOptions resolution;
    public string audioDevice;
    public string outputFilename;

    private WebCamDevice[] cameraDevices;
    private TMP_Dropdown.OptionData blankTempData;

    private bool isCameraSet;
    private bool isAnalysisSet;
    private bool isResolutionSet;
    private bool isAudioSet;
    private bool isOutputFilenameSet;

    private void Awake() {
        cameraDropdown.onValueChanged.AddListener(delegate { OnCameraOptionChanged(); });
        analysisDropdown.onValueChanged.AddListener(delegate { OnAnalysisOptionChanged(); });
        resolutionDropdown.onValueChanged.AddListener(delegate { OnResolutionOptionChanged(); });
        audioDeviceDropdown.onValueChanged.AddListener(delegate { OnAudioDeviceChanged(); });
        outputFilenameInputField.onValueChanged.AddListener(delegate { OnOutputFileNameChanged(); });
        SaveButton.onClick.AddListener(delegate { OnSavebuttonClicked(); });

        blankTempData = new TMP_Dropdown.OptionData("-");

        GetWebcamDevices();
        GetAnalysisOptions();
        GetResolutionOptions();
        SaveButton.interactable = false;
    }

    private void Update() {
        if (isCameraSet && isAnalysisSet && isResolutionSet && isAudioSet && isOutputFilenameSet) {
            SaveButton.interactable = true;
        } else {
            SaveButton.interactable = false;
        }
    }

    #region CAMERA SETTINGS

    private void OnCameraOptionChanged() {
        if (cameraDropdown.value > 0) {
            WebCamTexture webcamTexture = new WebCamTexture(webcam.name);

            webcamTexture.Stop();

            webcam = cameraDevices[cameraDropdown.value - 1];

            webcamTexture.name = webcam.name;

            cameraImage.texture = webcamTexture;

            if (webcamTexture.isReadable) {
                webcamTexture.Play();
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

    #endregion

    #region AUDIO SETTINGS

    private void OnAudioDeviceChanged() {
        isAudioSet = true;
    }

    #endregion

    #region FILENAME SETTINGS

    private void OnOutputFileNameChanged() {
        if (outputFilenameInputField.text != "") {
            isOutputFilenameSet = true;
        } else {
            isOutputFilenameSet = false;
        }
    }

    private void SetOutputFilename() {
        outputFilename = outputFilenameInputField.text;
    }

    #endregion

    private void OnSavebuttonClicked() {
        SetOutputFilename();

        viewManager.GoToMainMenu();
    }
}
