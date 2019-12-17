using System;
using InControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    public class LauncherManager : Photon.PunBehaviour {

        private const int JUMP = 0;
        private const int ACTION = 1;
        private const int LIGHT = 2;
        private const int CLING = 3;

        private const int UP = 4;
        private const int DOWN = 5;
        private const int LEFT = 6;
        private const int RIGHT = 7;

        public PhotonLogLevel logLevel = PhotonLogLevel.ErrorsOnly;
        public GameObject startPanel;
        public GameObject connectPanel;
        public GameObject settingsPanel;

        public GameObject progressLabel;
        public Button exitButton;
        public Text versionText;

        public InputField nameInputField;
        public Text publicLabel;
		public Button connectExplorerButton;
		public Button connectNightmareButton;
		public Button connectObserverButton;

        public Text privateLabel;
        public InputField lobbyInputField;
        public Button connectPrivateButton;

        public Slider musicSlider;
        public Slider effectSlider;

        public Button keyboardUp;
        public Button keyboardDown;
        public Button keyboardLeft;
        public Button keyboardRight;

        public Button keyboardJump;
        public Button keyboardAction;
        public Button keyboardLight;
        public Button keyboardCling;

        private bool isConnecting;
        private bool isPrivate;
        private string lobbyName;
        private bool isRebinding;
        private int inputRebinding;

        private LobbyMusicBehavior lobbyMusicBehavior;
        
	    public void Awake() {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            exitButton.gameObject.SetActive(
                Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.LinuxPlayer);
            
            PhotonNetwork.logLevel = logLevel;
            PhotonNetwork.autoJoinLobby = false;
            PhotonNetwork.automaticallySyncScene = true;
            PhotonNetwork.autoCleanUpPlayerObjects = true;
            PhotonNetwork.sendRate = 30;
            PhotonNetwork.sendRateOnSerialize = 30;

            OpenStartPanel();
            versionText.text = "Game Version: " + Constants.GAME_VERSION;
            PlayerStateContainer.ResetInstance();

            lobbyMusicBehavior = FindObjectOfType<LobbyMusicBehavior>();
            lobbyMusicBehavior.StartMusic();
        }

        public void Update() {
            if (isRebinding) {
                CheckForRebinds();
            }
            HandleConnectButtons();
        }

        #region Panel and Button Handling

        private void HandleConnectButtons() {
            bool nameEntered = nameInputField.text.Length != 0;
            publicLabel.gameObject.SetActive(nameEntered);
            connectExplorerButton.gameObject.SetActive(nameEntered);
            connectNightmareButton.gameObject.SetActive(nameEntered);
            connectObserverButton.gameObject.SetActive(nameEntered);
            privateLabel.gameObject.SetActive(nameEntered);
            lobbyInputField.gameObject.SetActive(nameEntered);

            bool canConnectPrivate = nameEntered && lobbyInputField.text.Length != 0;
            connectPrivateButton.gameObject.SetActive(canConnectPrivate);
        }

        public void ExitGame() {
            Application.Quit();
        }

        public void OpenStartPanel() {
            isRebinding = false;
            startPanel.SetActive(true);
            connectPanel.SetActive(false);
            settingsPanel.SetActive(false);
            progressLabel.SetActive(false);
        }

        public void OpenConnectPanel() {
            startPanel.SetActive(false);
            connectPanel.SetActive(true);
            settingsPanel.SetActive(false);
            progressLabel.SetActive(false);
        }

        public void OpenProgressionPanel() {
            SceneManager.LoadScene("TalentScene");
        }

        public void OpenSettingsPanel() {
            startPanel.SetActive(false);
            connectPanel.SetActive(false);
            settingsPanel.SetActive(true);
            progressLabel.SetActive(false);

            ControlBindingContainer container = ControlBindingContainer.GetInstance();

            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.value = container.musicVolume;
            effectSlider.minValue = 0f;
            effectSlider.maxValue = 1f;
            effectSlider.value = container.effectVolume;
            
            keyboardUp.GetComponentInChildren<Text>().text = container.upKey.ToString();
            keyboardDown.GetComponentInChildren<Text>().text = container.downKey.ToString();
            keyboardLeft.GetComponentInChildren<Text>().text = container.leftKey.ToString();
            keyboardRight.GetComponentInChildren<Text>().text = container.rightKey.ToString();
            keyboardJump.GetComponentInChildren<Text>().text = container.jumpKey.ToString();
            keyboardAction.GetComponentInChildren<Text>().text = container.actionKey.ToString();
            keyboardLight.GetComponentInChildren<Text>().text = container.lightKey.ToString();
            keyboardCling.GetComponentInChildren<Text>().text = container.clingKey.ToString();
        }

        public void CloseAllPanels() {
            startPanel.SetActive(false);
            connectPanel.SetActive(false);
            settingsPanel.SetActive(false);
        }

        #endregion

        #region Key Bindings.

        public void ResetBindings() {
            isRebinding = false;
            ControlBindingContainer.ResetInstance();
            OpenSettingsPanel();
        }

        public void ListenForKeyBind(int inputType) {
            isRebinding = true;
            inputRebinding = inputType;
        }

        private void CheckForRebinds() {
            Key selectedKey = Key.None;
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
                if (Input.GetKeyDown(keyCode)) {
                    selectedKey = Array.Find(KeyInfo.KeyList, keyInfo => Array.Exists(keyInfo.keyCodes, containedCode => containedCode == keyCode)).Key;
                }
            }
            if (selectedKey != Key.None) {
                switch (inputRebinding) {
                    case UP:
                        ControlBindingContainer.GetInstance().upKey = selectedKey;
                        break;
                    case DOWN:
                        ControlBindingContainer.GetInstance().downKey = selectedKey;
                        break;
                    case LEFT:
                        ControlBindingContainer.GetInstance().leftKey = selectedKey;
                        break;
                    case RIGHT:
                        ControlBindingContainer.GetInstance().rightKey = selectedKey;
                        break;
                    case JUMP:
                        ControlBindingContainer.GetInstance().jumpKey = selectedKey;
                        break;
                    case ACTION:
                        ControlBindingContainer.GetInstance().actionKey = selectedKey;
                        break;
                    case LIGHT:
                        ControlBindingContainer.GetInstance().lightKey = selectedKey;
                        break;
                    case CLING:
                        ControlBindingContainer.GetInstance().clingKey = selectedKey;
                        break;
                }
                ControlBindingContainer.SaveInstance();
                isRebinding = false;
                OpenSettingsPanel();
            }
        }

        public void SetVolume() {
            ControlBindingContainer.GetInstance().musicVolume = musicSlider.value;
            ControlBindingContainer.SaveInstance();
            lobbyMusicBehavior.LoadVolume();
        }

        public void SetEffectVolume() {
            ControlBindingContainer.GetInstance().effectVolume = effectSlider.value;
            ControlBindingContainer.SaveInstance();
        }

        #endregion

        #region Photon Connection

        public void LaunchDemoScene() {
            SceneManager.LoadScene("DemoScene");
        }

        public void ConnectAsExplorer() {
            PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.EXPLORER;
            isPrivate = false;
            Connect();
        }

        public void ConnectAsNightmare() {
            PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.NIGHTMARE;
            isPrivate = false;
            Connect();
        }

        public void ConnectAsObserver() {
            PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.OBSERVER;
            isPrivate = false;
            Connect();
        }

        public void ConnectAsPrivate() {
            PlayerStateContainer.Instance.TeamSelection = PlayerStateContainer.OBSERVER;
            isPrivate = true;
            lobbyName = lobbyInputField.text.ToLower();
            if (lobbyName != null && lobbyName.Length != 0) Connect();
        }

        private void Connect() {
            isConnecting = true;
            CloseAllPanels();
            progressLabel.SetActive(true);
            if (!PhotonNetwork.connected) {
                PhotonNetwork.ConnectUsingSettings(Constants.GAME_VERSION);
            } else {
                JoinLobby();
            }
        }

        public override void OnConnectedToMaster() {
            if (isConnecting) {
                JoinLobby();
            }
        }

        private void JoinLobby() {
            PhotonNetwork.JoinLobby(new TypedLobby(Constants.LOBBY_NAME, LobbyType.SqlLobby));
        }

        public override void OnJoinedLobby() {
            PhotonPlayer player = PhotonNetwork.player;
			ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable();
			properties[PlayerStateContainer.TEAM_SELECTION] = PlayerStateContainer.Instance.TeamSelection;
            PlayerStateContainer.Instance.IsReady = PlayerStateContainer.STATUS_NOT_READY;
			properties[PlayerStateContainer.IS_READY] = PlayerStateContainer.Instance.IsReady;
			player.SetCustomProperties(properties);
            if (isPrivate) {
                RoomOptions options = new RoomOptions {
                    IsOpen = true,
                    IsVisible = false,
                    MaxPlayers = 0,
                    PublishUserId = true,
                    CustomRoomPropertiesForLobby = new string[] { "C0", "C1" },
                    CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "C0", 1 }, { "C1", 1 } }
                };
                PhotonNetwork.JoinOrCreateRoom(lobbyName, options, new TypedLobby(Constants.LOBBY_NAME, LobbyType.SqlLobby));
            } else if (PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.EXPLORER) {
                string filter = "C0 = 1";
                PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, new TypedLobby(Constants.LOBBY_NAME, LobbyType.SqlLobby), filter);
            } else if (PlayerStateContainer.Instance.TeamSelection == PlayerStateContainer.NIGHTMARE) {
                string filter = "C1 = 1";
                PhotonNetwork.JoinRandomRoom(null, 0, MatchmakingMode.FillRoom, new TypedLobby(Constants.LOBBY_NAME, LobbyType.SqlLobby), filter);
            } else {
                PhotonNetwork.JoinRandomRoom();
            }
        }

        public override void OnDisconnectedFromPhoton() {
            OpenStartPanel();
            progressLabel.SetActive(false);
        }

        public override void OnPhotonRandomJoinFailed(object[] codeAndMsg) {
            Debug.Log("Random join failed. Creating new room.");
            RoomOptions options = new RoomOptions();
            options.IsOpen = true;
            options.IsVisible = true;
            options.MaxPlayers = 0;
            options.PublishUserId = true;
            options.CustomRoomPropertiesForLobby = new string[]{ "C0", "C1" };
            options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable(){{ "C0", 1 }, { "C1", 1 }};
            PhotonNetwork.CreateRoom(null, options, new TypedLobby(Constants.LOBBY_NAME, LobbyType.SqlLobby));
        }

        public override void OnJoinedRoom() {
            Debug.Log("This client is now in a room.");
            if (PhotonNetwork.room.PlayerCount == 1) {
                PhotonNetwork.LoadLevel("LobbyScene");
            }
        }

        #endregion
    }
}
