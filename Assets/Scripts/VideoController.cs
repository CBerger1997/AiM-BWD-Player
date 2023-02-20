using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.IO;
using System.Collections;
using System.Threading;

public delegate void NewBSocialData(BSocialUnity.BSocialPredictions p);

public class VideoController : MonoBehaviour {
    [SerializeField] Button settingsButton;

    [SerializeField] Button playButton;
    [SerializeField] Button pauseButton;
    [SerializeField] Button stopButton;
    [SerializeField] Button BackButton;
    [SerializeField] Button RewindButton;
    [SerializeField] Button NextButton;
    [SerializeField] Button FastForwardButton;
    [SerializeField] Button AudioButton;

    [SerializeField] ViewManager viewManager;
    [SerializeField] RawImage WebcamOutput;
    [SerializeField] Camera videoCamera;
    [SerializeField] RenderTexture videoTexture;
    [SerializeField] Slider AudioSlider;
    [SerializeField] TMP_Text AudioValueText;
    [SerializeField] TMP_Dropdown VideoPathDropdown;
    [SerializeField] TMP_Text CurrentVideoText;
    [SerializeField] TMP_Text NextVideoText;
    [SerializeField] TMP_Text StartFrameText;
    [SerializeField] TMP_Text EndFrameText;
    [SerializeField] TMP_Text FrameCountText;
    [SerializeField] VideoPlayer[] VideoPlayers;
    [SerializeField] RawImage[] ExternalVideoImages;

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

#region BSocial
    public static event NewBSocialData EvNewBSocialData;
    private Thread BSocialThread;
    private bool BSocialThreadIsFree = true;
    private bool BSocialOK = false;
    private string BSocialLicenceKeyPath;
    public Color32[] textureData;
    public Texture2D txBuffer;

    /*
     * BSocial SDK v1.4.0 Copyright BlueSkeye AI LTD.
     * For Academic Use Only
     * Original Setup Code Copyright Timur Almaev, Chief Engineer
     * This Setup Code Copyright Luke Rose, Automotive Engineering Lead
     */
    private bool InitBSocial() {
        
        BSocialLicenceKeyPath = Path.Combine(Application.streamingAssetsPath, "bsocial.lic");

        Debug.Log("B-Social licence key path : " + BSocialLicenceKeyPath);

        BSocialUnity.BSocialWrapper_create();
        int rcode = BSocialUnity.BSocialWrapper_load_online_licence_key(BSocialLicenceKeyPath);
        
        if (rcode != 0)
        {
            Debug.LogError("Start: ERROR - BSocialWrapper_load_online_license_key() failed");
            return false;
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

        BSocialUnity.BSocialWrapper_set_nthreads(4); // Change for optimal performance, BSocial needs at least 10FPS, 15FPS+ preferred
        BSocialUnity.BSocialWrapper_reset();

        isShowing = true;

        return BSocialOK;
    }

    private void BSocialUpdate() {
        if (!(BSocialOK && BSocialThreadIsFree && webcamTexture.isPlaying)) return;
         
        webcamTexture.GetPixels32(textureData);

        BSocialUnity.BSocialWrapper_set_image_native(
                ref textureData, webcamTexture.width, webcamTexture.height, true, false, BSocialUnity.BSocialWrapper_Rotation.BM_NO_ROTATION);

        BSocialUnity.BSocialWrapper_overlay_native(
                ref textureData, webcamTexture.width, webcamTexture.height, true, false, BSocialUnity.BSocialWrapper_Rotation.BM_NO_ROTATION);

        BSocialThread = new Thread(BSocialProcessing);
        BSocialThreadIsFree = false;
        BSocialThread.Start();

        if(!txBuffer)    
            txBuffer = new Texture2D(webcamTexture.width, webcamTexture.height);       
        txBuffer.name = $"BSocial Webcam Capture";
        txBuffer.SetPixels32(textureData);
        txBuffer.Apply(); 
    }

    private void BSocialProcessing()
    {
        Debug.Log("Processing");
        BSocialUnity.BSocialWrapper_run();

        BSocialUnity.BSocialPredictions predictions = BSocialUnity.BSocialWrapper_get_predictions();

        //trigger any registered events
        EvNewBSocialData?.Invoke(predictions);

        // Sleep a little bit and set the signal to get the next frame
        Thread.Sleep(1);

        BSocialThreadIsFree = true;        
    }
#endregion

    private void Awake() {

        SetupListeners();

        AudioSlider.gameObject.SetActive(false);

        prevClipCounters = new List<int>();

        currentActivePlayerIndex = 0;

        VideoPlayers[0].gameObject.GetComponent<RawImage>().enabled = true;
        VideoPlayers[1].gameObject.GetComponent<RawImage>().enabled = false;

        ExternalVideoImages[0].enabled = true;
        ExternalVideoImages[1].enabled = false;

        VideoPathDropdown.onValueChanged.AddListener(delegate { GetNextVideoValue(VideoPathDropdown.value == 0 ? false : true); });

        CurrentVideoText.text = "Current Video: 0";
        NextVideoText.text = "Next Video: 1";

        isLoadingNextVideo = false;

        isInactivePaused = false;
    }

    private void SetupListeners() {
        settingsButton.onClick.AddListener(delegate { OnSettingsClicked(); });
        playButton.onClick.AddListener(delegate { OnPlayClicked(); });
        pauseButton.onClick.AddListener(delegate { OnPauseClicked(); });
        stopButton.onClick.AddListener(delegate { OnStopClicked(); });
        BackButton.onClick.AddListener(delegate { OnBackClicked(); });
        RewindButton.onClick.AddListener(delegate { OnRewindClicked(); });
        NextButton.onClick.AddListener(delegate { OnNextClicked(); });
        FastForwardButton.onClick.AddListener(delegate { OnFastForwardClicked(); });
        AudioButton.onClick.AddListener(delegate { OnAudioClicked(); });
        AudioSlider.onValueChanged.AddListener(delegate { OnAudioSliderChanged(); });
    }

    void Update() {
        //if (isShowing) {
        //    BSocialUpdate();
        //}

        if (VideoPlayers[currentActivePlayerIndex].isPlaying) {
            //Enabel/disable buttons
            playButton.gameObject.SetActive(false);
            pauseButton.gameObject.SetActive(true);
            stopButton.interactable = true;

            if (((long)VideoPlayers[currentActivePlayerIndex].frame >= (endFrame - 168)) && !isLoadingNextVideo) {

                //Get the nextVideoCounter value
                GetNextVideoValue(VideoPathDropdown.value == 0 ? false : true);

                //Get the start frame for next video and set the text
                startFrame = GetTimingsForVideoCounter(nextClipCounter, true);
                StartFrameText.text = "Start Frame: " + startFrame.ToString();

                //Set video loading to true
                isLoadingNextVideo = true;

                //Preload the next video
                PreLoadNextVideo(nextClipCounter);
            }
        } else if (VideoPlayers[currentActivePlayerIndex].isPaused) {
            //Enabel/disable buttons
            playButton.gameObject.SetActive(true);
            pauseButton.gameObject.SetActive(false);
            stopButton.interactable = true;
        } else {
            //Enabel/disable buttons
            playButton.gameObject.SetActive(true);
            pauseButton.gameObject.SetActive(false);
            stopButton.interactable = false;
        }

        if (VideoPlayers[1].isPlaying) {
            //Debug.Log(VideoPlayers[1].frame);
        }

        BackButton.interactable = curClipCounter == 0 ? false : true;
        NextButton.interactable = curClipCounter == 11 ? false : true;
    }

    public void OnShow() {
        videos = Resources.LoadAll<VideoClip>(settingsManager.videoFilePath) as VideoClip[];

        videoCamera.targetDisplay = settingsManager.displayDevice;
        VideoPlayers[currentActivePlayerIndex].targetCamera = videoCamera;

        webcamTexture = new WebCamTexture(settingsManager.webcam.name, 600, 600, 30);

        //BSocialOK = InitBSocial();

        WebcamOutput.texture = webcamTexture;
        //WebcamOutput.texture = BSocialUnity.OverlayTexture;

        webcamTexture.Play();

        if (!Display.displays[settingsManager.displayDevice].active) {
            Display.displays[settingsManager.displayDevice].Activate();
        }

        curClipCounter = 0;

        LoadVideo(curClipCounter);
    }

    private void LoadVideo(int videoVal) {
        VideoPlayers[currentActivePlayerIndex].clip = videos[videoVal];

        VideoPlayers[currentActivePlayerIndex].Prepare();

        VideoPlayers[currentActivePlayerIndex].SetDirectAudioVolume(0, AudioSlider.value);
        VideoPlayers[currentActivePlayerIndex].SetDirectAudioVolume(1, AudioSlider.value);

        GetNextVideoValue(VideoPathDropdown.value == 0 ? false : true);

        CurrentVideoText.text = "Current Video: " + videoVal.ToString();

        endFrame = GetTimingsForVideoCounter(curClipCounter, false);

        EndFrameText.text = "End Frame: " + endFrame.ToString();
        FrameCountText.text = "Frame Count: " + VideoPlayers[currentActivePlayerIndex].clip.frameCount.ToString();
    }

    private void PreLoadNextVideo(int videoVal) {
        int nextActiveIndex;

        if (currentActivePlayerIndex == 0) {
            nextActiveIndex = 1;
        } else {
            nextActiveIndex = 0;
        }

        if (nextClipCounter < 12) {
            //Prepare the next video a few seconds before
            VideoPlayers[nextActiveIndex].clip = videos[nextClipCounter];
            VideoPlayers[nextActiveIndex].Prepare();

            //Once the overlap time has ended, swap the video players
            StartCoroutine(CheckForEndOfVideo());
        }
    }

    IEnumerator CheckForEndOfVideo() {
        bool waiting = true;

        int nextActiveIndex = 0;

        if (currentActivePlayerIndex == 0) {
            nextActiveIndex = 1;
        } else {
            nextActiveIndex = 0;
        }
        while (waiting) {
            //Start playing next video
            if (VideoPlayers[currentActivePlayerIndex].frame >= endFrame - startFrame && !VideoPlayers[nextActiveIndex].isPlaying) {
                VideoPlayers[nextActiveIndex].Play();
            }

            //Once the overlap time has ended or the video stopped playing, swap the video players
            if (VideoPlayers[currentActivePlayerIndex].frame >= endFrame || !VideoPlayers[currentActivePlayerIndex].isPlaying) {
                VideoPlayers[currentActivePlayerIndex].gameObject.GetComponent<RawImage>().enabled = false;
                VideoPlayers[nextActiveIndex].gameObject.GetComponent<RawImage>().enabled = true;

                ExternalVideoImages[currentActivePlayerIndex].enabled = false;
                ExternalVideoImages[nextActiveIndex].enabled = true;

                endFrame = GetTimingsForVideoCounter(nextClipCounter, false);
                EndFrameText.text = "End Frame: " + endFrame.ToString();

                prevClipCounters.Add(curClipCounter);
                currentActivePlayerIndex = nextActiveIndex;

                curClipCounter = nextClipCounter;

                FrameCountText.text = "Frame Count: " + VideoPlayers[currentActivePlayerIndex].clip.frameCount.ToString();

                CurrentVideoText.text = "Current Video: " + curClipCounter.ToString();

                GetNextVideoValue(VideoPathDropdown.value == 0 ? false : true);

                isLoadingNextVideo = false;
                waiting = false;
            }

            yield return new WaitForEndOfFrame();
        }
    }

    #region VideoLogic

    private void GetNextVideoValue(bool changePath) {
        switch (curClipCounter) {
            case 0:
                nextClipCounter = changePath == true ? 1 : 2;
                break;
            case 1:
                nextClipCounter = changePath == true ? 6 : 2;
                break;
            case 2:
                nextClipCounter = changePath == true ? 7 : 3;
                break;
            case 3:
                nextClipCounter = changePath == true ? 8 : 4;
                break;
            case 4:
                nextClipCounter = changePath == true ? 9 : 5;
                break;
            case 5:
                nextClipCounter = changePath == true ? 10 : 11;
                break;
            case 6:
                nextClipCounter = 3;
                break;
            case 7:
                nextClipCounter = 4;
                break;
            case 8:
                nextClipCounter = 5;
                break;
            case 9:
                nextClipCounter = 11;
                break;
            case 10:
                nextClipCounter = 11;
                break;
            case 11:
                nextClipCounter = 12;
                break;
            case 12:
                Debug.Log("End Reached");
                break;
            default:
                Debug.LogError("Video counter is outside the video range");
                break;
        }

        NextVideoText.text = "Next Video: " + nextClipCounter.ToString();
    }

    private int GetTimingsForVideoCounter(int val, bool isStart) {
        int frameVal = 0;

        switch (val) {
            case 0:
                if (isStart) {
                    frameVal = 0;
                } else {
                    frameVal = 954;
                }
                break;
            case 1:
                if (isStart) {
                    frameVal = 149;
                } else {
                    frameVal = 6675;
                }
                break;
            case 2:
                if (isStart) {
                    frameVal = 0;
                } else {
                    frameVal = 2489;
                }
                break;
            case 3:
                if (isStart) {
                    frameVal = 12;
                } else {
                    frameVal = 3865;
                }
                break;
            case 4:
                if (isStart) {
                    frameVal = 6;
                } else {
                    frameVal = 2379;
                }
                break;
            case 5:
                if (isStart) {
                    frameVal = 43;
                } else {
                    frameVal = 2743;
                }
                break;
            case 6:
                if (isStart) {
                    frameVal = 0;
                } else {
                    frameVal = 3177;
                }
                break;
            case 7:
                if (isStart) {
                    frameVal = 44;
                } else {
                    frameVal = 1525;
                }
                break;
            case 8:
                if (isStart) {
                    frameVal = 65;
                } else {
                    frameVal = 2097;
                }
                break;
            case 9:
                if (isStart) {
                    frameVal = 3;
                } else {
                    frameVal = 2806;
                }
                break;
            case 10:
                if (isStart) {
                    frameVal = 63;
                } else {
                    frameVal = 4945;
                }
                break;
            case 11:
                if (isStart) {
                    frameVal = 0;
                } else {
                    frameVal = 621;
                }
                break;
            case 12:
                Debug.Log("End Reached");
                break;
            default:
                Debug.LogError("Video counter is outside the video range");
                break;
        }

        return frameVal;
    }

    #endregion

    #region UI LISTENER FUNCTIONS

    private void OnPlayClicked() {
        int val;

        foreach (VideoPlayer player in VideoPlayers) {
            if (player.isPrepared && (player == VideoPlayers[currentActivePlayerIndex] || isInactivePaused)) {
                if(player == VideoPlayers[val = currentActivePlayerIndex == 0 ? 1 : 0]) {
                    isInactivePaused = false;
                }
                player.playbackSpeed = 1;
                player.Play();
            }
        }
    }

    private void OnPauseClicked() {
        int val;

        foreach (VideoPlayer player in VideoPlayers) {
            if (player.isPlaying) {
                player.Pause();
                if (player == VideoPlayers[val = currentActivePlayerIndex == 0 ? 1 : 0]) {
                    isInactivePaused = true;
                }
            }
        }
    }

    private void OnStopClicked() {
        if (VideoPlayers[currentActivePlayerIndex].isPlaying) {
            VideoPlayers[currentActivePlayerIndex].Stop();
        }

        curClipCounter = 0;
        nextClipCounter = 0;

        LoadVideo(curClipCounter);
    }

    private void OnSettingsClicked() {
        viewManager.GoToSettings();
    }

    private void OnBackClicked() {
        //curClipCounter = prevClipCounter[prevClipCounter.Count - 1];
        //prevClipCounter.RemoveAt(prevClipCounter.Count - 1);

        //LoadVideo(curClipCounter);

        //currentActiveVideoPlayer.Play();
    }

    private void OnNextClicked() {
        //prevClipCounter.Add(curClipCounter);
        //curClipCounter = nextClipCounter;
        //NextVideoLogic(VideoPathDropdown.value == 0 ? false : true);

        //LoadVideo(curClipCounter);

        //currentActiveVideoPlayer.Play();
    }

    private void OnRewindClicked() {
        foreach (VideoPlayer player in VideoPlayers) {
            if (player.isPlaying) {
                if (player.isPlaying) {
                    player.Stop();
                }
            }
        }
    }

    private void OnFastForwardClicked() {
        foreach (VideoPlayer player in VideoPlayers) {
            if (player.isPlaying) {
                if (player.playbackSpeed < 4) {
                    player.playbackSpeed *= 2;
                }
            }
        }
    }

    private void OnAudioClicked() {
        AudioSlider.gameObject.SetActive(AudioSlider.gameObject.activeSelf ? false : true);
    }

    private void OnAudioSliderChanged() {
        foreach (VideoPlayer player in VideoPlayers) {
            if (player.audioTrackCount > 0) {
                player.SetDirectAudioVolume(0, AudioSlider.value);
                player.SetDirectAudioVolume(1, AudioSlider.value);
                AudioValueText.text = Mathf.Round(AudioSlider.value * 100).ToString() + "%";
            }
        }
    }

    #endregion

     private void OnApplicationQuit()
    {
        Debug.Log("Quitting ... ");

        /* Deallocate memory taken by B-Social if it was init-d */

        if (BSocialOK)
        {
            // BSocialUnity.BSocialWrapper_release();
        }
    }
}
