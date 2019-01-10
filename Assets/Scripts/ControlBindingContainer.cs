using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

using InControl;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    [Serializable]
    public class ControlBindingContainer {
        
        private static ControlBindingContainer instance;

        public Key upKey;
        public Key downKey;
        public Key leftKey;
        public Key rightKey;

        public Key jumpKey;
        public Key actionKey;
        public Key lightKey;
        public Key clingKey;

        public Mouse jumpMouse;
        public Mouse actionMouse;
        public Mouse lightMouse;
        public Mouse clingMouse;

        public InputControlType jumpController;
        public InputControlType actionController;
        public InputControlType lightController;
        public InputControlType clingController;

        public float musicVolume;

        public ControlBindingContainer() {
            upKey = Key.W;
            downKey = Key.S;
            leftKey = Key.A;
            rightKey = Key.D;

            jumpKey = Key.Space;
            actionKey = Key.Backspace;
            lightKey = Key.Control;
            clingKey = Key.Shift;

            jumpMouse = Mouse.LeftButton;
            actionMouse = Mouse.RightButton;
            lightMouse = Mouse.MiddleButton;
            clingMouse = Mouse.Button4;

            jumpController = InputControlType.Action1;
            actionController = InputControlType.Action2;
            lightController = InputControlType.LeftBumper;
            clingController = InputControlType.RightBumper;

            musicVolume = 0.5f;
        }

        public ActionSet GetActionSet() {
            ActionSet aSet = new ActionSet();

            aSet.Up.AddDefaultBinding(upKey);
			aSet.Up.AddDefaultBinding(InputControlType.DPadUp);
			aSet.Up.AddDefaultBinding(InputControlType.LeftStickUp);

            aSet.Down.AddDefaultBinding(downKey);
			aSet.Down.AddDefaultBinding(InputControlType.DPadDown);
			aSet.Down.AddDefaultBinding(InputControlType.LeftStickDown);

            aSet.Left.AddDefaultBinding(leftKey);
			aSet.Left.AddDefaultBinding(InputControlType.DPadLeft);
			aSet.Left.AddDefaultBinding(InputControlType.LeftStickLeft);

            aSet.Right.AddDefaultBinding(rightKey);
			aSet.Right.AddDefaultBinding(InputControlType.DPadRight);
			aSet.Right.AddDefaultBinding(InputControlType.LeftStickRight);

			aSet.ActionPrimary.AddDefaultBinding(jumpKey);
			aSet.ActionPrimary.AddDefaultBinding(jumpMouse);
			aSet.ActionPrimary.AddDefaultBinding(jumpController);

			aSet.ActionSecondary.AddDefaultBinding(actionKey);
			aSet.ActionSecondaryMouse.AddDefaultBinding(actionMouse);
			aSet.ActionSecondary.AddDefaultBinding(actionController);

			aSet.ActivateLight.AddDefaultBinding(lightKey);
			aSet.ActivateLight.AddDefaultBinding(lightMouse);
			aSet.ActivateLight.AddDefaultBinding(lightController);

			aSet.Grab.AddDefaultBinding(clingKey);
			aSet.Grab.AddDefaultBinding(clingMouse);
			aSet.Grab.AddDefaultBinding(clingController);

            return aSet;
        }

        public static void SaveInstance() {
            string dataPath = Path.Combine(Application.persistentDataPath, Constants.CONTROL_FILE_NAME);
            using (StreamWriter writer = File.CreateText(dataPath)) {
                writer.Write(JsonUtility.ToJson(instance));
            }
        }

        public static void ReloadInstance() {
            instance = null;
            GetInstance();
        }

        public static ControlBindingContainer GetInstance() {
            if (instance != null) return instance;
            string dataPath = Path.Combine(Application.persistentDataPath, Constants.CONTROL_FILE_NAME);
            if (File.Exists(dataPath)) {
                using (StreamReader reader = File.OpenText(dataPath)) {
                    string jsonString = reader.ReadToEnd();
                    instance = JsonUtility.FromJson<ControlBindingContainer>(jsonString);
                }
            } else {
                instance = new ControlBindingContainer();
            }
            return instance;
        }

        public static void ResetInstance() {
            instance = new ControlBindingContainer();
            SaveInstance();
        }
    }
}
