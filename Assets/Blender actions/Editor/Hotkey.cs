using UnityEngine;
using UnityEditor;
using System;

namespace BlenderActions
{
	/// <summary>Holds all necessary info to describe and save the user's hotkey binding for any action</summary>
	public class Hotkey
	{
		/// <summary>Enum identifier of this variable</summary>
		public Actions InternalName;

		/// <summary>Prevents this action from triggering untill the user releases the relevant key.</summary>
		public bool ActiveOnLastCheck = false;

		/// <summary>Turns this hotkey into a one that only activates once, every time the button started to be pressed.</summary>
		public bool CheckKeyPress = true;

		#region Hotkey conditions
		/// <summary>What is the main keyboard key activator for this hotkey</summary>
		public KeyCode MainKeyCode = KeyCode.Delete;
		/// <summary>What is the secondary keyboard key activator for this hotkey</summary>
		public KeyCode SecondaryKeyCode = KeyCode.Delete;
		/// <summary>What is the main mouse button activator for this hotkey</summary>
		public int MainMouseButton = -1;
		/// <summary>What is the secondary mouse button activator for this hotkey</summary>
		public int SecondaryMouseButton = -1;
		/// <summary>Triggers only if pressed with Control held down.</summary>
		public bool Control = false;
		/// <summary>Triggers only if pressed with Alt held down.</summary>
		public bool Alt = false;
		/// <summary>Triggers only if pressed with Shift held down.</summary>
		public bool Shift = false;
		/// <summary>It does not matter what modifier is pressed - it always triggers.</summary>
		public bool Any = false;
		#endregion

		/// <summary>The name displayed in the config window in English</summary>
		public string DisplayNameENG = "NOT SET";
		/// <summary>The name displayed in the config window in Ukrainian</summary>
		public string DisplayNameUA = "NOT SET";
		/// <summary>The name displayed in the config window in Russian</summary>
		public string DisplayNameRU = "NOT SET";

		/// <summary>The tooltip displayed in the config window in English</summary>
		public string TooltipENG = "NOT SET";
		/// <summary>The tooltip displayed in the config window in Ukrainian</summary>
		public string TooltipUA = "NOT SET";
		/// <summary>The tooltip displayed in the config window in Russian</summary>
		public string TooltipRU = "NOT SET";

		/// <summary>A method from the BlenderActions class that is launched if this key has been activated</summary>
		public Action OnActivated;
		/// <summary>What group this action is in. Helps to prevent hotkey clashes
		///		(when the same hotkey is set for multiple actions).</summary>
		public ActionType ActionGroup;

		/// <summary>Helps display the user which groups of hotkeys have been deactivated with a menu checkbox</summary>
		public HotkeyType HotkeyType;

		/// <summary>Has this hotkey been invoked this frame. Is used during clash check and the actual run call</summary>
		public bool HotkeyActivatedThisFrame = false;


		public Hotkey()
		{
		}

		/// <summary>Check if this hotkey was invoked this frame</summary>
		public void IsKeyDown(Event current, bool rmb_down)
		{
			HotkeyActivatedThisFrame = false;

			if (current != null
				&& (!rmb_down || MainMouseButton == 1 || SecondaryMouseButton == 1)
				&& (((MainKeyCode != KeyCode.None || MainMouseButton != -1)

						&& ((current.type == EventType.KeyDown && current.keyCode != KeyCode.Delete
								&& (current.keyCode == MainKeyCode || current.keyCode == SecondaryKeyCode))
							|| (current.type == EventType.MouseDown 
								&& (current.button == MainMouseButton || current.button == SecondaryMouseButton))))))
			{
				if (!ActiveOnLastCheck || !CheckKeyPress)
				{
					if (Any
						|| (((!Control && !current.control) || (Control && current.control))
							&& ((!Shift && !current.shift) || (Shift && current.shift))
							&& ((!Alt && !current.alt) || (Alt && current.alt))))
						HotkeyActivatedThisFrame = true;
				}

				if (CheckKeyPress)
					ActiveOnLastCheck = true;	// Prevent this action from triggering in the future
			}
			// Capture a "release button" so that we can allow our action to trigger again
			else if (current != null
				&& ((current.type == EventType.KeyUp
						&& (current.keyCode == MainKeyCode || current.keyCode == SecondaryKeyCode))
					|| (current.type == EventType.MouseUp
						&& (MainMouseButton == current.button || SecondaryMouseButton == current.button))))
				ActiveOnLastCheck = false;
		}

		/// <summary>Run this hotkey's action if the hotkey was invoked this frame and there was no clash</summary>
		public void ActivateIfApplicable()
		{
			if (HotkeyActivatedThisFrame)
			{
				OnActivated();

				HotkeyActivatedThisFrame = false;
			}
		}
	}
}
