using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MobileController : MonoBehaviour
{
    void Start()
    {
        // Initialize the mobile controller
        Debug.Log("Mobile Controller Initialized");
        CheckDevices();
    }
    
    void CheckDevices()
    {
        // Check if the device is a mobile device
        if (Application.isMobilePlatform)
        {
            Debug.Log("Running on a mobile platform");
            // turn on mobile controls UI
            GameObject.Find("Canvas").transform.Find("GUI").Find("Controller").gameObject.SetActive(true);

        }
        else
        {
            Debug.Log("Disabled mobile controls UI");
            // turn off mobile controls UI except for the pauseButton
            foreach (Transform child in GameObject.Find("Canvas").transform.Find("GUI").Find("Controller"))
            {
                if (child.name != "PauseButton")
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }

}
