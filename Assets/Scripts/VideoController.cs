using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.IO;

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
    [SerializeField] VideoPlayer videoPlayer1;
    [SerializeField] VideoPlayer videoPlayer2;
    [SerializeField] RawImage WebcamOutput;
    [SerializeField] Camera videoCamera;
    [SerializeField] RenderTexture videoTexture;
    [SerializeField] Slider AudioSlider;
    [SerializeField] TMP_Text AudioValueText;
    [SerializeField] TMP_Dropdown VideoPathDropdown;
    [SerializeField] TMP_Text CurrentVideoText;
    [SerializeField] TMP_Text NextVideoText;

    RawImage videoImage1;
    RawImage videoImage2;

    [SerializeField] SettingsManager settingsManager;

    private VideoPlayer currentActiveVideoPlayer;
    private int videoCounter;
    private int nextVideoCounter;
    private List<int> prevVideoCounter;

    private void Awake() {
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

        AudioSlider.gameObject.SetActive(false);

        prevVideoCounter = new List<int>();

        currentActiveVideoPlayer = videoPlayer1;

        videoImage1 = videoPlayer1.GetComponent<RawImage>();
        videoImage2 = videoPlayer2.GetComponent<RawImage>();

        VideoPathDropdown.onValueChanged.AddListener(delegate { NextVideoLogic(VideoPathDropdown.value == 0 ? false : true); });

        CurrentVideoText.text = "Current Video: 0";
        NextVideoText.text = "Next Video: 1";
    }

    void Update() {
        if (currentActiveVideoPlayer.isPlaying) {
            playButton.gameObject.SetActive(false);
            pauseButton.gameObject.SetActive(true);
            stopButton.interactable = true;
        } else if (currentActiveVideoPlayer.isPaused) {
            playButton.gameObject.SetActive(true);
            pauseButton.gameObject.SetActive(false);
            stopButton.interactable = true;
        } else {
            playButton.gameObject.SetActive(true);
            pauseButton.gameObject.SetActive(false);
            stopButton.interactable = false;
        }

        BackButton.interactable = videoCounter == 0 ? false : true;
        NextButton.interactable = videoCounter == 11 ? false : true;

        if ((ulong)currentActiveVideoPlayer.frame == currentActiveVideoPlayer.frameCount - 5) {
            OnNextClicked();
        }
    }

    public void OnShow() {
        videoCamera.targetDisplay = settingsManager.displayDevice;
        currentActiveVideoPlayer.targetCamera = videoCamera;

        WebCamTexture webcamTexture = new WebCamTexture(settingsManager.webcam.name, 600, 600, 30);

        WebcamOutput.texture = webcamTexture;

        webcamTexture.Play();

        if (!Display.displays[settingsManager.displayDevice].active) {
            Display.displays[settingsManager.displayDevice].Activate();
        }

        if (settingsManager.resolution == SettingsManager.ResolutionOptions._1920x960) {
            //Load files from 2K
        } else if (settingsManager.resolution == SettingsManager.ResolutionOptions._4096x2160) {
            //Load files from 4k
        }

        videoCounter = 0;

        LoadVideo(videoCounter);
    }

    private void LoadVideo(int videoVal) {
        string currentVideoPath = settingsManager.videoFilePath + videoCounter.ToString() + ".mp4";

        currentActiveVideoPlayer.url = currentVideoPath;

        currentActiveVideoPlayer.Prepare();

        currentActiveVideoPlayer.SetDirectAudioVolume(0, AudioSlider.value);
        currentActiveVideoPlayer.SetDirectAudioVolume(1, AudioSlider.value);

        NextVideoLogic(VideoPathDropdown.value == 0 ? false : true);

        CurrentVideoText.text = "Current Video: " + videoCounter.ToString();
    }

    #region VideoLogic

    private void NextVideoLogic(bool changePath) {
        switch (videoCounter) {
            case 0:
                nextVideoCounter = 1;
                break;
            case 1:
                nextVideoCounter = changePath == true ? 6 : 2;
                break;
            case 2:
                nextVideoCounter = changePath == true ? 7 : 3;
                break;
            case 3:
                nextVideoCounter = changePath == true ? 8 : 4;
                break;
            case 4:
                nextVideoCounter = changePath == true ? 9 : 5;
                break;
            case 5:
                nextVideoCounter = changePath == true ? 10 : 11;
                break;
            case 6:
                nextVideoCounter = 3;
                break;
            case 7:
                nextVideoCounter = 4;
                break;
            case 8:
                nextVideoCounter = 5;
                break;
            case 9:
                nextVideoCounter = 11;
                break;
            case 10:
                nextVideoCounter = 11;
                break;
            case 11:
                currentActiveVideoPlayer.Stop();
                break;
            default:
                Debug.LogError("Video counter is outside the video range");
                break;
        }

        NextVideoText.text = "Next Video: " + nextVideoCounter.ToString();
    }

    #endregion

    #region UI LISTENER FUNCTIONS

    private void OnPlayClicked() {
        currentActiveVideoPlayer.playbackSpeed = 1;

        currentActiveVideoPlayer.Play();
    }

    private void OnPauseClicked() {
        if (currentActiveVideoPlayer.isPlaying) {
            currentActiveVideoPlayer.Pause();
        }
    }

    private void OnStopClicked() {
        if (currentActiveVideoPlayer.isPlaying) {
            currentActiveVideoPlayer.Stop();
        }

        videoCounter = 0;
        nextVideoCounter = 0;

        LoadVideo(videoCounter);
    }

    private void OnSettingsClicked() {
        viewManager.GoToSettings();
    }

    private void OnBackClicked() {
        videoCounter = prevVideoCounter[prevVideoCounter.Count - 1];
        prevVideoCounter.RemoveAt(prevVideoCounter.Count - 1);

        LoadVideo(videoCounter);

        currentActiveVideoPlayer.Play();
    }

    private void OnNextClicked() {
        prevVideoCounter.Add(videoCounter);
        videoCounter = nextVideoCounter;
        NextVideoLogic(VideoPathDropdown.value == 0 ? false : true);

        LoadVideo(videoCounter);

        currentActiveVideoPlayer.Play();
    }

    private void OnRewindClicked() {
        if (currentActiveVideoPlayer.isPlaying) {
            currentActiveVideoPlayer.Stop();
        }
    }

    private void OnFastForwardClicked() {
        currentActiveVideoPlayer.playbackSpeed *= 2;
    }

    private void OnAudioClicked() {
        AudioSlider.gameObject.SetActive(AudioSlider.gameObject.activeSelf ? false : true);
    }

    private void OnAudioSliderChanged() {
        currentActiveVideoPlayer.SetDirectAudioVolume(0, AudioSlider.value);
        currentActiveVideoPlayer.SetDirectAudioVolume(1, AudioSlider.value);
        AudioValueText.text = Mathf.Round(AudioSlider.value * 100).ToString() + "%";
    }

    #endregion
}
