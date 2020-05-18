using UnityEditor;
using UnityEngine;


namespace BlenderActions
{
	/// <summary>Reads any numeric input of the user and turns them into digits. Only relavant duiring transformation actions.</summary>
	public class NumericInput
	{
		public BlenderActions BA;
		/// <summary>The accumulated number the user has entered so far (after processing)</summary>
		public float EnteredNumber = 0;
		/// <summary>Has the user pressed the dot key during this number entering session</summary>
		bool DotKeyWasPressed = false;
		/// <summary>How many float digits we curently have after the dot</summary>
		float FloatDigits = 1;
		/// <summary>Memorizes if minus was pressed during this session</summary>
		bool MinusWasPressed = false;

		public NumericInput(BlenderActions ba)
		{
			BA = ba;
		}


		public void OnSceneGUI(SceneView sceneView)
		{
			#region #Color(darkblue);
			if (BA.CurrentEvent.type == EventType.KeyDown)
			{
				// Counting entered numbers.
				if (BA.CurrentEvent.keyCode == KeyCode.Comma
					|| BA.CurrentEvent.keyCode == KeyCode.Period
					|| BA.CurrentEvent.keyCode == KeyCode.KeypadPeriod)
					DotKeyWasPressed = true;
				else if (BA.CurrentEvent.keyCode == KeyCode.Minus
					|| BA.CurrentEvent.keyCode == KeyCode.KeypadMinus)
				{
					MinusWasPressed = !MinusWasPressed;

					if (BA.CurrentEvent.type != EventType.Layout && BA.CurrentEvent.type != EventType.Layout)
						BA.CurrentEvent.Use();
				}
				else if (BA.CurrentEvent.keyCode == KeyCode.Alpha0
					|| BA.CurrentEvent.keyCode == KeyCode.Keypad0)
				{
					if (DotKeyWasPressed)
						FloatDigits++;
					else
						EnteredNumber *= 10;
					if (BA.CurrentEvent.type != EventType.Layout && BA.CurrentEvent.type != EventType.Layout)
						BA.CurrentEvent.Use();
				}
				else if (BA.CurrentEvent.keyCode == KeyCode.Alpha1
					|| BA.CurrentEvent.keyCode == KeyCode.Keypad1)
				{
					if (DotKeyWasPressed)
					{
						EnteredNumber = EnteredNumber + 1 / Mathf.Pow(10, FloatDigits);
						FloatDigits++;
					}
					else
						EnteredNumber = EnteredNumber * 10 + 1;
					if (BA.CurrentEvent.type != EventType.Layout && BA.CurrentEvent.type != EventType.Layout)
						BA.CurrentEvent.Use();
				}
				else if (BA.CurrentEvent.keyCode == KeyCode.Alpha2
					|| BA.CurrentEvent.keyCode == KeyCode.Keypad2)
				{
					if (DotKeyWasPressed)
					{
						EnteredNumber = EnteredNumber + 2 / Mathf.Pow(10, FloatDigits);
						FloatDigits++;
					}
					else
						EnteredNumber = EnteredNumber * 10 + 2;
					if (BA.CurrentEvent.type != EventType.Layout && BA.CurrentEvent.type != EventType.Layout)
						BA.CurrentEvent.Use();
				}

				// Counting entered numbers.
				else if (BA.CurrentEvent.keyCode == KeyCode.Alpha3
					|| BA.CurrentEvent.keyCode == KeyCode.Keypad3)
				{
					if (DotKeyWasPressed)
					{
						EnteredNumber = EnteredNumber + 3 / Mathf.Pow(10, FloatDigits);
						FloatDigits++;
					}
					else
						EnteredNumber = EnteredNumber * 10 + 3;
					if (BA.CurrentEvent.type != EventType.Layout && BA.CurrentEvent.type != EventType.Layout)
						BA.CurrentEvent.Use();
				}

				// Counting entered numbers.
				else if (BA.CurrentEvent.keyCode == KeyCode.Alpha4
					|| BA.CurrentEvent.keyCode == KeyCode.Keypad4)
				{
					if (DotKeyWasPressed)
					{
						EnteredNumber = EnteredNumber + 4 / Mathf.Pow(10, FloatDigits);
						FloatDigits++;
					}
					else
						EnteredNumber = EnteredNumber * 10 + 4;
					if (BA.CurrentEvent.type != EventType.Layout && BA.CurrentEvent.type != EventType.Layout)
						BA.CurrentEvent.Use();
				}

				// Counting entered numbers.
				else if (BA.CurrentEvent.keyCode == KeyCode.Alpha5
					|| BA.CurrentEvent.keyCode == KeyCode.Keypad5)
				{
					if (DotKeyWasPressed)
					{
						EnteredNumber = EnteredNumber + 5 / Mathf.Pow(10, FloatDigits);
						FloatDigits++;
					}
					else
						EnteredNumber = EnteredNumber * 10 + 5;
					if (BA.CurrentEvent.type != EventType.Layout && BA.CurrentEvent.type != EventType.Layout)
						BA.CurrentEvent.Use();
				}

				// Counting entered numbers.
				else if (BA.CurrentEvent.keyCode == KeyCode.Alpha6
					|| BA.CurrentEvent.keyCode == KeyCode.Keypad6)
				{
					if (DotKeyWasPressed)
					{
						EnteredNumber = EnteredNumber + 6 / Mathf.Pow(10, FloatDigits);
						FloatDigits++;
					}
					else
						EnteredNumber = EnteredNumber * 10 + 6;
					if (BA.CurrentEvent.type != EventType.Layout && BA.CurrentEvent.type != EventType.Layout)
						BA.CurrentEvent.Use();
				}

				// Counting entered numbers.
				else if (BA.CurrentEvent.keyCode == KeyCode.Alpha7
					|| BA.CurrentEvent.keyCode == KeyCode.Keypad7)
				{
					if (DotKeyWasPressed)
					{
						EnteredNumber = EnteredNumber + 7 / Mathf.Pow(10, FloatDigits);
						FloatDigits++;
					}
					else
						EnteredNumber = EnteredNumber * 10 + 7;
					if (BA.CurrentEvent.type != EventType.Layout && BA.CurrentEvent.type != EventType.Layout)
						BA.CurrentEvent.Use();
				}

				// Counting entered numbers.
				else if (BA.CurrentEvent.keyCode == KeyCode.Alpha8
					|| BA.CurrentEvent.keyCode == KeyCode.Keypad8)
				{
					if (DotKeyWasPressed)
					{
						EnteredNumber = EnteredNumber + 8 / Mathf.Pow(10, FloatDigits);
						FloatDigits++;
					}
					else
						EnteredNumber = EnteredNumber * 10 + 8;
					if (BA.CurrentEvent.type != EventType.Layout && BA.CurrentEvent.type != EventType.Layout)
						BA.CurrentEvent.Use();
				}

				// Counting entered numbers.
				else if (BA.CurrentEvent.keyCode == KeyCode.Alpha9
					|| BA.CurrentEvent.keyCode == KeyCode.Keypad9)
				{
					if (DotKeyWasPressed)
					{
						EnteredNumber = EnteredNumber + 9 / Mathf.Pow(10, FloatDigits);
						FloatDigits++;
					}
					else
						EnteredNumber = EnteredNumber * 10 + 9;
					if (BA.CurrentEvent.type != EventType.Layout && BA.CurrentEvent.type != EventType.Layout)
						BA.CurrentEvent.Use();
				}

				if (EnteredNumber != 0)
				{
					if ((MinusWasPressed && EnteredNumber > 0) || (!MinusWasPressed && EnteredNumber < 0))
						EnteredNumber *= -1;
				}
			}
			#endregion
		}
	}
}
