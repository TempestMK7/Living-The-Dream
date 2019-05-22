using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.cygnusprojects.TalentTree;

namespace Com.Tempest.Nightmare {

    public class TalentClickBehavior : MonoBehaviour {
    
        public void OnClick() {
            FindObjectOfType<LauncherManager>().OnTalentClick(GetComponentInParent<TalentUI>().Talent);
        }
    }
}
