using UnityEngine;

public class ViewManager : MonoBehaviour {

    //Controls what is shown

    [SerializeField] private GameObject SettingsMenu;
    //[SerializeField] private GameObject videoCamera;
    //[SerializeField] private GameObject videoCanvas;
    [SerializeField] private GameObject videoPlayer;

    private void Awake() {
        SettingsMenu.SetActive(true);

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
        //videoCamera.SetActive(false);
        //videoCanvas.SetActive(false);
        videoPlayer.SetActive(false);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        //videoCamera.SetActive(true);
        //videoCanvas.SetActive(true);
#endif
    }

    public void GoToVideoView()
    {
        //Hide Settings
        SettingsMenu.SetActive(false);

        //Show
        //videoCamera.SetActive(true);
        //videoCanvas.SetActive(true);

        //Play the video
        videoPlayer.SetActive(true);
        videoPlayer.GetComponent<VideoController>().OnShow();
        videoPlayer.GetComponent<VideoController>().OnPlayClicked();
    }
}
