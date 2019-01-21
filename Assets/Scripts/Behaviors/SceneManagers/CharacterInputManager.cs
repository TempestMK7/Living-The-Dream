using InControl;
using UnityEngine;

namespace Com.Tempest.Nightmare {

	public class CharacterInputManager : Photon.MonoBehaviour {

		private GeneratedGameManager managerBehavior;
		private DemoSceneManager demoBehavior;
		private ActionSet actionSet;
		private IControllable controllable;

		private void Awake() {
			managerBehavior = GetComponent<GeneratedGameManager>();
			demoBehavior = GetComponent<DemoSceneManager>();
			actionSet = ControlBindingContainer.GetInstance().GetActionSet();
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
	}
}
