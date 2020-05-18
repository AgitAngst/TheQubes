//#define U5
#define U5_6
//#define U2017

using UnityEngine;
using UnityEditor;

namespace BlenderActions
{
	public class ColliderContextMenu
	{

		[MenuItem("CONTEXT/BoxCollider/Start Blender Actions")]
		private static void StartBlenderActionsBox(MenuCommand menuCommand)
		{
			if (BlenderActions.Main != null)
			{
				BlenderActions.Main.EditedBoxCollider = menuCommand.context as BoxCollider;
				BlenderActions.Main.StartColliderEditMode(BlenderActions.Main.EditedBoxCollider);    // this clears all colliders, so calling previous line twice.
				BlenderActions.Main.EditedBoxCollider = menuCommand.context as BoxCollider;
			}
		}

		[MenuItem("CONTEXT/SphereCollider/Start Blender Actions")]
		private static void StartBlenderActionsSphere(MenuCommand menuCommand)
		{
			if (BlenderActions.Main != null)
			{
				BlenderActions.Main.EditedSphereCollider = menuCommand.context as SphereCollider;
				BlenderActions.Main.StartColliderEditMode(BlenderActions.Main.EditedSphereCollider);    // this clears all colliders, so calling previous line twice.
				BlenderActions.Main.EditedSphereCollider = menuCommand.context as SphereCollider;
			}
		}

		[MenuItem("CONTEXT/CapsuleCollider/Start Blender Actions")]
		private static void StartBlenderActionsCapsule(MenuCommand menuCommand)
		{
			if (BlenderActions.Main != null)
			{
				BlenderActions.Main.EditedCapsuleCollider = menuCommand.context as CapsuleCollider;
				BlenderActions.Main.StartColliderEditMode(BlenderActions.Main.EditedCapsuleCollider);    // this clears all colliders, so calling previous line twice.
				BlenderActions.Main.EditedCapsuleCollider = menuCommand.context as CapsuleCollider;
			}
		}

#if U2017
		[MenuItem("CONTEXT/BoxCollider2D/Start Blender Actions")]
		private static void StartBlenderActionsBox2D(MenuCommand menuCommand)
		{
			if (BlenderActions.Main != null)
			{
				BlenderActions.Main.EditedBoxCollider2D = menuCommand.context as BoxCollider2D;
				BlenderActions.Main.StartColliderEditMode(BlenderActions.Main.EditedBoxCollider2D);    // this clears all colliders, so calling previous line twice.
				BlenderActions.Main.EditedBoxCollider2D = menuCommand.context as BoxCollider2D;
			}
		}

		[MenuItem("CONTEXT/CapsuleCollider2D/Start Blender Actions")]
		private static void StartBlenderActionsCapsule2D(MenuCommand menuCommand)
		{
			if (BlenderActions.Main != null)
			{
				BlenderActions.Main.EditedCapsuleCollider2D = menuCommand.context as CapsuleCollider2D;
				BlenderActions.Main.StartColliderEditMode(BlenderActions.Main.EditedCapsuleCollider2D);    // this clears all colliders, so calling previous line twice.
				BlenderActions.Main.EditedCapsuleCollider2D = menuCommand.context as CapsuleCollider2D;
			}
		}

		[MenuItem("CONTEXT/CircleCollider2D/Start Blender Actions")]
		private static void StartBlenderActionsCircle2D(MenuCommand menuCommand)
		{
			if (BlenderActions.Main != null)
			{
				BlenderActions.Main.EditedCircleCollider2D = menuCommand.context as CircleCollider2D;
				BlenderActions.Main.StartColliderEditMode(BlenderActions.Main.EditedCircleCollider2D);    // this clears all colliders, so calling previous line twice.
				BlenderActions.Main.EditedCircleCollider2D = menuCommand.context as CircleCollider2D;
			}
		}

		[MenuItem("CONTEXT/PolygonCollider2D/Start Blender Actions")]
		private static void StartBlenderActionsPolygon2D(MenuCommand menuCommand)
		{
			if (BlenderActions.Main != null)
			{
				BlenderActions.Main.EditedPolygonCollider2D = menuCommand.context as PolygonCollider2D;
				BlenderActions.Main.StartColliderEditMode(BlenderActions.Main.EditedPolygonCollider2D);    // this clears all colliders, so calling previous line twice.
				BlenderActions.Main.EditedPolygonCollider2D = menuCommand.context as PolygonCollider2D;
			}
		}

		[MenuItem("CONTEXT/EdgeCollider2D/Start Blender Actions")]
		private static void StartBlenderActionsEdge2D(MenuCommand menuCommand)
		{
			if (BlenderActions.Main != null)
			{
				BlenderActions.Main.EditedEdgeCollider2D = menuCommand.context as EdgeCollider2D;
				BlenderActions.Main.StartColliderEditMode(BlenderActions.Main.EditedEdgeCollider2D);    // this clears all colliders, so calling previous line twice.
				BlenderActions.Main.EditedEdgeCollider2D = menuCommand.context as EdgeCollider2D;
			}
		}

		[MenuItem("CONTEXT/WheelCollider/Start Blender Actions")]
		private static void StartBlenderActionsWheel(MenuCommand menuCommand)
		{
			if (BlenderActions.Main != null)
			{
				BlenderActions.Main.EditedWheelCollider = menuCommand.context as WheelCollider;
				BlenderActions.Main.StartColliderEditMode(BlenderActions.Main.EditedWheelCollider);    // this clears all colliders, so calling previous line twice.
				BlenderActions.Main.EditedWheelCollider = menuCommand.context as WheelCollider;
			}
		}
#endif
	}
}