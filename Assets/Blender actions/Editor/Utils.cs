//#define U5
#define U5_6
//#define U2017

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace BlenderActions
{
	public class Utils
	{

		/// <summary>Changes certain names of the KyeCode enumeration so that they are easier to read for the user</summary>
		/// <param name="k">Key code to change</param>
		/// <returns>Processed key code in string format</returns>
		public static string ProcessKeyCodeDisplayName(KeyCode k)
		{
			string result = k.ToString();
			if (result.Contains("Alpha"))
				result = result.Substring(5, result.Length - 5);
			else if (result.Contains("Keypad"))
				result = "Numpad" + result.Substring(6, result.Length - 6);

			return result;
		}

		//#colreg(green);
		#region Collider misc utils
		/// <summary>Adds a vector to the existing position of the edited collider center.</summary>
		/// <param name="moveTo">A vector to add to the collider's center</param>
		/// <param name="ba">A reference to the main instance of the BlenderActions class to take data from</param>
		public static void MoveEditableColliderCenterLocalSpace(Vector3 moveTo, BlenderActions ba)
		{
			Vector2 moveTo2D = new Vector2(moveTo.x, moveTo.y);

			if (ba.EditedBoxCollider != null)
				ba.EditedBoxCollider.center += moveTo;
			else if (ba.EditedSphereCollider != null)
				ba.EditedSphereCollider.center += moveTo;
			else if (ba.EditedCapsuleCollider != null)
				ba.EditedCapsuleCollider.center += moveTo;
#if U2017
			else if (ba.EditedBoxCollider2D != null)
				ba.EditedBoxCollider2D.offset += moveTo2D;
			else if (ba.EditedCapsuleCollider2D != null)
				ba.EditedCapsuleCollider2D.offset += moveTo2D;
			else if (ba.EditedPolygonCollider2D != null)
				ba.EditedPolygonCollider2D.offset += moveTo2D;
			else if (ba.EditedCircleCollider2D != null)
				ba.EditedCircleCollider2D.offset += moveTo2D;
			else if (ba.EditedEdgeCollider2D != null)
				ba.EditedEdgeCollider2D.offset += moveTo2D;
			else if (ba.EditedWheelCollider != null)
				ba.EditedWheelCollider.center += moveTo;
#endif
		}

		/// <summary>Sets the center of the currently edited collider.</summary>
		/// <param name="newCenter">A vector to use as the new center of the edited collider</param>
		/// <param name="ba">A reference to the main instance of the BlenderActions class to take data from</param>
		public static void SetEditableColliderCenterLocalSpace(Vector3 newCenter, BlenderActions ba)
		{
			Vector2 newCenter2D = new Vector2(newCenter.x, newCenter.y);

			if (ba.EditedBoxCollider != null)
				ba.EditedBoxCollider.center = newCenter;
			else if (ba.EditedSphereCollider != null)
				ba.EditedSphereCollider.center = newCenter;
			else if (ba.EditedCapsuleCollider != null)
				ba.EditedCapsuleCollider.center = newCenter;
#if U2017
			else if (ba.EditedBoxCollider2D != null)
				ba.EditedBoxCollider2D.offset = newCenter2D;
			else if (ba.EditedCapsuleCollider2D != null)
				ba.EditedCapsuleCollider2D.offset = newCenter2D;
			else if (ba.EditedPolygonCollider2D != null)
				ba.EditedPolygonCollider2D.offset = newCenter2D;
			else if (ba.EditedCircleCollider2D != null)
				ba.EditedCircleCollider2D.offset = newCenter2D;
			else if (ba.EditedEdgeCollider2D != null)
				ba.EditedEdgeCollider2D.offset = newCenter2D;
			else if (ba.EditedWheelCollider != null)
				ba.EditedWheelCollider.center = newCenter;
#endif
		}

		/// <summary>Returns the center of the currently edited collider (for 2D colliders only x and y values are filled.</summary>
		/// <param name="ba">A reference to the main instance of the BlenderActions class to take data from</param>
		public static Vector3 GetEditableColliderCenterLocalSpace(BlenderActions ba)
		{
			Vector3 result = Vector3.zero;
			if (ba.EditedBoxCollider != null)
				result = ba.EditedBoxCollider.center;
			else if (ba.EditedSphereCollider != null)
				result = ba.EditedSphereCollider.center;
			else if (ba.EditedCapsuleCollider != null)
				result = ba.EditedCapsuleCollider.center;
#if U2017
			else if (ba.EditedBoxCollider2D != null)
				result = ba.EditedBoxCollider2D.offset;
			else if (ba.EditedCapsuleCollider2D != null)
				result = ba.EditedCapsuleCollider2D.offset;
			else if (ba.EditedPolygonCollider2D != null)
				result = ba.EditedPolygonCollider2D.offset;
			else if (ba.EditedCircleCollider2D != null)
				result = ba.EditedCircleCollider2D.offset;
			else if (ba.EditedEdgeCollider2D != null)
				result = ba.EditedEdgeCollider2D.offset;
			else if (ba.EditedWheelCollider != null)
				result = ba.EditedWheelCollider.center;
#endif

			return result;
		}
		#endregion

		#region Vertex snapping during collider editing
		/// <summary>Get the point on the surface of the CapsuleCollider that is closest to the given point (ALL in world space).</summary>
		/// <param name="capsuleCollider">The capsule collider to find the closest point on</param>
		/// <param name="targetPointWorldSpace">The point in world space that is the search guide</param>
		/// <returns>A vertex in world space</returns>
		public static Vector3 FindClosestCapsuleColliderVertex(CapsuleCollider capsuleCollider, Vector3 targetPosWorldSpace)
		{
			return capsuleCollider.ClosestPoint(targetPosWorldSpace);
		}

		/// <summary>Get the point on the surface of the SphereCollider that is closest to the given point (ALL in world space).</summary>
		/// <param name="sphereCollider">The sphere collider to find the closest point on</param>
		/// <param name="targetPointWorldSpace">The point in world space that is the search guide</param>
		/// <returns>A vertex in world space</returns>
		public static Vector3 FindClosestSphereColliderVertex(SphereCollider sphereCollider, Vector3 targetPosWorldSpace)
		{
			return sphereCollider.ClosestPoint(targetPosWorldSpace);
		}
		/// <summary>Returns the closest vertex of a box collider to the mouse cursor. Compares vertices in GUI space,
		/// returned vertex is in local space.</summary>
		public static Vector3 FindClosestBoxColliderVertex(BoxCollider boxCollider, Camera sceneCam, Vector2 mousePos)
		{
			// Compute bounding box's vertices in world space.
			Vector3[] vertices = new Vector3[]
			{
				boxCollider.transform.TransformPoint(boxCollider.center
					+ new Vector3(boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z) * 0.5f),
				boxCollider.transform.TransformPoint(boxCollider.center
					+ new Vector3(-boxCollider.size.x, -boxCollider.size.y, boxCollider.size.z) * 0.5f),
				boxCollider.transform.TransformPoint(boxCollider.center
					+ new Vector3(-boxCollider.size.x, -boxCollider.size.y, -boxCollider.size.z) * 0.5f),
				boxCollider.transform.TransformPoint(boxCollider.center
					+ new Vector3(boxCollider.size.x, -boxCollider.size.y, -boxCollider.size.z) * 0.5f),

				boxCollider.transform.TransformPoint(boxCollider.center
					+ new Vector3(boxCollider.size.x, boxCollider.size.y, boxCollider.size.z) * 0.5f),
				boxCollider.transform.TransformPoint(boxCollider.center
					+ new Vector3(-boxCollider.size.x, boxCollider.size.y, boxCollider.size.z) * 0.5f),
				boxCollider.transform.TransformPoint(boxCollider.center
					+ new Vector3(-boxCollider.size.x, boxCollider.size.y, -boxCollider.size.z) * 0.5f),
				boxCollider.transform.TransformPoint(boxCollider.center
					+ new Vector3(boxCollider.size.x, boxCollider.size.y, -boxCollider.size.z) * 0.5f),
			};

			// Find GUI space mouse position
			Ray mouseRayWorldSpace = sceneCam.ScreenPointToRay(mousePos);
			Vector2 mouseGUIPos = HandleUtility.WorldToGUIPoint(mouseRayWorldSpace.origin);

			float lastClosestDistance = -1;

			Vector3 resultWorldSpace = Vector3.zero;

			// Go through vertices and compare how far they are from mouse in GUI space.
			foreach (var vertex in vertices)
			{
				Vector2 vertexGUISpace = HandleUtility.WorldToGUIPoint(vertex);          // transform GUI-space

				float distance = Vector2.SqrMagnitude(mouseGUIPos - vertexGUISpace);

				if (lastClosestDistance == -1 || distance < lastClosestDistance)
				{
					resultWorldSpace = vertex;
					lastClosestDistance = distance;
				}
			}

			return resultWorldSpace;
		}
		#endregion
		//#endcolreg

		#region Vertex snapping math.
		/// <summary>Transforms Unity's Bounds variable into a Rect in GUI space. Is used to quickly cull far-away models
		/// in the scene when searching for the vertex snap vertices.</summary>
		/// <param name="bounds">The model's Bounds in world space.</param>
		/// <returns>GUI space Rect</returns>
		public static Rect WorldSpaceBoundsToGUISpaceRect(Bounds bounds)
		{
			//#colreg(red*0.3);
			Vector3 cen = bounds.center;
			Vector3 ext = bounds.extents;
			Vector2[] extentPoints = new Vector2[8]
			{
				HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
				HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
				HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
				HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),
				HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
				HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
				HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
				HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
			};
			Vector2 min = extentPoints[0];
			Vector2 max = extentPoints[0];
			foreach (Vector2 v in extentPoints)
			{
				min = Vector2.Min(min, v);
				max = Vector2.Max(max, v);
			}

			return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
			//#endcolreg
		}

		/// <summary>Searches the second vertex needed for Find2VerticesForVertexSnapping method amongst SELECTED GameObject PIVOTS.</summary>
		/// <param name="transform">The parent</param>
		/// <param name="lastClosestDistanceGUIspace">The distance to cursor from the closest vertex in GUI space so far</param>
		/// <param name="meshFilter">The reusable mesh filter variable</param>
		/// <param name="renderer">The reusable renderer variable</param>
		/// <param name="vertices">The reusable vertices array</param>
		/// <param name="closestVertexGUISpace">INPUT, the previously found closest vertex to the mouse cursor amongst NON-selected meshes</param>
		/// <param name="closestSelectedVertexWorldSpace">OUTPUT Is returned as the closest vertex to the mouse cursor amongst SELECTED
		/// models world space</param>
		public static void IterateChildGameObjects(Transform transform, ref GameObject ClosestGO,
			ref float lastClosestDistanceGUIspace, MeshFilter meshFilter,
			MeshRenderer renderer, Vector3[] vertices, ref Vector2 closestVertexGUISpace, ref Vector3 closestSelectedVertexWorldSpace,
			ref VertexSnappingResult methodResult)
		{
			//#colreg(red*0.3);
			if (transform != null && transform.gameObject != null && transform.gameObject.activeInHierarchy && transform.gameObject.activeSelf)
			{
				Vector3 worldPoint = transform.position;
				float distance = Vector2.SqrMagnitude(closestVertexGUISpace - HandleUtility.WorldToGUIPoint(worldPoint));

				if (lastClosestDistanceGUIspace < 0 || distance < lastClosestDistanceGUIspace)  // find the closest pivot GUI-space
				{
					lastClosestDistanceGUIspace = distance;
					ClosestGO = transform.gameObject;
					closestSelectedVertexWorldSpace = worldPoint;   // our second ultimate goal
					methodResult = VertexSnappingResult.BothVectorsFound;
				}

				int children = transform.childCount;
				for (int i = 0; i < children; ++i)
					IterateChildGameObjects(transform.GetChild(i), ref ClosestGO,
						ref lastClosestDistanceGUIspace, meshFilter, renderer, vertices,
						ref closestVertexGUISpace, ref closestSelectedVertexWorldSpace, ref methodResult);
			}
			//#endcolreg
		}

		/// <summary>Searches the second vertex needed for Find2VerticesForVertexSnapping method amongst SELECTED MESHES.</summary>
		/// <param name="transform">The parent</param>
		/// <param name="lastClosestDistanceGUIspace">The distance to cursor from the closest vertex in GUI space so far</param>
		/// <param name="meshRenderer">The reusable mesh renderer variable</param>
		/// <param name="meshFilter">The reusable mesh filter variable</param>
		/// <param name="skinnedMesh">The reusable skinned mesh renderer variable</param>
		/// <param name="vertices">The reusable vertices array</param>
		/// <param name="curTransform">The reusable transform variable that is used to translate vertices from local to world space</param>
		/// <param name="closestVertexGUISpace">INPUT, the previously found closest vertex to the mouse cursor amongst NON-selected meshes</param>
		/// <param name="closestSelectedVertexWorldSpace">OUTPUT Is returned as the closest vertex to the mouse cursor amongst SELECTED
		/// models world space</param>
		/// <param name="ignoreZeroesOnX">When searching for a vertex among SELECTED models, 
		/// ignore those that have zero values in x variable in local space</param>
		/// <param name="ignoreZeroesOnY">When searching for a vertex among SELECTED models, 
		/// ignore those that have zero values in y variable in local space</param>
		/// <param name="ignoreZeroesOnZ">When searching for a vertex among SELECTED models, 
		/// ignore those that have zero values in z variable in local space</param>
		/// <param name="pivot">Transformation pivot moved to local space which is subtracted before we check for zeroes</param>
		public static void IterateChildMeshes(Transform transform, ref GameObject ClosestGO,
			ref float lastClosestDistanceGUIspace, MeshRenderer meshRenderer,
			MeshFilter meshFilter, SkinnedMeshRenderer skinnedMesh, Vector3[] vertices, Transform curTransform,
			ref Vector2 closestVertexGUISpace, ref Vector3 closestSelectedVertexWorldSpace, ref VertexSnappingResult methodResult,
			bool ignoreZeroesOnX = false, bool ignoreZeroesOnY = false, bool ignoreZeroesOnZ = false, Vector3 pivot = default(Vector3))
		{
			//#colreg(red*0.3);
			if (transform != null && transform.gameObject != null && transform.gameObject.activeInHierarchy && transform.gameObject.activeSelf)
			{
				bool checkSuccesfull = false;

				Vector3 vertex = Vector3.zero;
				meshFilter = null;
				meshFilter = transform.gameObject.GetComponent<MeshFilter>();
				meshRenderer = transform.gameObject.GetComponent<MeshRenderer>();

				// We don't check for mouse Rect here, because it is intentional that the
				//	tool works even from across the globe for SELECTED meshes

				if (meshFilter != null && meshRenderer != null && meshRenderer.enabled && meshRenderer.isVisible
					&& meshFilter.sharedMesh != null)
				{
					vertices = null;
					vertices = meshFilter.sharedMesh.vertices;  // Get local-space vertices

					if (vertices != null && vertices.Length > 0)
					{
						curTransform = null;
						curTransform = meshFilter.transform;
						if (curTransform != null)
							checkSuccesfull = true;
					}
				}

				if (!checkSuccesfull)
				{
					skinnedMesh = transform.gameObject.GetComponent<SkinnedMeshRenderer>();

					if (skinnedMesh != null && skinnedMesh.enabled && skinnedMesh.isVisible
						&& skinnedMesh.sharedMesh != null)
					{
						vertices = null;
						vertices = skinnedMesh.sharedMesh.vertices;  // Get local-space vertices

						if (vertices != null && vertices.Length > 0)
						{
							curTransform = null;
							curTransform = skinnedMesh.transform;
							if (curTransform != null)
								checkSuccesfull = true;
						}
					}
				}

				if (checkSuccesfull)
				{
					if (vertices != null)
					{
						int n = vertices.Length;
						while (--n > -1)
						{
							vertex = vertices[n];

							// Zero values in vertices in local space can produce glitches during scaling
							//	when there is a transform lock set. This way we try to filter them out.
							//	Blender does this better somehow, this is my way of doing it.
							if ((!ignoreZeroesOnX || Mathf.Abs((vertex - pivot).x) > 0.000001f)
								&& (!ignoreZeroesOnY || Mathf.Abs((vertex - pivot).y) > 0.000001f)
								&& (!ignoreZeroesOnZ || Mathf.Abs((vertex - pivot).z) > 0.000001f))
							{
								Vector3 worldPoint = curTransform.TransformPoint(vertex);    // transform world-space
								float distance = Vector2.SqrMagnitude(closestVertexGUISpace - HandleUtility.WorldToGUIPoint(worldPoint));

								// find the closest vertex GUI-space
								if (lastClosestDistanceGUIspace < 0 || distance < lastClosestDistanceGUIspace)
								{
									lastClosestDistanceGUIspace = distance;
									ClosestGO = transform.gameObject;
									closestSelectedVertexWorldSpace = worldPoint;   // our second ultimate goal
									methodResult = VertexSnappingResult.BothVectorsFound;
								}
							}
						}
					}
				}

				int children = transform.childCount;
				for (int i = 0; i < children; ++i)
					IterateChildMeshes(transform.GetChild(i), ref ClosestGO,
						ref lastClosestDistanceGUIspace, meshRenderer, meshFilter, skinnedMesh,
						vertices, curTransform, ref closestVertexGUISpace, ref closestSelectedVertexWorldSpace, ref methodResult);
			}
			//#endcolreg
		}

		/// <summary>Encompas every possible outcome of the Find2VerticesForVertexSnapping method.</summary>
		public enum VertexSnappingResult
		{
			NoVectorsFound,
			FirstVectorFound,
			BothVectorsFound,
		}


		/// <summary>Finds 2 3d points in World space, where 1st is the closest vertex to the mouse cursor
		/// amongst NON-selected models, and the 2nd one is the closest vertex to the mouse cursor amongst
		/// SELECTED models.</summary>
		/// <param name="sceneCam">The camera of the editor window</param>
		/// <param name="mousePos">The screen space position of the mouse</param>
		/// <param name="closestVertexWorldSpace">Is returned as the closest vertex to the mouse cursor amongst NON-selected models</param>
		/// <param name="closestSelectedVertexWorldSpace">Is returned as the closest vertex to the mouse cursor amongst SELECTED models</param>
		/// <param name="returnOnly1stVertex">If set, ignores whether there is any selected models and only searches for 1 vertex 
		/// which is the closest vertex to the mouse cursor amongst ALL models</param>
		/// <param name="ignoreZeroesOnX">When searching for a vertex among SELECTED models, 
		/// ignore those that have zero values in x variable in local space</param>
		/// <param name="ignoreZeroesOnY">When searching for a vertex among SELECTED models, 
		/// ignore those that have zero values in y variable in local space</param>
		/// <param name="ignoreZeroesOnZ">When searching for a vertex among SELECTED models, 
		/// ignore those that have zero values in z variable in local space</param>
		/// <param name="pivot">Transformation pivot moved to local space which is subtracted before we check for zeroes</param>
		public static VertexSnappingResult Find2VerticesForVertexSnapping(Camera sceneCam, Vector2 mousePos,
			out Vector3 closestVertexWorldSpace, out Vector3 closestSelectedVertexWorldSpace, out GameObject ClosestGO,
			bool returnOnly1stVertex = false,
			bool ignoreZeroesOnX = false, bool ignoreZeroesOnY = false, bool ignoreZeroesOnZ = false, Vector3 pivot = default(Vector3))
		{
			//#colreg(red*0.3);
			ClosestGO = null;
			VertexSnappingResult methodResult = VertexSnappingResult.NoVectorsFound;

			closestVertexWorldSpace = Vector3.zero;
			closestSelectedVertexWorldSpace = Vector3.zero;

			Object[] allMeshRenderers = Object.FindObjectsOfType(typeof(MeshRenderer)) as Object[];
			Object[] allSkinnedMeshes = Object.FindObjectsOfType(typeof(SkinnedMeshRenderer)) as Object[];
			Object[] allMeshes = new Object[allMeshRenderers.Length + allSkinnedMeshes.Length];
			allMeshRenderers.CopyTo(allMeshes, 0);
			allSkinnedMeshes.CopyTo(allMeshes, allMeshRenderers.Length);

			// Find world space things
			Ray mouseRayWorldSpace = sceneCam.ScreenPointToRay(mousePos);
			Bounds mouseBoundsWorldSpace = new Bounds(mouseRayWorldSpace.origin, new Vector3(1f, 1f, 1f));

			// Find GUI space things
			Vector2 mouseGUIPos = HandleUtility.WorldToGUIPoint(mouseRayWorldSpace.origin);
			Rect mouseRect = WorldSpaceBoundsToGUISpaceRect(mouseBoundsWorldSpace);

			mouseRect.xMin -= sceneCam.pixelWidth * 0.01f;
			mouseRect.yMin -= sceneCam.pixelHeight * 0.01f;

			mouseRect.xMax += sceneCam.pixelWidth * 0.01f;
			mouseRect.yMax += sceneCam.pixelHeight * 0.01f;


			Vector3[] vertices = null;
			MeshFilter filter = null;
			Rect rect;
			MeshRenderer meshRenderer = null;
			SkinnedMeshRenderer skinnedRenderer = null;
			Transform currentTransform = null;

			Vector2 closestVertexGUISpace = Vector2.zero;
			float lastClosestDistanceGUIspace = 700;    // This is the radius of the search. Hardcoded, no need to make it modular.

			Vector3 vertex = Vector3.zero;



			// Find a vertex, among non-selected meshes, that is closest to the mouse.
			for (int i = 0; i < allMeshes.Length; i++)
			{
				meshRenderer = allMeshes[i] as MeshRenderer;

				bool checkSuccessful = false;

				if (meshRenderer != null && meshRenderer.gameObject != null
					&& meshRenderer.gameObject.transform != null && meshRenderer.enabled && meshRenderer.isVisible
					&& meshRenderer.gameObject.activeInHierarchy && meshRenderer.gameObject.activeSelf
					&& (!Selection.Contains(meshRenderer.gameObject) || returnOnly1stVertex))
				{
					filter = null;
					filter = meshRenderer.gameObject.GetComponent<MeshFilter>();
					if (filter != null && filter.sharedMesh != null)
					{
						rect = WorldSpaceBoundsToGUISpaceRect(meshRenderer.bounds);

						if (rect.Overlaps(mouseRect, true))
						{
							vertices = null;
							vertices = filter.sharedMesh.vertices;  // Get local-space vertices
							if (vertices != null && vertices.Length > 0)
							{
								currentTransform = null;
								currentTransform = filter.transform;
								if (currentTransform != null)
									checkSuccessful = true;
							}
						}
					}
				}

				if (!checkSuccessful)
				{
					skinnedRenderer = allMeshes[i] as SkinnedMeshRenderer;

					if (skinnedRenderer != null && skinnedRenderer.gameObject != null
						&& skinnedRenderer.gameObject.transform != null && skinnedRenderer.enabled && skinnedRenderer.isVisible
						&& skinnedRenderer.gameObject.activeInHierarchy && skinnedRenderer.gameObject.activeSelf
						&& (!Selection.Contains(skinnedRenderer.gameObject) || returnOnly1stVertex)
						&& skinnedRenderer.sharedMesh != null)
					{
						rect = WorldSpaceBoundsToGUISpaceRect(skinnedRenderer.bounds);

						if (rect.Overlaps(mouseRect, true))
						{
							vertices = null;
							vertices = skinnedRenderer.sharedMesh.vertices;  // Get local-space vertices
							if (vertices != null && vertices.Length > 0)
							{
								currentTransform = null;
								currentTransform = skinnedRenderer.transform;
								if (currentTransform != null)
									checkSuccessful = true;
							}
						}
						checkSuccessful = true;
					}
				}

				if (checkSuccessful)
				{
					if (vertices != null)
					{
						int n = vertices.Length;
						while (--n > -1)
						{
							vertex = vertices[n];
							Vector3 worldPoint = currentTransform.TransformPoint(vertex);    // transform world-space
							Vector2 vertexGUISpace = HandleUtility.WorldToGUIPoint(worldPoint);          // transform GUI-space

							float distance = Vector2.SqrMagnitude(mouseGUIPos - vertexGUISpace);

							// Check if it's close enough to the mouse in GUI space. All comparisons
							//	are in GUI space because that's how the user sees the working area.
							if (distance < lastClosestDistanceGUIspace)
							{
								lastClosestDistanceGUIspace = distance;
								closestVertexWorldSpace = worldPoint;       // our first ultimate goal.
								closestVertexGUISpace = vertexGUISpace;  // save for later
								methodResult = VertexSnappingResult.FirstVectorFound;
							}
						}
					}
				}
			}


			// ONLY if the first (NON-selected) vertex is found we search for the second (SELECTED) vertex
			if (!returnOnly1stVertex && Selection.gameObjects.Length > 0 &&
				// We have a vertex from a non-selected mesh near our mouse.
				lastClosestDistanceGUIspace < 700)
			{
				lastClosestDistanceGUIspace = -1;   // Reuse this variable
				for (int i = 0; i < Selection.gameObjects.Length; i++)
				{
					if (Selection.gameObjects[i] != null)
					{
						IterateChildMeshes(Selection.gameObjects[i].transform, ref ClosestGO, ref lastClosestDistanceGUIspace, meshRenderer,
							filter, skinnedRenderer, vertices, currentTransform, ref closestVertexGUISpace,
							ref closestSelectedVertexWorldSpace, ref methodResult, ignoreZeroesOnX, ignoreZeroesOnY, ignoreZeroesOnZ, pivot);
					}
				}

				if (lastClosestDistanceGUIspace < 0)    // We have no MeshFilter-s. Use GameObjects' pivots instead
				{
					lastClosestDistanceGUIspace = -1;   // Reuse this variable
					for (int i = 0; i < Selection.gameObjects.Length; i++)
					{
						if (Selection.gameObjects[i] != null)
						{
							IterateChildGameObjects(Selection.gameObjects[i].transform, ref ClosestGO, ref lastClosestDistanceGUIspace, filter,
								 meshRenderer, vertices, ref closestVertexGUISpace, ref closestSelectedVertexWorldSpace, ref methodResult);
						}
					}
				}

			}

			return methodResult;
			//#endcolreg
		}
		#endregion

		#region Rendering various UI elements
		public static void DrawPlane(Plane plane, Color faceColor, Color outlineColor, float distance = 50)
		{
			// We have to get the four vertices from the plane. Grab the normal.
			Vector3 planeNormal = plane.normal;
			Vector3 vecOnPlane;

			Vector3[] vecs = new Vector3[4];

			if (planeNormal != Vector3.forward)
				vecOnPlane = Vector3.Cross(planeNormal, Vector3.forward).normalized * distance;
			else
				vecOnPlane = Vector3.Cross(planeNormal, Vector3.up).normalized * distance;

			Quaternion rotation = Quaternion.AngleAxis(90, planeNormal);

			vecs[0] = planeNormal * plane.distance + vecOnPlane;

			vecOnPlane = rotation * vecOnPlane;
			vecs[1] = planeNormal * plane.distance + vecOnPlane;

			vecOnPlane = rotation * vecOnPlane;
			vecs[2] = planeNormal * plane.distance + vecOnPlane;

			vecOnPlane = rotation * vecOnPlane;
			vecs[3] = planeNormal * plane.distance + vecOnPlane;

			Handles.DrawSolidRectangleWithOutline(vecs, faceColor, outlineColor);
		}

		public static void DrawDisc(Color color, Vector3 camViewPlaneNormal, Vector3 position)
		{
			Handles.color = new Color(0, 0, 0, 1);
			Handles.DrawSolidDisc(position, camViewPlaneNormal, HandleUtility.GetHandleSize(position) * 0.065f);
			Handles.color = color;
			Handles.DrawSolidDisc(position, camViewPlaneNormal, HandleUtility.GetHandleSize(position) * 0.055f);
		}

		public static void DrawDottedLineToTheCenter(Vector3 avgPos)
		{
			Camera sceneCam = SceneView.lastActiveSceneView.camera;

			// Recompute the avgPos so that it's closer to the camera.
			Vector3 avgPosGUI = sceneCam.WorldToScreenPoint(avgPos);
			avgPosGUI.z = 0.5f;
			Vector3 avgPosWorld = sceneCam.ScreenToWorldPoint(avgPosGUI);

			// Raw mouse coordinates have y flipped from what unity uses.
			Vector2 mousePos = BlenderActions.Main.CurrentEvent.mousePosition;
			mousePos.y = sceneCam.pixelHeight - mousePos.y;

			Vector3 mousePos3DGUI = new Vector3(mousePos.x, mousePos.y, 0.5f);
			Vector3 mousePos3DWorld = sceneCam.ScreenToWorldPoint(mousePos3DGUI);


			// Render a dotted line
			float mag = Vector3.Distance(avgPosWorld, mousePos3DWorld);
			float size = HandleUtility.GetHandleSize(avgPosWorld) * 0.15f;
			int numberOfChunks = (int)Mathf.Floor(mag / size);

			if (numberOfChunks > 65)
				numberOfChunks = 65;

			if (numberOfChunks % 2 == 0)
				numberOfChunks += 1;

			float step = mag / numberOfChunks;

			Vector3 difference = (mousePos3DWorld - avgPosWorld).normalized * step;
			Vector3 curStartPos = avgPosWorld;
			Vector3 curEndPos = avgPosWorld + difference;
			while (numberOfChunks > -1)
			{
				Handles.color = new Color(1, 1, 1, 1);
				Handles.DrawAAPolyLine(4f, curStartPos + difference * 0.01f, curEndPos - difference * 0.01f);
				Handles.color = new Color(0, 0, 0, 1);
				Handles.DrawAAPolyLine(4.5f, curStartPos, curEndPos);
				Handles.DrawAAPolyLine(4.5f, curStartPos, curEndPos);
				Handles.DrawAAPolyLine(4.5f, curStartPos, curEndPos);
				Handles.DrawAAPolyLine(4.5f, curStartPos, curEndPos);

				// adding twice to create empty spaces
				curStartPos = curStartPos + difference * 2;

				if (numberOfChunks == 0)
					curEndPos = mousePos3DWorld;
				else
					curEndPos = curEndPos + difference * 2;

				numberOfChunks -= 2;
			}
		}

		public static void DrawLines(Vector3 originalAvgPivots, Transform[] selected,
			Lock TransformLock, Space TransformSpace, bool checkSpace = true)
		{
			const float lineLength = 5000;

			if (TransformLock != Lock.None)
			{
				bool drawX = (TransformLock == Lock.X_Axis || TransformLock == Lock.XY_Plane
					|| TransformLock == Lock.XZ_Plane) ? true : false;
				bool drawY = (TransformLock == Lock.Y_Axis || TransformLock == Lock.XY_Plane
					|| TransformLock == Lock.YZ_Plane) ? true : false;
				bool drawZ = (TransformLock == Lock.Z_Axis || TransformLock == Lock.XZ_Plane
					|| TransformLock == Lock.YZ_Plane) ? true : false;

				Vector3 p1 = Vector3.zero;
				Vector3 p2 = Vector3.zero;

				if (!checkSpace || TransformSpace == Space.Self)
				{
					foreach (Transform t in selected)
					{
						if (drawX)
						{

							p1 = t.position + t.TransformDirection(Vector3.right) * lineLength;
							p2 = t.position + t.TransformDirection(Vector3.left) * lineLength;
							Handles.color = new Color(1, 0, 0, 0.5f);
							Handles.DrawLine(p1, p2);
						}

						if (drawY)
						{
							p1 = t.position + t.TransformDirection(Vector3.up) * lineLength;
							p2 = t.position + t.TransformDirection(Vector3.down) * lineLength;
							Handles.color = new Color(0, 1, 0, 0.5f);
							Handles.DrawLine(p1, p2);
						}

						if (drawZ)
						{
							p1 = t.position + t.TransformDirection(Vector3.forward) * lineLength;
							p2 = t.position + t.TransformDirection(Vector3.back) * lineLength;
							Handles.color = new Color(0, 0, 1, 0.5f);
							Handles.DrawLine(p1, p2);
						}
					}
				}
				else
				{
					if (drawX)
					{
						p1 = originalAvgPivots;
						p2 = originalAvgPivots;
						p1.x += lineLength;
						p2.x -= lineLength;
						Handles.color = new Color(1, 0, 0, 0.5f);
						Handles.DrawLine(p1, p2);
					}

					if (drawY)
					{
						p1 = originalAvgPivots;
						p2 = originalAvgPivots;
						p1.y += lineLength;
						p2.y -= lineLength;
						Handles.color = new Color(0, 1, 0, 0.5f);
						Handles.DrawLine(p1, p2);
					}

					if (drawZ)
					{
						p1 = originalAvgPivots;
						p2 = originalAvgPivots;
						p1.z += lineLength;
						p2.z -= lineLength;
						Handles.color = new Color(0, 0, 1, 0.5f);
						Handles.DrawLine(p1, p2);
					}
				}
			}
		}
		#endregion
	}

	/// <summary>Specify how should the model be transformed during translation/rotation/scale</summary>
	public enum Lock
	{
		None,
		X_Axis,
		Y_Axis,
		Z_Axis,
		XY_Plane,
		XZ_Plane,
		YZ_Plane,
	}
}