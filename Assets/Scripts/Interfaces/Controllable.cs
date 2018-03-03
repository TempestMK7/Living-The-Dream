using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public interface IControllable {

        void SendInputs(float horizontalAxis, float verticalAxis, bool grabHeld);
        void SendAction();
    }
}