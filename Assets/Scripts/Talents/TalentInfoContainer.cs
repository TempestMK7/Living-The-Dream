﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Com.Tempest.Nightmare {
    public class TalentInfoContainer {

        public static Dictionary<TalentEnum, DescriptionContainer> infoDictionary = new Dictionary<TalentEnum, DescriptionContainer> {
            // General Talents.
            { TalentEnum.SIGHT_RANGE, new DescriptionContainer("Sight Range", "Increases your base sight range while your light is turned off.  This will not increase the range of your light when activated.", 3, 50,
                new string[] { "Increases your base sight range by 5%.", "Increases your base sight range by 10%.", "Increases your base sight range by 15%." }) },
            { TalentEnum.UPGRADE_EFFECTIVENESS, new DescriptionContainer("Upgrade Effectiveness", "Increases the effectiveness of upgrades on your movement or attack.  This will affect all upgrades collected by you and your teammates, but it will not increase your teammates' movement.", 3, 50,
                new string[] { "Increases the effect of upgrades by 5%.", "Increases the effect of upgrades by 10%.", "Increases the effect of upgrades by 15%." }) },
            { TalentEnum.CHEST_DURATION, new DescriptionContainer("Chest Powerup Duration", "Increases the amount of time that chest powerups affect you for.  The base powerup time is 30 seconds.", 3, 100,
                new string[] { "Increase powerup duration by 5 seconds.", "Increase powerup duration by 10 seconds.", "Increase powerup duration by 15 seconds." }) },
            { TalentEnum.CHEST_LOCATOR, new DescriptionContainer("Chest Locator", "Mirrors will now display the positions of chests when activated.", 2, 100,
                new string[] { "Mirrors will display closed chests.", "Mirrors will also display opened chests." }) },
            { TalentEnum.MIRROR_ACTIVATION, new DescriptionContainer("Mirror Activation Speed", "Reduces the amount of time needed to activate mirrors.  The base activation time is 5 seconds.", 3, 200,
                new string[] { "Mirrors only take 4 seconds to activate.", "Mirrors only take 2 seconds to activate.", "Mirrors activate instantly." }) },
            { TalentEnum.MIRROR_FADE_DELAY, new DescriptionContainer("Mirror Fade Delay", "Mirrors will continue to show you object locations for a few seconds after to move away from them.", 3, 200,
                new string[] { "Mirrors will continue to show objects for 3 seconds.", "Mirrors will continue to show objects for 6 seconds.", "Mirrors will continue to show objects for 9 seconds." }) },
            { TalentEnum.JUMP_HEIGHT_OR_ACCELERATION, new DescriptionContainer("Increased Agility", "This increases the base jump speed of non-flying characters and the acceleration speed of flying characters.", 3, 250,
                new string[] { "Jump speed or acceleration speed are increased by 5%.", "Jump speed or acceleration speed are increased by 10%.", "Jump speed or acceleration speed are increased by 15%." }) },
            { TalentEnum.MOVEMENT_SPEED, new DescriptionContainer("Movement Speed", "Increases the base movement speed for your character.  I fully expect this talent to break the game, but I'm running out of ideas.", 3, 250,
                new string[] { "Increases base movement speed by 3%.", "Increases base movement speed by 6%.", "Increases base movement speed by 10%." }) },

            // Explorer Talents.
            { TalentEnum.BONFIRE_SPEED, new DescriptionContainer("Bonfire Speed", "This increases the rate at which you light bonfires.", 3, 50,
                new string[] { "Increases bonfire speed by 5%.", "Increases bonfire speed by 10%.", "Increases bonfire speed by 15%." }) },
            { TalentEnum.FIRST_BONFIRE_SPEED, new DescriptionContainer("First Bonfire Bonus", "Touching a bonfire for the first time each game will instantly add a lot of progress to that bonfire.", 3, 50,
                new string[] { "Adds 20% of the bonfire's charge time.", "Adds 40% of the bonfire's charge time.", "Adds 60% of the bonfire's charge time." }) },
            { TalentEnum.PORTAL_NOTIFICATIONS, new DescriptionContainer("Portal Notifications", "You will receive screen-edge notifications whenever a portal is opened by any explorer.", 2, 100,
                new string[] { "Shows notifications for portals that are opened.", "Shows notifications for portals that are in progress." }) },
            { TalentEnum.BONFIRE_PROGRESS_NOTIFICATIONS, new DescriptionContainer("Bonfire Progress Notifications", "Adds screen-edge notifications for all bonfires that are currently in progress.", 1, 100,
                new string[] { "Adds notifications for bonfires that are in progress." }) },
            { TalentEnum.PORTAL_COOLDOWN_REDUCTION, new DescriptionContainer("Portal Cooldown", "Reduces the cooldown period for jumping through activated portals.  The base cooldown is 60 seconds.", 3, 200,
                new string[] { "Reduces the cooldown by 5 seconds.", "Reduces the cooldown by 10 seconds.", "Reduces the cooldown by 15 seconds." }) },
            { TalentEnum.HEALTH_STATION_COOLDOWN_REDUCTION, new DescriptionContainer("Health Station Cooldown", "Using a health station will immediately refund part of the normal cooldown.", 3, 200,
                new string[] { "Refunds 10% of the cooldown.", "Refunds 25% of the cooldown.", "Refunds 50% of the cooldown." }) },
            { TalentEnum.FASTER_FALL_SPEED, new DescriptionContainer("Faster Fall Speed", "Adds a downward snap to your fast fall, allowing for more unpreditable movement.", 3, 250,
                new string[] { "Pressing down while falling will slightly increase fall speed.", "Pressing down while falling will significantly increase fall speed.", "Pressing down while falling immediately sets you to terminal velocity." }) },
            { TalentEnum.RESET_MOVEMENT_ON_WALL_SLIDE, new DescriptionContainer("Reset Movement Ability On Wall Slide", "Grabbing onto walls will reset your double-jump, dash, or jetpack usage.  Normally, jumps and dashes are only reset when landing on the ground.", 2, 250,
                new string[] { "Grabbing a wall (by pressing the wall grab button) will reset movement abilities.", "Touching a wall will reset movement abilities." }) },
        };

        public class DescriptionContainer {
            public string Name { get; set; }
            public string Description { get; set; }
            public int NumRanks { get; set; }
            public int BaseCost { get; set; }
            public string[] LevelDescriptions { get; set; }

            public DescriptionContainer(string name, string description, int numRanks, int baseCost, string[] levelDescriptions) {
                Name = name;
                Description = description;
                NumRanks = numRanks;
                BaseCost = baseCost;
                LevelDescriptions = levelDescriptions;
            }
        }
    }
}
