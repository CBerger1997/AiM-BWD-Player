using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;

public class VideoController : MonoBehaviour {


    [SerializeField] Button settingsButton;

    [SerializeField] Button playButton;
    [SerializeField] Button pauseButton;
    [SerializeField] Button stopButton;

    [SerializeField] ViewManager viewManager;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] RawImage videoImage1;
    [SerializeField] RawImage videoImage2;
    [SerializeField] RawImage WebcamOutput;
    [SerializeField] Camera videoCamera;
    [SerializeField] RenderTexture videoTexture;

    [SerializeField] SettingsManager settingsManager;

    [SerializeField] List<VideoClip> videoClips;

    private int videoCounter;

    private void Awake() {
        settingsButton.onClick.AddListener(delegate { OnSettingsClicked(); });
        playButton.onClick.AddListener(delegate { OnPlayClicked(); });
        pauseButton.onClick.AddListener(delegate { OnPauseClicked(); });
        stopButton.onClick.AddListener(delegate { OnStopClicked(); });

        //videoPlayer.renderMode = VideoRenderMode.RenderTexture;
    }

    void Update() {
        if(videoPlayer.isPlaying) {
            playButton.interactable = false;
            pauseButton.interactable = true;
            stopButton.interactable = true;
        } else if(videoPlayer.isPaused) {
            playButton.interactable = true;
            pauseButton.interactable = false;
            stopButton.interactable = true;
        } else {
            playButton.interactable = true;
            pauseButton.interactable = false;
            stopButton.interactable = false;
        }
    }

    public void OnShow() {
        videoCamera.targetDisplay = settingsManager.displayDevice;
        videoPlayer.targetCamera = videoCamera;

        WebCamTexture webcamTexture = new WebCamTexture(settingsManager.webcam.name);

        WebcamOutput.texture = webcamTexture;

        webcamTexture.Play();

        if(!Display.displays[settingsManager.displayDevice].active) {
            Display.displays[settingsManager.displayDevice].Activate();
        }

        if(settingsManager.resolution == SettingsManager.ResolutionOptions._1920x960) {
            //Load files from 2K
        } else if(settingsManager.resolution == SettingsManager.ResolutionOptions._4096x2160) {
            //Load files from 4k
        }

        videoCounter = 0;

        Debug.Log(settingsManager.videoFilePath);

        string currentVideoPath = settingsManager.videoFilePath + @"\" + videoCounter.ToString() + ".mov";

        videoPlayer.url = currentVideoPath;

        videoPlayer.Prepare();
    }

    private void OnPlayClicked() {
        videoPlayer.Play();
    }

    private void OnPauseClicked() {
        if (videoPlayer.isPlaying) {
            videoPlayer.Pause();
        }
    }

    private void OnStopClicked() {
        if (videoPlayer.isPlaying) {
            videoPlayer.Stop();
        }
    }

    private void OnSettingsClicked() {
        viewManager.GoToSettings();
    }
}
