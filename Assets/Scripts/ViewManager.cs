using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewManager : MonoBehaviour {

    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject MainMenu;

    private void Awake() {
        GoToSettings();
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
