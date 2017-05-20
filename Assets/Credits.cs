using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Credits : MonoBehaviour {
    public GameObject creditsPanel;
    bool isEnabled = false;
    public void ToggleCredits(){
        if(isEnabled == false){
            creditsPanel.SetActive(true);
            isEnabled = true;
        } else {
            creditsPanel.SetActive(false);
            isEnabled = false;
        }
    }
}
