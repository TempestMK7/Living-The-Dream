using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    [Serializable]
    public class AccountStateContainer {

        private static AccountStateContainer Instance;

        public int unspentEmbers;
        public string talentState;

        public static void SaveInstance() {
            string dataPath = Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_NAME);
            using (StreamWriter writer = File.CreateText(dataPath)) {
                writer.Write(JsonUtility.ToJson(Instance));
            }
        }

        public static void LoadInstance() {
            string dataPath = Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_NAME);
            if (File.Exists(dataPath)) {
                using (StreamReader reader = File.OpenText(dataPath)) {
                    string jsonString = reader.ReadToEnd();
                    Instance = JsonUtility.FromJson<AccountStateContainer>(jsonString);
                }
            } else {
                Instance = new AccountStateContainer();
            }
        }

        public static AccountStateContainer getInstance() {
            if (Instance == null) {
                LoadInstance();
            }
            return Instance;
        }
    }
}
