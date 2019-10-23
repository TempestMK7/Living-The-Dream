using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Items {
    public enum ItemEnum {

        NONE,

        // Shoes for gravity-bound characters.
        BASIC_SHOES,
        SNEAKERS,
        RUNNING_SHOES,
        BASKET_BALL_SHOES,

        // Shoes for flyers.
        WIND_STONE,
        TORNADO_STONE,
        GALE_STONE,

        // Health items for explorers.
        BASIC_ARMOR,
        // Small HP, movement bonuses.
        LEATHER_ARMOR,
        FITTED_LEATHER_ARMOR,
        LIGHT_WEIGHT_ARMOR,
        // Large HP, movement penalties.
        METAL_ARMOR,
        IRON_CHAIN_ARMOR,
        MITHRIL_CHAIN_ARMOR,

        // Attack items for nightmares.
        ICE_STONE,
        // Small attack, movement debuffs on hit or proximity.
        CHILLING_STONE,
        FREEZING_STONE,
        DEEP_FREEZING_STONE,
        // Large attack, little-no movement debuff.
        ICICLE,
        ICE_SHARD,
        ICE_BLADE
    }
}
