using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public delegate void NewBSocialData(BSocialUnity.BSocialPredictions p);

/*
     * BSocial SDK v1.4.0 Copyright BlueSkeye AI LTD.
     * For Academic Use Only
     * Original Setup Code Copyright Timur Almaev, Chief Engineer
     * This Setup Code Copyright Luke Rose, Automotive Engineering Lead
     */


public class TrackingManager : MonoBehaviour
{
    //Manages facial tracking and Bluesyeye BSocial

    //Managers
    [SerializeField] private CameraManager cameraManager;
    [SerializeField] private SceneOrderManager2 sceneOrderManager;
    [SerializeField] private VideoManager videoManager;

    bool isShowingBsocialOverlay;

    bool isCollectingBaseline;
    bool isWaitingBaseline;
    bool isWaitingPrediction;
    bool isCollectPredictionPerSecond;

    int baselineCounter;
    float arousalBaseline;
    float valenceBaseline;

    int currentPredictionCounter;
    float currentValenceAverage;
    float currentArousalAverage;

    float initialValenceBaseline;
    float initialArousalBaseline;

    public static event NewBSocialData EvNewBSocialData;
    private Thread BSocialThread;
    private bool BSocialThreadIsFree = true;
    private bool BSocialOK = false;
    private string BSocialLicenceKeyPath;
    public Color32[] textureData;
    public Texture2D txBuffer;

    [SerializeField] RawImage trackingOverlay;

    private void Awake()
    {
        ResetTrackingManager();
    }

    public void ResetTrackingManager()
    {
        //Reset Bsocial
        isShowingBsocialOverlay = false;

        isCollectingBaseline = false;
        isWaitingBaseline = false;
        isWaitingPrediction = false;
        isCollectPredictionPerSecond = false;

        baselineCounter = 0;
        arousalBaseline = 0;
        valenceBaseline = 0;
        currentPredictionCounter = 0;
        currentValenceAverage = 0;
        currentArousalAverage = 0;
    }

    void Update()
    {
        if (isShowingBsocialOverlay && sceneOrderManager.isFirstScene)
        {
            UpdateOverlay();
        }

        if (isCollectingBaseline && !isWaitingBaseline)
        {
            //Debug.Log($"[{GetType().Name}] Bsocial - base update");
            StartCoroutine(WaitForSecondBaselinePrediction());
        }
        else if (isCollectPredictionPerSecond && !isWaitingPrediction)
        {
            //Debug.Log($"[{GetType().Name}] Bsocial - predict update");
            StartCoroutine(WaitForSecondPrediction());

            if (videoManager.isEndOfFirstScene())
            {
                Debug.Log($"[{GetType().Name}] End of first scene...");

                CompleteAnalysis();
            }
        }
    }

    private void CompleteAnalysis()
    {
        isCollectPredictionPerSecond = false;

        currentArousalAverage = currentArousalAverage / currentPredictionCounter;
        currentValenceAverage = currentValenceAverage / currentPredictionCounter;

        //Generate a new scene order
        sceneOrderManager.GenerateSceneOrder(currentArousalAverage, currentValenceAverage, currentPredictionCounter, arousalBaseline, valenceBaseline);

        //cameraManager.UpdateWebcam();

        trackingOverlay.texture = cameraManager.webcamTexture;
    }

    public void InitBSocialTesting()
    {
        //Begin
        ResetTrackingManager();

        BSocialOK = Init();        
    }

    public bool TestDataGathered()
    {
        //Simply returns a boolean to say we've got some valence and arousal data captured
        return initialValenceBaseline != 0 && initialArousalBaseline != 0;
    }
   


    private bool Init()
    {
        //Init the main BSocial SDK
        BSocialLicenceKeyPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, "bsocial.lic");

        Debug.Log("B-Social licence key path : " + BSocialLicenceKeyPath);

        BSocialUnity.BSocialWrapper_create();

        int rcode = BSocialUnity.BSocialWrapper_load_online_licence_key(BSocialLicenceKeyPath);

        if (rcode != 0)
        {
            Debug.LogError("Start: ERROR - BSocialWrapper_load_online_license_key() failed");
            return false;
        }
        else
        {
            Debug.Log($"[{GetType().Name}] Start: BSocialWrapper_load_online_license_key() SUCCESS!");
        }

        BSocialUnity.BSocialWrapper_set_body_tracking_enabled(false);

        BSocialUnity.BSocialWrapper_set_min_face_diagonal(100); // Alter this if you have issues with small faces/bounding boxes (min = 1)

        rcode = BSocialUnity.BSocialWrapper_init_embedded(); // Init with embedded/encrypted models (don't need to pass anything in)

        BSocialOK = rcode == 0;

        if (rcode != 0)
        {
            Debug.LogError("Start: ERROR - BSocialWrapper_init_embedded() failed");
            return false;
        }
        else
        {
            Debug.Log($"[{GetType().Name}] Start: BSocialWrapper_init_embedded() SUCCESS!");
        }

        BSocialUnity.BSocialWrapper_set_nthreads(4); // Change for optimal performance, BSocial needs at least 10FPS, 15FPS+ preferred
        BSocialUnity.BSocialWrapper_reset();

        isShowingBsocialOverlay = true;

        return BSocialOK;
    }


    private void UpdateOverlay()
    {
        //Debug.Log($"[{GetType().Name}] BSocial overlay updated...");

        if (!(BSocialOK && BSocialThreadIsFree && cameraManager.webcamTexture.isPlaying))
            return;


        //SET UP CAMERA TEXTURE

        /*if (cameraManager.webcamTexture.width != cameraManager.textureRequestedWidth) //IS IT NOT POSSIBLE TO USE TRACKING WITH OTHER SIZES?? SET IT FROM THER CameraManager FOR NOW
        {
            Debug.LogWarning($"[{GetType().Name}] UpdateOverlay - Overlay Denied - UNUSUAL WEBCAM TEXTURE DIMENSIONS SET");
            return;
        }

        if (cameraManager.webcamTexture.height != cameraManager.textureRequestedHeight)
        {
            Debug.LogWarning($"[{GetType().Name}] UpdateOverlay - Overlay Denied - UNUSUAL WEBCAM TEXTURE DIMENSIONS SET");
            return;
        }*/

        textureData = new Color32[cameraManager.cameraWidth * cameraManager.cameraHeight];

        cameraManager.webcamTexture.GetPixels32(textureData); //Set the webcam to show the overlay
        //


        BSocialUnity.BSocialWrapper_set_image_native(ref textureData, cameraManager.webcamTexture.width, cameraManager.webcamTexture.height, true, false, BSocialUnity.BSocialWrapper_Rotation.BM_NO_ROTATION);

        BSocialUnity.BSocialWrapper_overlay_native(ref textureData, cameraManager.webcamTexture.width, cameraManager.webcamTexture.height, true, false, BSocialUnity.BSocialWrapper_Rotation.BM_NO_ROTATION);

        //Presumably this starts the analysis
        BSocialThread = new Thread(GetPredictions);
        BSocialThreadIsFree = false;
        BSocialThread.Start();


        //PRESUMABLY THIS IS THE OVERLAY FROM THE TRACKING
        if (!txBuffer) txBuffer = new Texture2D(cameraManager.cameraWidth, cameraManager.cameraHeight);

        trackingOverlay.texture = BSocialUnity.OverlayTexture;

        txBuffer.name = $"BSocial Webcam Capture";
        txBuffer.SetPixels32(textureData);
        txBuffer.Apply();
    }

    

    private void GetPredictions()
    {
        //Debug.Log($"[{GetType().Name}] BSocial_GetPredictions...");

        BSocialUnity.BSocialWrapper_run();

        BSocialUnity.BSocialPredictions predictions = BSocialUnity.BSocialWrapper_get_predictions();

        GatherValenceArousalValues(predictions);

        //Trigger any registered events
        EvNewBSocialData?.Invoke(predictions);

        // Sleep a little bit and set the signal to get the next frame
        Thread.Sleep(1);

        BSocialThreadIsFree = true;
    }

    private void GatherValenceArousalValues(BSocialUnity.BSocialPredictions prediction)
    {
        //Debug.Log($"[{GetType().Name}] BSocial_GatherValenceArousalValues. Valence : " + prediction.affect.valence +", Arousal : "+ prediction.affect.arousal);

        initialValenceBaseline += prediction.affect.valence;
        initialArousalBaseline += prediction.affect.arousal;

        if (isCollectingBaseline && baselineCounter < 15 && !isWaitingBaseline)
        {
            Debug.Log($"[{GetType().Name}] Gathering Valence & Arousal Values - BASELINE DATA GATHERED");
            valenceBaseline += prediction.affect.valence;
            arousalBaseline += prediction.affect.arousal;
        }
        else if (isCollectPredictionPerSecond && !isWaitingPrediction)
        {
            Debug.Log($"[{GetType().Name}] Gathering Valence & Arousal Values - PREDICTION DATA GATHERED");
            currentValenceAverage += prediction.affect.valence;
            currentArousalAverage += prediction.affect.arousal;
            currentPredictionCounter++;
        }
    }


    public void BeginAnalysis()
    {
        if (baselineCounter < 15)
        {
            isCollectingBaseline = true;  //THIS TRIGGERS THE START OF THE ANALYSIS (?)
             //Debug.Log($"[{GetType().Name}] Collecting Baseline");
        }
        else
        {
            isCollectPredictionPerSecond = true;
            Debug.Log($"[{GetType().Name}] Collecting Prediction Per Second");
        }
    }

    IEnumerator WaitForSecondBaselinePrediction()
    {
        isWaitingBaseline = true;
        //Debug.Log($"[{GetType().Name}] Wait For Second Baseline Prediction...");
        yield return new WaitForSeconds(1);

        isWaitingBaseline = false;
        baselineCounter++;
        //Debug.Log($"[{GetType().Name}] Wait For Second Baseline Prediction - baselineCounter : " + baselineCounter);

        if (baselineCounter == 15)
        {
            arousalBaseline = arousalBaseline / 15;
            valenceBaseline = valenceBaseline / 15;

            isCollectingBaseline = false;
            isCollectPredictionPerSecond = true;

            Debug.Log($"[{GetType().Name}] Second Baseline Prediction Complete");
        }
    }

    IEnumerator WaitForSecondPrediction()
    {
        isWaitingPrediction = true;
        //Debug.Log($"[{GetType().Name}] Wait For Second Prediction...");
        yield return new WaitForSeconds(1);

        isWaitingPrediction = false;
        //Debug.Log($"[{GetType().Name}] Wait For Second Prediction - nearly complete...");
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Quitting...");

        /* Deallocate memory taken by B-Social if it was init-d */

        if (BSocialOK)
        {
            //BSocialUnity.BSocialWrapper_release();
            //              *   *   * SEEM TO HAVE SOME MEMORY LEAK WARNINGS FROM UNITY ON ENDING - DOES THIS NEED RELEASING/TIDY UP ?? *   *   *
        }
    }
}
