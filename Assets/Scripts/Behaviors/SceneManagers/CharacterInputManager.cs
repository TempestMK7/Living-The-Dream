using InControl;
using UnityEngine;

namespace Com.Tempest.Nightmare {

	public class CharacterInputManager : Photon.MonoBehaviour {

		private GeneratedGameManager managerBehavior;
		private DemoSceneManager demoBehavior;
		private ActionSet actionSet;
		private IControllable controllable;

		private bool isPaused = false;

		private void Awake() {
			managerBehavior = GetComponent<GeneratedGameManager>();
			demoBehavior = GetComponent<DemoSceneManager>();
			ResetActionSet();
		}

		private void OnDestroy() {
			actionSet.Destroy();
		}

		public void PauseInputs() {
			isPaused = true;
		}

		public void UnpauseInputs() {
			isPaused = false;
		}

		private void FixedUpdate() {
			if (actionSet.Menu.WasPressed) {
				if (managerBehavior != null) {
					managerBehavior.ToggleSettingsPanel();
				}
			}

			// This object can be paused by anything else that relies on player inputs such as the Settings Panel.
			if (isPaused) return;

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
					controllable.ActionPrimaryPressed(new Vector3());
				}
				if (actionSet.ActionPrimary.WasReleased) {
					controllable.ActionPrimaryReleased();
				}
				if (actionSet.ActionPrimaryMouse.WasPressed) {
					Vector3 direction = new Vector3(Input.mousePosition.x - (Screen.width / 2f), Input.mousePosition.y - (Screen.height / 2f));
					controllable.ActionPrimaryPressed(direction);
				}
				if (actionSet.ActionPrimaryMouse.WasReleased) {
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
				if (actionSet.ActionSecondaryMouse.WasReleased) {
					controllable.ActionSecondaryReleased();
				}
				if (actionSet.ActivateLight.WasPressed) {
					controllable.LightTogglePressed();
				}
			}
		}

		public void ClearControllable() {
			controllable = null;
		}

		public void ResetActionSet() {
			actionSet = ControlBindingContainer.GetInstance().GetActionSet();
		}
	}
}
