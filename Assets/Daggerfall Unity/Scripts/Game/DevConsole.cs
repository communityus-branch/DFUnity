﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DaggerfallWorkshop.Game;
using UnityEngine.UI;

namespace DaggerfallWorkshop.Game {

    // Displays internal game messages and allows the user to execute internal game commands
    // Call displayText(...) to append a message to the log
    public class DevConsole : MonoBehaviour {
        public Text outputTextField;
             
        DaggerfallUnity dfUnity;
        float deltaTime = 0.0f;
        static string _outputText = "";
        string _userCommand = "";

        public void displayText(string text, bool newline = true) {
            // TODO: can string.concat ever buffer overflow?
            _outputText += System.DateTime.UtcNow + " *** " + text;
            if (newline) { 
                _outputText += "\n";
            }
            outputTextField.text = _outputText;
        }

        public static void flushText() {
            _outputText = "";
        }

        // Use this for initialization
        void Start () {
            displayText("Dev console created");
            enabled = false;
        }

        // Sanity check
        void Check() {
            if (!dfUnity) { 
                if(!DaggerfallUnity.FindDaggerfallUnity(out dfUnity)) { 
                    DaggerfallUnity.LogMessage(Error.ERROR_GAME_DAG_UNITY_NOT_FOUND);
                    Application.Quit();
                }
            }
        }

        // Update is called once per frame
        void Update () {
            deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        }

        void OnGUI() {
            drawFPS();
        }

/*        void dispatchCommand() {
            string[] args = _userCommand.Split(' ');
            displayText("> " + _userCommand);
            _userCommand = "";
            if (args.Length > 0) {
                parseCommand(args);
            }
        }*/

        bool parseCommand(string[] args) {
            bool handled = false;
            
            switch(args[0]) { 
                case ("spawn_enemy"):
                    if (args.Length == 2) {
                        int mobileType = System.Convert.ToInt32(args[1]);
                        if (mobileType > -1 && mobileType < 147) {
                            Logger.GetInstance().log("Spawning enemy of type " + (MobileTypes)mobileType + ".\n");
                            GameObject.FindGameObjectWithTag("EnemySpawner").SendMessage("SpawnEnemy", mobileType); 
                            handled = true;
                        }
                    }
                    break;

                default:
                    break;
            }


            return handled;
        }

        public void dispatchCommand(string command) {
            string[] args = command.Split(' ');
            displayText("> " + command);
            if (args.Length > 0) {
                parseCommand(args);
            }
        }
        void drawFPS() {
            int w = Screen.width, h = Screen.height;
            Vector2 scrollPosition = Vector2.zero;

            GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "Dev Console");

            // FPS
            GUIStyle style = new GUIStyle();
            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = new Color(1.0f, 0.0f, 0.0f, 1.0f);
            float msec = deltaTime * 1000.0f;
            float fps = 1.0f / deltaTime;
            string text = string.Format("{0:0.0} ms ({1:0.} fps)", msec, fps);
            GUI.Label(rect, text, style);

            /*GUILayout.BeginArea(new Rect(0, 50, w, h - 100));
            scrollPosition.y = Mathf.Infinity; // this seems silly (and will go away when input is implemented)
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(w), GUILayout.Height(h - 100));
            GUILayout.TextField(_outputText, "Label");
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            
            // TODO: SECURITY: userCommand should be validated 
            GUI.SetNextControlName("InputField");
            _userCommand = GUI.TextField(new Rect(0, h - 50, w, 50), _userCommand, 2048);*/
        }
    }
}