using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Com.Tempest.Nightmare {

    class TalentButton : MonoBehaviour {

        public Text rankLabel;
        public TalentEnum selectedTalent;

        private TalentInfoContainer.DescriptionContainer talentDescription;

        public void OnTalentClick() {
            TalentSceneManager manager = FindObjectOfType<TalentSceneManager>();
            manager.OnTalentClick(selectedTalent);
        }

        public void SetRankLabel(int currentRank, int maxRank) {
            rankLabel.text = currentRank + "/" + maxRank;
        }
    }
}
