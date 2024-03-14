using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

public class SettingsManager : MonoBehaviour 
{
    //Displays and configures web cam and starts tracking on button press

    //Managers
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private ViewManager viewManager;


    /*public enum AnalysisOptions {
        Valence_Baseline,
        Arousal_Baseline,
        Arousal_Over_Valence,
        Valence_Over_Arousal
    }*/

    //public AnalysisOptions analysis { get; set; }
    //public ResolutionOptions resolution { get; set; }

    public int numOfScreenings = 2; // HARDCODED IN A 2 SCREENING NUMBER HERE;  // { get; set; }

    [SerializeField] private TMP_Dropdown cameraDropdown;
    public int webcamNumSelected = 0;

    [SerializeField] private Button BeginButton;

    //FIX MAC PERMISSIONS FOR WEBCAM

    void Start()
    {
        BeginButton.onClick.AddListener(delegate { BeginTrackingClicked(); });
        cameraDropdown.onValueChanged.AddListener(OnWebcamChanged);
    }

    private void BeginTrackingClicked()
    {
        viewManager.DisplayTrackingScreen();
    }

    private void OnWebcamChanged(int index)
    {
        webcamNumSelected = cameraDropdown.value + 1;
        //Debug.Log($"[{GetType().Name}] webcamNumSelected : " + webcamNumSelected);
        SelectWebcam();
    }

    private void Update()
    {
        BeginButton.interactable = (webcamNumSelected > 0);
    }

    public void ResetSettingsManager()
    {
        PopulateWebcamDeviceList();
        DefaultToFirstWebcam();
        SelectWebcam();
    }

    private void PopulateWebcamDeviceList()
    {
        //Reset
        cameraDropdown.ClearOptions();

        //Add options from device list
        cameraDropdown.AddOptions(cameraManager.deviceNames);

        //Warn if none found
        if (cameraManager.deviceNames.Count == 0)
        {
            viewManager.DisplayWebcamError();
            Debug.LogError("No camera devices found!");
        }

        //Set dropdown to one chosen if set previously
        cameraDropdown.value = webcamNumSelected - 1;

        cameraDropdown.RefreshShownValue();
    }

    private void DefaultToFirstWebcam()
    {
        //If we can, auto-select the first webcam make the Continue button available
        if (cameraManager.cameraDevices.Length > 0)
        {
            if (webcamNumSelected == 0) webcamNumSelected = 1;
        }       
    }

    private void SelectWebcam()
    {
        if (webcamNumSelected > 0)
        {
            Debug.Log($"[{GetType().Name}] SelectWebcam : Webcam selected :  " + webcamNumSelected);
            cameraManager.SetAndPlayWebcam();
        }
    }

}
