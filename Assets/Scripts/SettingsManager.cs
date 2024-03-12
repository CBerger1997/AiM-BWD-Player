using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Windows.WebCam;

public class SettingsManager : MonoBehaviour 
{
    //Configures web cam, video resolution etc, num of screenings

    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private ViewManager viewManager;

    /*public enum AnalysisOptions {
        Valence_Baseline,
        Arousal_Baseline,
        Arousal_Over_Valence,
        Valence_Over_Arousal
    }*/

    /*public enum ResolutionOptions {
        _2K
    }*/

    //public AnalysisOptions analysis { get; set; }
    //public ResolutionOptions resolution { get; set; }

    public int numOfScreenings = 2; // HARDCODED IN A 2 SCREENING NUMBER HERE;  // { get; set; }

    [SerializeField] private TMP_Dropdown cameraDropdown;
    public int webcamNumSelected = 0;

    [SerializeField] private Button BeginButton;

    //private int maxScreenings = 10;


    //FIX CORRECT WEBCAM BEING SELECTED 
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
        if (GameObject.Find("Dropdown List")) //Dummy way to set the height, but can't work out to fix otherwise!
        {
            RectTransform myRectTransform = GameObject.Find("Dropdown List").GetComponent<RectTransform>();
            myRectTransform.sizeDelta = new Vector2(myRectTransform.sizeDelta.x, 100);
        }

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

        //Populate the dropdown
        cameraDropdown.AddOptions(cameraManager.deviceNames);

        //Warn if none found
        if (cameraManager.deviceNames.Count == 0)
        {
            viewManager.DisplayWebcamError();
            Debug.LogError("No camera devices found!");
        }

        //Set dropdown to one chosen if set previously
        cameraDropdown.value = webcamNumSelected - 1;
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
