﻿using UnityEngine;
using System.Collections;
using DaggerfallWorkshop.Demo;
using DaggerfallWorkshop.Game;

public class UIManager : MonoBehaviour {
    private bool _isUIOpen = false;
    public bool isUIOpen { 
        get { return _isUIOpen; }
    }

    public Canvas MenuOptions1;
    public Canvas DevConsole;
    public Camera playerCamera; // necessary to change PlayerMouseLook behavior

	// Use this for initialization
	void Start () {
        MenuOptions1.enabled = false;
        DevConsole.enabled = false;

	}
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.Alpha1)) {
            MenuOptions1.enabled = !MenuOptions1.enabled;
            _isUIOpen = MenuOptions1.enabled;
            //playerCamera.GetComponent<PlayerMouseLook>().enableMLook = !MenuOptions1.enabled;
        } else if (Input.GetButtonDown("DevConsole")) { 
            if (!_isUIOpen) {
                _isUIOpen = true;
                DevConsole.enabled = true;
                DevConsole.GetComponent<DevConsole>().enabled = true;
            } else {
                _isUIOpen = false;
                DevConsole.enabled = false;
                DevConsole.GetComponent<DevConsole>().enabled = false;
            }
        }
	}


    void devConsole_displayText(string line) {
        DevConsole.GetComponent<DevConsole>().displayText(line);
    }
}
