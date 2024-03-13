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


    public void SetAndPlayWebcam()
    {
        //Debug.Log($"[{GetType().Name}] SetWebcam...");

        
        //Init camera
        if (webcamTexture != null)
        {
            webcamTexture.Stop();
            //Destroy(webcamTexture);
        }

        //webcamTexture = new WebCamTexture();
        //webcamTexture.requestedFPS = wTextureRequestedFPS;

        
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

        if (webcamTexture.isReadable)
        {
            webcamTexture.Play();
            
            Debug.Log($"[{GetType().Name}] SetWebcam : Play Webcam : " + webcamTexture.name +", Dimensions : " + webcamTexture.width +", "+ webcamTexture.height);

            if (webcamTexture.isPlaying)
            {
                StartCoroutine(WaitAndScaleGameObject(webcamTexture));

                Debug.Log($"[{GetType().Name}] SetWebcam - webcam: {webcamTexture.name} is playing.");
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

        //NEEDS FINISHING

        if (displayWidth / aspectRatio > displayHeight)
        {
            newWidth = (float)displayHeight * aspectRatio;
            newHeight = (float)displayHeight;
            Debug.Log($"[{GetType().Name}] WaitAndScaleGameObject - Increase Width");
        }
        else
        {
            newWidth = (float)displayWidth;
            newHeight = (float)displayWidth / aspectRatio;
            Debug.Log($"[{GetType().Name}] WaitAndScaleGameObject - Reduce Height");
        }

        

        Vector2 newSizeDelta = new Vector2 (newWidth, newHeight);

        Debug.Log($"[{GetType().Name}] WaitAndScaleGameObject - Set new dimensions : {newWidth} x {newHeight}");

        webcamTextureDisplay.GetComponent<RectTransform>().sizeDelta = newSizeDelta;


        //WARNING CAMERA TEXTURE
        if (cameraManager.webcamTexture.width != cameraManager.textureRequestedWidth)
        {
            Debug.LogWarning($"[{GetType().Name}] WaitAndScaleGameObject - WRONG WEBCAM TEXTURE DIMENSIONS");
            return;
        }

        if (cameraManager.webcamTexture.height != cameraManager.textureRequestedHeight)
        {
            Debug.LogWarning($"[{GetType().Name}] WaitAndScaleGameObject - WRONG WEBCAM TEXTURE DIMENSIONS");
            return;
        }
    }

}
