using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Com.Tempest.Nightmare {

    public enum BaseTalent {
        // Level 1
        VISION_RANGE,
        POWERUP_DURATION,

        // Level 2
        UPGRADE_EFFECT,
        OBJECTIVE_SPEED,
        COOLDOWN_REDUCTION,

        // Level 3
        MOVEMENT_SPEED,
        JUMP_POWER,
        ACCELERATION,

        // Ultimate
        REDUCED_GRAVITY,
        JETPACK_FORCE,
        RESET_DASH_ON_WALL_SLIDE,
        CANCEL_WALL_REFLECTION,
        CANCEL_PROJECTILE_GRAVITY,
        WALL_CLIMB
    }
}
