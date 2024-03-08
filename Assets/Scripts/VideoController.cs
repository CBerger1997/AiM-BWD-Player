using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.IO;
using System.Collections;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

public delegate void NewBSocialData ( BSocialUnity.BSocialPredictions p );

public class VideoController : MonoBehaviour
{
    //Now attached to Video Player GameObject

    //Video controls such as play fast forward etc

    [SerializeField] ViewManager viewManager;
    [SerializeField] RawImage WebcamOutput;
    //[SerializeField] Camera videoCamera;
    [SerializeField] RenderTexture videoTexture;
   
    [SerializeField] VideoPlayer[] VideoPlayers;
    //[SerializeField] RawImage[] ExternalVideoImages;

    [SerializeField] SettingsManager settingsManager;
    [SerializeField] SceneOrderManager2 sceneOrderManager;

    public int currentClipCounter;
    public int nextClipCounter;
    private List<int> prevClipCounters;

    private int startFrame;
    private int endFrame;
    public bool isLoadingNextVideo;

    public int currentActivePlayerIndex;

    public VideoClip[] videos;

    //private bool isInactivePaused;
    private WebCamTexture webcamTexture;

    bool isShowingBsocialOverlay;

    bool isCollectingBaseline;
    bool isWaitingBaseline;
    bool isWaitingPrediction;
    bool isCollectPredictionPerSecond;

    bool isFirstScene;

    int baselineCounter;
    float arousalBaseline;
    float valenceBaseline;
    int currentPredictionCounter;
    float currentValenceAverage;
    float currentArousalAverage;

    float initialValenceBaseline;
    float initialArousalBaseline;
    int testTrackingSecs = 15;

    int currentSceneIndex;


    #region BSocial

    
    public static event NewBSocialData EvNewBSocialData;
    private Thread BSocialThread;
    private bool BSocialThreadIsFree = true;
    private bool BSocialOK = false;
    private string BSocialLicenceKeyPath;
    public Color32[] textureData;
    public Texture2D txBuffer;

    private int camWidth = 1280;
    private int camHeight = 720;

    /*
     * BSocial SDK v1.4.0 Copyright BlueSkeye AI LTD.
     * For Academic Use Only
     * Original Setup Code Copyright Timur Almaev, Chief Engineer
     * This Setup Code Copyright Luke Rose, Automotive Engineering Lead
     */
    private bool BSocial_Init()
    {

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
            Debug.Log($"[{GetType().Name}] Start: HOORAY!!!! - BSocialWrapper_load_online_license_key() SUCCESS!");
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
            Debug.Log($"[{GetType().Name}] Start: HOORAY!!!! - BSocialWrapper_init_embedded() SUCCESS!");
        }

        BSocialUnity.BSocialWrapper_set_nthreads(4); // Change for optimal performance, BSocial needs at least 10FPS, 15FPS+ preferred
        BSocialUnity.BSocialWrapper_reset();

        //Get webcam dimensions
        /*WebCamDevice[] devices = WebCamTexture.devices;
        if (devices.Length > 0)
        {
            WebCamDevice selectedDevice = devices[settingsManager.webcamNumSelected]; // Select this right one !!!!! eg. may not be 0
            WebCamTexture tempTexture = new WebCamTexture(settingsManager.webcam.name);
            camWidth = tempTexture.width;
            camHeight = tempTexture.height;
            Debug.Log($"[{GetType().Name}] BSocial_Init - WebcamDevice - Dimensions: " + camWidth+", x "+camHeight);

        }
        else
        {
            Debug.Log($"[{GetType().Name}] BSocial_Init - WebcamDevice - Using Default Diemnsions: " + camWidth + ", x " + camHeight);
        }*/

        webcamTexture = new WebCamTexture ( settingsManager.webcam.name, camWidth, camHeight, 30 );


        //WebcamOutput.texture = webcamTexture;

        webcamTexture.Play ();

        if ( webcamTexture.isPlaying )
        {
            Debug.Log($"[{GetType().Name}] BSocial_Init - Using webcam: {webcamTexture.name}" );
        }

        isShowingBsocialOverlay = true;

        return BSocialOK;
    }

    
    private void BSocial_UpdateOverlay ()
    {
        //Debug.Log($"[{GetType().Name}] BSocial overlay updated...");

        if ( !( BSocialOK && BSocialThreadIsFree && webcamTexture.isPlaying ) )
            return;

        //PRESUMABLY THIS IS THE WEBCAM BEING ANALYSED BY BLUESKEYES
        if ( webcamTexture.width != 1280 )
        {
            Debug.Log($"[{GetType().Name}] BSocial_UpdateOverlay - NEW WEBCAM TEXTURE DIMENSIONS SET" );
            return;
        }

        if ( webcamTexture.height != 720 )
        {
            Debug.Log($"[{GetType().Name}] BSocial_UpdateOverlay - NEW WEBCAM TEXTURE DIMENSIONS SET" );
            return;
        }

        textureData = new Color32[ camWidth * camHeight ];

        webcamTexture.GetPixels32 ( textureData );

        BSocialUnity.BSocialWrapper_set_image_native (
        ref textureData, webcamTexture.width, webcamTexture.height, true, false, BSocialUnity.BSocialWrapper_Rotation.BM_NO_ROTATION );

        BSocialUnity.BSocialWrapper_overlay_native (
        ref textureData, webcamTexture.width, webcamTexture.height, true, false, BSocialUnity.BSocialWrapper_Rotation.BM_NO_ROTATION );

        //Presumabnly this starts the analysis?
        BSocialThread = new Thread (BSocial_GetPredictions);
        BSocialThreadIsFree = false;
        BSocialThread.Start ();

        
        //PRESUMABLY THIS IS THE OVERLAY FROM BLUESKEYES
        if ( !txBuffer )
            txBuffer = new Texture2D (camWidth, camHeight);

        WebcamOutput.texture = BSocialUnity.OverlayTexture;
        

        txBuffer.name = $"BSocial Webcam Capture";
        txBuffer.SetPixels32 ( textureData );
        txBuffer.Apply ();
        
    }

    private void BSocial_GetPredictions ()
    {
        //Debug.Log($"[{GetType().Name}] BSocial_GetPredictions...");

        BSocialUnity.BSocialWrapper_run ();

        BSocialUnity.BSocialPredictions predictions = BSocialUnity.BSocialWrapper_get_predictions ();

        GatherValenceArousalValues( predictions );

        //trigger any registered events
        EvNewBSocialData?.Invoke ( predictions );

        // Sleep a little bit and set the signal to get the next frame
        Thread.Sleep ( 1 );

        BSocialThreadIsFree = true;
    }
    
    private void GatherValenceArousalValues( BSocialUnity.BSocialPredictions prediction)
    {
        //Debug.Log($"[{GetType().Name}] GatherValenceArousalValues. Valence : " + prediction.affect.valence +", Arousal : "+ prediction.affect.arousal);

        initialValenceBaseline += prediction.affect.valence;
        initialArousalBaseline += prediction.affect.arousal;

        if (isCollectingBaseline && baselineCounter < 15 && !isWaitingBaseline)
        {
            Debug.Log($"[{GetType().Name}] Gathering Valence & Arousal Values - VIDEO PREDICTION BASELINE DATA GATHERED");
            valenceBaseline += prediction.affect.valence;
            arousalBaseline += prediction.affect.arousal;
        }
        else if (isCollectPredictionPerSecond && !isWaitingPrediction)
        {
            Debug.Log($"[{GetType().Name}] Gathering Valence & Arousal Values - VIDEO PREDICTION VALENCE AROUSAL DATA GATHERED");
            currentValenceAverage += prediction.affect.valence;
            currentArousalAverage += prediction.affect.arousal;
            currentPredictionCounter++;
        }
    }

    #endregion

    private void Awake ()
    {
        ResetValues();
        ResetPlayers();        
    }

    private void Start()
    {
        sceneOrderManager = new SceneOrderManager2(settingsManager.numOfScreenings);

        foreach (VideoPlayer vp in VideoPlayers)
        {
            vp.gameObject.SetActive(false);
        }
    }

    void Update ()
    {

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
        // DEBUG ONLY
        if (currentClipCounter > 0 && VideoPlayers[currentActivePlayerIndex].isPlaying && Input.GetKeyDown("n"))
        {
            VideoPlayers[currentActivePlayerIndex].frame = (endFrame - 300);

            Debug.Log("N key pressed - Skip towards end of video");
        }
#endif
        

        if ( isShowingBsocialOverlay && isFirstScene )
        {
            BSocial_UpdateOverlay();
            //Debug.Log($"[{GetType().Name}] Bsocial - Stage 1...");
        }

        if ( isCollectingBaseline && !isWaitingBaseline )
        {
            Debug.Log($"[{GetType().Name}] Bsocial - Stage 2...");
            Debug.Log($"[{GetType().Name}] base update" );
            StartCoroutine ( WaitForSecondBaselinePrediction () );
        }
        else if ( isCollectPredictionPerSecond && !isWaitingPrediction )
        {
            Debug.Log($"[{GetType().Name}] Bsocial - Stage 3...");
            Debug.Log($"[{GetType().Name}] predict update" );
            StartCoroutine ( WaitForSecondPrediction () );

            if ( ( long ) VideoPlayers[ currentActivePlayerIndex ].frame >= ( endFrame - 288 ) )
            {
                Debug.Log($"[{GetType().Name}] End of scene 0" );

                isCollectPredictionPerSecond = false;

                currentArousalAverage = currentArousalAverage / currentPredictionCounter;
                currentValenceAverage = currentValenceAverage / currentPredictionCounter;

                Debug.Log($"[{GetType().Name}] sceneOrderManager : "+ sceneOrderManager+", currentArousalAverage: " + currentArousalAverage + ", currentValenceAverage: "+ currentValenceAverage +", "+ currentPredictionCounter);
                sceneOrderManager.SetValenceArousalValues ( currentValenceAverage > valenceBaseline ? true : false, currentArousalAverage > arousalBaseline ? true : false );

                sceneOrderManager.CreateSceneOrder (); //BENN - THIS SEEMS TO CREATE HUGE SCENE ORDERS - POSSIBLY BECAUSE WE ARE FORCING IT'S REPETITION !!!

                int count = 1;

                Debug.Log($"[{GetType().Name}] COUNT: " + sceneOrderManager.currentSceneOrder.Count );
                

                foreach ( Scene scene in sceneOrderManager.currentSceneOrder )
                {
                    //ScreeningOrderText.text += scene.index;
                    Debug.Log($"[{GetType().Name}] Adding scene of index : " + scene.index);

                    if ( count != sceneOrderManager.currentSceneOrder.Count )
                    {
                        //ScreeningOrderText.text += ", ";
                        //Debug.Log($"[{GetType().Name}] , ");
                    }
                    //Debug.Log($"[{GetType().Name}] "+ count + ") >> " + scene.index );
                    count++;
                }

                isFirstScene = false;

                webcamTexture = new WebCamTexture ( settingsManager.webcam.name, camWidth, camHeight, 30 );

                WebcamOutput.texture = webcamTexture;

                webcamTexture.Play ();
            }
        }

        if ( VideoPlayers[ currentActivePlayerIndex ].isPlaying )
        {
            if ( ( ( long ) VideoPlayers[ currentActivePlayerIndex ].frame >= ( endFrame - 168 ) ) && !isLoadingNextVideo )
            {
                //Debug.Log($"[{GetType().Name}] Preparing to play another video at end of this video...");

                //Get the nextVideoCounter value
                GetNextVideoValue ();

                //Get the start frame for next video
                startFrame = GetTimingsForVideoCounter ( nextClipCounter, true );
                
                //Debug.Log($"[{GetType().Name}] Start Frame: " + startFrame.ToString());

                //Preload the next video                
                PreLoadNextVideo ( nextClipCounter );
            }
        }

        if ( VideoPlayers[ 1 ].isPlaying )
        {
            //Debug.Log(VideoPlayers[1].frame);
        }
    }

    public void InitBSocialAndPlayVideos ()
    {
        //Debug.Log($"[{GetType().Name}] OnShow : settingsManager.videoFilePath : " + settingsManager.videoFilePath);

        //videos = Resources.LoadAll<VideoClip> ( settingsManager.videoFilePath ) as VideoClip[];
        videos = Resources.LoadAll<VideoClip>("BWD 2K");//, typeof(VideoClip));

        //Debug.Log($"[{GetType().Name}] Number of video files found : "+ videos.Length);

        //videoCamera.targetDisplay = settingsManager.displayDevice;
        //VideoPlayers[ currentActivePlayerIndex ].targetCamera = videoCamera;

        BSocialOK = BSocial_Init ();

        Invoke("PlayVideosAfterDelay", testTrackingSecs); //Second delay then triggering videos
    }


    void PlayVideosAfterDelay()
    {
        if (initialValenceBaseline != 0 && initialArousalBaseline != 0) //After were sure we are picking up SOME data...
        {
            foreach (VideoPlayer vp in VideoPlayers)
            {
                vp.gameObject.SetActive(true);
            }

            viewManager.VideoView();
            ResetAndLoadFirstVideo(); //Play Videos with initial analysis 
        }
        else
        {
            viewManager.TrackingErrorView();
            Debug.Log("Tracking does not appear to be detecting Valence or Arousal after "+testTrackingSecs+" seconds");
        }
    }

    private void LoadVideo ( int videoVal )
    {
        VideoPlayers[ currentActivePlayerIndex ].clip = videos[ videoVal ];

        VideoPlayers[ currentActivePlayerIndex ].Prepare ();

        endFrame = GetTimingsForVideoCounter ( currentClipCounter, false );

        Debug.Log($"[{GetType().Name}] Loading & Preparing Video : " + videoVal.ToString());// + ", End Frame:"+ endFrame.ToString() + ", Frame Count:" + VideoPlayers[currentActivePlayerIndex].clip.frameCount.ToString());
    }

    private void PreLoadNextVideo ( int videoVal )
    {
        if (currentSceneIndex > sceneOrderManager.currentSceneOrder.Count)
        {
            //DON'T PRELOAD A VIDEO - WEVE WATCHED THE CREDITS. GO BACK TO START SCREEN

            //Debug.Log($"[{GetType().Name}] <b>Coming to the end of the Video - ResetValues</b>");
            ResetValues();

            //AWAKE
            ResetPlayers();

            //ONSHOW
            ResetAndLoadFirstVideo();

            //Reset
            sceneOrderManager.ResetSceneOrderForNextScreening();

            //Return
            viewManager.SettingsView();
        }
        else if ( nextClipCounter < 12 )
        {
            //Set player index
            int nextActiveIndex;
            if (currentActivePlayerIndex == 0)
            {
                nextActiveIndex = 1;
            }
            else
            {
                nextActiveIndex = 0;
            }
            Debug.Log($"[{GetType().Name}] Prepare Video Player " + (nextActiveIndex + 1) + " for use");

            //Set video loading to true
            isLoadingNextVideo = true;

            //Prepare the next video a few seconds before
            VideoPlayers[ nextActiveIndex ].clip = videos[ nextClipCounter ];
            VideoPlayers[ nextActiveIndex ].Prepare ();

            //Once the overlap time has ended, swap the video players
            StartCoroutine ( VideoIsEnding () );
        }
    }

    IEnumerator VideoIsEnding()
    {
        Debug.Log($"[{GetType().Name}] VideoIsEnding...");

        bool waiting = true;

        int nextActiveIndex = 0;

        if ( currentActivePlayerIndex == 0 )
        {
            nextActiveIndex = 1;
        }
        else
        {
            nextActiveIndex = 0;
        }
        //Debug.Log($"[{GetType().Name}] Check For End Of Video - nextActiveIndex : " + nextActiveIndex);

        while ( waiting )
        {
            //Start playing next video
            if ( VideoPlayers[ currentActivePlayerIndex ].frame >= endFrame - startFrame && !VideoPlayers[ nextActiveIndex ].isPlaying )
            {
                VideoPlayers[ nextActiveIndex ].Play ();
                Debug.Log($"[{GetType().Name}] VideoIsEnding - Preload :" +VideoPlayers[nextActiveIndex].name);
            }

            //Once the overlap time has ended or the video stopped playing, swap the video players
            if ( VideoPlayers[ currentActivePlayerIndex ].frame >= endFrame || !VideoPlayers[ currentActivePlayerIndex ].isPlaying )
            {
                VideoPlayers[ currentActivePlayerIndex ].gameObject.GetComponent<RawImage> ().enabled = false;
                VideoPlayers[ nextActiveIndex ].gameObject.GetComponent<RawImage> ().enabled = true;

                //ExternalVideoImages[ currentActivePlayerIndex ].enabled = false;
                //ExternalVideoImages[ nextActiveIndex ].enabled = true;

                endFrame = GetTimingsForVideoCounter ( nextClipCounter, false );

                prevClipCounters.Add ( currentClipCounter );
                currentActivePlayerIndex = nextActiveIndex;

                currentClipCounter = nextClipCounter;


                //Debug.Log($"[{GetType().Name}] CheckForEndOfVideo - End Frame : " + endFrame.ToString() + " of Frame Count : " + VideoPlayers[currentActivePlayerIndex].clip.frameCount.ToString());

                string orderStr = ""; int count = 0;
                foreach (Scene s in sceneOrderManager.currentSceneOrder) { orderStr += s.index; count++; if (count < sceneOrderManager.currentSceneOrder.Count) orderStr += ","; }
                Debug.Log($"[{GetType().Name}] VideoIsEnding - Scene Order : " +orderStr+" ("+ sceneOrderManager.currentSceneOrder.Count + " total)");
                if (sceneOrderManager.currentSceneOrder.Count == 0) Debug.LogError($"[{GetType().Name}] VideoIsEnding - currentSceneOrder.Count == 0. NO SCENES ORDERED!");

                Debug.Log($"[{GetType().Name}] VideoIsEnding - <b>Current Video : " + currentClipCounter.ToString()+"</b>");

                if ( currentSceneIndex < sceneOrderManager.currentSceneOrder.Count )
                {
                    Debug.Log($"[{GetType().Name}] VideoIsEnding- Next Video : " + sceneOrderManager.currentSceneOrder[currentSceneIndex].index.ToString());
                }

                isLoadingNextVideo = false;
                waiting = false;
            }

            yield return new WaitForEndOfFrame ();
        }
    }


    #region VideoLogic
    
    IEnumerator WaitForSecondBaselinePrediction ()
    {
        isWaitingBaseline = true;
        Debug.Log($"[{GetType().Name}] Wait For Second Baseline Prediction...");
        yield return new WaitForSeconds ( 1 );

        isWaitingBaseline = false;
        baselineCounter++;
        Debug.Log($"[{GetType().Name}] Wait For Second Baseline Prediction - baselineCounter : "+ baselineCounter);

        if ( baselineCounter == 15 )
        {
            arousalBaseline = arousalBaseline / 15;
            valenceBaseline = valenceBaseline / 15;
            isCollectingBaseline = false;
            isCollectPredictionPerSecond = true;
            Debug.Log($"[{GetType().Name}] Second Baseline Prediction Complete");
        }
    }

    IEnumerator WaitForSecondPrediction ()
    {
        isWaitingPrediction = true;
        Debug.Log($"[{GetType().Name}] Wait For Second Prediction...");
        yield return new WaitForSeconds ( 1 );
        isWaitingPrediction = false;
        Debug.Log($"[{GetType().Name}] Wait For Second Prediction - nearly complete...");
    }

    private void GetNextVideoValue ()
    {
        if ( currentSceneIndex != sceneOrderManager.currentSceneOrder.Count )
        {
            nextClipCounter = sceneOrderManager.currentSceneOrder[ currentSceneIndex ].index;
            Debug.Log($"[{GetType().Name}] Next Video : " + nextClipCounter.ToString ());
        }
        currentSceneIndex++;
    }

    private int GetTimingsForVideoCounter ( int val, bool isStart )
    {
        int frameVal = 0;

        switch ( val )
        {
            case 0:
                if ( isStart )
                {
                    frameVal = 0;
                }
                else
                {
                    frameVal = 1320;
                }
                break;
            case 1:
                if ( isStart )
                {
                    frameVal = 149;
                }
                else
                {
                    frameVal = 6675;
                }
                break;
            case 2:
                if ( isStart )
                {
                    frameVal = 0;
                }
                else
                {
                    frameVal = 2489;
                }
                break;
            case 3:
                if ( isStart )
                {
                    frameVal = 12;
                }
                else
                {
                    frameVal = 3865;
                }
                break;
            case 4:
                if ( isStart )
                {
                    frameVal = 6;
                }
                else
                {
                    frameVal = 2379;
                }
                break;
            case 5:
                if ( isStart )
                {
                    frameVal = 43;
                }
                else
                {
                    frameVal = 2743;
                }
                break;
            case 6:
                if ( isStart )
                {
                    frameVal = 0;
                }
                else
                {
                    frameVal = 3177;
                }
                break;
            case 7:
                if ( isStart )
                {
                    frameVal = 44;
                }
                else
                {
                    frameVal = 1525;
                }
                break;
            case 8:
                if ( isStart )
                {
                    frameVal = 65;
                }
                else
                {
                    frameVal = 2097;
                }
                break;
            case 9:
                if ( isStart )
                {
                    frameVal = 3;
                }
                else
                {
                    frameVal = 2806;
                }
                break;
            case 10:
                if ( isStart )
                {
                    frameVal = 63;
                }
                else
                {
                    //frameVal = 5104; Changed as video shorter than this frame count
                    frameVal = 4968;
                }
                break;
            case 11:
                if ( isStart )
                {
                    frameVal = 0;
                }
                else
                {
                    frameVal = 621;
                }
                break;
            case 12:
                Debug.Log ( "End Reached" );
                break;
            default:
                Debug.LogError ( "Video counter is outside the video range" );
                break;
        }

        return frameVal;
    }

    #endregion



    public void OnPlayClicked (VideoPlayer source)
    {
        foreach ( VideoPlayer player in VideoPlayers )
        {
            if (player.clip) Debug.Log($"[{GetType().Name}] Play " + player.name + " : clip " + player.clip);

            if ( player.isPrepared && ( player == VideoPlayers[ currentActivePlayerIndex ]))// || isInactivePaused ) )
            {
                //Debug.Log($"[{GetType().Name}] Player isPrepared. isInactivePaused? : " + isInactivePaused);

                if ( player == VideoPlayers[ currentActivePlayerIndex == 0 ? 1 : 0 ] )
                {
                    //isInactivePaused = false;
                    Debug.Log($"[{GetType().Name}] Active Player : " + currentActivePlayerIndex);// + ", isInactivePaused? : " + isInactivePaused);
                }

                if ( currentClipCounter == 0 )
                {
                    if ( baselineCounter < 15 )
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
            }

            player.playbackSpeed = 1;

            player.Play ();
        }
    }





    private void ResetValues ()
    {
        isShowingBsocialOverlay = false;

        isCollectingBaseline = false;
        isWaitingBaseline = false;
        isWaitingPrediction = false;
        isCollectPredictionPerSecond = false;

        isFirstScene = true;

        baselineCounter = 0;
        arousalBaseline = 0;
        valenceBaseline = 0;
        currentPredictionCounter = 0;
        currentValenceAverage = 0;
        currentArousalAverage = 0;

        currentSceneIndex = 0;

        isLoadingNextVideo = false;
        //isInactivePaused = false;
    }

    private void ResetPlayers()
    {
        prevClipCounters = new List<int>();

        currentActivePlayerIndex = 0;

        VideoPlayers[0].gameObject.GetComponent<RawImage>().enabled = true;
        VideoPlayers[1].gameObject.GetComponent<RawImage>().enabled = false;

        VideoPlayers[0].gameObject.GetComponent<VideoPlayer>().clip = null;
        VideoPlayers[1].gameObject.GetComponent<VideoPlayer>().clip = null;

        //ExternalVideoImages[ 0 ].enabled = true;
        //ExternalVideoImages[ 1 ].enabled = false;
    }

    private void ResetAndLoadFirstVideo()
    {
        currentClipCounter = 0;
        LoadVideo(0);
        VideoPlayers[currentActivePlayerIndex].prepareCompleted += OnPlayClicked;
    }

    private void OnApplicationQuit ()
    {
        Debug.Log ( "Quitting..." );

        /* Deallocate memory taken by B-Social if it was init-d */

        if ( BSocialOK )
        {
            //BSocialUnity.BSocialWrapper_release();
        }
    }
}
