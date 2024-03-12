using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Cursor = UnityEngine.Cursor;

public class ViewManager : MonoBehaviour 
{
    [SerializeField] private GameObject SplashScreen;

#if UNITY_EDITOR
    private int splashScreenSecs = 1; //set to at least 4 for release
#else
    private int splashScreenSecs = 6; //set to at least 4 for release
#endif

    [SerializeField] private GameObject SettingsScreen;
    [SerializeField] private SettingsManager settingsManager;
    [SerializeField] private GameObject NetworkError;
    [SerializeField] private GameObject TrackingError;
    [SerializeField] private GameObject WebcamError;
    [SerializeField] private GameObject BeginButton;

    [SerializeField] private GameObject Background;

    [SerializeField] private GameObject TrackingScreen;
    [SerializeField] private TrackingManager trackingManager;
    [SerializeField] private CameraManager cameraManager;

    [SerializeField] private GameObject WatchScreen;
    [SerializeField] private GameObject WebcamTexture;
    [SerializeField] private VideoManager videoManager;

    public int testTrackingSecs = 15;

    void Start()
    {
        DisplaySplashScreen();

        Invoke("SettingsViewAfterDelay", splashScreenSecs); //Seconds delay before starting

        InvokeRepeating("DisplayNetworkCheck", splashScreenSecs, 4.0f);
    }

    void DisplaySplashScreen()
    {
        Debug.Log($"[{GetType().Name}] Display Splash Screen.");

        HideAll();

        Cursor.visible = false;
        SplashScreen.SetActive(true);
    }

    void HideAll()
    {
        SplashScreen.SetActive(false);

        SettingsScreen.SetActive(false);

        WebcamError.SetActive(false);
        TrackingError.SetActive(false);
        NetworkError.SetActive(false);

        Background.SetActive(false);
        TrackingScreen.SetActive(false);
        WebcamTexture.SetActive(false);
        WatchScreen.SetActive(false);
    }

    void SettingsViewAfterDelay()
    {
        DisplaySettingsScreen();
    }

    public void DisplayWatchScreen()
    {
        Debug.Log($"[{GetType().Name}] Display Watch Screen");

        HideAll();

        Cursor.visible = false;
        WatchScreen.SetActive(true);
    }

    public void DisplayTrackingScreen()
    {
        Debug.Log($"[{GetType().Name}] Display Tracking Screen");

        HideAll();

        Cursor.visible = true;
        Background.SetActive(true);
        TrackingScreen.SetActive(true);
        WebcamTexture.SetActive(true);

        
        trackingManager.InitBSocialTesting();
        Invoke("TestTrackingAndPlay", testTrackingSecs); //Second delay then triggering videos

        //cameraManager.UpdateWebcam();
    }

    void TestTrackingAndPlay()
    {
        if (trackingManager.TestDataGathered()) //After were sure we are picking up SOME data...
        {
            DisplayWatchScreen();

            videoManager.ResetAndPlayWhenPrepared(); //Play Videos with initial analysis 
        }
        else
        {
            DisplaySettingsScreen();
            DisplayTrackingError();
            Debug.Log("Tracking does not appear to be detecting Valence or Arousal after " + testTrackingSecs + " seconds");
        }
    }

    public void DisplaySettingsScreen()
    {
        Debug.Log($"[{GetType().Name}] Display Settings Screen");

        HideAll();

        Cursor.visible = true;
        Background.SetActive(true);
        SettingsScreen.SetActive(true);
        WebcamTexture.SetActive(true);

        settingsManager.ResetSettingsManager();
    }

    public void DisplayNetworkCheck()
    {
        //Check for network on the settings screen
        if (SettingsScreen.activeSelf)
        {
            NetworkError.SetActive(false);
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                NetworkError.SetActive(true);
                Debug.Log($"[{GetType().Name}] Network Error - Network Unreachable");
            }
            BeginButton.SetActive(Application.internetReachability != NetworkReachability.NotReachable);
        }
    }

    public void DisplayTrackingError()
    {
        if (SettingsScreen.activeSelf)
        {
            Debug.Log($"[{GetType().Name}] Display Tracking Error");

            DisplaySettingsScreen();

            TrackingError.SetActive(true);

            trackingManager.ResetTrackingManager();
        }
    }

    public void DisplayWebcamError()
    {
        if (SettingsScreen.activeSelf)
        {
            Debug.Log($"[{GetType().Name}] Display Webcam Error");

            DisplaySettingsScreen();

            WebcamError.SetActive(true);
        }
    }
}
