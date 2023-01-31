using UnityEngine;

public class ViewManager : MonoBehaviour {

    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject MainMenu;
    [SerializeField] private GameObject videoCamera;
    [SerializeField] private GameObject videoCanvas;

    private void Awake() {
        GoToSettings();

#if UNITY_EDITOR_WIN
        videoCamera.SetActive(false);
        videoCanvas.SetActive(false);
#elif UNITY_STANDALONE_WIN
        videoCamera.SetActive(true);
        videoCanvas.SetActive(true);
#endif
    }
    
    public void GoToSettings() {
        SettingsMenu.SetActive(true);
        MainMenu.SetActive(false);
    }

    public void GoToMainMenu() {
        SettingsMenu.SetActive(false);
        MainMenu.SetActive(true);
        MainMenu.GetComponent<VideoController>().OnShow();
    }
}
