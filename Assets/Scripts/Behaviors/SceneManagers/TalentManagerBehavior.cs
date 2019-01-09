using System;
using UnityEngine;
using com.cygnusprojects.TalentTree;

namespace Com.Tempest.Nightmare {

    public class TalentManagerBehavior : TalentusEngine {

        public const string DOUBLE_JUMP_PREFIX = "DJ ";
        public const string JETPACK_PREFIX = "JP ";
        public const string DASH_PREFIX = "DE ";
        public const string GHAST_PREFIX = "GN ";
        public const string CRYO_PREFIX = "CN ";
        public const string GOBLIN_PREFIX = "GO ";

        public const string SIGHT_RANGE = "Sight Range";
        public const string CHEST_DURATION = "Chest Duration";
        public const string BONFIRE_SPEED = "Bonfire Speed";
        public const string UPGRADES = "Upgrades";
        public const string JUMP_HEIGHT = "Jump Height";
        public const string MOVEMENT_SPEED = "Movement Speed";
        public const string ACCELERATION = "Acceleration";
        public const string COOLDOWN_REDUCTION = "Cooldown Reduction";

        public const string REDUCED_GRAVITY = "Reduced Gravity";
        public const string INCREASED_JETPACK_FORCE = "Increased Jetpack Force";
        public const string RESET_DASH = "Reset Dash On Wall Slide";
        public const string CANCEL_WALL_REFLECTION = "Cancel Wall Reflection";
        public const string CANCEL_PROJECTILE_GRAVITY = "Cancel Projectile Gravity";
        public const string WALL_CLIMB = "Wall Climb";

        public override void Start() {
            string savedState = AccountStateContainer.getInstance().talentState;
            if (savedState != null && savedState.Length != 0) {
                LoadFromString(savedState);
            } else {
                TalentTree.ResetAll("Double Jump Explorer", "Ghast Nightmare");
                AccountStateContainer.getInstance().talentState = SaveToString();
            }
            AvailableSkillPoints = AccountStateContainer.getInstance().unspentEmbers;
            base.Start();
        }

        public override void Apply() {
            base.Apply();
            AccountStateContainer.getInstance().unspentEmbers = TalentTree.PointsToAssign;
            AccountStateContainer.getInstance().talentState = SaveToString();
            AccountStateContainer.SaveInstance();
        }

        public void RefundAll() {
            AccountStateContainer.getInstance().unspentEmbers += TalentTree.RefundAll("Double Jump Explorer", "Ghast Nightmare");
            AccountStateContainer.getInstance().talentState = SaveToString();
            AccountStateContainer.SaveInstance();
            Start();
        }

        public int GetUnspentPoints() {
            return TalentTree.PointsToAssign;
        }

        public int GetTalentLevel(string talentName) {
            TalentTreeNodeBase node = TalentTree.FindTalent(talentName);
            int level = 0;
            foreach (var c in node.Cost) {
                if (c.Bought) level++;
            }
            return level;
        }
    }
}
