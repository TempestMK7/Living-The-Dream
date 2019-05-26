using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace Com.Tempest.Nightmare {

    public class GlobalTalentContainer {

        private static GlobalTalentContainer Instance;

        public int UnspentEmbers { get; set; }
        public Dictionary<TalentEnum, int> DoubleJumpTalents { get; set; }
        public Dictionary<TalentEnum, int> JetpackTalents { get; set; }
        public Dictionary<TalentEnum, int> DashTalents { get; set; }
        public Dictionary<TalentEnum, int> GhastTalents { get; set; }
        public Dictionary<TalentEnum, int> CryoTalents { get; set; }
        public Dictionary<TalentEnum, int> GoblinTalents { get; set; }

        private static GlobalTalentContainer BuildNewContainer() {
            GlobalTalentContainer output = new GlobalTalentContainer();
            output.UnspentEmbers = 0;

            output.DoubleJumpTalents = new Dictionary<TalentEnum, int>();
            output.JetpackTalents = new Dictionary<TalentEnum, int>();
            output.DashTalents = new Dictionary<TalentEnum, int>();

            output.GhastTalents = new Dictionary<TalentEnum, int>();
            output.CryoTalents = new Dictionary<TalentEnum, int>();
            output.GoblinTalents = new Dictionary<TalentEnum, int>();

            foreach (TalentEnum talent in Enum.GetValues(typeof(TalentEnum))) {
                output.DoubleJumpTalents[talent] = 0;
                output.JetpackTalents[talent] = 0;
                output.DashTalents[talent] = 0;
                output.GhastTalents[talent] = 0;
                output.CryoTalents[talent] = 0;
                output.GoblinTalents[talent] = 0;
            }

            return output;
        }

        public static void SaveInstance() {
            string dataPath = Path.Combine(Application.persistentDataPath, Constants.TALENT_FILE_NAME);
            using (StreamWriter writer = File.CreateText(dataPath)) {
                writer.Write(JsonConvert.SerializeObject(Instance));
            }
        }

        private static void LoadContainer() {
            string dataPath = Path.Combine(Application.persistentDataPath, Constants.TALENT_FILE_NAME);
            if (File.Exists(dataPath)) {
                using (StreamReader reader = File.OpenText(dataPath)) {
                    string jsonString = reader.ReadToEnd();
                    Instance = JsonConvert.DeserializeObject<GlobalTalentContainer>(jsonString);
                }
            } else {
                Instance = BuildNewContainer();
            }
        }

        public static GlobalTalentContainer GetInstance() {
            if (Instance == null) LoadContainer();
            return Instance;
        }
    }
}
