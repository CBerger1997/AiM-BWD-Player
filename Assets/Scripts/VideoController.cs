using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.IO;
using System.Collections;

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
        if (VideoPlayers[currentActivePlayerIndex].isPlaying) {
            //Enabel/disable buttons
            playButton.gameObject.SetActive(false);
            pauseButton.gameObject.SetActive(true);
            stopButton.interactable = true;

            if (((long)VideoPlayers[currentActivePlayerIndex].frame >= (endFrame - 168)) && !isLoadingNextVideo) {

                //Debugging
                Debug.Log(VideoPlayers[currentActivePlayerIndex]);
                Debug.Log((ulong)VideoPlayers[currentActivePlayerIndex].frame);
                Debug.Log(VideoPlayers[currentActivePlayerIndex].frameCount);
                Debug.Log(VideoPlayers[currentActivePlayerIndex].frameCount - 168);
                Debug.Log("CALLUM LOG: Initiating preload of next video: " + nextClipCounter.ToString() + ".mp4");

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

        WebCamTexture webcamTexture = new WebCamTexture(settingsManager.webcam.name, 600, 600, 30);

        WebcamOutput.texture = webcamTexture;

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

        Debug.Log("CALLUM LOG: Load Video: " + curClipCounter.ToString() + ".mp4");

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

        //Get the path for the next video
        Debug.Log("CALLUM LOG: Preloading video: " + videoVal.ToString() + ".mp4");


        //Prepare the next video a few seconds before
        VideoPlayers[nextActiveIndex].clip = videos[nextClipCounter];
        VideoPlayers[nextActiveIndex].Prepare();

        Debug.Log("CALLUM LOG: Preparing video: " + videoVal.ToString() + ".mp4");

        //Once the overlap time has ended, swap the video players
        StartCoroutine(CheckForEndOfVideo());
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

            if (VideoPlayers[currentActivePlayerIndex].frame >= endFrame) {
                Debug.Log("TIME TO SWAP");
            }

            //Once the overlap time has ended or the video stopped playing, swap the video players
            if (VideoPlayers[currentActivePlayerIndex].frame >= endFrame || !VideoPlayers[currentActivePlayerIndex].isPlaying) {
                Debug.Log(VideoPlayers[currentActivePlayerIndex].frame);
                Debug.Log(endFrame);
                Debug.Log("CALLUM LOG: Next video swap over Initiated");

                Debug.Log("CALLUM LOG: Swapping video player images");

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
                nextClipCounter = 1;
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
                VideoPlayers[currentActivePlayerIndex].Stop();
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
}
