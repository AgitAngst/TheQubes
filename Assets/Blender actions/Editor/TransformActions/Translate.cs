using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace BlenderActions
{
    public class Translate : BaseTransform
    {
		/// <summary>Original center of a collider on this object in local space (if we are in collider editing mode).</summary>
		protected Vector3 OrigColliderCenterLocalSpace;
		/// <summary>Screen space position of the mouse at the start of the translation action.</summary>
		private Vector2 OriginalMousePos;
		/// <summary>Add these to OrigAvgPivot to get a pivot of each object.</summary>
		private Vector3[] OrigObjectsOffsets;
		/// <summary>Averaged pivots of the top level of selected GameObject-s.</summary>
		private Vector3 LastFrameAvgPivot;
		/// <summary>Last frame position of every selected object from the top hierarchy level.</summary>
        private Vector3[] LastFrameLocalPos;

		#region "Shift-slowdown" functionality.
		/// <summary>Accumulated while user is holding down "shift".</summary>
		protected Vector2 MouseOffset;
		/// <summary>Last frame screen space mouse posision.</summary>
		protected Vector2 LastMousePos;
		#endregion

        public Translate(BlenderActions blenderActions)
			: base(blenderActions)
        {
        }

		/// <summary>Initialize starting variables for this action.</summary>
		/// <param name="recordUndo">Should a separate Undo be recorded for this action?</param>
        public override void Start()
        {
			#region #Color(darkorange*0.5);
			base.Start();
			//#colreg(green);
			if (BA.ColliderBeingEdited != null)
				OrigColliderCenterLocalSpace = Utils.GetEditableColliderCenterLocalSpace(BA);
			//#endcolreg
			OriginalMousePos = BA.CurrentEvent.mousePosition;
            OriginalMousePos.y = SceneView.lastActiveSceneView.camera.pixelHeight - OriginalMousePos.y;
			LastMousePos = OriginalMousePos;
			MouseOffset = Vector2.zero;

            OrigObjectsOffsets = new Vector3[SelectedTransforms.Length];
            LastFrameLocalPos = new Vector3[SelectedTransforms.Length];
            OrigAvgPivot = Vector3.zero;

            for (int i = 0; i < SelectedTransforms.Length; i++)
            {
                OrigAvgPivot += SelectedTransforms[i].position;
                LastFrameLocalPos[i] = SelectedTransforms[i].position;
            }

            OrigAvgPivot /= SelectedTransforms.Length;
            LastFrameAvgPivot = OrigAvgPivot;

            // Now that we have the average position, we get a bunch of vectors to each objects.
            for (int i = 0; i < SelectedTransforms.Length; i++)
                OrigObjectsOffsets[i] = SelectedTransforms[i].position - OrigAvgPivot;
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
				{	// Prevents situations, when a GameObject was deleted, while being operated on
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
					|| EditorWindow.mouseOverWindow != sceneView)
					{
						UpdatePositions(OrigAvgPivot);

						CalculatePosition();
					}

					Utils.DrawLines(OrigAvgPivot, SelectedTransforms, BA.TransformLock, BA.TransformSpace);

					Camera sceneCam = SceneView.lastActiveSceneView.camera;
					Vector3 camViewPlaneNormal = sceneCam.transform.forward;

					if (BA.TransformSpace == Space.World)
					{
						Utils.DrawDisc(SavableEditorPrefs.DiscWorldSpaceColor,
							sceneCam.transform.forward, LastFrameAvgPivot);
					}
					else
					{
						foreach (Transform t in SelectedTransforms)
							Utils.DrawDisc(SavableEditorPrefs.DiscLocalSpaceColor,
								sceneCam.transform.forward, t.position);
					}
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
				// First - save the current sates.
				Vector3 currentColliderCenter = Vector3.zero;
				List<Vector3> currentLocalPositions = new List<Vector3>();

				if (BA.ColliderBeingEdited != null)//#colreg(green);
					currentColliderCenter = Utils.GetEditableColliderCenterLocalSpace(BA);//#endcolreg
				else
					for (int i = 0; i < SelectedTransforms.Length; i++)
						currentLocalPositions.Add(SelectedTransforms[i].localPosition);

				// Reset the models
				UpdatePositions(OrigAvgPivot);

				//Save an undo record
				Undo.IncrementCurrentGroup();
				if (BA.ColliderBeingEdited != null)//#colreg(green);
					Undo.RecordObject(BA.ColliderBeingEdited, "Translate");//#endcolreg
				else
					for (int i = 0; i < SelectedTransforms.Length; i++)
						Undo.RecordObject(SelectedTransforms[i], "Translate");

				// Restore the saved models states
				if (BA.ColliderBeingEdited != null)//#colreg(green);
					Utils.SetEditableColliderCenterLocalSpace(currentColliderCenter, BA);//#endcolreg
				else
					for (int i = 0; i < SelectedTransforms.Length; i++)
						SelectedTransforms[i].localPosition = currentLocalPositions[i];
			}
		}

		/// <summary>Revert any changes made, remove an Undo record if needed.</summary>
		public override void Cancel()
		{
            base.Cancel();

			if (!TerminateAction)
				UpdatePositions(OrigAvgPivot);
		}

		/// <summary>The actual logic of the translation action.</summary>
		private void CalculatePosition()
        {
			#region #Color(darkcyan);
            Camera sceneCam = SceneView.lastActiveSceneView.camera;

			// Prevent a glitch that inverts movement in some cases.
			Vector3 camViewPlaneNormal = sceneCam.transform.forward;
			Vector3 Z_Difference = sceneCam.transform.position - OrigAvgPivot;
			float dot = Vector3.Dot(Z_Difference, camViewPlaneNormal);


			// Raw mouse coordinates have y flipped from what unity uses.
			Vector2 mousePos = BA.CurrentEvent.mousePosition;
			mousePos.y = sceneCam.pixelHeight - mousePos.y;

			if (BA.SlowDownTransformsThisFrame)
				MouseOffset += (mousePos - LastMousePos) * 0.9f;

			Vector2 toNewMouse = mousePos - OriginalMousePos - MouseOffset;
			if (dot > 0 && !sceneCam.orthographic)
				toNewMouse *= -1;

			LastMousePos = mousePos;

			Vector2 objInSP = sceneCam.WorldToScreenPoint(OrigAvgPivot);
            Vector2 newObjPosInSP = objInSP + toNewMouse;

            Vector3 moveTo = Vector3.zero;
            Plane movePlane = new Plane();
            Ray rayToNewPos = sceneCam.ScreenPointToRay(newObjPosInSP);
			rayToNewPos.origin -= rayToNewPos.direction * 100;




			if (BA.TransformSpace == Space.World)
            {
                if (BA.TransformLock == Lock.None)
                {
                    movePlane = new Plane(camViewPlaneNormal, OrigAvgPivot);
                    moveTo = CastRayAndGetPosition(movePlane, rayToNewPos);
                }
                else if (BA.TransformLock == Lock.X_Axis || BA.TransformLock == Lock.Y_Axis
					|| BA.TransformLock == Lock.Z_Axis)
                {
                    Vector3 planeNormal = camViewPlaneNormal;
                    moveTo = OrigAvgPivot;

                    if (BA.TransformLock == Lock.X_Axis)
                        planeNormal.x = 0;
                    else if (BA.TransformLock == Lock.Y_Axis)
                        planeNormal.y = 0;
                    else
                        planeNormal.z = 0;

                    moveTo = GetGlobalProjectedAxisMotion(planeNormal.normalized, rayToNewPos, sceneCam);
                }
                else
                {
                    if (BA.TransformLock == Lock.YZ_Plane)
                        movePlane = new Plane(Vector3.Cross(Vector3.up, Vector3.forward), OrigAvgPivot);
                    else if (BA.TransformLock == Lock.XZ_Plane)
						movePlane = new Plane(Vector3.Cross(Vector3.right, Vector3.forward), OrigAvgPivot);
                    else
                        movePlane = new Plane(Vector3.Cross(Vector3.right, Vector3.up), OrigAvgPivot);

                    moveTo = CastRayAndGetPosition(movePlane, rayToNewPos);
				}

				UpdatePositions(moveTo);

				// Handle vertex snapping ONLY AFTER the original transformations.	#colreg(red*0.3);
				if (!BA.NumericSnap && BA.VertexSnapON_ThisFrame)
				{
					BA.LastVertexSnapTime = EditorApplication.timeSinceStartup + BlenderActions.VertexSnapTimeInterval;
					BA.AntiHang = 0;

					moveTo = Vector3.zero;

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
					Vector3 closestVertexLocalSpace = Vector3.zero;
					Vector3 closestSelectedVertexLocalSpace = Vector3.zero;
					GameObject closestGO = null;
					if (Utils.Find2VerticesForVertexSnapping(sceneCam, correctedMousePos,
						out closestVertexWorldSpace, out closestSelectedVertexWorldSpace, out closestGO, returnOnly1stVertex)
						== targetResult)
					{
						if (BA.Position3DCursorToLastSnapPoint)                                //#color(orange*3);
							BA.The3DCursorPos = closestVertexWorldSpace;                       //#color(orange*3);

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

							// Move to local space as the collider offset is applied in local space.
							closestVertexLocalSpace = BA.ColliderBeingEdited.transform.InverseTransformPoint(closestVertexWorldSpace);
							closestSelectedVertexLocalSpace
								= BA.ColliderBeingEdited.transform.InverseTransformPoint(closestSelectedVertexWorldSpace);

							moveTo = closestVertexLocalSpace - closestSelectedVertexLocalSpace;
						}//#endcolreg
						else
							moveTo = closestVertexWorldSpace - closestSelectedVertexWorldSpace;

						if (BA.TransformLock != Lock.None)
						{
							if (BA.TransformLock == Lock.X_Axis)
							{
								moveTo.y = 0;
								moveTo.z = 0;
							}
							else if (BA.TransformLock == Lock.Y_Axis)
							{
								moveTo.x = 0;
								moveTo.z = 0;
							}
							else if (BA.TransformLock == Lock.Z_Axis)
							{
								moveTo.y = 0;
								moveTo.x = 0;
							}
							else
							{
								if (BA.TransformLock == Lock.YZ_Plane)
									moveTo.x = 0;
								else if (BA.TransformLock == Lock.XZ_Plane)
									moveTo.y = 0;
								else
									moveTo.z = 0;
							}
						}


						if (BA.ColliderBeingEdited != null)                         //#colreg(green);
							Utils.MoveEditableColliderCenterLocalSpace(moveTo, BA); //#endcolreg
						else
						{
							for (int k = 0; k < SelectedTransforms.Length; k++)
								SelectedTransforms[k].position += moveTo;

							LastFrameAvgPivot += moveTo;
						}
					}
				}// 	#endcolreg
			}
			else
            {
                for (int i = 0; i < SelectedTransforms.Length; i++)
                {
                    if (BA.TransformLock == Lock.X_Axis || BA.TransformLock == Lock.Y_Axis
						|| BA.TransformLock == Lock.Z_Axis)
                    {
						Vector3 vec1 = Vector3.zero;
						Vector3 vec2 = Vector3.zero;

						if (BA.TransformLock == Lock.X_Axis)
						{
							vec1 = SelectedTransforms[i].right;
							vec2 = SelectedTransforms[i].forward;
						}
						else if (BA.TransformLock == Lock.Y_Axis)
						{
							vec1 = SelectedTransforms[i].up;
							vec2 = SelectedTransforms[i].forward;
						}
						else
						{
							vec1 = SelectedTransforms[i].right;
							vec2 = SelectedTransforms[i].forward;
						}

						movePlane = new Plane(Vector3.Cross(vec1, vec2), OrigAvgPivot);
						moveTo = CastRayAndGetPositionLocal(movePlane, rayToNewPos, i) + OrigObjectsOffsets[i];

						moveTo = SelectedTransforms[i].InverseTransformVector(moveTo);
						Vector3 localPos = SelectedTransforms[i].InverseTransformVector(SelectedTransforms[i].position);
						Vector3 difference = moveTo - localPos;

						if (BA.TransformLock == Lock.X_Axis)
						{
							difference.y = 0;
							difference.z = 0;
						}
						else if (BA.TransformLock == Lock.Y_Axis)
						{
							difference.x = 0;
							difference.z = 0;
						}
						else
						{
							difference.y = 0;
							difference.x = 0;
						}

						moveTo = localPos + difference;
							
						moveTo = SelectedTransforms[i].TransformVector(moveTo);
					}
					else
                    {
                        Vector3 vec1 = Vector3.zero;
                        Vector3 vec2 = Vector3.zero;

                        if (BA.TransformLock == Lock.YZ_Plane)
						{
							vec1 = SelectedTransforms[i].up;
							vec2 = SelectedTransforms[i].forward;
                        }
                        else if (BA.TransformLock == Lock.XZ_Plane)
						{
							vec1 = SelectedTransforms[i].right;
                            vec2 = SelectedTransforms[i].forward;
						}
                        else
                        {
                            vec1 = SelectedTransforms[i].right;
							vec2 = SelectedTransforms[i].up;
						}

                        movePlane = new Plane(Vector3.Cross(vec1, vec2), SelectedTransforms[i].position);
                        moveTo = CastRayAndGetPositionLocal(movePlane, rayToNewPos, i) + OrigObjectsOffsets[i];
					}

					UpdateSinglePosition(moveTo, i);
				}

				// Handle vertex snapping ONLY AFTER the original transformations.	#colreg(red*0.3);
				if (!BA.NumericSnap && BA.VertexSnapON_ThisFrame)
				{
					BA.LastVertexSnapTime = EditorApplication.timeSinceStartup + BlenderActions.VertexSnapTimeInterval;
					BA.AntiHang = 0;

					moveTo = Vector3.zero;

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
					Vector3 closestVertexLocalSpace = Vector3.zero;
					Vector3 closestSelectedVertexLocalSpace = Vector3.zero;
					GameObject closestGO = null;
					if (Utils.Find2VerticesForVertexSnapping(sceneCam, correctedMousePos,
						out closestVertexWorldSpace, out closestSelectedVertexWorldSpace, out closestGO, returnOnly1stVertex)
						== targetResult)
					{
						if (BA.Position3DCursorToLastSnapPoint)
							BA.The3DCursorPos = closestVertexWorldSpace;

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

							// Move to local space as the collider offset is applied in local space.
							closestVertexLocalSpace = BA.ColliderBeingEdited.transform.InverseTransformPoint(closestVertexWorldSpace);
							closestSelectedVertexLocalSpace
								= BA.ColliderBeingEdited.transform.InverseTransformPoint(closestSelectedVertexWorldSpace);

							moveTo = closestVertexLocalSpace - closestSelectedVertexLocalSpace;
						}//#endcolreg
						else
						{
							closestVertexLocalSpace
								= closestGO.transform.InverseTransformDirection(closestVertexWorldSpace);
							closestSelectedVertexLocalSpace
								= closestGO.transform.InverseTransformDirection(closestSelectedVertexWorldSpace);

							moveTo = closestVertexLocalSpace - closestSelectedVertexLocalSpace;
						}

						if (BA.TransformLock == Lock.X_Axis || BA.TransformLock == Lock.Y_Axis
							|| BA.TransformLock == Lock.Z_Axis)
						{
							if (BA.TransformLock == Lock.X_Axis)
							{
								moveTo.y = 0;
								moveTo.z = 0;
							}
							else if (BA.TransformLock == Lock.Y_Axis)
							{
								moveTo.x = 0;
								moveTo.z = 0;
							}
							else
							{
								moveTo.y = 0;
								moveTo.x = 0;
							}
						}
						else
						{
							if (BA.TransformLock == Lock.YZ_Plane)
								moveTo.x = 0;
							else if (BA.TransformLock == Lock.XZ_Plane)
								moveTo.y = 0;
							else
								moveTo.z = 0;
						}


						if (BA.ColliderBeingEdited != null)                         //#colreg(green);
							Utils.MoveEditableColliderCenterLocalSpace(moveTo, BA); //#endcolreg
						else
						{
							moveTo = closestGO.transform.TransformDirection(moveTo);
							for (int i = 0; i < SelectedTransforms.Length; i++)
							{
								SelectedTransforms[i].position += moveTo;
								LastFrameLocalPos[i] += moveTo;
							}
						}
					}
				}// #endcolreg;
			}
			#endregion
		}

        private Vector3 CastRayAndGetPosition(Plane plane, Ray ray)
        {
            float distance;

			if (plane.Raycast(ray, out distance))
				return ray.origin + (ray.direction * distance);
			else
				return LastFrameAvgPivot;
        }

        private Vector3 CastRayAndGetPositionLocal(Plane plane, Ray ray, int index)
        {
            float distance;

            if (plane.Raycast(ray, out distance))
                return ray.origin + (ray.direction * distance);
            else
                return LastFrameLocalPos[index];
		}

		/// <summary>Apply the calculated translation for WORLD space translations.</summary>
		private void UpdatePositions(Vector3 moveTo)
		{
			#region #Color(darkcyan*2);
			if (BA.NumericSnap)
				moveTo = PerformNumericSnap(moveTo);

			if (BA.ColliderBeingEdited != null)						//#colreg(green);
				TranslateCollider(moveTo);							//#endcolreg
			else
			{
				LastFrameAvgPivot = moveTo;

				for (int i = 0; i < SelectedTransforms.Length; i++)
					SelectedTransforms[i].position = moveTo + OrigObjectsOffsets[i];
			}
			#endregion
		}

		private Vector3 GetGlobalProjectedAxisMotion(Vector3 planeNormal, Ray mouseRay, Camera sceneCam)
        {
            Plane movePlane = new Plane(planeNormal, OrigAvgPivot);
            float distance;
            Vector3 contactPoint;
            Vector3 moveTo = OrigAvgPivot;

			// Prevent a glitch that inverts movement in some cases.
			float dot = Vector3.Dot(planeNormal, mouseRay.direction);

			if (movePlane.Raycast(mouseRay, out distance))
            {
                contactPoint = mouseRay.origin + (mouseRay.direction * distance);
				if (dot < 0 && !sceneCam.orthographic)
					contactPoint = 2 * OrigAvgPivot - contactPoint;

				if (BA.TransformLock == Lock.X_Axis)
					moveTo.x = contactPoint.x;
                else if (BA.TransformLock == Lock.Y_Axis)
					moveTo.y = contactPoint.y;
                else
                    moveTo.z = contactPoint.z;
            }
            else
                moveTo = LastFrameAvgPivot;

            return moveTo;
        }

		
		/// <summary>The implementation of the numeric snap option.</summary>
		private Vector3 PerformNumericSnap(Vector3 vecToSnap)
        {
            if (SavableEditorPrefs.TranslateSnapIncrement == 0)
            {
                // I guess no snapping...
                return vecToSnap;
            }

            vecToSnap /= SavableEditorPrefs.TranslateSnapIncrement;

            vecToSnap.x = Mathf.Round(vecToSnap.x);
            vecToSnap.y = Mathf.Round(vecToSnap.y);
            vecToSnap.z = Mathf.Round(vecToSnap.z);

            vecToSnap *= SavableEditorPrefs.TranslateSnapIncrement;

            return vecToSnap;
		}

		/// <summary>The implementation of the collider translation option.</summary>
		private void TranslateCollider(Vector3 moveTo)
		{//#colreg(green);
			if (BA.ColliderBeingEdited != null)
			{
				moveTo = BA.ColliderBeingEdited.transform.InverseTransformPoint(moveTo) + OrigColliderCenterLocalSpace;

				Utils.SetEditableColliderCenterLocalSpace(moveTo, BA);
			}
		}//#endcolreg

		/// <summary>Apply the calculated translation for LOCAL space translations.</summary>
		private void UpdateSinglePosition(Vector3 moveTo, int index)
		{
			#region #Color(darkcyan*2);
			if (BA.NumericSnap)
				moveTo = PerformNumericSnap(moveTo);

			LastFrameLocalPos[index] = moveTo;
			if (BA.ColliderBeingEdited != null)		//#colreg(green);
				TranslateCollider(moveTo);							//#endcolreg
			else
				SelectedTransforms[index].position = moveTo;
			#endregion
		}
	}
}
