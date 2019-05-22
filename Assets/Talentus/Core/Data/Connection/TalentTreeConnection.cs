using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;

namespace com.cygnusprojects.TalentTree
{
    [Serializable]
	public class TalentTreeConnection : TalentTreeConnectionBase 
	{
        #region Variables

        #endregion

        #region Implementation
#if UNITY_EDITOR
        public override void UpdateConnectionGUI(Event e, Rect viewRect, GUISkin viewSkin, Color requiredTypeColor, Color optionalTypeColor)
        {
            base.UpdateConnectionGUI(e, viewRect, viewSkin, requiredTypeColor, optionalTypeColor);

        }

        public override void DrawConnectionProperties(Event e, Rect propertyRect, GUISkin viewSkin)
        {
            base.DrawConnectionProperties(e, propertyRect, viewSkin);

        }
#endif
        #endregion

        #region Utilities

        #endregion
    }
}
