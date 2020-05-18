using UnityEngine;
using UnityEditor;
using System.Collections;

namespace BlenderActions
{
	/// <summary>Basic class for all transformation actions.</summary>
	public abstract class BaseTransform
	{
		/// <summary>The reference to the blender actions class</summary>
		protected BlenderActions BA;

		/// <summary>Averaged pivots of the top level of selected GameObject-s before translation started.</summary>
		protected Vector3 OrigAvgPivot;
		/// <summary>A copy of the Transform-s array of selected GameObject-s.</summary>
		protected Transform[] SelectedTransforms;
		protected GameObject[] SelectedGOs;
		/// <summary>A copy of the Transform-s array of selected GameObject-s.</summary>
		protected GameObject ActiveGO;
		/// <summary>Is restored once the action is done.</summary>
		private Tool LastUsedTool = Tool.Move;
		/// <summary>Is restored once the action is done.</summary>
		private int HotControlID;

		/// <summary>Holds a link to the class, responsible for building numbers out of user input if the user has started a transform 
		/// action and has not finished it yet. Otherwise - null. Is recreated every time a user starts a transform action</summary>
		protected NumericInput NumericInput;

		/// <summary>If set to 'true' certain functions would work differently and the class would just seek to stop updating</summary>
		protected bool TerminateAction = false;


		public BaseTransform(BlenderActions blenderActions)
		{
			BA = blenderActions;
		}

		/// <summary>Happens once the "start ... action" hotkey has been registered</summary>
		/// <param name="recordUndo"></param>
		public virtual void Start()
		{
			TerminateAction = false;

			ActiveGO = Selection.activeGameObject;
			SelectedGOs = Selection.gameObjects;
			SelectedTransforms = Selection.GetTransforms(SelectionMode.TopLevel);

			NumericInput = new NumericInput(BA);

			HotControlID = EditorGUIUtility.hotControl;
			if (!SavableEditorPrefs.HideUnityGizmo)
				LastUsedTool = Tools.current;
			BA.ResetTransformLock();
		}

		/// <summary>Happens every OnSceneGUI in editor</summary>
		public virtual void OnSceneGUI(SceneView sceneView)
		{
			NumericInput.OnSceneGUI(sceneView);
		}

		/// <summary>Is called every time a transform action has finished (applied or canceled).</summary>
		public void TransformActionFinished()
		{
			HandleUtility.AddDefaultControl(HotControlID);

			if (!SavableEditorPrefs.HideUnityGizmo)
				Tools.current = LastUsedTool;
		}

		/// <summary>Applies all the changes made during this transformation action. Saves an Undo record if requested.</summary>
		public virtual void Confirm()
		{
			TransformActionFinished();
			Selection.activeGameObject = ActiveGO;
			BA.TransformActionFinished();
		}

		/// <summary>Cancels the result of the translation action</summary>
		public virtual void Cancel()
		{
			TransformActionFinished();
			BA.TransformActionFinished();
		}
	}
}