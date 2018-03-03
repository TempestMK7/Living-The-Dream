namespace Com.Tempest.Nightmare {

    public interface IControllable {

        void SendInputs(float horizontalAxis, float verticalAxis, bool grabHeld);
        void SendAction();
    }
}