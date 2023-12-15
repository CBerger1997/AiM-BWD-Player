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
    //Now attached to Video Parent GameObject (within Canvas)

    //Video controls such as play fast forward etc

    /*[SerializeField] Button settingsButton;

    [SerializeField] Button playButton;
    [SerializeField] Button pauseButton;
    [SerializeField] Button stopButton;
    [SerializeField] Button BackButton;
    [SerializeField] Button RewindButton;
    [SerializeField] Button NextButton;
    [SerializeField] Button FastForwardButton;
    [SerializeField] Button AudioButton;
    [SerializeField] Button QRButton;
    [SerializeField] Button ResetButton;*/

    [SerializeField] ViewManager viewManager;
    [SerializeField] RawImage WebcamOutput;
    //[SerializeField] Camera videoCamera;
    [SerializeField] RenderTexture videoTexture;
    /*[SerializeField] Slider AudioSlider;
    [SerializeField] TMP_Text AudioValueText;
    [SerializeField] TMP_Text CurrentVideoText;
    [SerializeField] TMP_Text ScreeningOrderText;
    [SerializeField] TMP_Text NextVideoText;
    [SerializeField] TMP_Text StartFrameText;
    [SerializeField] TMP_Text EndFrameText;
    [SerializeField] TMP_Text FrameCountText;*/
    [SerializeField] VideoPlayer[] VideoPlayers;
    //[SerializeField] RawImage[] ExternalVideoImages;
    //[SerializeField] RawImage ExternalQRCodes;
    //[SerializeField] GameObject QRPanel;
    //[SerializeField] Button[] QRButtons;
    //[SerializeField] Texture2D[] QRImages;

    [SerializeField] SettingsManager settingsManager;

    public int curClipCounter;
    public int nextClipCounter;
    private List<int> prevClipCounters;

    private int startFrame;
    private int endFrame;
    public bool isLoadingNextVideo;

    public int currentActivePlayerIndex;

    public VideoClip[] videos;

    private bool isInactivePaused;
    private WebCamTexture webcamTexture;

    bool isShowing = false;
    bool isCollectingBaseline = false;
    bool isWaitingBaseline = false;
    bool isWaitingPrediction = false;
    bool isCollectPredictionPerSecond = false;

    bool isFirstScene = true;

    int baselineCounter = 0;
    float arousalBaseline = 0;
    float valenceBaseline = 0;
    int currentPredictionCounter = 0;
    float currentValenceAverage = 0;
    float currentArousalAverage = 0;

    int currentSceneIndex = 0;

    SceneOrderManager2 sceneOrderManager;

    #region BSocial

    //BENN DISABLED
    //public static event NewBSocialData EvNewBSocialData;
    private Thread BSocialThread;
    private bool BSocialThreadIsFree = true;
    private bool BSocialOK = false;
    private string BSocialLicenceKeyPath;
    public Color32[] textureData;
    public Texture2D txBuffer;

    //BENN DISABLED
    /*
     * BSocial SDK v1.4.0 Copyright BlueSkeye AI LTD.
     * For Academic Use Only
     * Original Setup Code Copyright Timur Almaev, Chief Engineer
     * This Setup Code Copyright Luke Rose, Automotive Engineering Lead
     */
    private bool InitBSocial ()
    {
        /*
        //BENN DISABLED
        BSocialLicenceKeyPath = Path.Combine ( Application.streamingAssetsPath, "bsocial.lic" );

        Debug.Log ( "B-Social licence key path : " + BSocialLicenceKeyPath );

        BSocialUnity.BSocialWrapper_create ();
        int rcode = BSocialUnity.BSocialWrapper_load_online_licence_key ( BSocialLicenceKeyPath );

        if ( rcode != 0 )
        {
            Debug.LogError ( "Start: ERROR - BSocialWrapper_load_online_license_key() failed" );
            return false;
        }
        else
        {
            Debug.Log($"[{GetType().Name}] Start: HOORAY!!!! - BSocialWrapper_load_online_license_key() SUCCESS!");
        }

        BSocialUnity.BSocialWrapper_set_body_tracking_enabled ( false );

        BSocialUnity.BSocialWrapper_set_min_face_diagonal ( 100 ); // Alter this if you have issues with small faces/bounding boxes (min = 1)

        rcode = BSocialUnity.BSocialWrapper_init_embedded (); // Init with embedded/encrypted models (don't need to pass anything in)

        BSocialOK = rcode == 0;

        if ( rcode != 0 )
        {
            Debug.LogError ( "Start: ERROR - BSocialWrapper_init_embedded() failed" );
            return false;
        }
        else
        {
            Debug.Log($"[{GetType().Name}] Start: HOORAY!!!! - BSocialWrapper_init_embedded() SUCCESS!");
        }

        BSocialUnity.BSocialWrapper_set_nthreads ( 4 ); // Change for optimal performance, BSocial needs at least 10FPS, 15FPS+ preferred
        BSocialUnity.BSocialWrapper_reset ();
        */
        webcamTexture = new WebCamTexture ( settingsManager.webcam.name, 1280, 720, 30 );

        WebcamOutput.texture = webcamTexture;

        webcamTexture.Play ();

        if ( webcamTexture.isPlaying )
        {
            Debug.Log($"[{GetType().Name}] Using webcam: {webcamTexture.name}" );
        }

        isShowing = true;

        return true; // BSocialOK; //BENN DISABLED
    }

    
    private void BSocialUpdate ()
    {
        if ( !( BSocialOK && BSocialThreadIsFree && webcamTexture.isPlaying ) )
            return;

        if ( webcamTexture.width != 1280 )
        {
            Debug.Log($"[{GetType().Name}] UNEXPECTED WEBCAM TEXTURE DIMENSIONS" );
            return;
        }

        if ( webcamTexture.height != 720 )
        {
            Debug.Log($"[{GetType().Name}] UNEXPECTED WEBCAM TEXTURE DIMENSIONS" );
            return;
        }

        textureData = new Color32[ 1280 * 720 ];

        webcamTexture.GetPixels32 ( textureData );

        //BSocialUnity.BSocialWrapper_set_image_native (
        //ref textureData, webcamTexture.width, webcamTexture.height, true, false, BSocialUnity.BSocialWrapper_Rotation.BM_NO_ROTATION ); //BENN DISABLED

        //BSocialUnity.BSocialWrapper_overlay_native (
        //ref textureData, webcamTexture.width, webcamTexture.height, true, false, BSocialUnity.BSocialWrapper_Rotation.BM_NO_ROTATION ); //BENN DISABLED

        BSocialThread = new Thread ( BSocialProcessing );
        BSocialThreadIsFree = false;
        BSocialThread.Start ();

        /*
        //BENN DISABLED
        if ( !txBuffer )
            txBuffer = new Texture2D ( 1280, 720 );

        WebcamOutput.texture = BSocialUnity.OverlayTexture;

        txBuffer.name = $"BSocial Webcam Capture";
        txBuffer.SetPixels32 ( textureData );
        txBuffer.Apply ();
        */
    }

    private void BSocialProcessing ()
    {
        //Debug.Log($"[{GetType().Name}] Processing");
        //BSocialUnity.BSocialWrapper_run (); //BENN DISABLED

        //BSocialUnity.BSocialPredictions predictions = BSocialUnity.BSocialWrapper_get_predictions (); //BENN DISABLED

        GatherValenceArousalValues();//  predictions ); //BENN DISABLED

        //trigger any registered events
        //EvNewBSocialData?.Invoke ( predictions ); //BENN DISABLED

        // Sleep a little bit and set the signal to get the next frame
        Thread.Sleep ( 1 );

        BSocialThreadIsFree = true;
    }
    #endregion

    private void Awake ()
    {
        //SetupListeners ();

        //AudioSlider.gameObject.SetActive ( false );

        prevClipCounters = new List<int> ();

        currentActivePlayerIndex = 0;

        VideoPlayers[ 0 ].gameObject.GetComponent<RawImage> ().enabled = true;
        VideoPlayers[ 1 ].gameObject.GetComponent<RawImage> ().enabled = false;

        //ExternalVideoImages[ 0 ].enabled = true;
        //ExternalVideoImages[ 1 ].enabled = false;
        //ExternalQRCodes.enabled = false;

        //QRPanel.gameObject.SetActive ( false );

        //CurrentVideoText.text = "Current Video: 0";
        //NextVideoText.text = "Next Video: -";
        //ScreeningOrderText.text = "Screening Order: -";

        isLoadingNextVideo = false;

        isInactivePaused = false;
    }

    /*private void SetupListeners ()
    {
        settingsButton.onClick.AddListener ( delegate { OnSettingsClicked (); } );
        playButton.onClick.AddListener ( delegate { OnPlayClicked (); } );
        pauseButton.onClick.AddListener ( delegate { OnPauseClicked (); } );
        stopButton.onClick.AddListener ( delegate { OnStopClicked (); } );
        BackButton.onClick.AddListener ( delegate { OnBackClicked (); } );
        RewindButton.onClick.AddListener ( delegate { OnRewindClicked (); } );
        NextButton.onClick.AddListener ( delegate { OnNextClicked (); } );
        FastForwardButton.onClick.AddListener ( delegate { OnFastForwardClicked (); } );
        AudioButton.onClick.AddListener ( delegate { OnAudioClicked (); } );
        AudioSlider.onValueChanged.AddListener ( delegate { OnAudioSliderChanged (); } );
        QRButton.onClick.AddListener ( delegate { OnQROverallButtonClicked (); } );
        ResetButton.onClick.AddListener ( delegate { ResetValuesForNextScreening (); } );

        for ( int i = 0; i < QRButtons.Length; i++ )
        {
            int copy = i;
            QRButtons[ i ].onClick.AddListener ( delegate { OnQRButtonClicked ( copy ); } );
        }
    
    }*/

    void Update ()
    {
        if ( isShowing && isFirstScene )
        {
            //BSocialUpdate (); //BENN DISABLED
            //Debug.Log($"[{GetType().Name}] First Scene is Showing (BSocialUpdate)...");
        }

        if ( isCollectingBaseline && !isWaitingBaseline )
        {
            Debug.Log($"[{GetType().Name}] base update" );
            StartCoroutine ( WaitForSecondBaselinePrediction () );
        }
        else if ( isCollectPredictionPerSecond && !isWaitingPrediction )
        {
            Debug.Log($"[{GetType().Name}] predict update" );
            StartCoroutine ( WaitForSecondPrediction () );

            if ( ( long ) VideoPlayers[ currentActivePlayerIndex ].frame >= ( endFrame - 288 ) )
            {
                Debug.Log($"[{GetType().Name}] End of scene 0" );

                isCollectPredictionPerSecond = false;

                currentArousalAverage = currentArousalAverage / currentPredictionCounter;
                currentValenceAverage = currentValenceAverage / currentPredictionCounter;

                sceneOrderManager.SetValenceArousalValues ( currentValenceAverage > valenceBaseline ? true : false, currentArousalAverage > arousalBaseline ? true : false );

                sceneOrderManager.CreateSceneOrder ();

                int count = 1;

                Debug.Log($"[{GetType().Name}] COUNT: " + sceneOrderManager.currentSceneOrder.Count );

                //ScreeningOrderText.text = "Screening Order: ";
                Debug.Log($"[{GetType().Name}] Screening Order: ");

                foreach ( Scene scene in sceneOrderManager.currentSceneOrder )
                {
                    //ScreeningOrderText.text += scene.index;
                    Debug.Log($"[{GetType().Name}] " + scene.index);

                    if ( count != sceneOrderManager.currentSceneOrder.Count )
                    {
                        //ScreeningOrderText.text += ", ";
                        Debug.Log($"[{GetType().Name}] , ");
                    }
                    Debug.Log($"[{GetType().Name}] "+ count + ") >> " + scene.index );
                    count++;
                }

                isFirstScene = false;

                webcamTexture = new WebCamTexture ( settingsManager.webcam.name, 1280, 720, 30 );

                WebcamOutput.texture = webcamTexture;

                webcamTexture.Play ();
            }
        }

        if ( VideoPlayers[ currentActivePlayerIndex ].isPlaying )
        {
            //Enable / disable buttons
            //playButton.gameObject.SetActive ( false );
            //pauseButton.gameObject.SetActive ( true );
            //stopButton.interactable = true;

            if ( ( ( long ) VideoPlayers[ currentActivePlayerIndex ].frame >= ( endFrame - 168 ) ) && !isLoadingNextVideo )
            {
                //Get the nextVideoCounter value
                GetNextVideoValue ();

                //Get the start frame for next video and set the text
                startFrame = GetTimingsForVideoCounter ( nextClipCounter, true );
                //StartFrameText.text = "Start Frame: " + startFrame.ToString ();
                Debug.Log($"[{GetType().Name}] Start Frame: " + startFrame.ToString());

                //Set video loading to true
                isLoadingNextVideo = true;

                //Preload the next video
                PreLoadNextVideo ( nextClipCounter );
            }
        }
        else if ( VideoPlayers[ currentActivePlayerIndex ].isPaused )
        {
            //Enabel/disable buttons
            //playButton.gameObject.SetActive ( true );
            //pauseButton.gameObject.SetActive ( false );
            //stopButton.interactable = true;
        }
        else
        {
            //Enabel/disable buttons
            //playButton.gameObject.SetActive ( true );
            //pauseButton.gameObject.SetActive ( false );
            //stopButton.interactable = false;
        }

        if ( VideoPlayers[ 1 ].isPlaying )
        {
            //Debug.Log(VideoPlayers[1].frame);
        }

        //BackButton.interactable = curClipCounter == 0 ? false : true;
        //NextButton.interactable = curClipCounter == 11 ? false : true;
    }

    public void OnShow ()
    {
        sceneOrderManager = new SceneOrderManager2 ( settingsManager.numOfScreenings );

        Debug.Log($"[{GetType().Name}] OnShow : settingsManager.videoFilePath : " + settingsManager.videoFilePath);

        //videos = Resources.LoadAll<VideoClip> ( settingsManager.videoFilePath ) as VideoClip[];
        videos = Resources.LoadAll<VideoClip>("BWD 2K");//, typeof(VideoClip)); //BENN - SEEMS TO WORK ON WINDOWS WHEN THE ABOVE DOES NOT...

        Debug.Log($"[{GetType().Name}] OnShow : videos found : "+ videos.Length);
        //currentActivePlayerIndex = 0;//BENN FORCED

        //videoCamera.targetDisplay = settingsManager.displayDevice;
        //VideoPlayers[ currentActivePlayerIndex ].targetCamera = videoCamera;

        //BSocialOK = InitBSocial ();//BENN DISABLED
        InitBSocial(); //BENN TEMP FOR ABOVE

        /*if ( !Display.displays[ settingsManager.displayDevice ].active )
        {
            Display.displays[ settingsManager.displayDevice ].Activate ();
        }*/

        curClipCounter = 0;

        LoadVideo ( curClipCounter );
    }

    private void LoadVideo ( int videoVal )
    {
        VideoPlayers[ currentActivePlayerIndex ].clip = videos[ videoVal ];

        VideoPlayers[ currentActivePlayerIndex ].Prepare ();

        //VideoPlayers[ currentActivePlayerIndex ].SetDirectAudioVolume ( 0, AudioSlider.value );
        //VideoPlayers[ currentActivePlayerIndex ].SetDirectAudioVolume ( 1, AudioSlider.value );

        //CurrentVideoText.text = "Current Video: " + videoVal.ToString ();

        endFrame = GetTimingsForVideoCounter ( curClipCounter, false );

        //EndFrameText.text = "End Frame: " + endFrame.ToString ();
        //FrameCountText.text = "Frame Count: " + VideoPlayers[ currentActivePlayerIndex ].clip.frameCount.ToString ();

        Debug.Log($"[{GetType().Name}] LoadVideo : Current Video: " + videoVal.ToString() + ", End Frame:"+ endFrame.ToString() + ", Frame Count:" + VideoPlayers[currentActivePlayerIndex].clip.frameCount.ToString());
    }

    private void PreLoadNextVideo ( int videoVal )
    {
        int nextActiveIndex;

        if ( currentActivePlayerIndex == 0 )
        {
            nextActiveIndex = 1;
        }
        else
        {
            nextActiveIndex = 0;
        }

        if ( nextClipCounter < 12 )
        {
            //Prepare the next video a few seconds before
            VideoPlayers[ nextActiveIndex ].clip = videos[ nextClipCounter ];
            VideoPlayers[ nextActiveIndex ].Prepare ();

            //Once the overlap time has ended, swap the video players
            StartCoroutine ( CheckForEndOfVideo () );
        }
    }

    IEnumerator CheckForEndOfVideo ()
    {
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
        while ( waiting )
        {
            //Start playing next video
            if ( VideoPlayers[ currentActivePlayerIndex ].frame >= endFrame - startFrame && !VideoPlayers[ nextActiveIndex ].isPlaying )
            {
                VideoPlayers[ nextActiveIndex ].Play ();
                Debug.Log($"[{GetType().Name}] Start playing next video...");
            }

            //Once the overlap time has ended or the video stopped playing, swap the video players
            if ( VideoPlayers[ currentActivePlayerIndex ].frame >= endFrame || !VideoPlayers[ currentActivePlayerIndex ].isPlaying )
            {
                VideoPlayers[ currentActivePlayerIndex ].gameObject.GetComponent<RawImage> ().enabled = false;
                VideoPlayers[ nextActiveIndex ].gameObject.GetComponent<RawImage> ().enabled = true;

                //ExternalVideoImages[ currentActivePlayerIndex ].enabled = false;
                //ExternalVideoImages[ nextActiveIndex ].enabled = true;

                endFrame = GetTimingsForVideoCounter ( nextClipCounter, false );
                //EndFrameText.text = "End Frame: " + endFrame.ToString ();
                Debug.Log($"[{GetType().Name}] ...End Frame: " + endFrame.ToString());

                prevClipCounters.Add ( curClipCounter );
                currentActivePlayerIndex = nextActiveIndex;

                curClipCounter = nextClipCounter;

                //FrameCountText.text = "Frame Count: " + VideoPlayers[ currentActivePlayerIndex ].clip.frameCount.ToString ();
                Debug.Log($"[{GetType().Name}] ...Frame Count: " + VideoPlayers[currentActivePlayerIndex].clip.frameCount.ToString());

                //CurrentVideoText.text = "Current Video: " + curClipCounter.ToString ();
                Debug.Log($"[{GetType().Name}] ...Current Video: " + curClipCounter.ToString());

                Debug.Log($"[{GetType().Name}] ...currentSceneOrder.Count: " + sceneOrderManager.currentSceneOrder.Count);

                if ( currentSceneIndex < sceneOrderManager.currentSceneOrder.Count )
                {
                    //NextVideoText.text = "Next Video: " + sceneOrderManager.currentSceneOrder[ currentSceneIndex ].index.ToString ();
                    Debug.Log($"[{GetType().Name}] ...Next Video: " + sceneOrderManager.currentSceneOrder[currentSceneIndex].index.ToString());
                }

                isLoadingNextVideo = false;
                waiting = false;
            }

            yield return new WaitForEndOfFrame ();
        }
    }

    #region VideoLogic

    
    private void GatherValenceArousalValues ( )// BSocialUnity.BSocialPredictions prediction) //BENN DISABLED
    {
        if ( isCollectingBaseline && baselineCounter < 15 && !isWaitingBaseline )
        {
            Debug.Log($"[{GetType().Name}] VIDEO PREDICTION BASELINE" );
            valenceBaseline += 0.1f;//  prediction.affect.valence; //BENN DISABLED
            arousalBaseline += 0.15f;//  prediction.affect.arousal; //BENN DISABLED
        }
        else if ( isCollectPredictionPerSecond && !isWaitingPrediction )
        {
            Debug.Log($"[{GetType().Name}] VIDEO PREDICTION VALENCE AROUSAL" );
            currentValenceAverage += 0.1f;//  prediction.affect.valence; //BENN DISABLED
            currentArousalAverage += 0.105f;//  prediction.affect.arousal; //BENN DISABLED
            currentPredictionCounter++;
        }
    }

    
    IEnumerator WaitForSecondBaselinePrediction ()
    {
        isWaitingBaseline = true;
        yield return new WaitForSeconds ( 1 );
        isWaitingBaseline = false;
        baselineCounter++;
        if ( baselineCounter == 15 )
        {
            arousalBaseline = arousalBaseline / 15;
            valenceBaseline = valenceBaseline / 15;
            isCollectingBaseline = false;
            isCollectPredictionPerSecond = true;
            Debug.Log($"[{GetType().Name}] WaitForSecondBaselinePrediction complete");
        }
    }

    IEnumerator WaitForSecondPrediction ()
    {
        isWaitingPrediction = true;
        yield return new WaitForSeconds ( 1 );
        isWaitingPrediction = false;
        Debug.Log($"[{GetType().Name}] WaitForSecondPrediction complete");
    }

    private void GetNextVideoValue ()
    {
        if ( currentSceneIndex != sceneOrderManager.currentSceneOrder.Count )
        {
            nextClipCounter = sceneOrderManager.currentSceneOrder[ currentSceneIndex ].index;
            currentSceneIndex++;

            //NextVideoText.text = "Next Video: " + nextClipCounter.ToString ();
            Debug.Log($"[{GetType().Name}] Next Video: " + nextClipCounter.ToString ());
        }
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

    #region UI LISTENER FUNCTIONS

    public void OnPlayClicked ()
    {
        foreach ( VideoPlayer player in VideoPlayers )
        {
            Debug.Log($"[{GetType().Name}] Player " + player.name +"...");

            if ( player.isPrepared && ( player == VideoPlayers[ currentActivePlayerIndex ] || isInactivePaused ) )
            {
                Debug.Log($"[{GetType().Name}] Player isPrepared");
                Debug.Log($"[{GetType().Name}] Player isInactivePaused (1) : "+ isInactivePaused);

                if ( player == VideoPlayers[ currentActivePlayerIndex == 0 ? 1 : 0 ] )
                {
                    isInactivePaused = false;
                    Debug.Log($"[{GetType().Name}] isInactivePaused : (2) " + isInactivePaused);
                }
                if ( curClipCounter == 0 )
                {
                    if ( baselineCounter < 15 )
                    {
                        isCollectingBaseline = true;
                        Debug.Log($"[{GetType().Name}] isCollectingBaseline : " + isCollectingBaseline);
                    }
                    else
                    {
                        isCollectPredictionPerSecond = true;
                        Debug.Log($"[{GetType().Name}] isCollectPredictionPerSecond : " + isCollectPredictionPerSecond);
                    }
                }
            }
            

            player.playbackSpeed = 1;
            player.Play ();

            Debug.Log($"[{GetType().Name}] Video : " + player.name+" is Playing? : "+ player.isPlaying);
        }
    }


    /*private void OnPauseClicked ()
    {
        int val;

        foreach ( VideoPlayer player in VideoPlayers )
        {
            if ( player.isPlaying )
            {
                player.Pause ();
                if ( player == VideoPlayers[ val = currentActivePlayerIndex == 0 ? 1 : 0 ] )
                {
                    isInactivePaused = true;
                }
            }
        }
    }*/

    /*private void OnStopClicked ()
    {
        if ( VideoPlayers[ currentActivePlayerIndex ].isPlaying )
        {
            VideoPlayers[ currentActivePlayerIndex ].Stop ();
        }

        curClipCounter = 0;
        nextClipCounter = 0;

        LoadVideo ( curClipCounter );
    }*/

    /*private void OnSettingsClicked ()
    {
        viewManager.GoToSettings ();
    }*/

    //private void OnBackClicked ()
    //{
        //curClipCounter = prevClipCounter[prevClipCounter.Count - 1];
        //prevClipCounter.RemoveAt(prevClipCounter.Count - 1);

        //LoadVideo(curClipCounter);

        //currentActiveVideoPlayer.Play();
    //}

    //private void OnNextClicked ()
    //{
        //prevClipCounter.Add(curClipCounter);
        //curClipCounter = nextClipCounter;
        //NextVideoLogic(VideoPathDropdown.value == 0 ? false : true);

        //LoadVideo(curClipCounter);

        //currentActiveVideoPlayer.Play();
    //}

    /*private void OnRewindClicked ()
    {
        foreach ( VideoPlayer player in VideoPlayers )
        {
            if ( player.isPlaying )
            {
                if ( player.isPlaying )
                {
                    player.Stop ();
                }
            }
        }
    }*/

    /*private void OnFastForwardClicked ()
    {
        foreach ( VideoPlayer player in VideoPlayers )
        {
            if ( player.isPlaying )
            {
                if ( player.playbackSpeed < 4 )
                {
                    player.playbackSpeed *= 2;
                }
            }
        }
    }*/

    /*private void OnAudioClicked ()
    {
        AudioSlider.gameObject.SetActive ( AudioSlider.gameObject.activeSelf ? false : true );
    }*/

    /*private void OnAudioSliderChanged ()
    {
        foreach ( VideoPlayer player in VideoPlayers )
        {
            if ( player.audioTrackCount > 0 )
            {
                player.SetDirectAudioVolume ( 0, AudioSlider.value );
                player.SetDirectAudioVolume ( 1, AudioSlider.value );
                AudioValueText.text = Mathf.Round ( AudioSlider.value * 100 ).ToString () + "%";
            }
        }
    }*/

    /*private void OnQROverallButtonClicked ()
    {
        QRPanel.gameObject.SetActive ( QRPanel.gameObject.activeSelf ? false : true );
    }*/

    /*private void OnQRButtonClicked ( int val )
    {

        int nextActiveIndex;

        if ( currentActivePlayerIndex == 0 )
        {
            nextActiveIndex = 1;
        }
        else
        {
            nextActiveIndex = 0;
        }

        if ( ExternalQRCodes.enabled == true )
        {
            VideoPlayers[ currentActivePlayerIndex ].enabled = true;
            VideoPlayers[ nextActiveIndex ].enabled = false;
            ExternalQRCodes.enabled = false;
        }
        else
        {
            VideoPlayers[ currentActivePlayerIndex ].enabled = false;
            VideoPlayers[ nextActiveIndex ].enabled = false;
            //ExternalQRCodes.enabled = true; //BENN DISABLED
            //ExternalQRCodes.texture = QRImages[ val ];
            //ExternalQRCodes.GetComponent<RectTransform> ().sizeDelta = new Vector2 ( 700, 700 );
        }

    }*/

    #endregion

    private void ResetValuesForNextScreening ()
    {
        isCollectingBaseline = false;
        isWaitingBaseline = false;
        isWaitingPrediction = false;
        isCollectPredictionPerSecond = false;
        isLoadingNextVideo = false;
        isInactivePaused = false;

        isFirstScene = true;

        baselineCounter = 0;
        arousalBaseline = 0;
        valenceBaseline = 0;
        currentPredictionCounter = 0;
        currentValenceAverage = 0;
        currentArousalAverage = 0;

        currentSceneIndex = 0;

        //AWAKE

        //AudioSlider.gameObject.SetActive ( false );

        prevClipCounters = new List<int> ();

        currentActivePlayerIndex = 0;

        VideoPlayers[ 0 ].gameObject.GetComponent<RawImage> ().enabled = true;
        VideoPlayers[ 1 ].gameObject.GetComponent<RawImage> ().enabled = false;

        VideoPlayers[ 0 ].gameObject.GetComponent<VideoPlayer> ().clip = null;
        VideoPlayers[ 1 ].gameObject.GetComponent<VideoPlayer> ().clip = null;


        //ExternalVideoImages[ 0 ].enabled = true;
        //ExternalVideoImages[ 1 ].enabled = false;
        //ExternalQRCodes.enabled = false;

        //QRPanel.gameObject.SetActive ( false );

        //CurrentVideoText.text = "Current Video: 0";
        //NextVideoText.text = "Next Video: 1";
        //ScreeningOrderText.text = "Screening Order: -";

        //ONSHOW

        curClipCounter = 0;

        LoadVideo ( curClipCounter );

        sceneOrderManager.ResetSceneOrder ();
    }

    private void OnApplicationQuit ()
    {
        Debug.Log ( "Quitting ... " );

        /* Deallocate memory taken by B-Social if it was init-d */

        if ( true ) //BSocialOK ) //BENN DISABLED
        {
            // BSocialUnity.BSocialWrapper_release();
        }
    }
}
