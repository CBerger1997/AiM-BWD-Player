using UnityEngine;

public class ViewManager : MonoBehaviour {

    //Controls what is shown

    [SerializeField] private GameObject SplashMenu;
    [SerializeField] private GameObject SettingsManager;
    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject TrackingMenu;
    [SerializeField] private GameObject TrackingError;

    //[SerializeField] private GameObject videoCamera;
    //[SerializeField] private GameObject videoCanvas;
    [SerializeField] private GameObject videoPlayer;
    [SerializeField] private SettingsManager settingsManager;

    private void Awake() {
        SettingsMenu.SetActive(true);

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
        //videoCamera.SetActive(false);
        //videoCanvas.SetActive(false);
        videoPlayer.SetActive(false);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        //videoCamera.SetActive(true);
        //videoCanvas.SetActive(true);
        videoPlayer.SetActive(false);
#endif
    }

    void Start()
    {
        //Show Splash
        SplashMenu.SetActive(true); //Start with this menu

        Invoke("MoveToSettings", 4); //4 second delay before starting
        
        //Hide Settings
        SettingsMenu.SetActive(false);

        //Hide Tracking
        TrackingMenu.SetActive(false);

        //Hide the video
        videoPlayer.SetActive(false);

        //Hide the tracking error
        TrackingError.SetActive(false);

    }

    void MoveToSettings()
    {   
        Debug.Log("Close Splash Screen.");
        GoToSettingsView();
    }

    public void GoTrackingErrorView()
    {
        Cursor.visible = true;

        //Hide Splash
        SplashMenu.SetActive(false);

        SettingsManager.SetActive(true);
        TrackingError.SetActive(true);

        //Show Settings
        SettingsMenu.SetActive(true);
        settingsManager.InitSettings();

        //Old...
        //videoCamera.SetActive(false);
        //videoCanvas.SetActive(false);

        //Hide Tracking
        TrackingMenu.SetActive(false);

        //Hide the video
        videoPlayer.SetActive(false);
    }

    public void GoToVideoView()
    {
        //Hide Settings
        SettingsMenu.SetActive(false);

        //Old...
        //videoCamera.SetActive(true);
        //videoCanvas.SetActive(true);

        //Hide Tracking
        TrackingMenu.SetActive(false);

        //Play the video
        videoPlayer.SetActive(true);
        videoPlayer.GetComponent<VideoController>().OnShow();

        Cursor.visible = false;
    }

    public void GoToTrackingView()
    {
        Cursor.visible = true;

        //Hide Settings
        SettingsMenu.SetActive(false);

        //Old...
        //videoCamera.SetActive(true); //Was second display screen
        //videoCanvas.SetActive(true);//Was old Video Player

        //Show Tracking
        TrackingMenu.SetActive(true);

        //Show the video
        videoPlayer.SetActive(true); //TEMP - BENN - MAY NOT NEED THIS...  SORT OF NEGATES THE VIDEOVIEW ABOVE
        videoPlayer.GetComponent<VideoController>().OnShow();
    }

    public void GoToSettingsView()
    {
        Cursor.visible = true;

        //Hide Splash
        SplashMenu.SetActive(false);

        SettingsManager.SetActive(true);

        //Show Settings
        SettingsMenu.SetActive(true);
        settingsManager.InitSettings();

        //Old...
        //videoCamera.SetActive(false);
        //videoCanvas.SetActive(false);

        //Hide Tracking
        TrackingMenu.SetActive(false);

        //Hide the video
        videoPlayer.SetActive(false);
    }
}
