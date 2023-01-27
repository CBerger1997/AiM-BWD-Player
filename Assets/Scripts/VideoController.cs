using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

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

    RawImage videoImage1;
    RawImage videoImage2;

    [SerializeField] SettingsManager settingsManager;

    [SerializeField] List<VideoClip> videoClips;

    private VideoPlayer currentActiveVideoPlayer;
    private int videoCounter;

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

        currentActiveVideoPlayer = videoPlayer1;

        videoImage1 = videoPlayer1.GetComponent<RawImage>();
        videoImage2 = videoPlayer2.GetComponent<RawImage>();
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
            Debug.Log(currentActiveVideoPlayer.frame);
            OnNextClicked();
        }
    }

     public void OnShow() {
        videoCamera.targetDisplay = settingsManager.displayDevice;
        currentActiveVideoPlayer.targetCamera = videoCamera;

        WebCamTexture webcamTexture = new WebCamTexture(settingsManager.webcam.name);

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

        LoadVideo();
    }

    private void LoadVideo() {
        Debug.Log(settingsManager.videoFilePath);

        //string currentVideoPath = settingsManager.videoFilePath + videoCounter.ToString() + ".mov";

        //currentActiveVideoPlayer.url = currentVideoPath;

        currentActiveVideoPlayer.clip = videoClips[videoCounter];

        currentActiveVideoPlayer.Prepare();

        currentActiveVideoPlayer.SetDirectAudioVolume(0, AudioSlider.value);
        currentActiveVideoPlayer.SetDirectAudioVolume(1, AudioSlider.value);
    }

    private void NextVideoLogic() {

    }

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

        LoadVideo();
    }

    private void OnSettingsClicked() {
        viewManager.GoToSettings();
    }

    private void OnBackClicked() {
        videoCounter--;

        LoadVideo();

        currentActiveVideoPlayer.Play();
    }

    private void OnNextClicked() {
        videoCounter++;

        LoadVideo();

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
