using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private SettingsManager settingsManager;

    public readonly int textureRequestedWidth = 1280;
    public readonly int textureRequestedHeight = 720;
    private readonly int wTextureRequestedFPS = 30;

    public int cameraWidth = 1280;
    public int cameraHeight = 720;

    private int displayWidth = 1400;
    private int displayHeight = 700;


    public List<string> deviceNames;
    public WebCamDevice[] cameraDevices;

    public WebCamTexture webcamTexture { get; set; }
    //[SerializeField] private RawImage cameraImage;
    public RawImage webcamTextureDisplay;

    void Start()
    {
        GetWebcamDevices();
    }

    private void GetWebcamDevices()
    {
        deviceNames = new List<string>();

        //Create list of devices
        cameraDevices = WebCamTexture.devices;

        foreach (WebCamDevice device in cameraDevices)
        {
            deviceNames.Add(device.name);
            Debug.Log($"[{GetType().Name}] Webcam available  : " + device.name);
        }
    }

    //public void UpdateWebcam()
    //{
        //Get webcam dimensions
        /*WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            WebCamDevice selectedDevice = devices[settingsManager.webcamNumSelected]; // Select this right one !!!!! eg. may not be 0
            WebCamTexture tempTexture = new WebCamTexture(settingsManager.webcam.name);
            cameraWidth = tempTexture.width;
            cameraHeight = tempTexture.height;
            Debug.Log($"[{GetType().Name}] BSocial_Init - WebcamDevice - Dimensions: " + cameraWidth+", x "+cameraHeight);

        }
        else
        {
            Debug.Log($"[{GetType().Name}] BSocial_Init - WebcamDevice - Using Default Diemnsions: " + cameraWidth + ", x " + cameraHeight);
        }*/

        //cameraTexture = new WebCamTexture(webcam.name, cameraWidth, cameraHeight, 30);


        //trackingOverlay.texture = cameraTexture;

        //cameraTexture.Play();

        //if (cameraTexture.isPlaying)
        //{
        //    Debug.Log($"[{GetType().Name}] InitWebcam - Using webcam: {cameraTexture.name}");
        //}
    //}



    public void SetAndPlayWebcam()
    {
        //Debug.Log($"[{GetType().Name}] SetWebcam...");

        
        //Init camera
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
            //Destroy(webcamTexture);
        }

        webcamTexture = new WebCamTexture();
        webcamTexture.requestedFPS = wTextureRequestedFPS;

        
        //Application.RequestUserAuthorization(UserAuthorization.WebCam);

        webcamTexture = new WebCamTexture(
            cameraDevices[settingsManager.webcamNumSelected - 1].name,
            textureRequestedWidth,
            textureRequestedHeight,
            wTextureRequestedFPS
        );

        //Debug.Log($"[{GetType().Name}] SetWebcam : created new webcam texture");

        webcamTexture.name = cameraDevices[settingsManager.webcamNumSelected - 1].name;

        webcamTextureDisplay.texture = webcamTexture;//put the texture into the object
       

        //Debug.Log($"[{GetType().Name}] SetWebcam : Set image texture to Webcam texture");

        if (webcamTexture.isReadable)
        {
            Debug.Log($"[{GetType().Name}] SetWebcam : Play Webcam : " + webcamTexture.name);

            ScaleGameObject();

            webcamTexture.Play();
            
            Debug.Log($"[{GetType().Name}] SetWebcam : Webcam Dimensions : " + webcamTexture.width +", "+ webcamTexture.height);

            if (webcamTexture.isPlaying)
            {
                

                Debug.Log($"[{GetType().Name}] SetWebcam - webcam: {webcamTexture.name} is playing.");
            }
        }
    }

    
    void ScaleGameObject()
    {
        Debug.Log($"[{GetType().Name}] SetWebcam : Webcam Dimensions (2) : " + webcamTexture.width + ", " + webcamTexture.height);

        float aspectRatio = (float) webcamTexture.width / (float) webcamTexture.height;
        Debug.Log($"[{GetType().Name}] SetWebcam - aspectRatio: {aspectRatio}");

        float xScale = (float) (displayWidth / aspectRatio) / webcamTexture.width;
        float yScale = (float) displayHeight / webcamTexture.height;

        Debug.Log($"[{GetType().Name}] SetWebcam - xScale: {xScale}, yScale : {yScale}");

        Vector2 sizeDelta = new Vector2 (webcamTexture.width * xScale, webcamTexture.height * yScale);

        Debug.Log($"[{GetType().Name}] SetWebcam - Set to size : {sizeDelta.x} x {sizeDelta.y}");

        webcamTextureDisplay.GetComponent<RectTransform>().sizeDelta = sizeDelta;

        // Apply the scale factor to the GameObject's local scale
        /*obj.transform.localScale = new Vector3(
            obj.transform.localScale.x * scaleFactor.x,
            obj.transform.localScale.y * scaleFactor.y,
            obj.transform.localScale.z * scaleFactor.z
        );*/
    }
    
}
