using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraManager : MonoBehaviour
{
    //Manages the webcam

    //Managers
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
    public RawImage webcamTextureDisplay;

    void Start()
    {
        GetWebcamDevices();

        SetAlphaOfWebcamTeture(0); //make ther texture transparent so we can't see it before resizing and applying the webcam,
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


    public void SetAndPlayWebcam()
    {
        GetWebcamDevices();

        //Debug.Log($"[{GetType().Name}] SetWebcam...");


        //Init camera
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
            //Destroy(webcamTexture);
        }

        //Application.RequestUserAuthorization(UserAuthorization.WebCam);

        webcamTexture = new WebCamTexture(
            cameraDevices[settingsManager.webcamNumSelected - 1].name,
            textureRequestedWidth,
            textureRequestedHeight,
            wTextureRequestedFPS
        );

        //Debug.Log($"[{GetType().Name}] SetWebcam : created new webcam texture");

        webcamTexture.name = cameraDevices[settingsManager.webcamNumSelected - 1].name;

        if (webcamTexture.isReadable)
        {
            webcamTexture.Play();

            //Debug.Log($"[{GetType().Name}] SetWebcam : Play Webcam : " + webcamTexture.name +", Dimensions : " + webcamTexture.width +", "+ webcamTexture.height);

            if (webcamTexture.isPlaying)
            {
                StartCoroutine(WaitAndScaleGameObject(webcamTexture));

                //Debug.Log($"[{GetType().Name}] SetWebcam - webcam: {webcamTexture.name} is playing.");
            }
        }
    }


    private System.Collections.IEnumerator WaitAndScaleGameObject(WebCamTexture webcamTexture)
    {
        yield return new WaitForSeconds(2f);

        Debug.Log($"[{GetType().Name}] WaitAndScaleGameObject : Webcam Dimensions (After delay) : " + webcamTexture.width + ", " + webcamTexture.height);


        float aspectRatio = (float)webcamTexture.width / (float)webcamTexture.height;

        Debug.Log($"[{GetType().Name}] WaitAndScaleGameObject - aspectRatio: {aspectRatio}");

        float newWidth;
        float newHeight;


        if (displayWidth / aspectRatio > displayHeight)
        {
            newWidth = (float)displayHeight * aspectRatio;
            newHeight = (float)displayHeight;
        }
        else
        {
            newWidth = (float)displayWidth;
            newHeight = (float)displayWidth / aspectRatio;
        }



        Vector2 newSizeDelta = new Vector2(newWidth, newHeight);

        //CAMERA SIZE SET HERE SO THAT THE Blueskyeyes TRACKING INSTATIATES AN OVERLAY WITH THE CORRECT DIMENSIONS - NEEDS TESTING (SEEMS OK ATM)
        cameraWidth = (int)webcamTexture.width;
        cameraHeight = (int)webcamTexture.height;
        //

        Debug.Log($"[{GetType().Name}] WaitAndScaleGameObject - Set new dimensions : {newWidth} x {newHeight}");

        webcamTextureDisplay.GetComponent<RectTransform>().sizeDelta = newSizeDelta;


        //WARNING CAMERA TEXTURE
        if (webcamTexture.width != textureRequestedWidth)
        {
            Debug.LogWarning($"[{GetType().Name}] WaitAndScaleGameObject - UNUSUAL WEBCAM TEXTURE DIMENSIONS");
        }

        if (webcamTexture.height != textureRequestedHeight)
        {
            Debug.LogWarning($"[{GetType().Name}] WaitAndScaleGameObject - UNUSUAL WEBCAM TEXTURE DIMENSIONS");
        }


        //FINALLY - display the webcam
        SetAlphaOfWebcamTeture(1);
        webcamTextureDisplay.texture = webcamTexture;//put the texture into the object
    }



    void SetAlphaOfWebcamTeture(float f)
    {
        Color webcamTextureDisplayColour = webcamTextureDisplay.color;
        webcamTextureDisplayColour.a = f;
        webcamTextureDisplay.color = webcamTextureDisplayColour;
    }

    /*private void OnApplicationQuit()
    {
        Destroy(webcamTexture);
    }*/
}
