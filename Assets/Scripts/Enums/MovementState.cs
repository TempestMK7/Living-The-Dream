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
        PARRYING,
        HIT_FREEZE,
        HIT_STUN,
        RAG_DOLL
    }
}