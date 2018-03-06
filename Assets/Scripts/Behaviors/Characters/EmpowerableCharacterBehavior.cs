﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public abstract class EmpowerableCharacterBehavior : Photon.PunBehaviour {

        public float powerupDuration = 60f;

        private Dictionary<Powerup, float> powerupDictionary;

        public virtual void Awake() {
            powerupDictionary = new Dictionary<Powerup, float>();
        }

        [PunRPC]
        public void AddPowerup(Powerup p) {
            powerupDictionary[p] = Time.time;
            if (photonView.isMine) {
                FindObjectOfType<GameManagerBehavior>().DisplayAlert("You have been granted " + p.ToString(), GameManagerBehavior.ALL);
            }
        }

        public void AddRandomPowerup() {
            if (!photonView.isMine) return;
            Powerup[] possiblePowerups = GetUsablePowerups();
            photonView.RPC("AddPowerup", PhotonTargets.All, possiblePowerups[Random.Range(0, possiblePowerups.Length)]);
        }

        public bool HasPowerup(Powerup p) {
            return powerupDictionary.ContainsKey(p) && Time.time - powerupDictionary[p] < powerupDuration;
        }

        protected abstract Powerup[] GetUsablePowerups();
    }
}
