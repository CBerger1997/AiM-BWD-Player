using UnityEngine;

public class ViewManager : MonoBehaviour {

    //Controls what is shown

    [SerializeField] private GameObject SplashMenu;
    [SerializeField] private GameObject SettingsManager;
    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject TrackingMenu;
    [SerializeField] private GameObject TrackingError;
    [SerializeField] private GameObject NetworkError;
    [SerializeField] private GameObject BeginButton;

    //[SerializeField] private GameObject videoCamera;
    //[SerializeField] private GameObject videoCanvas;
    [SerializeField] private GameObject videoPlayer;
    [SerializeField] private SettingsManager settingsManager;

    private int splashScreenSecs = 6; //set to at least 4 for release

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
        Debug.Log("SplashMenuView");
        SplashMenu.SetActive(true); //Start with this menu

        Invoke("SettingsViewAfterDelay", splashScreenSecs); //Seconds delay before starting

        NetworkError.SetActive(false);
        SettingsMenu.SetActive(false);
        TrackingMenu.SetActive(false);
        videoPlayer.SetActive(false);
        TrackingError.SetActive(false);

    }

    void Update()
    {
        if (SettingsMenu.activeSelf) NetworkCheck();
    }

    void SettingsViewAfterDelay()
    {
        settingsManager.Init();
        Debug.Log("Close Splash Screen.");
        SettingsView();
    }

    

    public void VideoView()
    {
        Debug.Log("VideoView");
        Cursor.visible = false;

        SettingsMenu.SetActive(false);
        TrackingMenu.SetActive(false);

    }

    public void TrackingView()
    {
        Debug.Log("TrackingView");

        Cursor.visible = true;

        SettingsMenu.SetActive(false);

        TrackingMenu.SetActive(true);
        videoPlayer.SetActive(true);
        videoPlayer.GetComponent<VideoController>().InitBSocialAndPlayVideos();
    }

    public void SettingsView()
    {
        Debug.Log("SettingsView");
        Cursor.visible = true;

        SplashMenu.SetActive(false);
        TrackingMenu.SetActive(false);
        videoPlayer.SetActive(false);

        SettingsManager.SetActive(true);

        SettingsMenu.SetActive(true);
        settingsManager.InitSettings();
    }

    public void NetworkCheck()
    {
        NetworkError.SetActive(false);
        BeginButton.SetActive(true);
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            NetworkError.SetActive(true);
            BeginButton.SetActive(false);
        }
    }

    public void TrackingErrorView()
    {
        Debug.Log("TrackingErrorView");
        SettingsView();

        TrackingError.SetActive(true);
    }

    
}
