//#define U5
#define U5_6
//#define U2017

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace BlenderActions
{
    public class Scale : BaseTransform
	{
		private float OriginalDistance;
        private Vector3[] OriginalScales;
        private Vector3[] OriginalPositions;
		/// <summary>Screen space position of the mouse at the start of the translation action.</summary>
		private Vector2 OriginalMousePos;

		//#colreg(green);
		private Vector3 OriginalColliderCenterLocalSpace;
		private Vector3 OriginalColliderCenterWorldSpace;
		private Vector3 OriginalColliderSize;
		private float OriginalColliderHeight;
		private float OriginalColliderRadius;
		//#endcolreg

		#region "Shift-slowdown" functionality.
		/// <summary>Accumulated while user is holding down "shift".</summary>
		protected Vector2 MouseOffset;
		/// <summary>Last frame screen space mouse posision.</summary>
		protected Vector2 LastMousePos;
		#endregion

		public Scale(BlenderActions blenderActions)
			: base(blenderActions)
		{
        }

		/// <summary>Initialize starting variables for this action.</summary>
        public override void Start()
        {
			#region #Color(darkorange*0.5);
			base.Start();

			if (BA.ColliderBeingEdited != null)     //#colreg(green);
			{
				OriginalColliderCenterLocalSpace = Utils.GetEditableColliderCenterLocalSpace(BA);
				OriginalColliderCenterWorldSpace = BA.ColliderBeingEdited.transform.TransformPoint(OriginalColliderCenterLocalSpace);
				GetColliderParameters(out OriginalColliderSize, out OriginalColliderRadius, out OriginalColliderHeight);
			}       //#endcolreg
			OriginalMousePos = BA.CurrentEvent.mousePosition;


			// Just in case the order isn't guaranteed, I'm going to save the selecteds.
			OrigAvgPivot = Vector3.zero;
            OriginalScales = new Vector3[SelectedTransforms.Length];
			OriginalPositions = new Vector3[SelectedTransforms.Length];

			MouseOffset = Vector2.zero;
			LastMousePos = BA.CurrentEvent.mousePosition;

			for (int i = 0; i < SelectedTransforms.Length; i++)
            {
                OrigAvgPivot += SelectedTransforms[i].position;
                OriginalScales[i] = SelectedTransforms[i].localScale;
				OriginalPositions[i] = SelectedTransforms[i].position;
			}

            OrigAvgPivot /= SelectedTransforms.Length;

			Camera sceneCam = SceneView.lastActiveSceneView.camera;

			Vector3 pivot = OrigAvgPivot;
			if (BA.Use3DCursor)                                         //#color(orange*3);
				pivot = BA.The3DCursorPos;                              //#color(orange*3);
			else if (BA.ColliderBeingEdited != null)                    //#colreg(green);
				pivot = OriginalColliderCenterWorldSpace;               //#endcolreg

			Vector2 pivotPosScreenSpace = sceneCam.WorldToScreenPoint(pivot);
			pivotPosScreenSpace.y = sceneCam.pixelHeight - pivotPosScreenSpace.y;
            OriginalDistance = Vector2.Distance(LastMousePos, pivotPosScreenSpace);

            if (OriginalDistance == 0 || float.IsNaN(OriginalDistance))
                OriginalDistance = 0.1f;
			#endregion
		}





		void GetColliderParameters(out Vector3 size, out float radius, out float height)
		{//#colreg(green);
			size = Vector3.zero;
			radius = 0;
			height = 0;
			if (BA.EditedBoxCollider != null)
				size = BA.EditedBoxCollider.size;
			else if (BA.EditedSphereCollider != null)
				radius = BA.EditedSphereCollider.radius;
			else if (BA.EditedCapsuleCollider != null)
			{
				height = BA.EditedCapsuleCollider.height;
				radius = BA.EditedCapsuleCollider.radius;
			}
#if U2017
			else if (BA.EditedBoxCollider2D != null)
				size = BA.EditedBoxCollider2D.size;
			else if (BA.EditedCapsuleCollider2D != null)
				size = BA.EditedCapsuleCollider2D.size;
			//else if (BA.EditedPolygonCollider2D != null)
			//	OriginalColliderCenterLocalSpace = BA.EditedPolygonCollider2D.offset;
			else if (BA.EditedCircleCollider2D != null)
				radius = BA.EditedCircleCollider2D.radius;
			else if (BA.EditedEdgeCollider2D != null)
				radius = BA.EditedEdgeCollider2D.edgeRadius;
			else if (BA.EditedWheelCollider != null)
				radius = BA.EditedWheelCollider.radius;
#endif
		}//#endcolreg


		void SetColliderParameters(Vector3 size, float radius, float height)
		{//#colreg(green);
			if (BA.EditedBoxCollider != null)
				BA.EditedBoxCollider.size = size;
			else if (BA.EditedSphereCollider != null)
				BA.EditedSphereCollider.radius = radius;
			else if (BA.EditedCapsuleCollider != null)
			{
				BA.EditedCapsuleCollider.height = height;
				BA.EditedCapsuleCollider.radius = radius;
			}
#if U2017
			else if (BA.EditedBoxCollider2D != null)
				size = BA.EditedBoxCollider2D.size = size;
			else if (BA.EditedCapsuleCollider2D != null)
				size = BA.EditedCapsuleCollider2D.size = size;
			//else if (BA.EditedPolygonCollider2D != null)
			//	BA.EditedPolygonCollider2D.offset;
			else if (BA.EditedCircleCollider2D != null)
				BA.EditedCircleCollider2D.radius = radius;
			else if (BA.EditedEdgeCollider2D != null)
				BA.EditedEdgeCollider2D.edgeRadius = radius;
			else if (BA.EditedWheelCollider != null)
				BA.EditedWheelCollider.radius = radius;
#endif
		}//#endcolreg






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
						|| NumericInput.EnteredNumber != 0)    // #Color(lime*3);
					{
						CalculateScale();
					}


					// Draw a solid disc for each selected transform.
					Camera sceneCam = SceneView.lastActiveSceneView.camera;
					Vector3 camViewPlaneNormal = sceneCam.transform.forward;
					Utils.DrawLines(OrigAvgPivot, SelectedTransforms, BA.TransformLock, BA.TransformSpace, checkSpace: false);

					Vector3 discPos = OrigAvgPivot;
					if (BA.Use3DCursor)                     //#color(orange*3);
						discPos = BA.The3DCursorPos;        //#color(orange*3);
					else if (BA.ColliderBeingEdited != null)                //#color(green);
						discPos = OriginalColliderCenterWorldSpace;         //#color(green);
					Utils.DrawDisc(SavableEditorPrefs.DiscLocalSpaceColor,
						sceneCam.transform.forward, discPos);

					if (!BA.Use3DCursor && BA.ColliderBeingEdited == null && SelectedTransforms.Length > 1)
					{
						for (int i = 0; i < SelectedTransforms.Length; i++)
						{
							discPos = SelectedTransforms[i].position;
							Utils.DrawDisc(SavableEditorPrefs.DiscWorldSpaceColor,
								sceneCam.transform.forward, discPos);
						}
					}

					Vector3 pivot = OrigAvgPivot;
					if (BA.Use3DCursor)                                 //#color(orange*3);
						pivot = BA.The3DCursorPos;                      //#color(orange*3);
					else if (BA.ColliderBeingEdited != null)            //#color(green);
						pivot = OriginalColliderCenterWorldSpace;       //#color(green);

					Utils.DrawDottedLineToTheCenter(pivot);
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
				List<Vector3> currentLocalScale = new List<Vector3>();

				Vector3 currentSize = Vector3.zero;
				float currentRadius = 1;
				float currentHeight = 1;

				if (BA.ColliderBeingEdited != null)//#colreg(green);
				{
					currentColliderCenter = Utils.GetEditableColliderCenterLocalSpace(BA);
					GetColliderParameters(out currentSize, out currentRadius, out currentHeight);//#endcolreg
				}
				else
				{
					for (int i = 0; i < SelectedTransforms.Length; i++)
					{
						currentLocalPositions.Add(SelectedTransforms[i].localPosition);
						currentLocalScale.Add(SelectedTransforms[i].localScale);
					}
				}

				// Reset the models
				UpdateScale(Vector3.one);

				//Save an undo record
				Undo.IncrementCurrentGroup();
				if (BA.ColliderBeingEdited != null)//#colreg(green);
					Undo.RecordObject(BA.ColliderBeingEdited, "Scale");//#endcolreg
				else
					for (int i = 0; i < SelectedTransforms.Length; i++)
						Undo.RecordObject(SelectedTransforms[i], "Scale");

				// Restore the saved models states
				if (BA.ColliderBeingEdited != null)//#colreg(green);
				{
					Utils.SetEditableColliderCenterLocalSpace(currentColliderCenter, BA);
					SetColliderParameters(currentSize, currentRadius, currentHeight);//#endcolreg
				}
				else
				{
					for (int i = 0; i < SelectedTransforms.Length; i++)
					{
						SelectedTransforms[i].localPosition = currentLocalPositions[i];
						SelectedTransforms[i].localScale = currentLocalScale[i];
					}
				}
			}
		}

		/// <summary>Revert any changes made, remove an Undo record.</summary>
		public override void Cancel()
        {
            base.Cancel();

			if (!TerminateAction)
				UpdateScale(Vector3.one);
		}


		private Vector3 ApplyAxisLimtations(Vector3 inputScale)
		{
			Vector3 scaleBy = Vector3.one;

			if (BA.TransformLock == Lock.None)
				scaleBy = inputScale;
			else if (BA.TransformLock == Lock.X_Axis || BA.TransformLock == Lock.Y_Axis
					|| BA.TransformLock == Lock.Z_Axis)
			{
				if (BA.TransformLock == Lock.X_Axis)
					scaleBy.x *= inputScale.x;
				else if (BA.TransformLock == Lock.Y_Axis)
					scaleBy.y *= inputScale.y;
				else
					scaleBy.z *= inputScale.z;
			}
			else
			{
				scaleBy = inputScale;

				if (BA.TransformLock == Lock.YZ_Plane)
					scaleBy.x = 1;
				else if (BA.TransformLock == Lock.XZ_Plane)
					scaleBy.y = 1;
				else
					scaleBy.z = 1;
			}

			return scaleBy;
		}


		/// <summary>The actual logic of the scaling action.</summary>
        private void CalculateScale()
		{
			//#colreg(darkcyan);
			float scaleFactor = 1;

			Camera sceneCam = SceneView.lastActiveSceneView.camera;

			// Prevent a glitch that inverts movement in some cases.
			Vector3 camViewPlaneNormal = sceneCam.transform.forward;
			Vector3 Z_Difference = sceneCam.transform.position - OrigAvgPivot;
			float dot = Vector3.Dot(Z_Difference, camViewPlaneNormal);

			Vector2 upsideDownMousePos = BA.CurrentEvent.mousePosition;

			if (NumericInput.EnteredNumber != 0)						// #Color(lime*3);
				scaleFactor = NumericInput.EnteredNumber;				// #Color(lime*3);
			else
			{
				Vector3 pivot = OrigAvgPivot;
				if (BA.Use3DCursor)                                         //#color(orange*3);
					pivot = BA.The3DCursorPos;                              //#color(orange*3);
				else if (BA.ColliderBeingEdited != null)                    //#colreg(green);
					pivot = OriginalColliderCenterWorldSpace;				//#endcolreg

				Vector2 pivotPosScreenSpace = sceneCam.WorldToScreenPoint(pivot);
				pivotPosScreenSpace.y = sceneCam.pixelHeight - pivotPosScreenSpace.y;

				if (BA.SlowDownTransformsThisFrame)
					MouseOffset += (upsideDownMousePos - LastMousePos) * 0.9f;

				LastMousePos = upsideDownMousePos;
				upsideDownMousePos -= MouseOffset;

				if (dot > 0 && !sceneCam.orthographic)
					upsideDownMousePos = OriginalMousePos * 2 - upsideDownMousePos;

				float newDistance = Vector2.Distance(upsideDownMousePos, pivotPosScreenSpace);

				scaleFactor = newDistance / OriginalDistance;
			}

			Vector3 scaleBy = ApplyAxisLimtations(new Vector3(scaleFactor, scaleFactor, scaleFactor));
			UpdateScale(scaleBy);
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

				// If the user has set a transform lock, every time we evaluate a vertex
				//	we would gauge whether it would produce zero in one of it's variables in
				//	local space if the local space pivot point is subtracted from it.
				//	If it does - we drop the vertex
				bool ignoreZeroesOnX = false;
				bool ignoreZeroesOnY = false;
				bool ignoreZeroesOnZ = false;

				if (BA.TransformLock == Lock.XY_Plane || BA.TransformLock == Lock.XZ_Plane
					|| BA.TransformLock == Lock.X_Axis)
					ignoreZeroesOnX = true;

				if (BA.TransformLock == Lock.YZ_Plane || BA.TransformLock == Lock.XY_Plane
					|| BA.TransformLock == Lock.Y_Axis)
					ignoreZeroesOnY = true;

				if (BA.TransformLock == Lock.YZ_Plane || BA.TransformLock == Lock.XZ_Plane
					|| BA.TransformLock == Lock.Z_Axis)
					ignoreZeroesOnZ = true;

				Vector3 customPivotLocalSpace = OrigAvgPivot;
				if (BA.Use3DCursor)														//#color(orange*3);
					customPivotLocalSpace = BA.The3DCursorPos;							//#color(orange*3);
				else if (BA.ColliderBeingEdited != null)								//#colreg(green);
					customPivotLocalSpace = OriginalColliderCenterWorldSpace;			//#endcolreg

				if (BA.ColliderBeingEdited != null)                             //#colreg(green);
					customPivotLocalSpace 
						= BA.ColliderBeingEdited.transform.InverseTransformPoint(customPivotLocalSpace);            //#endcolreg
				else
					customPivotLocalSpace = SelectedTransforms[0].transform.InverseTransformPoint(customPivotLocalSpace);

				Vector3 closestVertexWorldSpace = Vector3.zero;
				Vector3 closestSelectedVertexWorldSpace = Vector3.zero;
				GameObject closestGO = null;
				if (Utils.Find2VerticesForVertexSnapping(sceneCam, correctedMousePos,
					out closestVertexWorldSpace, out closestSelectedVertexWorldSpace, out closestGO, returnOnly1stVertex,
					ignoreZeroesOnX, ignoreZeroesOnY, ignoreZeroesOnZ, customPivotLocalSpace)
						== targetResult)
				{

					Transform toLocalSpace = null;	// we will use this to later move to the local space

					// If we are in collider editing mode - find closestSelectedVertexWorldSpace			 #colreg(green);
					//	amongst the collider "vertices".
					if (BA.ColliderBeingEdited != null)
					{
						toLocalSpace = BA.ColliderBeingEdited.transform;
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
					else if (SelectedTransforms.Length > 0 && SelectedTransforms[0] != null)
						toLocalSpace = SelectedTransforms[0];

					// Move all the vertices into the local space because the scaling operation can only happen in local space.
					//	The first model's space is used as the only space for all the rest of the models.
					if (toLocalSpace != null)
					{
						Vector3 closestVertexLocalSpace = toLocalSpace.transform.InverseTransformPoint(closestVertexWorldSpace);

						Vector3 closestSelectedVertexLocalSpace
							= toLocalSpace.transform.InverseTransformPoint(closestSelectedVertexWorldSpace);

						// Subtract the pivot from each of the vertices to find the relative difference vector
						closestVertexLocalSpace -= customPivotLocalSpace;
						closestSelectedVertexLocalSpace -= customPivotLocalSpace;

						Vector3 vertexSnapScale = Vector3.one;

						if (BA.TransformLock == Lock.None)
						{
							// If no axis lock - compute magnitudes of both vectors and find their ratio
							float ratio = closestVertexLocalSpace.magnitude / closestSelectedVertexLocalSpace.magnitude;
							vertexSnapScale = new Vector3(ratio, ratio, ratio);
							if (float.IsNaN(ratio))
								vertexSnapScale = Vector3.one;
						}
						else
						{
							// If there is an axis lock, find the ratios in each of the three variables
							vertexSnapScale.x = closestVertexLocalSpace.x / closestSelectedVertexLocalSpace.x;
							if (float.IsNaN(vertexSnapScale.x) || float.IsInfinity(vertexSnapScale.x))
								vertexSnapScale.x = 1;
							vertexSnapScale.y = closestVertexLocalSpace.y / closestSelectedVertexLocalSpace.y;
							if (float.IsNaN(vertexSnapScale.y) || float.IsInfinity(vertexSnapScale.y))
								vertexSnapScale.y = 1;
								vertexSnapScale.z = closestVertexLocalSpace.z / closestSelectedVertexLocalSpace.z;
							if (float.IsNaN(vertexSnapScale.z) || float.IsInfinity(vertexSnapScale.z))
								vertexSnapScale.z = 1;

							vertexSnapScale = ApplyAxisLimtations(vertexSnapScale);
						}

						UpdateScale(new Vector3(scaleBy.x * vertexSnapScale.x,
							scaleBy.y * vertexSnapScale.y, scaleBy.z * vertexSnapScale.z));
					}
				}
			}   //#endcolreg
			//#endcolreg
		}

		/// <summary>The implementation of the numeric snap option.</summary>
		private void PerformNumericSnap()
        {
			if (BA.NumericSnap && SavableEditorPrefs.ScaleSnapIncrement != 0)
			{
				foreach (Transform t in SelectedTransforms)
				{
					Vector3 vecToSnap = t.localScale;

					vecToSnap /= SavableEditorPrefs.ScaleSnapIncrement;

					vecToSnap.x = Mathf.Round(vecToSnap.x);
					vecToSnap.y = Mathf.Round(vecToSnap.y);
					vecToSnap.z = Mathf.Round(vecToSnap.z);

					vecToSnap *= SavableEditorPrefs.ScaleSnapIncrement;

					t.localScale = vecToSnap;
				}
			}
		}

		/// <summary>The implementation of the collider scaling option (translation only)</summary>
		private void ApplyColliderTranslation(Vector3 newPos)
		{//#colreg(green);
			// Transform the new position into local space
			newPos = BA.ColliderBeingEdited.transform.InverseTransformPoint(newPos);
			//Apply the new position
			Utils.SetEditableColliderCenterLocalSpace(newPos, BA);
		}//#endcolreg


		/// <summary>The implementation of the collider scaling option (scale only)</summary>
		private void ApplyColliderScale(Vector3 scaleBy)
		{//#colreg(green);
			if (BA.EditedBoxCollider != null)
			{
				BA.EditedBoxCollider.size 
					= new Vector3(OriginalColliderSize.x * scaleBy.x,
						OriginalColliderSize.y * scaleBy.y,
						OriginalColliderSize.z * scaleBy.z);
			}
			else if (BA.EditedSphereCollider != null)
			{
				BA.EditedSphereCollider.radius = OriginalColliderRadius *
					Mathf.Max(Mathf.Max(scaleBy.x, scaleBy.y), scaleBy.z);
			}
			else if (BA.EditedCapsuleCollider != null)
			{
				if (BA.TransformLock == Lock.None)
				{
					BA.EditedCapsuleCollider.radius = OriginalColliderRadius *
						Mathf.Max(Mathf.Max(scaleBy.x, scaleBy.y), scaleBy.z);
					BA.EditedCapsuleCollider.height = OriginalColliderHeight;
				}
				else
				{
					BA.EditedCapsuleCollider.radius = OriginalColliderRadius;

					float scaleHeight = 1;
					if (BA.TransformLock == Lock.X_Axis)
						scaleHeight = scaleBy.x;
					else if (BA.TransformLock == Lock.Y_Axis)
						scaleHeight = scaleBy.y;
					else
						scaleHeight = scaleBy.z;

					BA.EditedCapsuleCollider.height = OriginalColliderHeight * scaleHeight;
				}
			}
		}//#endcolreg

		/// <summary>Apply the calculated scales.</summary>
		private void UpdateScale(Vector3 scaleBy)
		{
#region #Color(darkcyan*2);
			if (SelectedTransforms != null)
			{
				for (int i = 0; i < SelectedTransforms.Length; i++)
				{
					Matrix4x4 scaleMat = Matrix4x4.Scale(scaleBy);

					Vector3 scalingPivot = OriginalPositions[i];
					if (BA.Use3DCursor)                         //#color(orange*3);
						scalingPivot = BA.The3DCursorPos;       //#color(orange*3);
					else if (BA.ColliderBeingEdited != null)				//#colreg(green);
						scalingPivot = OriginalColliderCenterWorldSpace;	//#endcolreg

					// Scaling is easy: change the scale values of the model and u r done.
					//	The tricky part comes when we need to scale the model with our custom pivot (the 3D cursor).
					//	In theory, we just need to find an offset (model pivot pos - 3D cursor), scale it with
					//	a scale matrix and assign the new value to the position. However, this only works for uniform
					//	scaling. Once we start scaling on only X axis or any other axis this breaks. The key is
					//	to scale in local space. To do this, both vertices need to be transfered into the local space,
					//	get their difference computed there, get the difference scaled by the matrix, add the location
					//	of our 3D cursor back to that scaled difference and then transfered back to world space.

					Vector3 modelPivot = OriginalPositions[i];

					if (BA.ColliderBeingEdited != null)					//#colreg(green);
						modelPivot = OriginalColliderCenterWorldSpace;	//#endcolreg

					// Compute local space model pivot
					Vector3 modelPivotLocalSpace = SelectedTransforms[i].transform.InverseTransformPoint(modelPivot);
					// Compute local space scaling pivot
					Vector3 scalingPivotLocalSpace = SelectedTransforms[i].transform.InverseTransformPoint(scalingPivot);
					// Compute local space difference vector
					Vector3 differenceLocalSpace = modelPivotLocalSpace - scalingPivotLocalSpace;

					// Scale the offset in local space.
					differenceLocalSpace = scaleMat.MultiplyPoint(differenceLocalSpace);

					// Add the offset back in local space.
					modelPivotLocalSpace = differenceLocalSpace + scalingPivotLocalSpace;

					// Transfer the result into the world space.
					Vector3 newPos = SelectedTransforms[i].transform.TransformPoint(modelPivotLocalSpace);

					if (!float.IsNaN(newPos.x) && !float.IsNaN(newPos.y) && !float.IsNaN(newPos.z))
					{
						if (BA.ColliderBeingEdited != null)                  //#colreg(green);
							ApplyColliderTranslation(newPos);                //#endcolreg
						else
							SelectedTransforms[i].position = newPos;

						if (BA.ColliderBeingEdited != null)                  //#colreg(green);
							ApplyColliderScale(scaleBy);                     //#endcolreg
						else
							SelectedTransforms[i].localScale
								= new Vector3(OriginalScales[i].x * scaleBy.x,
									OriginalScales[i].y * scaleBy.y,
									OriginalScales[i].z * scaleBy.z);
					}
				}
			}
#endregion
		}
	}
}
