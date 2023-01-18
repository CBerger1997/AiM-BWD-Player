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

    private void Awake() {
        settingsButton.onClick.AddListener(delegate { OnSettingsClicked(); });
        playButton.onClick.AddListener(delegate { OnPlayClicked(); });
        pauseButton.onClick.AddListener(delegate { OnPauseClicked(); });
        stopButton.onClick.AddListener(delegate { OnStopClicked(); });

        string path = "C:" + @"\" + "Users" + @"\" + "callu" + @"\" + "OneDrive" + @"\" + "Desktop" + @"\" + "BWD Videos" + @"\" + "0.mov";

        var info = new FileInfo(path);

        videoPlayer.url = path;

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
    }

    void Start() {

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

    //private void OnResetClicked() {
    //    videoPlayer.rese();
    //}

    private void OnSettingsClicked() {
        viewManager.GoToSettings();
    }

    //public void ShowExplorer(string itemPath) {
    //    itemPath = itemPath.Replace(@"/", @"\");   // explorer doesn't like front slashes
    //    System.Diagnostics.Process.Start("explorer.exe", "/select," + itemPath);
    //}
}
