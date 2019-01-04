using InControl;
using UnityEngine;

namespace Com.Tempest.Nightmare {

	public class CharacterInputManager : Photon.MonoBehaviour {

		private GeneratedGameManager managerBehavior;
		private DemoSceneManager demoBehavior;
		private ActionSet actionSet;
		private IControllable controllable;

		private void Awake() {
			GetComponent<TouchManager>().enabled = Application.platform == RuntimePlatform.Android;

			managerBehavior = GetComponent<GeneratedGameManager>();
			demoBehavior = GetComponent<DemoSceneManager>();
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

			actionSet.ActionPrimary.AddDefaultBinding(Key.Space);
			actionSet.ActionPrimary.AddDefaultBinding(Mouse.LeftButton);
			actionSet.ActionPrimary.AddDefaultBinding(InputControlType.Action1);

			actionSet.ActionSecondary.AddDefaultBinding(Key.Backspace);
			actionSet.ActionSecondaryMouse.AddDefaultBinding(Mouse.RightButton);
			actionSet.ActionSecondary.AddDefaultBinding(InputControlType.Action2);

			actionSet.ActivateLight.AddDefaultBinding(Key.Control);
			actionSet.ActivateLight.AddDefaultBinding(Mouse.MiddleButton);
			actionSet.ActivateLight.AddDefaultBinding(InputControlType.LeftBumper);
			actionSet.ActivateLight.AddDefaultBinding(InputControlType.LeftTrigger);

			actionSet.Grab.AddDefaultBinding(Key.Shift);
			actionSet.Grab.AddDefaultBinding(Mouse.Button4);
			actionSet.Grab.AddDefaultBinding(InputControlType.RightBumper);
			actionSet.Grab.AddDefaultBinding(InputControlType.RightTrigger);
		}

		private void OnDestroy() {
			actionSet.Destroy();
		}

		private void FixedUpdate() {
			InputDevice inputDevice = InputManager.ActiveDevice;
			if (inputDevice == null)
				return;
			if (controllable == null) {
				if (managerBehavior != null) {
					controllable = managerBehavior.GetControllableCharacter();
				} else if (demoBehavior != null) {
					controllable = demoBehavior.GetControllableCharacter();
				}
			}

			if (controllable == null) {
				Camera.main.transform.position += new Vector3(actionSet.MoveX.Value / 2f, actionSet.MoveY.Value / 2f);
			} else {
				controllable.InputsReceived(actionSet.MoveX.Value, actionSet.MoveY.Value, actionSet.Grab.IsPressed);
				if (actionSet.ActionPrimary.WasPressed) {
					controllable.ActionPrimaryPressed();
				}
				if (actionSet.ActionPrimary.WasReleased) {
					controllable.ActionPrimaryReleased();
				}
				if (actionSet.ActionSecondary.WasPressed) {
					controllable.ActionSecondaryPressed(new Vector3());
				}
				if (actionSet.ActionSecondaryMouse.WasPressed) {
					Vector3 direction = new Vector3(Input.mousePosition.x - (Screen.width / 2f), Input.mousePosition.y - (Screen.height / 2f));
					controllable.ActionSecondaryPressed(direction);
				}
				if (actionSet.ActionSecondary.WasReleased) {
					controllable.ActionSecondaryReleased();
				}
				if (actionSet.ActivateLight.WasPressed) {
					controllable.LightTogglePressed();
				}
			}
		}
	}
}
