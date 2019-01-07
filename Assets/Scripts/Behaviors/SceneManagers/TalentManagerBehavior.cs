using System;
using UnityEngine;
using com.cygnusprojects.TalentTree;

namespace Com.Tempest.Nightmare {

    public class TalentManagerBehavior : TalentusEngine {

        public override void Start() {
            string savedState = AccountStateContainer.getInstance().talentState;
            if (savedState != null && savedState.Length != 0) {
                LoadFromString(savedState);
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
            AccountStateContainer.getInstance().unspentEmbers += TalentTree.RefundAll("Double Jump Explorer");
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
