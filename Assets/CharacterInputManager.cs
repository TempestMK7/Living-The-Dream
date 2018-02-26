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

            actionSet.Grab.AddDefaultBinding(InputControlType.RightBumper);
            actionSet.Grab.AddDefaultBinding(Key.Shift);
        }

        private void OnDestroy() {
            actionSet.Destroy();
        }

        private void FixedUpdate() {
            InputDevice inputDevice = InputManager.ActiveDevice;
            if (inputDevice == null) return;
            NightmareBehavior nightmare = managerBehavior.Nightmare;
            DreamerBehavior dreamer = managerBehavior.Dreamer;
            if (nightmare != null) {
                nightmare.Accelerate(actionSet.MoveX.Value, actionSet.MoveY.Value);
                if (actionSet.Action.WasPressed) {
                    nightmare.Dash();
                }
            }
            if (dreamer != null) {
                dreamer.Accelerate(actionSet.MoveX.Value, actionSet.MoveY.Value, actionSet.Grab.IsPressed);
                if (actionSet.Action.WasPressed) {
                    dreamer.Jump();
                }
            }
        }
    }
}

    
