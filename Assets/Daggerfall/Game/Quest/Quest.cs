﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DaggerfallWorkshop;
using System;

namespace Daggerfall.Game.Quest { 
    public class Quest : MonoBehaviour {
        public Dictionary<string, string> resources = new Dictionary<string, string>();
        public List<KeyValuePair<string, string>> QBN = new List<KeyValuePair<string, string>>();
        public List<Task> tasks = new List<Task>();
        public List<string> cdata = new List<string>();

        private Dictionary<string, GameTimer> timers;


        public Quest() { 
            //worldTime = DaggerfallUnity.Instance.WorldTime;
            //timers = new Dictionary<string,GameTimer>();
        }

        // Use this for initialization
        void Start () {
        }
        
        // Update is called once per frame
        void Update () {
        
        }

        public void createTimers() { 
            timers = new Dictionary<string, GameTimer>();
            foreach (KeyValuePair<string, string> qbnItem in QBN) { 
                if (qbnItem.Key == "clock") {
                    string[] pieces = qbnItem.Value.Split(' ');

                    GameTimer timer = new GameTimer(DaggerfallUnity.Instance.WorldTime);
                    timer.id = pieces[1];

                    string[] durationPieces = pieces[2].Split(':');
                    timer.durationSeconds = Int32.Parse(durationPieces[0]) * 3600 + Int32.Parse(durationPieces[1]) * 60;
                    
                    for (int x=3; x<pieces.Length; x++) {
                        timer.unknownInformation.Add(pieces[x]);
                    }
                    timers.Add(timer.id, timer);

                    //Logger.GetInstance().log(timer.dumpTimer());
                }
            }
        }

        public void dumpAllTimers() { 
            foreach (KeyValuePair<string, GameTimer> kvp in timers) {
                Logger.GetInstance().log(kvp.Value.dumpTimer());
            }
        }

        public void startAllTimers() { 
            foreach (KeyValuePair<string, GameTimer> kvp in timers) {
                kvp.Value.start();
            }
        }

        public string dumpQuest() {
            string output = "";
            foreach (string datum in cdata) { 
                output += "CDATA: " + datum + "\n";
             }

            foreach (KeyValuePair<string, string> kvp in resources) { 
                output += "sid: " + kvp.Key + ". Value: " + kvp.Value + "\n";
            }

            foreach (KeyValuePair<string, string> kvp in QBN) { 
                output += "tag: " + kvp.Key + ". Value: " + kvp.Value + "\n";
            }

            foreach (Task task in tasks) {
                output += "On task.";
                foreach (KeyValuePair<string, string> attribute in task.attributes) {
                    output += "Attribute: " + attribute.Key + ", value: " + attribute.Value + "\n";
                }
                foreach (string command in task.commands) {
                    output += "Command: " + command + "\n";
                }
            }
            return output;
        }
    }
}