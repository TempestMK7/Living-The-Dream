using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public abstract class EmpowerableCharacterBehavior : Photon.PunBehaviour {

        public float powerupDuration = 60f;

        private Dictionary<Powerup, float> powerupDictionary;

        protected void CheckPowerups() {
            foreach (Powerup p in powerupDictionary.Keys) {
                if (Time.time - powerupDictionary[p] < powerupDuration) {
                    powerupDictionary.Remove(p);
                }
            }
        }

        public void AddPowerup(Powerup p) {
            powerupDictionary.Add(p, Time.time);
        }

        public void AddRandomPowerup() {
            Powerup[] possiblePowerups = GetUsablePowerups();
            AddPowerup(possiblePowerups[Random.Range(0, possiblePowerups.Length)]);
        }

        public bool HasPowerup(Powerup p) {
            return powerupDictionary.ContainsKey(p);
        }

        protected abstract Powerup[] GetUsablePowerups();
    }
}
