namespace Com.Tempest.Nightmare {

    public interface IControllable {

        void InputsReceived(float horizontalAxis, float verticalAxis, bool grabHeld);
        void ActionPrimaryPressed();
        void ActionPrimaryReleased();
        void ActionSecondaryPressed();
        void ActionSecondaryReleased();
        void LightTogglePressed();
    }
}