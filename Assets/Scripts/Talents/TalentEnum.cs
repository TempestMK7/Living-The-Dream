using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Tempest.Nightmare {
    public enum TalentEnum {

        // General Talents.
        SIGHT_RANGE,
        UPGRADE_EFFECTIVENESS,
        CHEST_DURATION,
        CHEST_LOCATOR,
        MIRROR_ACTIVATION,
        MIRROR_FADE_DELAY,
        JUMP_HEIGHT_OR_ACCELERATION,
        MOVEMENT_SPEED,

        // Explorer Talents.
        BONFIRE_SPEED,
        FIRST_BONFIRE_SPEED,
        PORTAL_NOTIFICATIONS,
        BONFIRE_PROGRESS_NOTIFICATIONS,
        PORTAL_COOLDOWN_REDUCTION,
        HEALTH_STATION_COOLDOWN_REDUCTION,
        FASTER_FALL_SPEED,
        RESET_MOVEMENT_ON_WALL_SLIDE,

        // Nightmare Talents.
        EXPLORER_HEAL_NOTIFICATIONS,
        MIRROR_LOCK,
        EXPLORER_SAVE_NOTIFICATIONS,
        HEALTH_STATION_DENIAL,
        BONFIRE_LOCK,
        PORTAL_LOCK,
        ATTACK_COOLDOWN,
        FIRST_HIT_DAMAGE
    }
}
