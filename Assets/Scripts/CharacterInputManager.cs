using InControl;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    [RequireComponent(typeof(GameManagerBehavior))]
    public class CharacterInputManager : Photon.MonoBehaviour {

        private GameManagerBehavior managerBehavior;
        private ActionSet actionSet;

        private void Awake() {
            managerBehavior = GetComponent<GameManagerBehavior>();
            actionSet = new ActionSet();

            actionSet.Left.AddDefaultBinding(Key.LeftArrow);
            actionSet.Left.AddDefaultBinding(Key.A);
            actionSet.Left.AddDefaultBinding(InputControlType.DPadLeft);
            actionSet.Left.AddDefaultBinding(InputControlType.LeftStickLeft);

            actionSet.Right.AddDefaultBinding(Key.RightArrow);
            actionSet.Right.AddDefaultBinding(Key.D);
            actionSet.Right.AddDefaultBinding(InputControlType.DPadRight);
            actionSet.Right.AddDefaultBinding(InputControlType.LeftStickRight);

            actionSet.Up.AddDefaultBinding(Key.UpArrow);
            actionSet.Up.AddDefaultBinding(Key.W);
            actionSet.Up.AddDefaultBinding(InputControlType.DPadUp);
            actionSet.Up.AddDefaultBinding(InputControlType.LeftStickUp);

            actionSet.Down.AddDefaultBinding(Key.DownArrow);
            actionSet.Down.AddDefaultBinding(Key.S);
            actionSet.Down.AddDefaultBinding(InputControlType.DPadDown);
            actionSet.Down.AddDefaultBinding(InputControlType.LeftStickDown);

            actionSet.Action.AddDefaultBinding(Key.Space);
            actionSet.Action.AddDefaultBinding(InputControlType.Action1);

            actionSet.ActivateLight.AddDefaultBinding(Key.Control);
            actionSet.ActivateLight.AddDefaultBinding(InputControlType.LeftBumper);
            actionSet.ActivateLight.AddDefaultBinding(InputControlType.Action2);

            actionSet.Grab.AddDefaultBinding(Key.Shift);
            actionSet.Grab.AddDefaultBinding(InputControlType.RightBumper);
        }

        private void OnDestroy() {
            actionSet.Destroy();
        }

        private void FixedUpdate() {
            InputDevice inputDevice = InputManager.ActiveDevice;
            if (inputDevice == null) return;
            IControllable controllable = managerBehavior.GetControllableCharacter();
            if (controllable == null) {
                Camera.main.transform.position += new Vector3(actionSet.MoveX.Value / 2f, actionSet.MoveY.Value / 2f);
            } else {
                controllable.SendInputs(actionSet.MoveX.Value, actionSet.MoveY.Value, actionSet.Grab.IsPressed);
                if (actionSet.Action.WasPressed) {
                    controllable.SendAction();
                }
                if (actionSet.ActivateLight.WasPressed) {
                    controllable.SendLightToggle();
                }
            }
        }
    }
}

    
