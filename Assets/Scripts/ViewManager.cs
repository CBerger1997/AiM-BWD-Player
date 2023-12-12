using UnityEngine;

public class ViewManager : MonoBehaviour {

    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject videoCamera;
    [SerializeField] private GameObject videoCanvas;
    [SerializeField] private GameObject videoParent;
    public VideoController videoController;

    private void Awake() {
        
        GoToSettings();

#if UNITY_EDITOR_WIN || UNITY_EDITOR_OSX
        videoCamera.SetActive(false);
        videoCanvas.SetActive(false);
        videoParent.SetActive(false);
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        videoCamera.SetActive(true);
        videoCanvas.SetActive(true);
#endif
    }

    public void GoToSettings() {
        SettingsMenu.SetActive(true);
        MainMenu.SetActive(false);
    }

    /*public void GoToMainMenu() {
        SettingsMenu.SetActive(false);
        MainMenu.SetActive(true);
        MainMenu.GetComponent<VideoController>().OnShow();
    }*/

    //BENN CHANGED
    public void GoToMainMenu()
    {
        SettingsMenu.SetActive(false);
        MainMenu.SetActive(false);
        videoCamera.SetActive(true);
        videoCanvas.SetActive(true);

        videoParent.SetActive(true);
        videoController.OnPlayClicked();
    }
}
