using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using InControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {
    
    public class SettingsManager : MonoBehaviour {

        private const int JUMP = 0;
        private const int ACTION = 1;
        private const int LIGHT = 2;
        private const int CLING = 3;

        private const int UP = 4;
        private const int DOWN = 5;
        private const int LEFT = 6;
        private const int RIGHT = 7;

        public Button leaveMatchButton;

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

        private LobbyMusicBehavior lobbyMusicBehavior;
        private CharacterInputManager inputManager;
        private MusicManagerBehavior musicManager;

        private bool isRebinding;
        private int inputRebinding;
        
        void Awake() {
            lobbyMusicBehavior = FindObjectOfType<LobbyMusicBehavior>();
            inputManager = FindObjectOfType<CharacterInputManager>();
            musicManager = FindObjectOfType<MusicManagerBehavior>();
            InitializePanel();
        }

        void Update() {
            if (isRebinding) {
                CheckForRebinds();
            }
        }

        public void OnPanelLaunch(bool showLeaveButton) {
            if (inputManager != null) {
                inputManager.PauseInputs();
            }
            leaveMatchButton.gameObject.SetActive(showLeaveButton);
        }
        
        public void OnPanelClose() {
            if (inputManager != null) {
                inputManager.UnpauseInputs();
            }
            isRebinding = false;
        }

        public void InitializePanel() {
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

        public void ResetBindings() {
            isRebinding = false;
            ControlBindingContainer.ResetInstance();
            InitializePanel();
        }

        public void ListenForKeyBind(int inputType) {
            isRebinding = true;
            inputRebinding = inputType;
        }

        private void CheckForRebinds() {
            Key selectedKey = Key.None;
            foreach (KeyCode keyCode in Enum.GetValues(typeof(KeyCode))) {
                if (keyCode != KeyCode.Escape && Input.GetKeyDown(keyCode)) {
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
                InitializePanel();
                if (inputManager != null) {
                    inputManager.ResetActionSet();
                }
            }
        }

        public void SetVolume() {
            ControlBindingContainer.GetInstance().musicVolume = musicSlider.value;
            ControlBindingContainer.SaveInstance();
            lobbyMusicBehavior.LoadVolume();
            if (musicManager != null) {
                musicManager.ResetMusicVolume();
            }
        }

        public void SetEffectVolume() {
            ControlBindingContainer.GetInstance().effectVolume = effectSlider.value;
            ControlBindingContainer.SaveInstance();
        }
    }
}
