namespace Com.Tempest.Nightmare {

    public interface IControllable {

        void InputsReceived(float horizontalAxis, float verticalAxis, bool grabHeld);
        void ActionPressed();
        void ActionReleased();
        void LightTogglePressed();
    }
}