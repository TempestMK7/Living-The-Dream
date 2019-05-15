using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public class LobbySynchronizerBehavior : Photon.PunBehaviour, IPunObservable {

        private LobbyManagerBehavior lobbyBehavior;

        void Awake() {
            lobbyBehavior = FindObjectOfType<LobbyManagerBehavior>();
        }

        public void NotifyTeamChange() {
            lobbyBehavior.RefreshPlayerInformation(false);
            LobbySynchronizerBehavior[] synchronizers = FindObjectsOfType<LobbySynchronizerBehavior>();
            foreach (LobbySynchronizerBehavior behavior in synchronizers) {
                behavior.photonView.RPC("OnNotifyTeamChange", PhotonTargets.Others);
            }
        }

        [PunRPC]
        public void OnNotifyTeamChange() {
            if (photonView.isMine) {
                lobbyBehavior.RefreshPlayerInformation(false);
            }
        }

        public void NotifyReadyChange() {
            lobbyBehavior.RefreshPlayerInformation(false);
            LobbySynchronizerBehavior[] synchronizers = FindObjectsOfType<LobbySynchronizerBehavior>();
            foreach (LobbySynchronizerBehavior behavior in synchronizers) {
                behavior.photonView.RPC("OnNotifyReadyChange", PhotonTargets.Others);
            }
        }

        [PunRPC]
        public void OnNotifyReadyChange() {
            if (photonView.isMine) {
                lobbyBehavior.RefreshPlayerInformation(false);
            }
        }

        public void ResetTeamSelectionForPlayer(string userId) {
            LobbySynchronizerBehavior[] synchronizers = FindObjectsOfType<LobbySynchronizerBehavior>();
            foreach (LobbySynchronizerBehavior behavior in synchronizers) {
                behavior.photonView.RPC("OnResetTeamSelection", PhotonTargets.All, userId);
            }
        }

        [PunRPC]
        public void OnResetTeamSelection(string userId) {
            if (photonView.isMine && userId.Equals(PhotonNetwork.player.UserId)) {
                lobbyBehavior.ResetSelection();
            }
        }

        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
            
        }
    }
}
