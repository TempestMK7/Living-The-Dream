using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Tempest.Nightmare {
    public class TalentInfoDictionaryContainer {

        public static Dictionary<TalentEnum, TalentDescriptions> infoDictionary = new Dictionary<TalentEnum, TalentDescriptions> {
            // General Talents.
            { TalentEnum.SIGHT_RANGE, new TalentDescriptions("Sight Range", "Increases your base sight range while your light is turned off.  This will not increase the range of your light when activated.", 3, 50,
                new string[] { "Increases your base sight range by 5%.", "Increases your base sight range by 10%.", "Increases your base sight range by 15%." }) },
            { TalentEnum.UPGRADE_EFFECTIVENESS, new TalentDescriptions("Upgrade Effectiveness", "Increases the effectiveness of upgrades on your movement or attack.  This will affect all upgrades collected by you and your teammates, but it will not increase your teammates' movement.", 3, 50,
                new string[] { "Increases the effect of upgrades by 5%.", "Increases the effect of upgrades by 10%.", "Increases the effect of upgrades by 15%." }) },
            { TalentEnum.CHEST_DURATION, new TalentDescriptions("Chest Powerup Duration", "Increases the amount of time that chest powerups affect you for.  The base powerup time is 30 seconds.", 3, 100,
                new string[] { "Increase powerup duration by 5 seconds.", "Increase powerup duration by 10 seconds.", "Increase powerup duration by 15 seconds." }) },
            { TalentEnum.CHEST_LOCATOR, new TalentDescriptions("Chest Locator", "Mirrors will now display the positions of chests when activated.", 2, 100,
                new string[] { "Mirrors will display closed chests.", "Mirrors will also display opened chests." }) },
            { TalentEnum.MIRROR_ACTIVATION, new TalentDescriptions("Mirror Activation Speed", "Reduces the amount of time needed to activate mirrors.  The base activation time is 5 seconds.", 3, 200,
                new string[] { "Mirrors only take 4 seconds to activate.", "Mirrors only take 2 seconds to activate.", "Mirrors activate instantly." }) },
            { TalentEnum.MIRROR_FADE_DELAY, new TalentDescriptions("Mirror Fade Delay", "Mirrors will continue to show you object locations for a few seconds after to move away from them.", 3, 200,
                new string[] { "Mirrors will continue to show objects for 3 seconds.", "Mirrors will continue to show objects for 6 seconds.", "Mirrors will continue to show objects for 10 seconds." }) },
            { TalentEnum.JUMP_HEIGHT_OR_ACCELERATION, new TalentDescriptions("Increased Agility", "This increases the base jump speed of non-flying characters and the acceleration speed of flying characters.", 3, 300,
                new string[] { "Jump speed or acceleration speed are increased by 5%.", "Jump speed or acceleration speed are increased by 10%.", "Jump speed or acceleration speed are increased by 15%." }) },
            { TalentEnum.MOVEMENT_SPEED, new TalentDescriptions("Movement Speed", "Increased the base movement speed for your character.  I fully expect this talent to break the game, but I'm running out of ideas.", 3, 300,
                new string[] { "Increases base movement speed by 3%.", "Increases base movement speed by 6%.", "Increases base movement speed by 10%." }) },

            // Explorer Talents.
            { TalentEnum.BONFIRE_SPEED, new TalentDescriptions("Bonfire Speed", "", 3, 50,
                new string[] { "", "", "" }) },
            { TalentEnum.FIRST_BONFIRE_SPEED, new TalentDescriptions("First Bonfire Bonus", "", 3, 50,
                new string[] { "", "", "" }) },
            { TalentEnum.PORTAL_NOTIFICATIONS, new TalentDescriptions("Portal Notifications", "", 2, 100,
                new string[] { "", "", "" }) },
            { TalentEnum.BONFIRE_PROGRESS_NOTIFICATIONS, new TalentDescriptions("Bonfire Progress Notifications", "", 1, 100,
                new string[] { "", "", "" }) },
            { TalentEnum.PORTAL_COOLDOWN_REDUCTION, new TalentDescriptions("Portal Cooldown", "", 3, 200,
                new string[] { "", "", "" }) },
            { TalentEnum.HEALTH_STATION_COOLDOWN_REDUCTION, new TalentDescriptions("Health Station Cooldown", "", 3, 200,
                new string[] { "", "", "" }) },
            { TalentEnum.FASTER_FALL_SPEED, new TalentDescriptions("Faster Fall Speed", "", 3, 300,
                new string[] { "", "", "" }) },
            { TalentEnum.RESET_MOVEMENT_ON_WALL_SLIDE, new TalentDescriptions("Reset Movement Ability On Wall Slide", "", 1, 300,
                new string[] { "", "", "" }) },
        };

        public class TalentDescriptions {
            public string Name { get; set; }
            public string Description { get; set; }
            public int NumRanks { get; set; }
            public int BaseCost { get; set; }
            public string[] LevelDescriptions { get; set; }

            public TalentDescriptions(string name, string description, int numRanks, int baseCost, string[] levelDescriptions) {
                Name = name;
                Description = description;
                NumRanks = numRanks;
                BaseCost = baseCost;
                LevelDescriptions = levelDescriptions;
            }
        }
    }
}
