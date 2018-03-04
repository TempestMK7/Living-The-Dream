using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public abstract class EmpowerableCharacterBehavior : Photon.PunBehaviour {

        public float powerupDuration = 60f;

        private Dictionary<Powerup, float> powerupDictionary;

        public virtual void Awake() {
            powerupDictionary = new Dictionary<Powerup, float>();
        }

        public virtual void Update() {
            CheckPowerups();
        }

        protected void CheckPowerups() {
            List<Powerup> removeThese = new List<Powerup>();
            foreach (Powerup p in powerupDictionary.Keys) {
                if (Time.time - powerupDictionary[p] > powerupDuration) {
                    removeThese.Add(p);
                }
            }
            foreach (Powerup p in removeThese) {
                powerupDictionary.Remove(p);
            }
        }

        [PunRPC]
        public void AddPowerup(Powerup p) {
            powerupDictionary.Add(p, Time.time);
        }

        public void AddRandomPowerup() {
            if (!photonView.isMine) return;
            Powerup[] possiblePowerups = GetUsablePowerups();
            photonView.RPC("AddPowerup", PhotonTargets.All, possiblePowerups[Random.Range(0, possiblePowerups.Length)]);
        }

        public bool HasPowerup(Powerup p) {
            return powerupDictionary.ContainsKey(p);
        }

        protected abstract Powerup[] GetUsablePowerups();
    }
}
