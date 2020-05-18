using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace BlenderActions
{
	public class Rotate : BaseTransform
	{
		private Vector2 originalMousePos;
		private Vector3[] originalRotations;
		private Vector3[] originalPositions;

		#region Shift slowdown
		float OldAngle;
		float OldAngleWithOffset;
		float AngleOffset;
		#endregion

		public Rotate(BlenderActions blenderActions)
			: base(blenderActions)
		{
		}

		/// <summary>Initialize starting variables for this action.</summary>
		public override void Start()
		{
			#region #Color(darkorange*0.5);
			base.Start();

			originalMousePos = BA.CurrentEvent.mousePosition;

			// Just in case the order isn't guaranteed, I'm going to save the selecteds.
			OrigAvgPivot = Vector3.zero;
			originalRotations = new Vector3[SelectedTransforms.Length];
			originalPositions = new Vector3[SelectedTransforms.Length];

			for (int i = 0; i < SelectedTransforms.Length; i++)
			{
				OrigAvgPivot += SelectedTransforms[i].position;
				originalRotations[i] = SelectedTransforms[i].eulerAngles;
				originalPositions[i] = SelectedTransforms[i].position;
			}

			OrigAvgPivot /= SelectedTransforms.Length;

			OldAngle = 0;
			OldAngleWithOffset = 0;
			AngleOffset = 0;
			#endregion
		}

		/// <summary>The "Update" function. WARNING: this is called way more often then MonoBehaviour.Update().</summary>
		public override void OnSceneGUI(SceneView sceneView)
		{
			#region #Color(darkblue);
			if (BlenderActions.Main == null)
				Cancel();
			else
			{
				bool checkSuccessful = true;
				int n = SelectedTransforms.Length;
				while (--n > -1)
				{   // Prevents situations, when a GameObject was deleted, while being operated on
					if (SelectedTransforms[n] == null || SelectedTransforms[n].gameObject == null)
					{
						checkSuccessful = false;
						TerminateAction = true;
						break;
					}
				}

				if (!checkSuccessful)
					Cancel();
				else
				{

					base.OnSceneGUI(sceneView);

					if (BA.CurrentEvent.type == EventType.MouseMove
						|| BA.CurrentEvent.type == EventType.MouseDown
						|| BA.CurrentEvent.type == EventType.KeyDown
						|| BA.CurrentEvent.type == EventType.KeyUp
						|| BA.CurrentEvent.control

						// Exception - run script in other events if mouse is not sending
						//	MouseMove events (mouse over another Editor window
						|| EditorWindow.mouseOverWindow != sceneView
						|| NumericInput.EnteredNumber != 0)// #Color(lime*3);
					{
						ResetRotations();

						CalculateRotation(originalMousePos, BA.CurrentEvent.mousePosition);
					}

					Vector3 pivot = OrigAvgPivot;
					if (BA.Use3DCursor)                     //#color(orange*3);
						pivot = BA.The3DCursorPos;          //#color(orange*3);

					Utils.DrawLines(pivot, SelectedTransforms, BA.TransformLock, BA.TransformSpace);

					// Draw a solid disc.
					Camera sceneCam = SceneView.lastActiveSceneView.camera;
					Vector3 camViewPlaneNormal = sceneCam.transform.forward;
					if (BA.TransformSpace == Space.World)
					{
						Utils.DrawDisc(SavableEditorPrefs.DiscWorldSpaceColor,
							sceneCam.transform.forward, pivot);
					}
					else
					{
						Handles.color = SavableEditorPrefs.DiscLocalSpaceColor;
						foreach (var transform in SelectedTransforms)
						{
							Vector3 discPos = transform.position;
							if (BA.Use3DCursor)                                 //#color(orange*3);
								discPos = BA.The3DCursorPos;                    //#color(orange*3);
							Utils.DrawDisc(SavableEditorPrefs.DiscLocalSpaceColor,
								sceneCam.transform.forward, discPos);
						}
					}

					Vector3 point = OrigAvgPivot;
					if (BA.Use3DCursor)                                 //#color(orange*3);
						point = BA.The3DCursorPos;                      //#color(orange*3);
					Utils.DrawDottedLineToTheCenter(point);
				}
			}
			#endregion
		}
		public override void Confirm()
		{
			bool checkSuccessful = true;
			int n = SelectedTransforms.Length;
			while (--n > -1)
			{   // Prevents situations, when a GameObject was deleted, while being operated on
				if (SelectedTransforms[n] == null || SelectedTransforms[n].gameObject == null)
				{
					checkSuccessful = false;
					TerminateAction = true;
					break;
				}
			}

			if (!checkSuccessful)
				Cancel();
			else
			{
				base.Confirm();

				BA.ResetTransformLock();
				// Before confirming we reverse the models to their original states to save an undo record.
				//	This is better than saving undo at the start of the trasformation because it prevents undo glitches
				//	and empty undos.
				// First - save the current sates
				Vector3 currentColliderCenter = Vector3.zero;
				List<Vector3> currentLocalPositions = new List<Vector3>();
				List<Vector3> currentEulerAngles = new List<Vector3>();

				for (int i = 0; i < SelectedTransforms.Length; i++)
				{
					currentLocalPositions.Add(SelectedTransforms[i].localPosition);
					currentEulerAngles.Add(SelectedTransforms[i].eulerAngles);
				}

				// Reset the models
				ResetRotations();

				//Save an undo record
				Undo.IncrementCurrentGroup();
				for (int i = 0; i < SelectedTransforms.Length; i++)
					Undo.RecordObject(SelectedTransforms[i], "Rotate");

				// Restore saved states
				for (int i = 0; i < SelectedTransforms.Length; i++)
				{
					SelectedTransforms[i].localPosition = currentLocalPositions[i];
					SelectedTransforms[i].eulerAngles = currentEulerAngles[i];
				}
			}
		}


		/// <summary>Revert any changes made, remove an Undo record.</summary>
		public override void Cancel()
		{
			base.Cancel();

			if (!TerminateAction)
				ResetRotations();
		}

		private void ResetRotations()
		{
			if (SelectedTransforms != null)
			{
				for (int i = 0; i < SelectedTransforms.Length; i++)
				{
					SelectedTransforms[i].eulerAngles = originalRotations[i];
					SelectedTransforms[i].position = originalPositions[i];
				}
			}
		}

		/// <summary>The actual logic of the rotation action.</summary>
		private void CalculateRotation(Vector2 originalMouse, Vector2 newMouse)
		{//#colreg(darkcyan);
			Camera sceneCam = SceneView.lastActiveSceneView.camera;

			// Prevent a glitch that inverts movement in some cases.
			Vector3 camViewPlaneNormal = sceneCam.transform.forward;
			Vector3 Z_Difference = sceneCam.transform.position - OrigAvgPivot;
			Debug.Log(Vector3.Dot(Z_Difference, camViewPlaneNormal).ToString("0.#####"));
			float dot = Vector3.Dot(Z_Difference, camViewPlaneNormal);

			float angle = 0;
			Vector3 axis = Vector3.zero;
			Vector3 pivot = OrigAvgPivot;
			if (BA.Use3DCursor)                     //#color(orange*3);
				pivot = BA.The3DCursorPos;          //#color(orange*3);
			Vector2 inSP = sceneCam.WorldToScreenPoint(pivot);

			Vector2 toNewPos = newMouse - originalMouse;

			// Implementing numeric input for rotations.
			if (NumericInput.EnteredNumber != 0)		// #Color(lime*3);
				angle = NumericInput.EnteredNumber;		// #Color(lime*3);
			else
			{
				inSP.y = sceneCam.pixelHeight - inSP.y;

				angle = Vector2.Angle(originalMouse - inSP, newMouse - inSP);
				if (dot > 0)
					angle = 360 - angle;

				if (Vector3.Cross(originalMouse - inSP, newMouse - inSP).z < 0)
					angle = 360 - angle;

				#region Shift slowdown
				float clearSign = Mathf.Sign(angle - OldAngle);
				float clearDelta = Mathf.Abs(angle - OldAngle) - 360;

				if (BA.SlowDownTransformsThisFrame)
				{
					float delta = (angle - OldAngleWithOffset - AngleOffset) * 0.9f;

					if (Mathf.Abs(delta) > 300)
					{
						float sign = Mathf.Sign(AngleOffset);
						AngleOffset = -1 * sign * (360 - Mathf.Abs(AngleOffset)) + clearDelta * clearSign;
					}
					else
						AngleOffset += delta;
				}

				OldAngle = angle;

				angle -= AngleOffset;

				if (angle > 360)
					angle -= 360;
				else if (angle < -360)
					angle += 360;

				OldAngleWithOffset = angle;
				#endregion
			}



			if (toNewPos.sqrMagnitude != 0
				|| NumericInput.EnteredNumber != 0)      // #Color(lime*3);
			{
				if (BA.TransformSpace == Space.World)
				{
					if (BA.TransformLock == Lock.None)
						axis = -sceneCam.ScreenPointToRay(inSP).direction;
					else
					{
						Vector3 toCam = sceneCam.transform.position - pivot;

						if (BA.TransformLock == Lock.X_Axis || BA.TransformLock == Lock.YZ_Plane)
						{
							if (toCam.x >= 0)
								axis = Vector3.right;
							else
								axis = Vector3.left;
						}
						else if (BA.TransformLock == Lock.Y_Axis || BA.TransformLock == Lock.XZ_Plane)
						{
							if (toCam.y >= 0)
								axis = Vector3.up;
							else
								axis = Vector3.down;
						}
						else
						{
							if (toCam.z >= 0)
								axis = Vector3.forward;
							else
								axis = Vector3.back;
						}
					}

					UpdateRotations(axis, angle);
				}
				else
				{
					foreach (Transform t in SelectedTransforms)
					{
						Vector3 caminObjSP = t.transform.InverseTransformPoint(sceneCam.transform.position);

						if (BA.TransformLock == Lock.X_Axis || BA.TransformLock == Lock.YZ_Plane)
						{
							if (caminObjSP.x >= 0)
								axis = t.transform.TransformDirection(Vector3.right);
							else
								axis = t.transform.TransformDirection(Vector3.left);
						}
						else if (BA.TransformLock == Lock.Y_Axis || BA.TransformLock == Lock.XZ_Plane)
						{
							if (caminObjSP.y >= 0)
								axis = t.transform.TransformDirection(Vector3.up);
							else
								axis = t.transform.TransformDirection(Vector3.down);
						}
						else
						{
							if (caminObjSP.z >= 0)
								axis = t.transform.TransformDirection(Vector3.forward);
							else
								axis = t.transform.TransformDirection(Vector3.back);
						}

						Vector3 pointToRotateAround = t.position;
						if (BA.Use3DCursor)                                 //#color(orange*3);
							pointToRotateAround = BA.The3DCursorPos;        //#color(orange*3);
						t.RotateAround(pointToRotateAround, axis, angle);
					}

				}
				PerformNumericSnap();

				// Handle vertex snapping ONLY AFTER the original transformations.	#colreg(red*0.3);
				if (!BA.NumericSnap && BA.VertexSnapON_ThisFrame)
				{
					BA.LastVertexSnapTime = EditorApplication.timeSinceStartup + BlenderActions.VertexSnapTimeInterval;
					BA.AntiHang = 0;

					//#colreg(green);
					bool returnOnly1stVertex = false;
					Utils.VertexSnappingResult targetResult = Utils.VertexSnappingResult.BothVectorsFound;

					// If we are in collider editing mode - don't search for the closestSelectedVertexWorldSpace amongst models in scene.
					if (BA.ColliderBeingEdited != null)
					{
						returnOnly1stVertex = true;
						targetResult = Utils.VertexSnappingResult.FirstVectorFound;
					}
					//#endcolreg

					Vector2 correctedMousePos = BA.CurrentEvent.mousePosition;
					correctedMousePos.y = sceneCam.pixelHeight - correctedMousePos.y;

					Vector3 closestVertexWorldSpace = Vector3.zero;
					Vector3 closestSelectedVertexWorldSpace = Vector3.zero;
					GameObject closestGO = null;
					if (Utils.Find2VerticesForVertexSnapping(sceneCam, correctedMousePos,
						out closestVertexWorldSpace, out closestSelectedVertexWorldSpace, out closestGO, returnOnly1stVertex)
						== targetResult)
					{
						// If we are in collider editing mode - find closestSelectedVertexWorldSpace amongst the collider vertices. #colreg(green);
						if (BA.ColliderBeingEdited != null)
						{
							if (BA.EditedBoxCollider != null)
								closestSelectedVertexWorldSpace =
									Utils.FindClosestBoxColliderVertex(BA.EditedBoxCollider, sceneCam, correctedMousePos);
							else if (BA.EditedSphereCollider != null)
								closestSelectedVertexWorldSpace =
									Utils.FindClosestSphereColliderVertex(BA.EditedSphereCollider, closestVertexWorldSpace);
							else if (BA.EditedCapsuleCollider != null)
								closestSelectedVertexWorldSpace =
									Utils.FindClosestCapsuleColliderVertex(BA.EditedCapsuleCollider, closestVertexWorldSpace);
						}//#endcolreg
						float vertexSnapFullAngle = 0;

						// Crazy math from here:
						//	https://math.stackexchange.com/questions/2548811/find-an-angle-to-rotate-a-vector-around-a-ray-so-that-the-vector-gets-as-close-a/2549262#2549262						

						Vector3 deltaVertexWorldSpaceNormalized = (closestVertexWorldSpace - pivot).normalized;
						Vector3 deltaSelectedVertexWorldSpaceNormalized = (closestSelectedVertexWorldSpace - pivot).normalized;
						Vector3 axisNormalized = axis.normalized;
						Vector3 c = deltaSelectedVertexWorldSpaceNormalized
							- Vector3.Dot(deltaSelectedVertexWorldSpaceNormalized, axisNormalized) * axisNormalized;

						float temp = 1 / c.magnitude;
						if (!float.IsNaN(temp))
						{
							Vector3 e = temp * c;
							Vector3 f = Vector3.Cross(axisNormalized, e);
							vertexSnapFullAngle = Mathf.Atan2(Vector3.Dot(deltaVertexWorldSpaceNormalized, f),
								Vector3.Dot(deltaVertexWorldSpaceNormalized, e)) * Mathf.Rad2Deg;

							if (!float.IsNaN(vertexSnapFullAngle))
								UpdateRotations(axis, vertexSnapFullAngle);
						}
					}
				}   //#endcolreg
			}

		}//#endcolreg

		/// <summary>The implementation of the numeric snap option.</summary>
		private void PerformNumericSnap()
		{
			if (BA.NumericSnap && SavableEditorPrefs.RotateSnapIncrement != 0)
			{
				foreach (Transform t in SelectedTransforms)
				{
					Vector3 vecToSnap = t.eulerAngles;

					vecToSnap /= SavableEditorPrefs.RotateSnapIncrement;

					vecToSnap.x = Mathf.Round(vecToSnap.x);
					vecToSnap.y = Mathf.Round(vecToSnap.y);
					vecToSnap.z = Mathf.Round(vecToSnap.z);

					vecToSnap *= SavableEditorPrefs.RotateSnapIncrement;

					t.eulerAngles = vecToSnap;
				}
			}
		}

		/// <summary>Apply the calculated rotation.</summary>
		private void UpdateRotations(Vector3 axis, float angle)
		{
			#region #Color(darkcyan*2);
			if (SelectedTransforms != null)
				foreach (Transform t in SelectedTransforms)
				{
					Vector3 pointToRotateAround = OrigAvgPivot;
					if (BA.Use3DCursor)									//#color(orange*3);
						pointToRotateAround = BA.The3DCursorPos;        //#color(orange*3);
					t.RotateAround(pointToRotateAround, axis, angle);
				}
			#endregion
		}
	}
}
