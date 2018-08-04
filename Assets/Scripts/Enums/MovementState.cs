using System.Collections;
using System.Collections.Generic;

namespace Com.Tempest.Nightmare {

    public enum MovementState {
        GROUNDED,
        JUMPING,
        FALLING,
        DASHING,
        WALL_SLIDE_LEFT,
        WALL_SLIDE_RIGHT,
        WALL_JUMP,
        DAMAGED,
        DYING
    }
}