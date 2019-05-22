using UnityEngine;
using UnityEditor;
using System;

namespace PixelsoftGames.PixelUI
{
    public class CreateTextMenu : MonoBehaviour
    {
        const string skinName = "Text";
        const string skinPath = "Prefabs/Text/";
        const string path = "Prefabs/";

        #region Private Static Methods

        [MenuItem("Pixel UI/Create/" + skinName + "/Default (Pixel UI)")]
        static void CreateScrollView()
        {
            InstantiateObj(skinPath + "Default (Pixel UI)");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Text (Internal)")]
        static void CreateInternalText()
        {
            InstantiateObj(skinPath + "Text (Internal)");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Text (External)")]
        static void CreateExternalText()
        {
            InstantiateObj(skinPath + "Text (External)");
        }

        /// <summary>
        /// Retrieves prefabs from resources and instantiates on a canvas.
        /// </summary>
        static void InstantiateObj(string fullPath)
        {
            var prefab = Resources.Load(fullPath);
            var canvas = FindObjectOfType<Canvas>();

            try
            {
                if (canvas != null)
                    Instantiate(prefab, canvas.transform, false);
                else
                    Instantiate(prefab, Vector3.zero, Quaternion.identity);
            }
            catch(Exception ex)
            {
                Debug.LogError("Pixel UI: Could not instantiate the requested prefab because it does not exist or was not found.  Please ensure all" +
                    " controls are properly named and located in the appropriate folders.  Also ensure the skin is properly named and the skin directory" +
                    " is set properly in this script." + ex.Message);
            }
        }

        #endregion
    }
}