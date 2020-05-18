using UnityEngine;
using UnityEditor;

namespace BlenderActions
{
	/// <summary>Saves the user's preferences and keys in the system. This is shared across multiple Unity projects.</summary>
    [InitializeOnLoad]
    public class SavableEditorPrefs
    {
        public const string ColliderHolderName = "ColliderHolder";
        public const string AssetName = "Blender_actions";
		public static Color DiscWorldSpaceColor = new Color(0.96f, 0.77f, 0, 1);
		public static Color DiscLocalSpaceColor = new Color(0.11f, 0.67f, 0, 1);

		public static bool BlenderActionsON = false;
		public static Language Language = Language.ENG;

		public static bool DontShowConfigWindowAnyMore = false;
		public static bool ShowTutorialSectionInCfgWindow = true;

		public static bool HoldShiftToSlowDownTransforms = true;
        public static bool HoldControlForVertexSnapDuringTransforms = true;
        public static bool HoldControlAndShiftToPosition3DCursor = true;
		public static bool UseMiddleMouseToRotate = false;
		public static bool TransformActionsEnabled = true;
		public static bool ResetTransformsEnabled = true;
		public static bool NumericSnapEnabledByDefault = false;
        public static float TranslateSnapIncrement = 1;
        public static float RotateSnapIncrement = 45;
        public static float ScaleSnapIncrement = 1;
        public static bool CameraControlsEnabled = true;
        public static bool CreateCollidersEnabled = true;

		/// <summary>Allows to hide the default Unity gizmo. Is not shown in the config window as it has its own designated hotkey</summary>
		public static bool HideUnityGizmo = false;

		static SavableEditorPrefs()
        {
            LoadGeneral();
        }

		public static void SaveHideUnityGizmoState()
		{
			EditorPrefs.SetBool(AssetName + " - HideUnityGizmo", HideUnityGizmo);
		}

		/// <summary>Saves the variables of this class so that they can be used by different Unity projects</summary>
		public static void Save()
		{
			EditorPrefs.SetBool(AssetName + " - BlenderActionsON", BlenderActionsON);
			EditorPrefs.SetInt(AssetName + " - Language", (int)Language);
			EditorPrefs.SetBool(AssetName + " - DontShowWindow", DontShowConfigWindowAnyMore);
			EditorPrefs.SetBool(AssetName + " - ShowTutorial", ShowTutorialSectionInCfgWindow);
			
			EditorPrefs.SetBool(AssetName + " - HoldShiftToSlowDownTransforms", HoldShiftToSlowDownTransforms);
			EditorPrefs.SetBool(AssetName + " - HoldControlForVertexSnapDuringTransforms", HoldControlForVertexSnapDuringTransforms);
			EditorPrefs.SetBool(AssetName + " - HoldControlAndShiftToPosition3DCursor", HoldControlAndShiftToPosition3DCursor);
			EditorPrefs.SetBool(AssetName + " - UseMiddleMouseToRotate", UseMiddleMouseToRotate);
			EditorPrefs.SetBool(AssetName + " - ResetTransformsEnabled", ResetTransformsEnabled);
			EditorPrefs.SetBool(AssetName + " - TransformActionsEnabled", TransformActionsEnabled);
			EditorPrefs.SetBool(AssetName + " - NumericSnapEnabledByDefault", NumericSnapEnabledByDefault);
			EditorPrefs.SetFloat(AssetName + " - TranslateSnapIncrement", TranslateSnapIncrement);
			EditorPrefs.SetFloat(AssetName + " - RotateSnapIncrement", RotateSnapIncrement);
			EditorPrefs.SetFloat(AssetName + " - ScaleSnapIncrement", ScaleSnapIncrement);
			EditorPrefs.SetBool(AssetName + " - CameraControlsEnabled", CameraControlsEnabled);
			EditorPrefs.SetBool(AssetName + " - CreateCollidersEnabled", CreateCollidersEnabled);

			if (BlenderActions.Main != null)
			{
				foreach (var hotkey in BlenderActions.Main.AllActions)
				{
					EditorPrefs.SetInt(AssetName + " - " + hotkey.Value.InternalName + "_MainKey",
						(int)hotkey.Value.MainKeyCode);
					EditorPrefs.SetInt(AssetName + " - " + hotkey.Value.InternalName + "_SecondaryKey",
						(int)hotkey.Value.SecondaryKeyCode);
					EditorPrefs.SetInt(AssetName + " - " + hotkey.Value.InternalName + "_MainMouse",
						hotkey.Value.MainMouseButton);
					EditorPrefs.SetInt(AssetName + " - " + hotkey.Value.InternalName + "_SecondaryMouse",
						hotkey.Value.SecondaryMouseButton);
					EditorPrefs.SetBool(AssetName + " - " + hotkey.Value.InternalName + "_Control",
						hotkey.Value.Control);
					EditorPrefs.SetBool(AssetName + " - " + hotkey.Value.InternalName + "_Shift",
						hotkey.Value.Shift);
					EditorPrefs.SetBool(AssetName + " - " + hotkey.Value.InternalName + "_Alt",
						hotkey.Value.Alt);
					EditorPrefs.SetBool(AssetName + " - " + hotkey.Value.InternalName + "_Any",
						hotkey.Value.Any);
				}
			}
		}

		/// <summary>Loads general user preferences.</summary>
        public static void LoadGeneral()
        {
			// Check for a random key to see if we have any data at all.
			if (EditorPrefs.HasKey(AssetName + " - DontShowWindow"))
			{
				BlenderActionsON = EditorPrefs.GetBool(AssetName + " - BlenderActionsON");
				HideUnityGizmo = EditorPrefs.GetBool(AssetName + " - HideUnityGizmo");
				Language = (Language)EditorPrefs.GetInt(AssetName + " - Language");
				DontShowConfigWindowAnyMore = EditorPrefs.GetBool(AssetName + " - DontShowWindow");
				ShowTutorialSectionInCfgWindow = EditorPrefs.GetBool(AssetName + " - ShowTutorial");

				HoldShiftToSlowDownTransforms = EditorPrefs.GetBool(AssetName + " - HoldShiftToSlowDownTransforms");
				HoldControlForVertexSnapDuringTransforms = EditorPrefs.GetBool(AssetName + " - HoldControlForVertexSnapDuringTransforms");
				HoldControlAndShiftToPosition3DCursor = EditorPrefs.GetBool(AssetName + " - HoldControlAndShiftToPosition3DCursor");
				UseMiddleMouseToRotate = EditorPrefs.GetBool(AssetName + " - UseMiddleMouseToRotate");
				ResetTransformsEnabled = EditorPrefs.GetBool(AssetName + " - ResetTransformsEnabled");
				TransformActionsEnabled = EditorPrefs.GetBool(AssetName + " - TransformActionsEnabled");
				NumericSnapEnabledByDefault = EditorPrefs.GetBool(AssetName + " - NumericSnapEnabledByDefault");
				TranslateSnapIncrement = EditorPrefs.GetFloat(AssetName + " - TranslateSnapIncrement");
				RotateSnapIncrement = EditorPrefs.GetFloat(AssetName + " - RotateSnapIncrement");
				ScaleSnapIncrement = EditorPrefs.GetFloat(AssetName + " - ScaleSnapIncrement");
				CameraControlsEnabled = EditorPrefs.GetBool(AssetName + " - CameraControlsEnabled");
				CreateCollidersEnabled = EditorPrefs.GetBool(AssetName + " - CreateCollidersEnabled");
			}
		}

		/// <summary>Loads user preferences for hotkeys.</summary>
		public static void LoadHotkeys()
		{
			// Check for a random key to see if we have any data at all.
			if (EditorPrefs.HasKey(AssetName + " - DontShowWindow"))
			{
				if (BlenderActions.Main != null)
				{//
					foreach (var hotkey in BlenderActions.Main.AllActions)
					{
						if (EditorPrefs.HasKey(AssetName + " - " + hotkey.Value.InternalName + "_MainKey"))
							hotkey.Value.MainKeyCode = (KeyCode)EditorPrefs.GetInt(AssetName + " - " + hotkey.Value.InternalName + "_MainKey");
						if (EditorPrefs.HasKey(AssetName + " - " + hotkey.Value.InternalName + "_SecondaryKey"))
							hotkey.Value.SecondaryKeyCode = (KeyCode)EditorPrefs.GetInt(AssetName + " - " + hotkey.Value.InternalName + "_SecondaryKey");
						if (EditorPrefs.HasKey(AssetName + " - " + hotkey.Value.InternalName + "_MainMouse"))
							hotkey.Value.MainMouseButton = EditorPrefs.GetInt(AssetName + " - " + hotkey.Value.InternalName + "_MainMouse");
						if (EditorPrefs.HasKey(AssetName + " - " + hotkey.Value.InternalName + "_SecondaryMouse"))
							hotkey.Value.SecondaryMouseButton = EditorPrefs.GetInt(AssetName + " - " + hotkey.Value.InternalName + "_SecondaryMouse");
						if (EditorPrefs.HasKey(AssetName + " - " + hotkey.Value.InternalName + "_Control"))
							hotkey.Value.Control = EditorPrefs.GetBool(AssetName + " - " + hotkey.Value.InternalName + "_Control");
						if (EditorPrefs.HasKey(AssetName + " - " + hotkey.Value.InternalName + "_Shift"))
							hotkey.Value.Shift = EditorPrefs.GetBool(AssetName + " - " + hotkey.Value.InternalName + "_Shift");
						if (EditorPrefs.HasKey(AssetName + " - " + hotkey.Value.InternalName + "_Alt"))
							hotkey.Value.Alt = EditorPrefs.GetBool(AssetName + " - " + hotkey.Value.InternalName + "_Alt");
						if (EditorPrefs.HasKey(AssetName + " - " + hotkey.Value.InternalName + "_Any"))
							hotkey.Value.Any = EditorPrefs.GetBool(AssetName + " - " + hotkey.Value.InternalName + "_Any");
					}
				}
			}
		}
	}

	public enum Language
	{
		ENG,
		UA,
		RU,
	}
}