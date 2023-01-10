using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VideoController : MonoBehaviour {


    [SerializeField] Button settingsButton;
    [SerializeField] ViewManager viewManager;

    private void Awake() {
        settingsButton.onClick.AddListener(delegate { OnSettingsClicked(); });
    }

    void Start() {
        //Play first clip (baseline into clip)
        //Start analysis of data, run BSocial
        //Make call on what to move to based on settings
        //
    }

    void Update() {

    }

    private void OnSettingsClicked() {
        viewManager.GoToSettings();
    }
}
