using UnityEngine;
using UnityEditor;
using System;

namespace PixelsoftGames.PixelUI
{
    public class CreateRoundedMenu : MonoBehaviour
    {
        const string skinName = "Rounded";
        const string skinPath = "Prefabs/Rounded/";
        const string path = "Prefabs/";

        #region Private Static Methods

        [MenuItem("Pixel UI/Create/" + skinName + "/Scroll View")]
        static void CreateScrollView()
        {
            InstantiateObj(skinPath + "Scroll View");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Button")]
        static void CreateButton()
        {
            InstantiateObj(skinPath + "Button");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Shadow Button")]
        static void CreateShadowButton()
        {
            InstantiateObj(skinPath + "Shadow Button");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Dropdown")]
        static void CreateDropdown()
        {
            InstantiateObj(skinPath + "Dropdown");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Shadow Dropdown")]
        static void CreateShadowDropdown()
        {
            InstantiateObj(skinPath + "Shadow Dropdown");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Input")]
        static void CreateInput()
        {
            InstantiateObj(skinPath + "Input");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Shadow Input")]
        static void CreateShadowInput()
        {
            InstantiateObj(skinPath + "Shadow Input");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Panel")]
        static void CreatePanel()
        {
            InstantiateObj(skinPath + "Panel");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Radio Button")]
        static void CreateRadioButton()
        {
            InstantiateObj(skinPath + "Radio Button");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Scrollbar")]
        static void CreateScrollbar()
        {
            InstantiateObj(skinPath + "Scrollbar");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Shadow Scrollbar")]
        static void CreateShadowScrollbar()
        {
            InstantiateObj(skinPath + "Shadow Scrollbar");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Slider")]
        static void CreateSlider()
        {
            InstantiateObj(skinPath + "Slider");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Checkmark Toggle")]
        static void CreateToggleCheckmark()
        {
            InstantiateObj(skinPath + "Checkmark Toggle");
        }

        [MenuItem("Pixel UI/Create/" + skinName + "/Cross Toggle")]
        static void CreateCrossToggle()
        {
            InstantiateObj(skinPath + "Cross Toggle");
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