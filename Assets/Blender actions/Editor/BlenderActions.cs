//#define U5
#define U5_6
//#define U2017

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;

namespace BlenderActions
{
	/// <summary>Initializes every awailable action and their implementation methods. Holds main class references.</summary>
    [InitializeOnLoad]
    public class BlenderActions
    {
		/// <summary>The instance of this class that controlls every action in this asset. Can be null when the asset is turned OFF.</summary>
		public static BlenderActions Main;
		/// <summary>The list of all awailable actions.</summary>
		public Dictionary<Actions, Hotkey> AllActions;

		public const double VertexSnapTimeInterval = 0.2f;

		/// <summary>A leashe to prevent expensive vertex snapping code running too often.</summary>
		public double LastVertexSnapTime = 0;
		/// <summary>Ensures at least 10 empty frames between each vertex snap code call.</summary>
		public int AntiHang = 0;

		#region 3D Cursor      #color(gold);
		/// <summary>The World space position of the 3d cursor.</summary>
		public Vector3 The3DCursorPos;
		/// <summary>Whether the 3d cursor is used for rotation and scaling of objects.</summary>
		public bool Use3DCursor = false;
		/// <summary>Whether to reposition 3D cursor every time the user performs vertex snap during translation.</summary>
		public bool Position3DCursorToLastSnapPoint = true;
		#endregion

		#region Collider edit functionality #color(green);
		/// <summary>The link to the collider that is currently edited</summary>
		public Component ColliderBeingEdited;
		/// <summary>If we are editing a Box collider, this will not be null. Otherwise this will be null</summary>
		public BoxCollider EditedBoxCollider;
		/// <summary>If we are editing a Capsule collider, this will not be null. Otherwise this will be null</summary>
		public CapsuleCollider EditedCapsuleCollider;
		/// <summary>If we are editing a Sphere collider, this will not be null. Otherwise this will be null</summary>
		public SphereCollider EditedSphereCollider;
#if U2017
		/// <summary>If we are editing a Box collider 2D, this will not be null. Otherwise this will be null</summary>
		public BoxCollider2D EditedBoxCollider2D;
		/// <summary>If we are editing a Capsule collider 2D, this will not be null. Otherwise this will be null</summary>
		public CapsuleCollider2D EditedCapsuleCollider2D;
		/// <summary>If we are editing a Polygon collider 2D, this will not be null. Otherwise this will be null</summary>
		public PolygonCollider2D EditedPolygonCollider2D;
		/// <summary>If we are editing a Circle collider 2D, this will not be null. Otherwise this will be null</summary>
		public CircleCollider2D EditedCircleCollider2D;
		/// <summary>If we are editing a Edge collider 2D, this will not be null. Otherwise this will be null</summary>
		public EdgeCollider2D EditedEdgeCollider2D;
		/// <summary>If we are editing a Wheel collider, this will not be null. Otherwise this will be null</summary>
		public WheelCollider EditedWheelCollider;
#endif

		#endregion

		#region Hide default gizmo
		/// <summary>Memorize the tool the user used last, so that we can show it if they use the appropriate hotkey</summary>
		Tool LastUsedTool = Tool.Move;
		#endregion

		#region Transformation variables #color(cyan);
		/// <summary>Holds a link to one of the three transform classes if the user has started a transform 
		/// action and has not finished it yet. Otherwise - null.</summary>
		public BaseTransform ActiveTransformAction;
		/// <summary>Cached instance of the Translation class - is created during asset initialization.</summary>
		Translate Translation;
		/// <summary>Cached instance of the Rotation class - is created during asset initialization.</summary>
		Rotate Rotation;
		/// <summary>Cached instance of the Scale class - is created during asset initialization.</summary>
		Scale Scale;

		/// <summary>What limiting rules should be applied during the current transform action</summary>
		public Lock TransformLock = Lock.None;
		/// <summary>In what space should the current transform action be carried out. Only applicable for Translation and Rotation.</summary>
		public Space TransformSpace = Space.World;
		/// <summary>When true, slows down current transformation tool 10 times. Is true during the frame the
		/// user is holding down the appropriate hotkey</summary>
		public bool SlowDownTransformsThisFrame = false;
		/// <summary>When true, enables vertex snapping mode for the current transform action. Is true during the frame the
		/// user is holding down the appropriate hotkey</summary>
		public bool VertexSnapON_ThisFrame = false;
		/// <summary>When true, enables numeric snapping mode for the current transform action. Toggleable</summary>
		public bool NumericSnap = false;
		#endregion

		/// <summary>Is the right mouse button down?</summary>
		public bool RMB_Down = false;


		/// <summary>Static initialization. Done once every time the Unity project window is opened.</summary>
		static BlenderActions()
        {
			#region #Color(darkorange*0.5);
			SavableEditorPrefs.LoadGeneral();
			// Ensure the "Welcome" window is shown the first time user installs Blender Actions.
			if (!SavableEditorPrefs.DontShowConfigWindowAnyMore)
			{
				SavableEditorPrefs.DontShowConfigWindowAnyMore = true;
				ConfigWindow.OpenWindow();
			}

			if (SavableEditorPrefs.BlenderActionsON)
				TurnBlenderActionsON();
			#endregion
		}

		/// <summary>Removes custom events from Unity engine's event holders and destroys all created class instances.</summary>
		public static void TurnBlenderActionsON()
		{
			if (Main == null)
			{
				Main = new BlenderActions();
				Main.Initialize();
				SavableEditorPrefs.LoadHotkeys();
				SceneView.onSceneGUIDelegate += StaticOnSceneGUI;
				EditorApplication.update += OnEditorUpdateStatic;
				//EditorApplication.projectWindowItemOnGUI += ProjectOnGUI;
				EditorApplication.hierarchyWindowItemOnGUI += HierarchyOnGUI;
			}
		}

		/// <summary>Removes custom events from Unity engine's event holders and destroys all created class instances.</summary>
		public static void TurnBlenderActionsOFF()
		{
			if (Main != null)
			{
				Tools.current = Main.LastUsedTool;
				SceneView.onSceneGUIDelegate -= StaticOnSceneGUI;
				EditorApplication.update -= OnEditorUpdateStatic;
				//EditorApplication.projectWindowItemOnGUI -= ProjectOnGUI;
				EditorApplication.hierarchyWindowItemOnGUI -= HierarchyOnGUI;
				Main = null;
			}
		}

		/// <summary>General initialization</summary>
		public void Initialize()
		{//#colreg(darkorange*0.5);
			Main.The3DCursorPos = Vector3.zero;

			Main.Position3DCursorToLastSnapPoint = true;

			Main.ColliderBeingEdited = null;

			Main.Translation = new Translate(this);
			Main.Rotation = new Rotate(this);
			Main.Scale = new Scale(this);

			InitializeHotkeys();
		}//#endcolreg

		/// <summary>The hokey, holding the setting for the 'Cancel transform' action</summary>
		public Hotkey CancelTransformHotkey = null;

		/// <summary>Initialize Hotkeys and attach actions to them.</summary>
		public void InitializeHotkeys()
		{//#colreg(darkorange*0.5);
			AllActions = new Dictionary<Actions, Hotkey>();
			AllActions.Clear();

			#region Transform actions
			//#colreg(cyan);
			#region Start Translation
			Hotkey startTranslateAction = new Hotkey()
			{
				InternalName = Actions.StartTranslateAction,
				MainKeyCode = KeyCode.G,
				OnActivated = StartTranslateAction,
				ActionGroup = ActionType.TransformActivators,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Start Translation action",
				DisplayNameUA = "Включити інструмент переміщення",
				DisplayNameRU = "Включить инструмент перемещения",
				TooltipENG = "Starts translation action. " +
					"                                   " +
					" Keep in mind, this will automatically activate " +
					" Collider Editing Mode if only one GameObject is" +
					" selected it has no children and only one component which is a collider of the supported type.",
				TooltipUA = "Включає інструмент переміщення." +
					"                                   " +
					" Майте на увазі: автоматично активує режим редагування коллайдерів, якщо вибрано лише один" +
					" GameObject, у якого нема дочірніх GameObject-ів і який має лише один компонент: коллайдер, що" +
					" підтримується данною програмою.",
				TooltipRU = "Включает инструмент перемещения." +
					"                                   " +
					" Имейте в виду: автоматически активирует режим редактирования коллайдеров, если выбрано только" +
					" один GameObject, у которого нет наследников и есть только один компонент: коллайдер, поддерживаемый" +
					" данной программой.",
			};
			AllActions.Add(startTranslateAction.InternalName, startTranslateAction);
			#endregion
			#region Start Rotation
			Hotkey startRotateAction = new Hotkey()
			{
				InternalName = Actions.StartRotateAction,
				MainKeyCode = KeyCode.R,
				OnActivated = StartRotateAction,
				ActionGroup = ActionType.TransformActivators,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Start Rotation action",
				DisplayNameUA = "Включити інструмент обертання",
				DisplayNameRU = "Включить инструмент вращения",
				TooltipENG = "Starts translation action. " +
					"                                   " +
					" Keep in mind, this will automatically activates the Collider Editing Mode if only one GameObject is" +
					" selected it has no children and only one component which is a collider of the supported type.",
				TooltipUA = "Включає інструмент обертання." +
					"                                   " +
					" Майте на увазі: автоматично активує режим редагування коллайдерів, якщо вибрано лише один" +
					" GameObject, у якого нема дочірніх GameObject-ів і який має лише один компонент: коллайдер, що" +
					" підтримується данною програмою.",
				TooltipRU = "Включает инструмент вращения." +
					"                                   " +
					" Имейте в виду: автоматически активирует режим редактирования коллайдеров, если выбрано только" +
					" один GameObject, у которого нет наследников и есть только один компонент: коллайдер, поддерживаемый" +
					" данной программой.",
			};
			AllActions.Add(startRotateAction.InternalName, startRotateAction);
			#endregion
			#region Start Scaling
			Hotkey startScaleAction = new Hotkey()
			{
				InternalName = Actions.StartScaleAction,
				MainKeyCode = KeyCode.S,
				OnActivated = StartScaleAction,
				ActionGroup = ActionType.TransformActivators,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Start Scaling action",
				DisplayNameUA = "Включити інструмент масштабування",
				DisplayNameRU = "Включить инструмент скалирования",
				TooltipENG = "Starts scaling action. " +
					"                                   " +
					" Keep in mind, this will automatically activates the Collider Editing Mode if only one GameObject is" +
					" selected it has no children and only one component which is a collider of the supported type.",
				TooltipUA = "Включає інструмент масштабування." +
					"                                   " +
					" Майте на увазі: автоматично активує режим редагування коллайдерів, якщо вибрано лише один" +
					" GameObject, у якого нема дочірніх GameObject-ів і який має лише один компонент: коллайдер, що" +
					" підтримується данною програмою.",
				TooltipRU = "Включает инструмент скалирования." +
					"                                   " +
					" Имейте в виду: автоматически активирует режим редактирования коллайдеров, если выбрано только" +
					" один GameObject, у которого нет наследников и есть только один компонент: коллайдер, поддерживаемый" +
					" данной программой.",
			};
			AllActions.Add(startScaleAction.InternalName, startScaleAction);
			#endregion
			#region Transform Set X Axis
			Hotkey transformSetXAxis = new Hotkey()
			{
				InternalName = Actions.TransformSetXAxis,
				MainKeyCode = KeyCode.X,
				OnActivated = TransformSetXAxis,
				ActionGroup = ActionType.OnlyInTransformMode,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Lock movement to X axis during translation/rotation/scaling",
				DisplayNameUA = "Обмежити рух віссю 'X' під час трансформацій",
				DisplayNameRU = "Ограничить движение осью 'X' во время трансформаций",
				TooltipENG = "Lock any transformations to the world space X axis during translation/rotation/scaling," +
					" press this hotkey the second time to lock transformation to the local space X axis.",
				TooltipUA = "Обмежити трансформації віссю 'X' у просторі світу під час переміщення/обертання/масштабування," +
					" натисніть данну горячу клавішу ще раз, щоб обмежити трансформації віссю 'X' у локальному просторі.",
				TooltipRU = "Ограничить трансформации осью 'X' в мировом пространстве во время перемещения/вращения/скалирования," +
					" нажмите эту клавишу еще раз, чтобы ограничить трансформации осью 'X' в локальном пространстве.",
			};
			AllActions.Add(transformSetXAxis.InternalName, transformSetXAxis);
			#endregion
			#region Transform Set Y Axis
			Hotkey transformSetYAxis = new Hotkey()
			{
				InternalName = Actions.TransformSetYAxis,
				MainKeyCode = KeyCode.Y,
				OnActivated = TransformSetYAxis,
				ActionGroup = ActionType.OnlyInTransformMode,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Lock movement to Y axis during translation/rotation/scaling",
				DisplayNameUA = "Обмежити рух віссю 'Y' під час трансформацій",
				DisplayNameRU = "Ограничить движение осью 'Y' во время трансформаций",
				TooltipENG = "Lock any transformations to the world space Y axis during translation/rotation/scaling," +
					" press this hotkey the second time to lock transformation to the local space Y axis.",
				TooltipUA = "Обмежити трансформації віссю 'Y' у просторі світу під час переміщення/обертання/масштабування," +
					" натисніть данну горячу клавішу ще раз, щоб обмежити трансформації віссю 'Y' у локальному просторі.",
				TooltipRU = "Ограничить трансформации осью 'Y' в мировом пространстве во время перемещения/вращения/скалирования," +
					" нажмите эту клавишу еще раз, чтобы ограничить трансформации осью 'Y' в локальном пространстве.",
			};
			AllActions.Add(transformSetYAxis.InternalName, transformSetYAxis);
			#endregion
			#region Transform Set Z Axis
			Hotkey transformSetZAxis = new Hotkey()
			{
				InternalName = Actions.TransformSetZAxis,
				MainKeyCode = KeyCode.Z,
				OnActivated = TransformSetZAxis,
				ActionGroup = ActionType.OnlyInTransformMode,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Lock movement to Z axis during translation/rotation/scaling",
				DisplayNameUA = "Обмежити рух віссю 'Z' під час трансформацій",
				DisplayNameRU = "Ограничить движение осью 'Z' во время трансформаций",
				TooltipENG = "Lock any transformations to the world space Z axis during translation/rotation/scaling," +
					" press this hotkey the second time to lock transformation to the local space Z axis.",
				TooltipUA = "Обмежити трансформації віссю 'Z' у просторі світу під час переміщення/обертання/масштабування," +
					" натисніть данну горячу клавішу ще раз, щоб обмежити трансформації віссю 'Z' у локальному просторі.",
				TooltipRU = "Ограничить трансформации осью 'Z' в мировом пространстве во время перемещения/вращения/скалирования," +
					" нажмите эту клавишу еще раз, чтобы ограничить трансформации осью 'Z' в локальном пространстве.",
			};
			AllActions.Add(transformSetZAxis.InternalName, transformSetZAxis);
			#endregion
			#region Transform Set YZ Plane
			Hotkey transformSetYZPlane = new Hotkey()
			{
				InternalName = Actions.TransformSetYZPlane,
				MainKeyCode = KeyCode.X,
				Shift = true,
				OnActivated = TransformSetYZPlane,
				ActionGroup = ActionType.OnlyInTransformMode,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Lock movement to YZ plane during translation/rotation/scaling",
				DisplayNameUA = "Обмежити рух площиною 'YZ' під час трансформацій",
				DisplayNameRU = "Ограничить движение плоскостью 'YZ' во время трансформаций",
				TooltipENG = "Lock any transformations to the world space YZ plane during translation/rotation/scaling," +
					" press this hotkey the second time to lock transformation to the local space YZ plane.",
				TooltipUA = "Обмежити трансформації площиною 'YZ' у просторі світу під час переміщення/обертання/масштабування," +
					" натисніть данну горячу клавішу ще раз, щоб обмежити трансформації площиною 'YZ' у локальному просторі.",
				TooltipRU = "Ограничить трансформации плоскостью 'YZ' в мировом пространстве во время перемещения/вращения/скалирования," +
					" нажмите эту клавишу еще раз, чтобы ограничить трансформации плоскостью 'YZ' в локальном пространстве.",
			};
			AllActions.Add(transformSetYZPlane.InternalName, transformSetYZPlane);
			#endregion
			#region Transform Set XZ Plane
			Hotkey transformSetXZPlane = new Hotkey()
			{
				InternalName = Actions.TransformSetXZPlane,
				MainKeyCode = KeyCode.Y,
				Shift = true,
				OnActivated = TransformSetXZPlane,
				ActionGroup = ActionType.OnlyInTransformMode,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Lock movement to XZ plane during translation/rotation/scaling",
				DisplayNameUA = "Обмежити рух площиною 'XZ' під час трансформацій",
				DisplayNameRU = "Ограничить движение плоскостью 'XZ' во время трансформаций",
				TooltipENG = "Lock any transformations to the world space XZ plane during translation/rotation/scaling," +
					" press this hotkey the second time to lock transformation to the local space XZ plane.",
				TooltipUA = "Обмежити трансформації площиною 'XZ' у просторі світу під час переміщення/обертання/масштабування," +
					" натисніть данну горячу клавішу ще раз, щоб обмежити трансформації площиною 'XZ' у локальному просторі.",
				TooltipRU = "Ограничить трансформации плоскостью 'XZ' в мировом пространстве во время перемещения/вращения/скалирования," +
					" нажмите эту клавишу еще раз, чтобы ограничить трансформации плоскостью 'XZ' в локальном пространстве.",
			};
			AllActions.Add(transformSetXZPlane.InternalName, transformSetXZPlane);
			#endregion
			#region Transform Set XY Plane
			Hotkey transformSetXYPlane = new Hotkey()
			{
				InternalName = Actions.TransformSetXYPlane,
				MainKeyCode = KeyCode.Z,
				Shift = true,
				OnActivated = TransformSetXYPlane,
				ActionGroup = ActionType.OnlyInTransformMode,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Lock movement to XY plane during translation/rotation/scaling",
				DisplayNameUA = "Обмежити рух площиною 'XY' під час трансформацій",
				DisplayNameRU = "Ограничить движение плоскостью 'XY' во время трансформаций",
				TooltipENG = "Lock any transformations to the world space XY plane during translation/rotation/scaling," +
					" press this hotkey the second time to lock transformation to the local space XY plane.",
				TooltipUA = "Обмежити трансформації площиною 'XY' у просторі світу під час переміщення/обертання/масштабування," +
					" натисніть данну горячу клавішу ще раз, щоб обмежити трансформації площиною 'XY' у локальному просторі.",
				TooltipRU = "Ограничить трансформации плоскостью 'XY' в мировом пространстве во время перемещения/вращения/скалирования," +
					" нажмите эту клавишу еще раз, чтобы ограничить трансформации плоскостью 'XY' в локальном пространстве.",
			};
			AllActions.Add(transformSetXYPlane.InternalName, transformSetXYPlane);
			#endregion
			#region Apply Transformations
			Hotkey applyTransformations = new Hotkey()
			{
				InternalName = Actions.ApplyTransformations,
				MainMouseButton = 0,
				SecondaryKeyCode = KeyCode.Return,
				Any = true,
				CheckKeyPress = false,
				OnActivated = ApplyTransformations,
				ActionGroup = ActionType.OnlyInTransformMode,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Apply transformations",
				DisplayNameUA = "Зберегти результат переміщення/обертання/масштабування",
				DisplayNameRU = "Применить результат перемещения/вращения/скалирования",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(applyTransformations.InternalName, applyTransformations);
			#endregion
			#region Cancel Transformations
			CancelTransformHotkey = new Hotkey()
			{
				InternalName = Actions.CancelTransformations,
				MainMouseButton = 1,
				SecondaryKeyCode = KeyCode.Escape,
				Any = true,
				CheckKeyPress = false,
				OnActivated = CancelTransformations,
				ActionGroup = ActionType.OnlyInTransformMode,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Cancel transformations",
				DisplayNameUA = "Анулювати результат переміщення/обертання/масштабування",
				DisplayNameRU = "Отменить перемещение/вращение/скалирование",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(CancelTransformHotkey.InternalName, CancelTransformHotkey);
			#endregion
			#region Hold For Vertex Snap
			Hotkey holdForVertexSnap = new Hotkey()
			{
				InternalName = Actions.HoldForVertexSnap,
				MainKeyCode = KeyCode.Delete,
				OnActivated = HoldForVertexSnap,
				ActionGroup = ActionType.OnlyInTransformMode,
				HotkeyType = HotkeyType.TransformActivators,
				CheckKeyPress = false,
				DisplayNameENG = "Hold for vertex snap during translation/rotation/scaling",
				DisplayNameUA = "Затисніть для прив'язки до вершин під час трансформацій",
				DisplayNameRU = "Зажмите для привязки к вершинам во время трансформаций",
				TooltipENG = "Vertex snap allows you to specify a vertex of any selected model with a mouse cursor and then transform" +
					" that model in such a way that the specified vertex is on top of some other vertex on a non-selected" +
					" model in the scene, which was the closest to the mouse cursor at the moment. Works with translation, rotation and scale.",
				TooltipUA = "Прив'язка до вершин дозволяє вказати курсором миші вершину у вибраної моделі і трансформувати модель" +
					" таким чином, що ця вершина опиниться у той самій точці, що й наближча до курсору миші вершина іншої, не-вибраної" +
					" моделі у сцені. Працює як для переміщення так і для обертання і масштабування моделей.",
				TooltipRU = "Привязка к вершинам позволяет указать курсором мышки вершину у выбраной модели и трансформировать модель" +
					" таким образом, что эта вершина окажется в той же точке, что и ближайшая к курсору мишки вершина другой, не-выбранной" +
					" модели в сцене. Работает как с перемещением так и с вращением и скалированием моделей.",
			};
			AllActions.Add(holdForVertexSnap.InternalName, holdForVertexSnap);
			#endregion
			#region Toggle Numeric Snap
			Hotkey toggleNumericSnap = new Hotkey()
			{
				InternalName = Actions.ToggleNumericSnap,
				MainKeyCode = KeyCode.Delete,
				OnActivated = ToggleNumericSnap,
				ActionGroup = ActionType.OnlyInTransformMode,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Toggle snap-to-grid during translation/rotation/scaling",
				DisplayNameUA = "Переключити прив'язку-до-сітки під час трансформацій",
				DisplayNameRU = "Переключить привязку-к-сетке во время трансформаций",
				TooltipENG = "Numeric snap is the default Unity snap (snap to a grid). Allows you to transform model" +
					" in descrete intervals, instead of continuously.",
				TooltipUA = "Числовиа прив'язка (прив'язка до сітки) це стандартна прив'язка Unity. Дозволяє трансформувати" +
					" модель дискретними інтервалами замість неприривної трансформації.",
				TooltipRU = "Числовая привязка (привязка к сетке) это стандартная привязка Unity. Позволяет трансформировать" +
					" модель дискретными интервалами вместо непрерывной трансформации.",
			};
			//AllActions.Add(toggleNumericSnap.InternalName, toggleNumericSnap);
			#endregion
			#region Hold To Slow Down Transformations
			Hotkey holdToSlowDownTransformations = new Hotkey()
			{
				InternalName = Actions.HoldToSlowDownTransformations,
				MainKeyCode = KeyCode.Delete,
				OnActivated = HoldToSlowDownTransformations,
				ActionGroup = ActionType.OnlyInTransformMode,
				HotkeyType = HotkeyType.TransformActivators,
				CheckKeyPress = false,
				DisplayNameENG = "Hold to slow down movement during translation/rotation/scaling",
				DisplayNameUA = "Затисніть щоб сповільнити рухи під час трансформацій",
				DisplayNameRU = "Зажмите чтобы замедлить движения во время трансформаций",
				TooltipENG = "Use this function when you need presision in your transformations - this slows down transformations" +
					" 10 times and alows you to change objects with increased accuracy.",
				TooltipUA = "Використовуйте цей режим, коли Вам необхідна підвищена точність під час трансформації - режим сповільнює" +
					" швидкість трансформацій у 10 разів і дозволяє Вам змінювати об'єкти з максимальною точністю.",
				TooltipRU = "Используйте этот режим, когда Вам необходима повышеная точность во время трансформаций - режим замедляет" +
					" скорость трансформаций в 10 раз и позволяет Вам менять объекты с максимальной точностью.",
			};
			AllActions.Add(holdToSlowDownTransformations.InternalName, holdToSlowDownTransformations);
			#endregion
			#region Reset Translation
			Hotkey resetTranslation = new Hotkey()
			{
				InternalName = Actions.ResetTranslation,
				MainKeyCode = KeyCode.G,
				Alt = true,
				OnActivated = ResetTranslation,
				ActionGroup = ActionType.OverrideTransforms,   // Conflicts with start translation.
				HotkeyType = HotkeyType.TransformResets,
				DisplayNameENG = "Reset translation of selected objects to (0,0,0)",
				DisplayNameUA = "Скинути позицію усіх вибраних об'єктів до (0,0,0)",
				DisplayNameRU = "Сбросить положение всех выделеных объектов в (0,0,0)",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(resetTranslation.InternalName, resetTranslation);
			#endregion
			#region Reset Rotation
			Hotkey resetRotation = new Hotkey()
			{
				InternalName = Actions.ResetRotation,
				MainKeyCode = KeyCode.R,
				Alt = true,
				OnActivated = ResetRotation,
				ActionGroup = ActionType.OverrideTransforms,
				HotkeyType = HotkeyType.TransformResets,
				DisplayNameENG = "Reset rotation of selected objects to (0,0,0)",
				DisplayNameUA = "Скинути обертання усіх вибраних об'єктів до (0,0,0)",
				DisplayNameRU = "Сбросить вращение всех выделеных объектов в (0,0,0)",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(resetRotation.InternalName, resetRotation);
			#endregion
			#region Reset Scale
			Hotkey resetScale = new Hotkey()
			{
				InternalName = Actions.ResetScale,
				MainKeyCode = KeyCode.S,
				Alt = true,
				OnActivated = ResetScale,
				ActionGroup = ActionType.OverrideTransforms,
				HotkeyType = HotkeyType.TransformResets,
				DisplayNameENG = "Reset scale of selected objects to (1,1,1)",
				DisplayNameUA = "Скинути масштабування усіх вибраних об'єктів до (1,1,1)",
				DisplayNameRU = "Сбросить скалирование всех выделеных объектов до (1,1,1)",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(resetScale.InternalName, resetScale);
			#endregion
			#region Rotate Model 90 Degrees
			Hotkey rotate90Degrees = new Hotkey()
			{
				InternalName = Actions.Rotate90Degrees,
				MainKeyCode = KeyCode.R,
				Shift = true,
				OnActivated = Rotate90Degrees,
				ActionGroup = ActionType.OverrideTransforms,
				HotkeyType = HotkeyType.TransformActivators,
				DisplayNameENG = "Rotate selected 90 degrees around world's Y axis",
				DisplayNameUA = "Повернути модель на 90 градусів навколо вісі 'Y' у просторі світу",
				DisplayNameRU = "Повернуть модель на 90 градусов около оси 'Y' в мировом пространстве",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(rotate90Degrees.InternalName, rotate90Degrees);  //#endcolreg
			#endregion
			#endregion

			#region 3D cursor hotkeys
			// #colreg(orange*3);
			#region Set 3D Cursor Position
			Hotkey holdToSet3DCursorPosition = new Hotkey()
			{
				InternalName = Actions.HoldToSet3DCursorPosition,
				MainKeyCode = KeyCode.Delete,
				OnActivated = HoldToSet3DCursorPosition,
				CheckKeyPress = false,
				DisplayNameENG = "Hold to set 3D cursor position",
				DisplayNameUA = "Задати позицію 3D курсора",
				DisplayNameRU = "Задать позицию 3D курсора",
				TooltipENG = "Hold this button and hover a mouse over any vertex in the scene to position the" +
					" 3D cursor exactly where that vertex is.",
				TooltipUA = "Затисніть цю кнопку та наведіть курсор миші на будь-яку вершину у сцені, щоб" +
					" миттєво перемістити 3D курсор у цю точку.",
				TooltipRU = "Зажмите эту кнопку и наведите курсор мышки на любую вершину в сцене, чтобы" +
					" мгновенно переместить 3D курсор в эту точку.",
			};
			AllActions.Add(holdToSet3DCursorPosition.InternalName, holdToSet3DCursorPosition);
			#endregion
			#region Toggle 'Move 3D cursor to last translation vertex snap point'
			Hotkey toggleMove3DCursorToLastTranslationVertexSnapPoint = new Hotkey()
			{
				InternalName = Actions.ToggleMove3DCursorToLastTranslationVertexSnapPoint,
				MainKeyCode = KeyCode.Q,
				OnActivated = ToggleMove3DCursorToLastTranslationVertexSnapPoint,
				DisplayNameENG = "Toggle 'Move 3D cursor to last translation vertex snap point'",
				DisplayNameUA = "Встановлювати 3D курсор в останню вершину прив'язки.",
				DisplayNameRU = "Устанавливать 3D курсор в последнюю вершину привязки.'",
				TooltipENG = "This function repositions the 3D cursor into the last vertex snap position. Only reacts to" +
					" vertex snap actions performed during the Translation action." +
					" Quite often you need to rotate an object right after you've positioned it with a vertex snap" +
					" and you would most likely want to rotate said object around the vertex you've just snapped it to." +
					" That's when you would want to turn this function 'ON'.",
				TooltipUA = "Ця функція миттєво встановлює 3D курсор у точку останної прив'язки до вершини. Реагує на" +
					" прив'язки до вешин, зроблені лише під час переміщення об'єктів. Досить часто об'єкт необхідно повернути" +
					" одразу після його переміщення з прив'язкою до вершини і, скоріш за все, повертати цей обєект потрібно" +
					" буде як раз навколо вершини, до якої була виконана прив'язка - у таких випадках, ввімкніть цю функцію.",
				TooltipRU = "Эта функция мгновенно устанавливает 3D курсор в точку последней привязке к вершине. Реагирует на" +
					" привязки к вершинам, выполненные только во время перемещения объекта. Достаточно часто объект необходимо" +
					" повернуть сразу после перемещения и привязки к определенной вершине. При этом, повернуть данный объект" +
					" будет необходимо, скорее всего, вокруг той вершини, к которой он был привязан. В таких ситуациях -" +
					" активируйте данную функцию.",
			};
			AllActions.Add(toggleMove3DCursorToLastTranslationVertexSnapPoint.InternalName,
				toggleMove3DCursorToLastTranslationVertexSnapPoint);
			#endregion
			#region Reset 3D Cursor Position
			Hotkey reset3DCursorPosition = new Hotkey()
			{
				InternalName = Actions.Reset3DCursorPosition,
				MainKeyCode = KeyCode.C,
				Shift = true,
				OnActivated = Reset3DCursorPosition,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				DisplayNameENG = "Reset 3D Cursor Position to (0,0,0)",
				DisplayNameUA = "Встановити позицію 3D курсора у точку (0,0,0)",
				DisplayNameRU = "Установить положение 3D курсора в точку (0,0,0)",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(reset3DCursorPosition.InternalName, reset3DCursorPosition);
			#endregion
			#region Use 3D Cursor As Pivot For Rotation And Scale
			Hotkey use3DCursorAsPivotForRotationAndScale = new Hotkey()
			{
				InternalName = Actions.Use3DCursorAsPivotForRotationAndScale,
				MainKeyCode = KeyCode.BackQuote,
				OnActivated = Use3DCursorAsPivotForRotationAndScale,
				Alt = true,
				DisplayNameENG = "Use 3D cursor as pivot for rotation and scale",
				DisplayNameUA = "Використовувати 3D курсор як центр для обертання та масштабування",
				DisplayNameRU = "Использовать 3D курсор как центр для вращения и скалирования",
				TooltipENG = "When this function is turned on, the 3D cursor" +
					" would be used as a pivot for rotation and scale of the models.",
				TooltipUA = "Якщо ввімкнути цю функцію, 3D курсор буде використовуватись як центр для обертання" +
					" та масштабування.",
				TooltipRU = "Если включить эту функцию, 3D курсор будет использоваться как центр для" +
					" вращения и скалирования.",
			};
			AllActions.Add(use3DCursorAsPivotForRotationAndScale.InternalName, use3DCursorAsPivotForRotationAndScale);
			#endregion
			#region Use Model Pivot For Rotation And Scale
			Hotkey useModelPivotForRotationAndScale = new Hotkey()
			{
				InternalName = Actions.UseModelPivotForRotationAndScale,
				MainKeyCode = KeyCode.BackQuote,
				OnActivated = UseModelPivotForRotationAndScale,
				DisplayNameENG = "Use the local pivot of each model for rotation and scale",
				DisplayNameUA = "Використовувати локальний центр кожної моделі для обертання та масштаб.",
				DisplayNameRU = "Использовать локальный центр каждой модели для вращения и скалирования",
				TooltipENG = "When this function is turned ON, the dafault pivot point of each model is used as a" +
					" pivot for rotation and scaling of the models. In case of multiple models being selected, their" +
					" averaged pivot is used (the center of pivots).",
				TooltipUA = "Коли ця функція увімкнена, півот(центр) вибраної моделі буде викорисовуватись як центр для" +
					" обертання та масштабування моделі. У разі, якщо декілька моделей вибрано одночасно, центром є їх" +
					" усереднений півот.",
				TooltipRU = "Если эта функция включена, пивот(центр) выбранной модели будет использоваться как центр" +
					" для вращения и скалирования модели. В случае, когда несколько моделей выбрано одновременно, центром" +
					" будет их усредненный пивот.",
			};
			AllActions.Add(useModelPivotForRotationAndScale.InternalName, useModelPivotForRotationAndScale);
			#endregion
			//#endcolreg
			#endregion

			#region Other actions
			#region Hide Selected
			Hotkey hideSelected = new Hotkey()
			{
				InternalName = Actions.HideSelected,
				MainKeyCode = KeyCode.H,
				OnActivated = HideSelected,
				DisplayNameENG = "Hide selected objects",
				DisplayNameUA = "Сховати вибрані об'єкти",
				DisplayNameRU = "Спрятать выбранные объекты",
				TooltipENG = "If any of the selected GameObject-s or any of their children have a 'MeshRenderer', this" +
					" action would memorize their 'MeshRenderer.enabled' states and then set them to 'false'. WARNING!" +
					" DO NOT SAVE YOUR SCENE WITH HIDDEN OBJECTS! UNHIDE HIDDEN BEFORE SAVING!",
				TooltipUA = "Якщо будь-який з вибраних GameObject-ів або їх дочірніх GameObject-ів має компонент 'MeshRenderer'," +
					" ця операція запам'ятає значення їх 'MeshRenderer.enabled' а потім змінить це значення на 'false'." +
					" УВАГА! НЕ ЗАПИСУЙТЕ СЦЕНУ ІЗ СХОВАНИМИ ОБ'ЄКТАМИ! ВИКОНАЙТЕ ДІЮ 'ПОКАЗАТИ СХОВАНІ ОБ'ЄКТИ' ПЕРЕД" +
					" ЗАПИСОМ!",
				TooltipRU = "Если любой из выбраных GameObject-ов или их наследников имеет компонент 'MeshRenderer'," +
					" эта операция запомнит значение 'MeshRenderer.enabled' а потом поменяет эти значения на 'false'." +
					" ВНИМАНИЕ! НЕ СОХРАНЯЙТЕ СЦЕНУ СО СПРЯТАНЫМИ ОБЪЕКТАМИ! ВЫПОЛНИТЕ 'ПОКАЗАТЬ СПРЯТАННЫЕ ОБЪЕКТЫ' ПЕРЕД" +
					" СОХРАНЕНИЕМ!",
			};
			AllActions.Add(hideSelected.InternalName, hideSelected);
			#endregion
			#region Hide Unselected
			Hotkey hideUnselected = new Hotkey()
			{
				InternalName = Actions.HideUnselected,
				MainKeyCode = KeyCode.H,
				Shift = true,
				OnActivated = HideUnselected,
				DisplayNameENG = "Hide unselected objects",
				DisplayNameUA = "Сховати НЕ вибрані об'єкти",
				DisplayNameRU = "Спрятать НЕ выбранные объекты",
				TooltipENG = "If any of the NON-selected GameObject-s have a MeshRenderer, this" +
					" action would memorize their MeshRenderer.enabled states and then set them to 'false'.",
				TooltipUA = "Якщо будь-який з НЕ вибраних GameObject-ів має компонент 'MeshRenderer'," +
					" ця операція запам'ятає значення їх 'MeshRenderer.enabled' а потім змінить це значення на 'false'.",
				TooltipRU = "Если любой из НЕ выбраных GameObject-ов имеет компонент 'MeshRenderer'," +
					" эта операция запомнит значение 'MeshRenderer.enabled' а потом поменяет эти значения на 'false'.",
			};
			AllActions.Add(hideUnselected.InternalName, hideUnselected);
			#endregion
			#region Unhide Hidden
			Hotkey unhideHidden = new Hotkey()
			{
				InternalName = Actions.UnhideHidden,
				MainKeyCode = KeyCode.H,
				Control = true,
				OnActivated = UnhideHidden,
				DisplayNameENG = "Unhide hidden objects (smart*)",
				DisplayNameUA = "Показати сховані об'єкти (розумно*)",
				DisplayNameRU = "Показать спрятанные объекты (умно*)",
				TooltipENG = "Restores 'MeshRenderer.enabled' valuse, which were set to 'false' with the last call to 'Hide selected'" +
					" or 'Hide unselected'.",
				TooltipUA = " Повертає значення 'MeshRenderer.enabled' до того, яке було перед останнім запуском операції 'Сховати вибрані" +
					" об'єкти' або операції 'Сховати НЕ вибрані об'єкти'.",
				TooltipRU = "Возвращает значение переменной 'MeshRenderer.enabled' до того, которое было перед последним вызовом операции" +
					"'Спрятать выбранные объекты' или 'Спрятать НЕ выбранные объекты'.",
			};
			AllActions.Add(unhideHidden.InternalName, unhideHidden);
			#endregion
			#region Reset All Selected Prefabs
			Hotkey resetAllSelectedPrefabs = new Hotkey()
			{
				InternalName = Actions.ResetAllSelectedPrefabs,
				MainKeyCode = KeyCode.R,
				Control = true,
				Alt = true,
				OnActivated = ResetAllSelectedPrefabs,
				ActionGroup = ActionType.OverrideTransforms,
				DisplayNameENG = "Reset all selected prefabs",
				DisplayNameUA = "Скинути налаштування усих вибраних префабів",
				DisplayNameRU = "Сбросить настройки всех выбранных префабов",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(resetAllSelectedPrefabs.InternalName, resetAllSelectedPrefabs);
			#endregion
			#region Apply All Selected Prefabs
			Hotkey applyAllSelectedPrefabs = new Hotkey()
			{
				InternalName = Actions.ApplyAllSelectedPrefabs,
				MainKeyCode = KeyCode.A,
				Control = true,
				Alt = true,
				OnActivated = ApplyAllSelectedPrefabs,
				ActionGroup = ActionType.OverrideTransforms,
				DisplayNameENG = "Apply all selected prefabs",
				DisplayNameUA = "Запам'ятати налаштування усих вибраних префабів",
				DisplayNameRU = "Применить настройки всех выбранных префабов",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(applyAllSelectedPrefabs.InternalName, applyAllSelectedPrefabs);
			#endregion
			#region Duplicate Selection And Translate
			Hotkey duplicateSelectionAndTranslate = new Hotkey()
			{
				InternalName = Actions.DuplicateSelectionAndTranslate,
				MainKeyCode = KeyCode.D,
				Shift = true,
				OnActivated = DuplicateSelectionAndTranslate,
				ActionGroup = ActionType.OverrideTransforms,
				//DisplayNameENG = "Duplicate selection, reset prefabs and start Translation action",
				//DisplayNameUA = "Клонувати вибрані, скинути префаби та включити інструмент переміщення",
				//DisplayNameRU = "Клонировать выбранные, сбросить префабы и включить инструмент перемещ.",
				//TooltipENG = "This clones the selected objects and resets all cloned prefabs." +
				//	" This reset is done, because, for some reason, Unity changes the" +
				//	" scale valuse of the duplicated objects, breaking prefabs in the process. Non-prefabs are not reset." +
				//	" Afterwards this function instantly turns ON the Blender Translation action because repositioning of" +
				//	" the cloned objects is the most likely operation you would do after the duplication. Very useful to" +
				//	" rapidly create a large construction out of repeatable GameObjects (like asphalt road).",
				//TooltipUA = "Ця функція робить копію вибраних об'єктів та скидає налаштування у клонованих префабів." +
				//	" Скидання налаштувань префабів потрібно тому, що Unity" +
				//	" чомусь змінює масштаб моделі під час клонування і, цим самим, знищує прив'язку моделі до префабу." +
				//	" У не-префабів нічого не скидаєтся. Після цього, автоматично запускаєтся інструмент переміщення," +
				//	" оскільки переміщення, це найбільш вірогідна дія після клонування об'єкту. Дуже корисна функція для" +
				//	" побудови складної конструкції із одноманітних GameObject-ів (наприклад асфальтна дорога).",
				//TooltipRU = "Эта функция клоннирует выбранные объекты и сбрасывает настройки у всех склоннированых префабов." +
				//	" Сброс настроек необходим потому, что" +
				//	" Unity, по непонятной причине, меняет масштаб объектов при их клонировании, разрушая, при этом, привязку объекта" +
				//	" к префабу. У не-префабов ничего не сбрасывается. После этого автоматически запускается инструмент" +
				//	" перемещения, так как перемещение - наиболее вероятное действие после клоннирования объекта. Очень" +
				//	" полезная функция для построения сложной структуры из однообразных GameObject-ов (например асфальтная дорога).",
				DisplayNameENG = "Duplicate selection and start Translation action",
				DisplayNameUA = "Клонувати вибрані та включити інструмент переміщення",
				DisplayNameRU = "Клонировать выбранные и включить инструмент перемещ.",
				TooltipENG = "This clones the selected objects and instantly turns ON the Blender Translation action because repositioning of" +
					" the cloned objects is the most likely operation you would do after the duplication.",
				TooltipUA = "Ця функція робить копію вибраних об'єктів та автоматично запускає інструмент переміщення," +
					" оскільки переміщення - це найбільш вірогідна дія після клонування об'єкту. Дуже корисна функція для" +
					" побудови складної конструкції із одноманітних GameObject-ів (наприклад асфальтна дорога).",
				TooltipRU = "Эта функция клоннирует выбранные объекты и автоматически запускает инструмент" +
					" перемещения, так как перемещение - наиболее вероятное действие после клоннирования объекта. Очень" +
					" полезная функция для построения сложной структуры из однообразных GameObject-ов (например асфальтная дорога).",
			};
			AllActions.Add(duplicateSelectionAndTranslate.InternalName, duplicateSelectionAndTranslate);
			#endregion
			#region Toggle Perspective
			Hotkey togglePerspective = new Hotkey()
			{
				InternalName = Actions.TogglePerspective,
				MainKeyCode = KeyCode.Keypad5,
				OnActivated = TogglePerspective,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				HotkeyType = HotkeyType.CameraControls,
				DisplayNameENG = "Toggle editor camera perspective",
				DisplayNameUA = "Переключити перспективу камери редактора",
				DisplayNameRU = "Переключить перспективу камери редактора",
				TooltipENG = "Toggle editor camera from perspective to orthographic and back.",
				TooltipUA = "Переключити камеру редактора з перспективної до ортографічної і навпаки.",
				TooltipRU = "Переключить камеру редактора с перспективной до ортографической и назад.",
			};
			AllActions.Add(togglePerspective.InternalName, togglePerspective);
			#endregion
			#region Set Front Camera
			Hotkey setFrontCamera = new Hotkey()
			{
				InternalName = Actions.SetFrontCamera,
				MainKeyCode = KeyCode.Keypad1,
				OnActivated = SetFrontCamera,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				HotkeyType = HotkeyType.CameraControls,
				DisplayNameENG = "Set front view for editor camera",
				DisplayNameUA = "Повернути камеру редактора у позицію 'Вигляд спереду'",
				DisplayNameRU = "Повернуть камеру редактора в позицию 'Вид спереди'",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(setFrontCamera.InternalName, setFrontCamera);
			#endregion
			#region Set Back Camera
			Hotkey setBackCamera = new Hotkey()
			{
				InternalName = Actions.SetBackCamera,
				MainKeyCode = KeyCode.Keypad1,
				Control = true,
				OnActivated = SetBackCamera,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				HotkeyType = HotkeyType.CameraControls,
				DisplayNameENG = "Set back view for editor camera",
				DisplayNameUA = "Повернути камеру редактора у позицію 'Вигляд ззаду'",
				DisplayNameRU = "Повернуть камеру редактора в позицию 'Вид сзади'",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(setBackCamera.InternalName, setBackCamera);
			#endregion
			#region Set Left Camera
			Hotkey setLeftCamera = new Hotkey()
			{
				InternalName = Actions.SetLeftCamera,
				MainKeyCode = KeyCode.Keypad3,
				Control = true,
				OnActivated = SetLeftCamera,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				HotkeyType = HotkeyType.CameraControls,
				DisplayNameENG = "Set left view for editor camera",
				DisplayNameUA = "Повернути камеру редактора у позицію 'Вигляд з лівого боку'",
				DisplayNameRU = "Повернуть камеру редактора в позицию 'Вид слева'",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(setLeftCamera.InternalName, setLeftCamera);
			#endregion
			#region Set Right Camera
			Hotkey setRightCamera = new Hotkey()
			{
				InternalName = Actions.SetRightCamera,
				MainKeyCode = KeyCode.Keypad3,
				OnActivated = SetRightCamera,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				HotkeyType = HotkeyType.CameraControls,
				DisplayNameENG = "Set right view for editor camera",
				DisplayNameUA = "Повернути камеру редактора у позицію 'Вигляд з правого боку'",
				DisplayNameRU = "Повернуть камеру редактора в позицию 'Вид справа'",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(setRightCamera.InternalName, setRightCamera);
			#endregion
			#region Set Top Camera
			Hotkey setTopCamera = new Hotkey()
			{
				InternalName = Actions.SetTopCamera,
				MainKeyCode = KeyCode.Keypad7,
				OnActivated = SetTopCamera,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				HotkeyType = HotkeyType.CameraControls,
				DisplayNameENG = "Set top view for editor camera",
				DisplayNameUA = "Повернути камеру редактора у позицію 'Вигляд зверху'",
				DisplayNameRU = "Повернуть камеру редактора в позицию 'Вид сверху'",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(setTopCamera.InternalName, setTopCamera);
			#endregion
			#region Set Bottom Camera
			Hotkey setBottomCamera = new Hotkey()
			{
				InternalName = Actions.SetBottomCamera,
				MainKeyCode = KeyCode.Keypad7,
				Control = true,
				OnActivated = SetBottomCamera,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				HotkeyType = HotkeyType.CameraControls,
				DisplayNameENG = "Set bottom view for editor camera",
				DisplayNameUA = "Повернути камеру редактора у позицію 'Вигляд знизу'",
				DisplayNameRU = "Повернуть камеру редактора в позицию 'Вид снизу'",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(setBottomCamera.InternalName, setBottomCamera);
			#endregion
			#region Toggle Hide Gizmo
			Hotkey toggleHideGizmo = new Hotkey()
			{
				InternalName = Actions.ToggleHideGizmo,
				MainKeyCode = KeyCode.Space,
				OnActivated = ToggleHideGizmo,
				Control = true,
				DisplayNameENG = "Toggle show/hide Unity's default gizmo",
				DisplayNameUA = "Показати чи сховати дефолтне гізмо Unity",
				DisplayNameRU = "Показать или скрыть дефолтное гизмо Unity",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(toggleHideGizmo.InternalName, toggleHideGizmo);
			#endregion
			#region Deselect All
			Hotkey deselectAll = new Hotkey()
			{
				InternalName = Actions.DeselectAll,
				MainKeyCode = KeyCode.A,
				OnActivated = DeselectAll,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				DisplayNameENG = "Deselect all",
				DisplayNameUA = "Зняти виділення з усіх об'єктів",
				DisplayNameRU = "Снять выдиление со всех объектов",
				TooltipENG = "",
				TooltipUA = "",
				TooltipRU = "",
			};
			AllActions.Add(deselectAll.InternalName, deselectAll);
			#endregion
			#region Create empty GameObject, start rename
			Hotkey createEmptyGameObject = new Hotkey()
			{
				InternalName = Actions.CreateEmptyGameObject,
				MainKeyCode = KeyCode.A,
				Shift = true,
				OnActivated = CreateEmptyGameObject,
				DisplayNameENG = "Create an empty GameObject + rename",
				DisplayNameUA = "Створити пустий GameObject + перейменувати",
				DisplayNameRU = "Создать пустой GameObject + переименовать",
				TooltipENG = "Creates an empty GameObject as a child of the currently selected GameObject. If no GameObject is selected," +
					" creates an empty GameObject in the Scene root. Starts renaming interface afterwards.",
				TooltipUA = "Створює пустий GameObject як дочірній об'єкт вибраного в даний момент GameObject-у. Якщо не вибрано жодного" +
					" GameObject-у, створює пустий GameObject у корні сцени. Після цього, включає інтерфейс перейменування" +
					" новоствореного GameObject-у.",
				TooltipRU = "Создает пустой GameObject как дочерний объект для выделенного в даний момент GameObject-а. Если не" +
					" выбрано ни одного GameObject-а, создает пустой GameObject в корне сцены. После, запускает интерфейс переименования" +
					" для нового GameObject-а.",
			};
			AllActions.Add(createEmptyGameObject.InternalName, createEmptyGameObject);
			#endregion
			#region Parent selected to a new empty GameObject
			Hotkey parentSelectedToNewEmptyGameObject = new Hotkey()
			{
				InternalName = Actions.ParentSelectedToNewEmptyGameObject,
				MainKeyCode = KeyCode.Q,
				Shift = true,
				OnActivated = ParentSelectedToNewEmptyGameObject,
				DisplayNameENG = "Parent selected to a new empty GameObject + rename",
				DisplayNameUA = "Зробити вибрані об'єкти дочірніми до нового пустого GameObject + перейменувати",
				DisplayNameRU = "Сделать выбанные объекты дочерними в новом пустом GameObject-е + переименовать",
				TooltipENG = "If there are GameObject-s selected, creates a new empty GameObject in the current scope and parents" +
					" selected GameObject-s to the newly created one. Starts renaming interface afterwards.",
				TooltipUA = "Якщо є вибрані GameObject-и, створює новий пустий GameObject на поточному рівні ієрархії та робить" +
					" вибрані GameObject-и дочірніми до новоствореного. Після цього, включає інтерфейс перейменування" +
					" новоствореного GameObject-у.",
				TooltipRU = "Если есть выбранные GameObject-ы, создает новый пустой GameObject на текущем уровне иерархии и" +
					" делает выбранные GameObject-ы дочерними для только что созданного. После, запускает интерфейс переименования" +
					" для нового GameObject-а.",
			};
			AllActions.Add(parentSelectedToNewEmptyGameObject.InternalName, parentSelectedToNewEmptyGameObject);
			#endregion
			#region Toggle Enable/Disable object
			Hotkey enableDisableObject = new Hotkey()
			{
				InternalName = Actions.ToggleEnableDisableGO,
				MainKeyCode = KeyCode.E,
				Control = true,
				OnActivated = ToggleEnableDisableGOs,
				DisplayNameENG = "Toggle enable/disable selected GameObjects",
				DisplayNameUA = "Ввімкнути або вимкнути вибрані GameObject-и (перемикач)",
				DisplayNameRU = "Включить или выключить выбранные GameObject-ы (переключатель)",
				TooltipENG = "Simply toggles values of 'enabled' states of selected objects ON or OFF. If multiple objects are selected," +
					" the active object's current 'enabled' state will be used as a guide. For instance: 4 objects are selected, 2 are enabled," +
					" 2 are disabled. If the active object is enabled - all objects get disabled, if the active object is disabled - all" +
					" objects get enabled.",
				TooltipUA = "Просто переключає вибрані об'єекти між станами 'вимкнений' 'увімкнений'. Якщо декілька об'єктів вибрано одночасно," +
					" активний об'єкт використовуватиметься як головний. Наприклад: 4 об'єкти вибрано, 2 увімкнено, 2 вимкнено. Якщо активний" +
					" об'єкт вимкнено - усі об'єкти стануть увімкненими, якщо активний об'єкт увімкнено - усі об'єкти стануть вимкненими.",
				TooltipRU = "Просто переключает состояние выбранных объектов между 'включен/выключен'. Если несколько объекто выбранно" +
					" одновременно, активный выбранный объект будет направляющим. Например: 4 объекта выбрано, 2 включено, 2 выключено, если" +
					" активно выбранный объект включен, все объекты станут выключеными, если активно выбранный объект выключен - все объекты" +
					" станут включенными.",
			};
			AllActions.Add(enableDisableObject.InternalName, enableDisableObject);
			#endregion
			#region Perform redo (under construction - doe not work properly yet.)
			Hotkey performRedo = new Hotkey()
			{
				InternalName = Actions.PerformRedo,
				MainKeyCode = KeyCode.Z,
				Control = true,
				Shift = true,
				OnActivated = PerformRedo,
				DisplayNameENG = "Perform redo",
				DisplayNameUA = "Повторити дію",
				DisplayNameRU = "Повторить действие",
				TooltipENG = "Performs a 'redo' action. Is helpful if you want to have your own hotkey for this" +
					" action (Ctrl+Shift+Z is often used in other apps).",
				TooltipUA = "Виконує команду 'повторити дію'. Для випадків, коли Ви хочете мати власну гарячу клавішу для" +
					" данної команды (наприклад, багато програм використовує комбінацію Ctrl+Shift+Z).",
				TooltipRU = " Выполняет команду 'повторить действие'. Для случаев, когда Вы хотите иметь собственную гарячую клавишу" +
					" для данной команды (например, много програм использует комбинацию Ctrl+Shift+Z).",
			};
			//AllActions.Add(performRedo.InternalName, performRedo);
			#endregion

			#region Collider creators
			//#colreg(green);
			#region Create BoxCollider and start translation action
			Hotkey createBoxColliderGOStartTranslation = new Hotkey()
			{
				InternalName = Actions.CreateBoxColliderGO,
				MainKeyCode = KeyCode.B,
				Shift = true,
				OnActivated = CreateBoxColliderGOStartTranslation,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				HotkeyType = HotkeyType.ColliderCreators,
				DisplayNameENG = "Create BoxCollider and start translation",
				DisplayNameUA = "Створити BoxCollider та почати переміщення",
				DisplayNameRU = "Создать BoxCollider и начать перемещение",
				TooltipENG = "Creates a GameObject with a BoxCollider component attached and adds it as a child of the" +
					" curently selected GameObject. After that the Translation action is initiated.",
				TooltipUA = "Створює GameObject із компонентом BoxCollider та додає його до вибраного зараз GameObject." +
					" Після цього включається інструмент переміщення.",
				TooltipRU = "Создает GameObject с компонентом BoxCollider и добавляет его как наследника к выбранному в" +
					" данный момент GameObject-у. После этого включается инструмент перемещения.",
			};
			AllActions.Add(createBoxColliderGOStartTranslation.InternalName, createBoxColliderGOStartTranslation);
			#endregion
			#region Create SphereCollider and start translation action
			Hotkey createSphereColliderGOStartTranslation = new Hotkey()
			{
				InternalName = Actions.CreateSphereColliderGO,
				MainKeyCode = KeyCode.S,
				Shift = true,
				OnActivated = CreateSphereColliderGOStartTranslation,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				HotkeyType = HotkeyType.ColliderCreators,
				DisplayNameENG = "Create SphereCollider and start translation",
				DisplayNameUA = "Створити SphereCollider та почати переміщення",
				DisplayNameRU = "Создать SphereCollider и начать перемещение",
				TooltipENG = "Creates a GameObject with a SphereCollider component attached and adds it as a child of the" +
					" curently selected GameObject. After that the Translation action is initiated.",
				TooltipUA = "Створює GameObject із компонентом SphereCollider та додає його до вибраного зараз GameObject." +
					" Після цього включається інструмент переміщення.",
				TooltipRU = "Создает GameObject с компонентом SphereCollider и добавляет его как наследника к выбранному в" +
					" данный момент GameObject-у. После этого включается инструмент перемещения.",
			};
			AllActions.Add(createSphereColliderGOStartTranslation.InternalName, createSphereColliderGOStartTranslation);
			#endregion
			#region Create CapsuleCollider and start translation action
			Hotkey createCapsuleColliderGOStartTranslation = new Hotkey()
			{
				InternalName = Actions.CreateCapsuleColliderGO,
				MainKeyCode = KeyCode.C,
				Shift = true,
				OnActivated = CreateCapsuleColliderGOStartTranslation,
				ActionGroup = ActionType.IgnoreDuringTransforms,
				HotkeyType = HotkeyType.ColliderCreators,
				DisplayNameENG = "Create CapsuleCollider and start translation",
				DisplayNameUA = "Створити CapsuleCollider та почати переміщення",
				DisplayNameRU = "Создать CapsuleCollider и начать перемещение",
				TooltipENG = "Creates a GameObject with a CapsuleCollider component attached and adds it as a child of the" +
					" curently selected GameObject. After that the Translation action is initiated.",
				TooltipUA = "Створює GameObject із компонентом CapsuleCollider та додає його до вибраного зараз GameObject." +
					" Після цього включається інструмент переміщення.",
				TooltipRU = "Создает GameObject с компонентом CapsuleCollider и добавляет его как наследника к выбранному в" +
					" данный момент GameObject-у. После этого включается инструмент перемещения.",
			};
			AllActions.Add(createCapsuleColliderGOStartTranslation.InternalName, createCapsuleColliderGOStartTranslation);
			#endregion
			//#endcolreg

			#endregion
			#region Unhide all renderers
			Hotkey unhideAllRenderers = new Hotkey()
			{
				InternalName = Actions.UnhideAllRenderers,
				OnActivated = UnhideAllRenderers,
				DisplayNameENG = "Turn ON ALL MeshRenderer-s/SkinnedMeshRenderer-s",
				DisplayNameUA = "Включити УСІ компоненти MeshRenderer/SkinnedMeshRenderer",
				DisplayNameRU = "Включить ВСЕ компоненты MeshRenderer/SkinnedMeshRenderer",
				TooltipENG = "A fail-safe devise. If you ever end up in a situation, when you cannot unhide hidden objects," +
					" bind this key and activate this command.",
				TooltipUA = "Засіб боротьби з несправностями. Якщо Ви коли-небудь опинитесь у ситуації, де Ви не можете" +
					" ввімкнути сховані об'єкти за допомогою дії 'Показати сховані об'єкти' - назначте клавішу на цю дію" +
					" та натисніть її.",
				TooltipRU = "Средство устранения неполадок. Если Вы когда-либо окажитесь в ситуации, где Вы не можете" +
					" показать скрытые объекты с помощью действия 'Показать спрятанные объекты' - назначте клавишу на это" +
					" действие и нажмите ее.",
			};
			AllActions.Add(unhideAllRenderers.InternalName, unhideAllRenderers);
			#endregion
			#endregion
		}//#endcolreg


		public delegate void DelayedVoidAction();
		/// <summary>Helps to delay certain actions until OnSceneGUI is running.</summary>
		public DelayedVoidAction DelayedAction;

		/// <summary>An inferior solution to let through the delayed events for the hotkey check.</summary>
		public bool IgnoreEventTypeThisTime = false;

		static void StaticOnSceneGUI(SceneView sceneView)
		{
			if (Main == null)
			{
				SceneView.onSceneGUIDelegate -= StaticOnSceneGUI;
				EditorApplication.update -= OnEditorUpdateStatic;
				//EditorApplication.projectWindowItemOnGUI -= ProjectOnGUI;
				EditorApplication.hierarchyWindowItemOnGUI -= HierarchyOnGUI;
			}
			else
				Main.OnSceneGUI(sceneView);
		}

		/// <summary>The main logic behind hotkeys.</summary>
		public void CheckAndRunHotkeys()
		{
			CurrentEvent = Event.current;

			// Part of the code that ensures that held right mouse button is not
			// interfering with Blender Action's hotkeys.
			if (RMB_Down 
				&& (
					(CurrentEvent.type == EventType.MouseUp && CurrentEvent.button == 1)
					|| (CurrentEvent.type == EventType.MouseDown && CurrentEvent.button != 1)
				))
				RMB_Down = false;

			if (SceneView.lastActiveSceneView != null && SceneView.lastActiveSceneView.camera != null
				&& CurrentEvent != null)
			{

				Camera sceneCam = SceneView.lastActiveSceneView.camera;
				Vector2 mousePos = CurrentEvent.mousePosition;
				mousePos.y = sceneCam.pixelHeight - mousePos.y;

				// Run a check for hotkey down WITHOUT actually launching the underlying action
				//	To insure we can create a fail-safe system for cases when several of them clash
				//	(share the same hotkey).

				bool overrideTransforms = false;
				bool startTransformPressed = false;

				VertexSnapON_ThisFrame = false;             // resets each update to ensure "hold" mechanics
				SlowDownTransformsThisFrame = false;        // resets each update to ensure "hold" mechanics

				bool anyHotkeyActivated = false;

				foreach (var action in AllActions)
				{
					action.Value.IsKeyDown(CurrentEvent, RMB_Down);

					if (action.Value.HotkeyActivatedThisFrame)
					{
						if (action.Value.ActionGroup == ActionType.OverrideTransforms)
							overrideTransforms = true;
						if (action.Value.ActionGroup == ActionType.TransformActivators)
							startTransformPressed = true;

						anyHotkeyActivated = true;
					}
				}

				// Now check for clashes and, if none found, run the action.
				foreach (var action in AllActions)
				{
					if (action.Value.HotkeyActivatedThisFrame)
					{
						bool allowAction = true;

						if (overrideTransforms && action.Value.ActionGroup == ActionType.TransformActivators)
							allowAction = false;
						else if (startTransformPressed && action.Value.ActionGroup != ActionType.TransformActivators)
							allowAction = false;
						else if (action.Value.ActionGroup == ActionType.OnlyInTransformMode && ActiveTransformAction == null)
							allowAction = false;
						else if (action.Value.ActionGroup == ActionType.IgnoreDuringTransforms && ActiveTransformAction != null)
							allowAction = false;

						if (allowAction)
							action.Value.ActivateIfApplicable();
					}
				}

				if (!anyHotkeyActivated)
				{
					// Extra conditions: if control or shift are held during tranformation modes - turn on special effects.
					if (ActiveTransformAction != null && CurrentEvent != null)
					{
						if (SavableEditorPrefs.HoldShiftToSlowDownTransforms && CurrentEvent.control)
							HoldForVertexSnap();

						if (SavableEditorPrefs.HoldControlForVertexSnapDuringTransforms && CurrentEvent.shift)
							HoldToSlowDownTransformations();
					}
				}

				//Position 3D cursor with [Ctrl]+[Shift] when not transforming. #colreg(orange*3);
				if (SavableEditorPrefs.HoldControlAndShiftToPosition3DCursor && CurrentEvent != null
					&& CurrentEvent.control && CurrentEvent.shift && CurrentEvent.type != EventType.KeyDown
					&& CurrentEvent.type != EventType.MouseDown)
				{
					if (ActiveTransformAction == null)
					{
						// The code below serves two purposes: first - set a "leash" for the vertex snapping code
						//	to prevent it launching too often (it's extremely expensive) and second - prevent the
						//	vertex snapping code from hanging the editor by launching the vertex snapping code
						//	back-to-back every time the previous computation finishes.

						// "Leash" - run vertex snap code only so many times per second.
						if (EditorApplication.timeSinceStartup > LastVertexSnapTime)
						{
							if (AntiHang > 10)  // "Anti-hang" - ENSURE AT LEAST 10 empty frames between each vertex snap code calls.
							{
								LastVertexSnapTime = EditorApplication.timeSinceStartup + VertexSnapTimeInterval;
								AntiHang = 0;

								Vector3 closestVertexWorldSpace = Vector3.zero;
								Vector3 closestSelectedVertexWorldSpace = Vector3.zero;
								GameObject closestGO = null;

								if (Utils.Find2VerticesForVertexSnapping(sceneCam, mousePos,
									out closestVertexWorldSpace, out closestSelectedVertexWorldSpace, out closestGO, true)
									== Utils.VertexSnappingResult.FirstVectorFound)
									The3DCursorPos = closestVertexWorldSpace;
							}
							else
								AntiHang++;
						}
					}

					// Paint 3D Cursor.
					Utils.DrawDisc(SavableEditorPrefs.DiscLocalSpaceColor, sceneCam.transform.forward, The3DCursorPos);
				}
				//#endcolreg
			}
			// Part of the code that ensures that held right mouse button is not
			// interfering with Blender Action's hotkeys.

			if (CurrentEvent.type == EventType.MouseDown && CurrentEvent.button == 1)
				RMB_Down = true;
		}

		static void CommonOnGUI()
		{
			if (Main == null)
			{
				SceneView.onSceneGUIDelegate -= StaticOnSceneGUI;
				EditorApplication.update -= OnEditorUpdateStatic;
				//EditorApplication.projectWindowItemOnGUI -= ProjectOnGUI;
				EditorApplication.hierarchyWindowItemOnGUI -= HierarchyOnGUI;
			}
			else
				Main.CheckAndRunHotkeys();
		}


		//static void ProjectOnGUI(string guid, Rect selectionRect)
		//{
		//	CommonOnGUI();
		//}

		static void HierarchyOnGUI(int i, Rect r)
		{
			CommonOnGUI();
		}


		static void OnEditorUpdateStatic()
		{
			if (Main == null)
			{
				SceneView.onSceneGUIDelegate -= StaticOnSceneGUI;
				EditorApplication.update -= OnEditorUpdateStatic;
				//EditorApplication.projectWindowItemOnGUI -= ProjectOnGUI;
				EditorApplication.hierarchyWindowItemOnGUI -= HierarchyOnGUI;
			}
			else
				Main.OnEditorUpdate();
		}

		void OnEditorUpdate()
		{
			// Part of the code that ensures that held right mouse button is not
			// interfering with Blender Action's hotkeys.
			if (RMB_Down && EditorWindow.focusedWindow != SceneView.lastActiveSceneView)
					RMB_Down = false;
		}


		/// <summary>Holds a copy of current event</summary>
		public Event CurrentEvent;

		/// <summary>The "Update" function. WARNING: this is called way more often then MonoBehaviour.Update().</summary>
		void OnSceneGUI(SceneView sceneView)
		{
			#region #Color(darkblue);
			CurrentEvent = Event.current;

			Camera sceneCam = SceneView.lastActiveSceneView.camera;
			Vector2 mousePos = CurrentEvent.mousePosition;
			mousePos.y = sceneCam.pixelHeight - mousePos.y;

			if (DelayedAction != null)
				DelayedAction();

			CheckAndRunHotkeys();

			//#colreg(green);
			if (ColliderBeingEdited != null)    // Collider edit mode.
			{
				// If user selects more than 1 GameObject or a GameObject that is not the one that holds current ColliderBeingEdited -
				//		stop the collider editing mode.
				if (!Selection.Contains(ColliderBeingEdited.gameObject) || Selection.gameObjects.Length > 1
					// Stop it also if user pushes Esc or Space
					|| CurrentEvent.keyCode == KeyCode.Space || CurrentEvent.keyCode == KeyCode.Escape)
					ColliderBeingEdited = null;
			}


			// Paint green lines on borders to show the user that we are in the collider editing mode.
			if (ColliderBeingEdited != null)
			{
				Vector2 leftTopGUISpace = new Vector2(sceneCam.pixelRect.xMin + 2, sceneCam.pixelRect.yMin + 3);
				Vector2 rightBottomGUISpace = new Vector2(sceneCam.pixelWidth - 2, sceneCam.pixelHeight);
				Vector2 leftBottomGUISpace = new Vector2(sceneCam.pixelRect.xMin + 2, sceneCam.pixelHeight);
				Vector2 rightTopGUISpace = new Vector2(sceneCam.pixelWidth - 2, sceneCam.pixelRect.yMin + 3);
				Ray leftTopRay = HandleUtility.GUIPointToWorldRay(leftTopGUISpace);
				Vector3 leftTopWorldSpace = leftTopRay.origin + leftTopRay.direction;   // move slightly away from Near Plane
				Ray rightBottomRay = HandleUtility.GUIPointToWorldRay(rightBottomGUISpace);
				Vector3 rightBottomWorldSpace = rightBottomRay.origin + rightBottomRay.direction;
				Ray leftBottomRay = HandleUtility.GUIPointToWorldRay(leftBottomGUISpace);
				Vector3 leftBottomWorldSpace = leftBottomRay.origin + leftBottomRay.direction;
				Ray rightTopRay = HandleUtility.GUIPointToWorldRay(rightTopGUISpace);
				Vector3 rightTopWorldSpace = rightTopRay.origin + rightTopRay.direction;

				Handles.color = new Color(0, 1, 0, 0.5f);

				Handles.DrawAAPolyLine(7, leftTopWorldSpace, leftBottomWorldSpace);
				Handles.DrawAAPolyLine(7, leftBottomWorldSpace, rightBottomWorldSpace);
				Handles.DrawAAPolyLine(7, rightBottomWorldSpace, rightTopWorldSpace);
				Handles.DrawAAPolyLine(7, rightTopWorldSpace, leftTopWorldSpace);
			}
			//#endcolreg






			//#colreg(cyan);
			if (SavableEditorPrefs.TransformActionsEnabled && ActiveTransformAction != null)
			{
				if (EditorWindow.focusedWindow != sceneView)
					sceneView.Focus();

				HandleUtility.AddDefaultControl(0);

				// The code below serves two purposes: first - set a "leash" for the vertex snapping code
				//	to prevent it launching too often (it's extremely expensive) and second - prevent the
				//	vertex snapping code from hanging the editor by launching the vertex snapping code
				//	back-to-back every time the previous computation finishes.
				if (!NumericSnap && VertexSnapON_ThisFrame)
				{
					// "Leash" - run vertex snap code only so many times per second.
					if (EditorApplication.timeSinceStartup > LastVertexSnapTime)
					{
						if (AntiHang > 10)	// "Anti-hang" - ENSURE AT LEAST 10 empty frames between each vertex snap code calls.
						{
							AntiHang = 0;
							ActiveTransformAction.OnSceneGUI(sceneView);
						}
						else
							AntiHang++;
					}
				}
				else
				{
					AntiHang = 0;
					ActiveTransformAction.OnSceneGUI(sceneView);
				}

				HandleUtility.Repaint();
			}
			//#endcolreg






			if (SavableEditorPrefs.HideUnityGizmo)
				Tools.current = Tool.None;

			// Implementing Use middle mouse to rotate
			if (SavableEditorPrefs.UseMiddleMouseToRotate && CurrentEvent != null
				&& CurrentEvent.alt && CurrentEvent.button == 2)
			{
				CurrentEvent.button = 0;
				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
			#endregion
		}



























		#region Action methods
		//	#colreg(red*0.5);
		//	#colreg(orange*3);
		/// <summary>Moves 3D cursor to the vertex, nearest to the mouse cursor.</summary>
		void HoldToSet3DCursorPosition()
		{
			Camera sceneCam = SceneView.lastActiveSceneView.camera;
			Vector2 mousePos = CurrentEvent.mousePosition;
			mousePos.y = sceneCam.pixelHeight - mousePos.y;

			if (ActiveTransformAction == null)
			{
				Vector3 closestVertexWorldSpace = Vector3.zero;
				Vector3 closestSelectedVertexWorldSpace = Vector3.zero;
				GameObject closestGO;

				if (Utils.Find2VerticesForVertexSnapping(sceneCam, mousePos,
					out closestVertexWorldSpace, out closestSelectedVertexWorldSpace, out closestGO, true)
					== Utils.VertexSnappingResult.FirstVectorFound)
					The3DCursorPos = closestVertexWorldSpace;
			}

			Utils.DrawDisc(SavableEditorPrefs.DiscLocalSpaceColor, sceneCam.transform.forward, The3DCursorPos);
		}

		/// <summary>Toggles the ability to instantly move the 3D cursor to the last translation vertex snap point.</summary>
		void ToggleMove3DCursorToLastTranslationVertexSnapPoint()
		{
			Position3DCursorToLastSnapPoint = !Position3DCursorToLastSnapPoint;
			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}

		/// <summary>Set 3D cursor position to Vector3.zero.</summary>
		void Reset3DCursorPosition()
		{
			The3DCursorPos = Vector3.zero;
			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}

		/// <summary>Orders to use the 3D cursor as pivot for rotation and scale actions.</summary>
		void Use3DCursorAsPivotForRotationAndScale()
		{
			Use3DCursor = true;
			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}

		/// <summary>Orders to use the selected GameObjects local pivots as pivots for rotation and scale actions.</summary>
		void UseModelPivotForRotationAndScale()
		{
			Use3DCursor = false;
			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}
		//#endcolreg



		//	#colreg(cyan);
		#region Translation actions
		/// <summary>Resets the translations of the selected GameObject-s.</summary>
		void ResetTranslation()
		{
			if (SavableEditorPrefs.ResetTransformsEnabled)
			{
				foreach (GameObject go in Selection.gameObjects)
					Undo.RecordObject(go.transform, "Reset Position");

				foreach (GameObject go in Selection.gameObjects)
					go.transform.localPosition = Vector3.zero;

				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
		}


		/// <summary>Resets the rotations of the selected GameObject-s.</summary>
		void ResetRotation()
		{
			if (SavableEditorPrefs.ResetTransformsEnabled)
			{
				foreach (GameObject go in Selection.gameObjects)
					Undo.RecordObject(go.transform, "Reset Rotation");

				foreach (GameObject go in Selection.gameObjects)
					go.transform.localRotation = Quaternion.identity;

				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
		}


		/// <summary>Resets the scale values of the selected GameObject-s.</summary>
		void ResetScale()
		{
			if (SavableEditorPrefs.ResetTransformsEnabled)
			{
				foreach (GameObject go in Selection.gameObjects)
					Undo.RecordObject(go.transform, "Reset Scale");

				foreach (GameObject go in Selection.gameObjects)
					go.transform.localScale = Vector3.one;

				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
		}

		/// <summary>Rotates the selected GameObject-s 90 degrees around the Y axis in world space.</summary>
		void Rotate90Degrees()
		{
			Undo.RecordObjects(Selection.transforms, "Rotate 90 degrees");

			var selected = Selection.GetTransforms(SelectionMode.TopLevel);
			Vector3 pivot = Vector3.zero;

			if (Use3DCursor)                                //#color(orange*3);
				pivot = The3DCursorPos;                       //#color(orange*3);
			else
			{
				for (int i = 0; i < selected.Length; i++)
					pivot += selected[i].position;
				pivot /= selected.Length;
			}

			foreach (Transform t in selected)
				t.RotateAround(pivot, Vector3.up, 90);
			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}


		/// <summary>Reset the locks on transforms.</summary>
		public void ResetTransformLock()
		{
			TransformSpace = Space.World;
			TransformLock = Lock.None;
		}

		/// <summary>Check if we need to activate Collider Editing Mode: only one GO must be selected,
		/// it should not have children and it should only have one component: a supported collider.</summary>
		void CheckForColliderEditMode()
		{
			if (Selection.gameObjects.Length == 1)
			{
				GameObject selected = Selection.activeGameObject;
				if (selected.transform.childCount < 1 && selected.GetComponents<Component>().Length == 2)
				{
					BoxCollider box = selected.GetComponent<BoxCollider>();
					if (box != null)
					{
						StartColliderEditMode(box);
						EditedBoxCollider = box;
					}
					else
					{
						SphereCollider sphere = selected.GetComponent<SphereCollider>();
						if (sphere != null)
						{
							StartColliderEditMode(sphere);
							EditedSphereCollider = sphere;
						}
						else
						{
							CapsuleCollider capsule = selected.GetComponent<CapsuleCollider>();
							if (capsule != null)
							{
								StartColliderEditMode(capsule);
								EditedCapsuleCollider = capsule;
							}
#if U2017
							else
							{
								BoxCollider2D box2D = selected.GetComponent<BoxCollider2D>();
								if (box2D != null)
								{
									StartColliderEditMode(box2D);
									EditedBoxCollider2D = box2D;
								}
								else
								{
									CapsuleCollider2D capsule2D = selected.GetComponent<CapsuleCollider2D>();
									if (capsule2D != null)
									{
										StartColliderEditMode(capsule2D);
										EditedCapsuleCollider2D = capsule2D;
									}
									else
									{
										PolygonCollider2D poly2D = selected.GetComponent<PolygonCollider2D>();
										if (poly2D != null)
										{
											StartColliderEditMode(poly2D);
											EditedPolygonCollider2D = poly2D;
										}
										else
										{
											CircleCollider2D circle2D = selected.GetComponent<CircleCollider2D>();
											if (circle2D != null)
											{
												StartColliderEditMode(circle2D);
												EditedCircleCollider2D = circle2D;
											}
											else
											{
												EdgeCollider2D edge2D = selected.GetComponent<EdgeCollider2D>();
												if (edge2D != null)
												{
													StartColliderEditMode(edge2D);
													EditedEdgeCollider2D = edge2D;
												}
												else
												{
													WheelCollider wheel = selected.GetComponent<WheelCollider>();
													if (wheel != null)
													{
														StartColliderEditMode(wheel);
														EditedWheelCollider = wheel;
													}
													else
													{

													}
												}
											}
										}
									}
								}
							}
#endif
						}
					}
				}
			}
		}

		/// <summary>Start the translation action.</summary>
		void StartTranslateActionLogic()
		{
			if (SavableEditorPrefs.TransformActionsEnabled)
			{
				if (Selection.gameObjects.Length > 0)
				{
					if (ActiveTransformAction != null)
					{
						ActiveTransformAction.Cancel();
						ActiveTransformAction = null;
					}

					CheckForColliderEditMode();

					ActiveTransformAction = Translation;
					ActiveTransformAction.Start();

					if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
						CurrentEvent.Use();
				}
			}

			DelayedAction = null;
		}

		void StartTranslateAction()
		{
			DelayedAction = StartTranslateActionLogic;
		}

		void StartRotateActionLogic()
		{
			if (SavableEditorPrefs.TransformActionsEnabled)
			{
				if (Selection.gameObjects.Length > 0)
				{
					if (ActiveTransformAction != null)
					{
						ActiveTransformAction.Cancel();
						ActiveTransformAction = null;
					}

					CheckForColliderEditMode();

					ActiveTransformAction = Rotation;
					ActiveTransformAction.Start();

					if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
						CurrentEvent.Use();
				}
			}

			DelayedAction = null;
		}

		/// <summary>Start the rotation action.</summary>
		void StartRotateAction()
		{
			DelayedAction = StartRotateActionLogic;
		}


		/// <summary>Start the scaling action.</summary>
		void StartScaleActionLogic()
		{
			if (SavableEditorPrefs.TransformActionsEnabled)
			{
				if (Selection.gameObjects.Length > 0)
				{
					if (ActiveTransformAction != null)
					{
						ActiveTransformAction.Cancel();
						ActiveTransformAction = null;
					}

					CheckForColliderEditMode();

					ActiveTransformAction = Scale;
					ActiveTransformAction.Start();

					if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
						CurrentEvent.Use();
				}
			}

			DelayedAction = null;
		}
		void StartScaleAction()
		{
			DelayedAction = StartScaleActionLogic;
		}

		/// <summary></summary>
		/// <returns></returns>
		bool CheckAxisLockForColliders()
		{//#colreg(green);
			bool result = false;
			if (ColliderBeingEdited != null)
			{
				if (ActiveTransformAction == Scale)	// Special cases for scaling colliders
				{
					if (EditedCapsuleCollider != null)
					{
						result = true;
						if (TransformLock == Lock.None)
						{   // Only allow to lock onto ONE axis (the one the capsule is aligned to) and only in local space
							TransformSpace = Space.Self;
							if (EditedCapsuleCollider.direction == 0)
								TransformLock = Lock.X_Axis;
							else if (EditedCapsuleCollider.direction == 1)
								TransformLock = Lock.Y_Axis;
							else
								TransformLock = Lock.Z_Axis;
						}
						else
						{   // Also allow the standard no-lock uniform scale
							TransformSpace = Space.World;
							TransformLock = Lock.None;
						}

					}
					else if (EditedSphereCollider != null)
						result = true; // Does not allow any axis locks for sphere colliders
				}
			}
			return result;
		}//#endcolreg

		/// <summary>Limits current transform action to only be performed in X axis (second activation - local X axis).</summary>
		void TransformSetXAxis()
		{
			if (SavableEditorPrefs.TransformActionsEnabled)
			{
				if (!CheckAxisLockForColliders())
				{
					if (TransformLock == Lock.X_Axis)
					{
						if (TransformSpace == Space.World)
							TransformSpace = Space.Self;
						else
							TransformSpace = Space.World;
					}
					else
					{
						TransformSpace = Space.World;
						TransformLock = Lock.X_Axis;
					}
				}

				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
		}


		/// <summary>Limits current transform action to only be performed in Y axis (second activation - local Y axis).</summary>
		void TransformSetYAxis()
		{
			if (SavableEditorPrefs.TransformActionsEnabled)
			{
				if (!CheckAxisLockForColliders())
				{
					if (TransformLock == Lock.Y_Axis)
					{
						if (TransformSpace == Space.World)
							TransformSpace = Space.Self;
						else
							TransformSpace = Space.World;
					}
					else
					{
						TransformSpace = Space.World;
						TransformLock = Lock.Y_Axis;
					}
				}

				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
		}


		/// <summary>Limits current transform action to only be performed in Z axis (second activation - local Z axis).</summary>
		void TransformSetZAxis()
		{
			if (SavableEditorPrefs.TransformActionsEnabled)
			{
				if (!CheckAxisLockForColliders())
				{
					if (TransformLock == Lock.Z_Axis)
					{
						if (TransformSpace == Space.World)
							TransformSpace = Space.Self;
						else
							TransformSpace = Space.World;
					}
					else
					{
						TransformSpace = Space.World;
						TransformLock = Lock.Z_Axis;
					}
				}

				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
		}


		/// <summary>Limits current transform action to only be performed in XY plane (second activation - local XY plane).</summary>
		void TransformSetXYPlane()
		{
			if (SavableEditorPrefs.TransformActionsEnabled)
			{
				if (!CheckAxisLockForColliders())
				{
					if (TransformLock == Lock.XY_Plane)
					{
						if (TransformSpace == Space.World)
							TransformSpace = Space.Self;
						else
							TransformSpace = Space.World;
					}
					else
					{
						TransformSpace = Space.World;
						TransformLock = Lock.XY_Plane;
					}
				}

				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
		}


		/// <summary>Limits current transform action to only be performed in XZ plane (second activation - local XZ plane).</summary>
		void TransformSetXZPlane()
		{
			if (SavableEditorPrefs.TransformActionsEnabled)
			{
				if (!CheckAxisLockForColliders())
				{
					if (TransformLock == Lock.XZ_Plane)
					{
						if (TransformSpace == Space.World)
							TransformSpace = Space.Self;
						else
							TransformSpace = Space.World;
					}
					else
					{
						TransformSpace = Space.World;
						TransformLock = Lock.XZ_Plane;
					}
				}

				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
		}


		/// <summary>Limits current transform action to only be performed in YZ plane (second activation - local YZ plane).</summary>
		void TransformSetYZPlane()
		{
			if (SavableEditorPrefs.TransformActionsEnabled)
			{
				if (!CheckAxisLockForColliders())
				{
					if (TransformLock == Lock.YZ_Plane)
					{
						if (TransformSpace == Space.World)
							TransformSpace = Space.Self;
						else
							TransformSpace = Space.World;
					}
					else
					{
						TransformSpace = Space.World;
						TransformLock = Lock.YZ_Plane;
					}
				}

				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
		}

		public void TransformActionFinished()
		{
			ActiveTransformAction = null;
			AntiHang = 0;
		}


		/// <summary>Apply the results of a transform action.</summary>
		void ApplyTransformations()
		{
			if (SavableEditorPrefs.TransformActionsEnabled && ActiveTransformAction != null)
			{
				ActiveTransformAction.Confirm();

				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}	
		}

		/// <summary>Cancel the results of a transform action.</summary>
		void CancelTransformations()
		{
			if (SavableEditorPrefs.TransformActionsEnabled && ActiveTransformAction != null)
			{
				ActiveTransformAction.Cancel();

				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
		}


		/// <summary>Turns on vertex snap for translation/rotation/scale if any of the actions is active.</summary>
		void HoldForVertexSnap()
		{
			if (SavableEditorPrefs.TransformActionsEnabled && CurrentEvent.control)
				VertexSnapON_ThisFrame = true;
		}


		/// <summary>Toggles 'snap to a grid' parameter which is used during transform actions</summary>
		void ToggleNumericSnap()
		{
			if (SavableEditorPrefs.TransformActionsEnabled)
				NumericSnap = !NumericSnap;
		}


		/// <summary>Sets the transform slowdown parameter to 'true' for this frame.</summary>
		void HoldToSlowDownTransformations()
		{
			if (SavableEditorPrefs.TransformActionsEnabled)
				SlowDownTransformsThisFrame = true;
		}
		//#endcolreg
		#endregion

		#region Duplicate and/or Reset/Apply all selected prefabs.
		delegate void ChangePrefab(GameObject go);
		const int SelectionThresholdForProgressBar = 20;

		/// <summary>Checks if we have any prefabs among selected objects and, if true, applies the passed function on them</summary>
		/// <param name="changePrefabAction"></param>
		void SearchPrefabConnections(ChangePrefab changePrefabAction, bool checkRoot = false)
		{
			GameObject[] selectedTransforms = Selection.gameObjects;
			int numberOfTransforms = selectedTransforms.Length;
			bool showProgressBar = numberOfTransforms >= SelectionThresholdForProgressBar;
			int changedObjectsCount = 0;
			//Iterate through all the selected gameobjects
			try
			{
				for (int i = 0; i < numberOfTransforms; i++)
				{
					if (showProgressBar)
					{
						EditorUtility.DisplayProgressBar("Update prefabs",
							"Updating prefabs (" + i + "/" + numberOfTransforms + ")",
							(float)i / (float)numberOfTransforms);
					}

					var go = selectedTransforms[i];
					var prefabType = PrefabUtility.GetPrefabType(go);
					//Is the selected gameobject a prefab?
					if (prefabType == PrefabType.PrefabInstance || prefabType == PrefabType.DisconnectedPrefabInstance)
					{
						var prefabRoot = PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);
						if (prefabRoot != null && (!checkRoot || prefabRoot == go))	// Check if we are in the root of the prefab (good when duplicating)
						{
							changePrefabAction(prefabRoot);
							changedObjectsCount++;
						}
					}
				}
			}
			finally
			{
				if (showProgressBar)
					EditorUtility.ClearProgressBar();
			}
		}

		/// <summary>'Apply prefab' logic</summary>
		void ApplyToSelectedPrefabs(GameObject go)
		{
			var prefabAsset = PrefabUtility.GetPrefabParent(go);
			if (prefabAsset != null)
				PrefabUtility.ReplacePrefab(go, prefabAsset, ReplacePrefabOptions.ConnectToPrefab);
		}

		/// <summary>'Revert to prefab' logic</summary>
		void RevertSelectedToPrefabs(GameObject go)
		{
			PrefabUtility.ReconnectToLastPrefab(go);
			PrefabUtility.RevertPrefabInstance(go);
		}


		/// <summary>Mass 'Revert to prefab' button</summary>
		void ResetAllSelectedPrefabs()
		{
			SearchPrefabConnections(RevertSelectedToPrefabs);
			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}

		/// <summary>Mass 'Apply prefab' button</summary>
		void ApplyAllSelectedPrefabs()
		{
			SearchPrefabConnections(ApplyToSelectedPrefabs);
			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}

		/// <summary>Iterates over the children of the given transform and saves their GameOjbect-s in the given list.</summary>
		void IterateChildren(List<GameObject> listToAddTo, Transform currentTransform)
		{
			for (int i = 0; i < currentTransform.childCount; i++)
			{
				if (currentTransform.GetChild(i) != null && currentTransform.GetChild(i).gameObject != null)
				{
					listToAddTo.Add(currentTransform.GetChild(i).gameObject);
					IterateChildren(listToAddTo, currentTransform.GetChild(i));
				}
			}
		}

		/// <summary>Helper function that returns a list of all selected GameObject-s (including children).</summary>
		List<GameObject> GetAllSelectedObjects()
		{
			if (Selection.gameObjects.Length < 1 || Selection.gameObjects[0] == null)
				return null;
			else
			{
				List<GameObject> result = new List<GameObject>();
				result.Add(Selection.gameObjects[0]);
				IterateChildren(result, Selection.gameObjects[0].transform);
				return result;
			}
		}

		/// <summary>Duplicates selected GOs with Unity's duplication functionality 
		/// and launches the translation action for the newly cloned GOs</summary>
		void DuplicateSelectionAndTranslate()
		{
			if (Selection.activeGameObject != null)
			{
				if (EditorWindow.focusedWindow != null)
				{
					EditorWindow.focusedWindow.SendEvent(EditorGUIUtility.CommandEvent("Duplicate"));

					if (SavableEditorPrefs.TransformActionsEnabled)
						StartTranslateAction();

					if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
						CurrentEvent.Use();
				}
			}
		}
		#endregion

		#region Camera controls
		/// <summary>Toggle the perspective mode on the editor's camera.</summary>
		void TogglePerspective()
		{
			SceneView.lastActiveSceneView.in2DMode = false;
			SceneView.lastActiveSceneView.LookAt(SceneView.lastActiveSceneView.pivot,
												 SceneView.lastActiveSceneView.rotation,
												 SceneView.lastActiveSceneView.size,
												 !SceneView.lastActiveSceneView.orthographic);
		}

		void SetFrontCamera()
		{
			RotateCamera(Quaternion.Euler(0, 180, 0));
		}

		void SetBackCamera()
		{
			RotateCamera(Quaternion.Euler(0, 0, 0));
		}

		void SetRightCamera()
		{
			RotateCamera(Quaternion.Euler(0, 90, 0));
		}

		void SetLeftCamera()
		{
			RotateCamera(Quaternion.Euler(0, -90, 0));
		}

		void SetTopCamera()
		{
			RotateCamera(Quaternion.Euler(90, 180, 0));
		}

		void SetBottomCamera()
		{
			RotateCamera(Quaternion.Euler(-90, 180, 0));
		}

		void RotateCamera(Quaternion rot)
		{
			SceneView.lastActiveSceneView.in2DMode = false;

			if (Selection.activeTransform != null)
				SceneView.lastActiveSceneView.LookAt(Selection.activeTransform.position, rot);
			else
				SceneView.lastActiveSceneView.LookAt(SceneView.lastActiveSceneView.pivot, rot);

			SceneView.lastActiveSceneView.Repaint();
		}
		#endregion

		#region Hide selected GameObject-s
		/// <summary>A dictionary where every key is a GameObject with a SkinnedMeshRenderer and every value is
		/// "enabled" state of said SkinnedMeshRenderer BEFORE any of the "Hide..." action has taken place .</summary>
		Dictionary<SkinnedMeshRenderer, bool> SavedSkinnedMeshRenderers = new Dictionary<SkinnedMeshRenderer, bool>();
		/// <summary>A dictionary where every key is a GameObject with a MeshRenderer and every value is
		/// "enabled" state of said MeshRenderer BEFORE any of the "Hide..." action has taken place .</summary>
		Dictionary<MeshRenderer, bool> SavedMeshRenderers = new Dictionary<MeshRenderer, bool>();
		/// <summary>An array of all GameObjects (even the ones not seen in the scene).</summary>
		Object[] AllGOs = null;
		/// <summary>A list of all selected GameObject-s (including children).</summary>
		HashSet<GameObject> SelectedGOs = new HashSet<GameObject>();

		/// <summary>Has there been a call to "Unhide hidden" after the last "Hide.." call?
		/// This is needed to preserve sanity in situations, when user hides objects through hotkeys multiple times
		///	before finally unhiding them all</summary>
		bool UnhideCallRegistered = false;

		/// <summary>Saves all children of the given "transform" into the "list". Then launches itself on each child.</summary>
		void TraverseAndSaveChildren(Transform transform, HashSet<GameObject> list)
		{
			for (int i = 0; i < transform.childCount; i++)
			{
				Transform t = transform.GetChild(i);
				if (t != null && t.gameObject != null)
				{
					list.Add(t.gameObject);
					TraverseAndSaveChildren(t, list);
				}
			}
		}

		/// <summary>If any of the selected GameObject-s has a MeshRenderer, this sets its "enabled" state to "false".</summary>
		void HideSelected()
		{
			if (UnhideCallRegistered)
			{
				SavedSkinnedMeshRenderers.Clear();
				SavedMeshRenderers.Clear();
			}

			UnhideCallRegistered = false;

			// Build a list of all selected GOs, including children
			SelectedGOs.Clear();

			if (Selection.gameObjects != null && Selection.gameObjects.Length > 0)
			{
				foreach (var gameObject in Selection.gameObjects)
				{
					if (gameObject != null && gameObject.transform != null)
					{
						SelectedGOs.Add(gameObject);
						TraverseAndSaveChildren(gameObject.transform, SelectedGOs);
					}
				}
			}

			// Go through all selected objects and turn off their MeshRenderer-s.
			foreach (var gameObject in SelectedGOs)
			{
				if (gameObject != null)
				{
					MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
					if (meshRenderer != null)
					{
						Undo.RecordObject(meshRenderer, "Hide selected.");
						SavedMeshRenderers[meshRenderer] = meshRenderer.enabled;
						meshRenderer.enabled = false;
					}
					
					SkinnedMeshRenderer skinnedMesh = gameObject.GetComponent<SkinnedMeshRenderer>();
					if (skinnedMesh != null)
					{
						Undo.RecordObject(skinnedMesh, "Hide selected.");
						SavedSkinnedMeshRenderers[skinnedMesh] = skinnedMesh.enabled;
						skinnedMesh.enabled = false;
					}
				}
			}


			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}

		void HideUnselected()
		{
			if (UnhideCallRegistered)
			{
				SavedSkinnedMeshRenderers.Clear();
				SavedMeshRenderers.Clear();
			}

			UnhideCallRegistered = false;

			AllGOs = Resources.FindObjectsOfTypeAll(typeof(GameObject));

			// Build a list of all selected GOs, including children
			SelectedGOs.Clear();

			if (Selection.gameObjects != null && Selection.gameObjects.Length > 0)
			{
				foreach (var gameObject in Selection.gameObjects)
				{
					if (gameObject != null && gameObject.transform != null)
					{
						SelectedGOs.Add(gameObject);
						TraverseAndSaveChildren(gameObject.transform, SelectedGOs);
					}
				}
			}

			// Go through ALL gameObjects objects, check if they are in out scene (activeInHierarchy),
			//	check if they are not selected and if all checks pass, turn off their MeshRenderer-s.
			foreach (var obj in AllGOs)
			{
				var gameObject = obj as GameObject;
				if (gameObject != null && gameObject.activeInHierarchy && !SelectedGOs.Contains(gameObject))
				{
					MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
					if (meshRenderer != null)
					{
						Undo.RecordObject(meshRenderer, "Hide unselected.");
						
						// Try to preserve sanity in situations, when user hides objects through hotkeys multiple times
						//	before finally unhiding them all
						if (!SavedMeshRenderers.ContainsKey(meshRenderer))
							SavedMeshRenderers[meshRenderer] = meshRenderer.enabled;
						meshRenderer.enabled = false;
					}

					SkinnedMeshRenderer skinnedMesh = gameObject.GetComponent<SkinnedMeshRenderer>();
					if (skinnedMesh != null)
					{
						Undo.RecordObject(skinnedMesh, "Hide unselected.");

						// Try to preserve sanity in situations, when user hides objects through hotkeys multiple times
						//	before finally unhiding them
						if (!SavedSkinnedMeshRenderers.ContainsKey(skinnedMesh))
							SavedSkinnedMeshRenderers[skinnedMesh] = skinnedMesh.enabled;
						skinnedMesh.enabled = false;
					}
				}
			}

			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}



		/// <summary>Restore "renderer.enabled" variable of previously hidden objects
		/// to it's original state (preserves always hidden GameObjects).</summary>
		void UnhideHidden()
		{
			if (SavedMeshRenderers != null && SavedMeshRenderers.Count > 0)
			{
				UnhideCallRegistered = true;

				foreach (var pair in SavedMeshRenderers)
				{
					if (pair.Key != null)
					{
						MeshRenderer renderer = pair.Key;
						if (renderer != null)
						{
							Undo.RecordObject(renderer, "Unhide hidden.");
							renderer.enabled = pair.Value;
						}
					}
				}
			}

			if (SavedSkinnedMeshRenderers != null && SavedSkinnedMeshRenderers.Count > 0)
			{
				UnhideCallRegistered = true;

				foreach (var pair in SavedSkinnedMeshRenderers)
				{
					if (pair.Key != null)
					{
						SkinnedMeshRenderer skinnedMesh = pair.Key;
						if (skinnedMesh != null)
						{
							Undo.RecordObject(skinnedMesh, "Unhide hidden.");
							skinnedMesh.enabled = pair.Value;
						}
					}
				}
			}

			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}

		#endregion

		/// <summary>Hide default Unity Gizmo.</summary>
		void ToggleHideGizmo()
		{
			SavableEditorPrefs.HideUnityGizmo = !SavableEditorPrefs.HideUnityGizmo;
			SavableEditorPrefs.SaveHideUnityGizmoState();

			if (SavableEditorPrefs.HideUnityGizmo)
				LastUsedTool = Tools.current;
			else
				Tools.current = LastUsedTool;
			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}

		/// <summary>Clear user's selection</summary>
		void DeselectAll()
		{
			if (Selection.activeGameObject != null)
			{
				AllGOs = Resources.FindObjectsOfTypeAll(typeof(GameObject));
				Undo.RecordObjects(AllGOs, "Deselect all");
				Selection.activeGameObject = null;
				if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
					CurrentEvent.Use();
			}
		}

		#region Colliders
		//#colreg(green);
		/// <summary>Sets up necessary variables for the collider editing mode</summary>
		public void StartColliderEditMode(Component collider)
		{
				EditedBoxCollider = null;
				EditedCapsuleCollider = null;
				EditedSphereCollider = null;
#if U2017
				EditedBoxCollider2D = null;
				EditedCapsuleCollider2D = null;
				EditedPolygonCollider2D = null;
				EditedCircleCollider2D = null;
				EditedWheelCollider = null;
				EditedEdgeCollider2D = null;
#endif
				ColliderBeingEdited = collider;
				//SceneView.lastActiveSceneView.Focus();

				// Deselect all objects and only select the one with the collider.
				Selection.activeGameObject = null;
				Selection.activeGameObject = ColliderBeingEdited.gameObject;
		}

		/// <summary>Creates a GameObject and parents it to selection</summary>
		private GameObject CreateGOAndParentItToSelection(string actionName)
		{
			GameObject result = null;
			if (Selection.activeGameObject != null)
			{
				result = new GameObject();
				Undo.RegisterCreatedObjectUndo(result, actionName);
				Undo.SetTransformParent(result.transform, Selection.activeGameObject.transform, actionName);
				result.transform.localPosition = Vector3.zero;
				result.transform.localEulerAngles = Vector3.zero;
				result.transform.localScale = Vector3.one;
			}

			return result;
		}


		/// <summary>Creates a GO, then a BoxCollider component for that GO, parents the GO
		/// to active selected GO, sets up Collider Editing mode and starts translation action</summary>
		void CreateBoxColliderGOStartTranslation()
		{
			if (SavableEditorPrefs.CreateCollidersEnabled && Selection.activeGameObject != null)
			{
				Undo.IncrementCurrentGroup();

				GameObject colliderGO = CreateGOAndParentItToSelection("Create a BoxCollider");
				colliderGO.name = "BoxCollider";
				var collider = Undo.AddComponent<BoxCollider>(colliderGO);
				StartColliderEditMode(collider);
				EditedBoxCollider = collider;
				StartTranslateAction();
			}
		}

		/// <summary>Creates a GO, then a SphereCollider component for that GO, parents the GO
		/// to active selected GO, sets up Collider Editing mode and starts translation action</summary>
		void CreateSphereColliderGOStartTranslation()
		{
			if (SavableEditorPrefs.CreateCollidersEnabled && Selection.activeGameObject != null)
			{
				Undo.IncrementCurrentGroup();

				GameObject colliderGO = CreateGOAndParentItToSelection("Create a SphereCollider");
				colliderGO.name = "SphereCollider";
				var collider = Undo.AddComponent<SphereCollider>(colliderGO);
				StartColliderEditMode(collider);
				EditedSphereCollider = collider;
				StartTranslateAction();
			}
		}

		/// <summary>Creates a GO, then a CapsuleCollider component for that GO, parents the GO
		/// to active selected GO, sets up Collider Editing mode and starts translation action</summary>
		void CreateCapsuleColliderGOStartTranslation()
		{
			if (SavableEditorPrefs.CreateCollidersEnabled && Selection.activeGameObject != null)
			{
				GameObject colliderGO = CreateGOAndParentItToSelection("Create a CapsuleCollider");
				colliderGO.name = "CapsuleCollider";
				var collider = Undo.AddComponent<CapsuleCollider>(colliderGO);
				StartColliderEditMode(collider);
				EditedCapsuleCollider = collider;
				StartTranslateAction();
			}
		}
		//#endcolreg
		#endregion

		/// <summary>When to trigger renaming operation</summary>
		double TargetGameTimeToStartRenameOp = 0;
		/// <summary>Which GameObject to expand during the delayed action</summary>
		GameObject GOToExpand = null;

		void DelayedStartRenameOperation()
		{
			if (Selection.activeGameObject == null || (EditorApplication.timeSinceStartup - TargetGameTimeToStartRenameOp > 5))
				DelayedAction = null;
			else if (EditorApplication.timeSinceStartup > TargetGameTimeToStartRenameOp)
			{
				var type = typeof(EditorWindow).Assembly.GetType("UnityEditor.SceneHierarchyWindow");

				var hierarchyWindow = EditorWindow.GetWindow(type);

				// Expand hierarchy item first (if needed)
				if (GOToExpand != null)
				{
					MethodInfo methodInfo = type.GetMethod("SetExpandedRecursive");
					var methods = type.GetMethods();
					methodInfo.Invoke(hierarchyWindow, new object[] { GOToExpand.GetInstanceID(), true });
					GOToExpand = null;
				}

				// Then launch renaming interface
				var rename = type.GetMethod("RenameGO", BindingFlags.Instance | BindingFlags.NonPublic);
				rename.Invoke(hierarchyWindow, null);
				DelayedAction = null;
			}
		}

		/// <summary>Creates an empty GameObject and parents it to actively selected one. Starts renaming mode afterwards</summary>
		void CreateEmptyGameObject()
		{
			GameObject result = new GameObject();
			Undo.RegisterCreatedObjectUndo(result, "Create an empty GameObject");
			if (Selection.activeGameObject != null)
				Undo.SetTransformParent(result.transform, Selection.activeGameObject.transform, "Create an empty GameObject");
			result.transform.localPosition = Vector3.zero;
			result.transform.localEulerAngles = Vector3.zero;
			result.transform.localScale = Vector3.one;
			Selection.activeObject = null;
			Selection.activeObject = result;

			// Delay the renaming
			TargetGameTimeToStartRenameOp = EditorApplication.timeSinceStartup + 0.2f;
			DelayedAction = DelayedStartRenameOperation;
		}

		/// <summary>If there are GameObject-s selected, creates a new empty GameObject in the current scope and parents
		/// selected GameObject-s to the newly created one. Starts renaming mode afterwards</summary>
		void ParentSelectedToNewEmptyGameObject()
		{
			if (Selection.activeGameObject != null)
			{
				// Only the top level of selected GOs should get parented.
				var topLevelSelectedTransforms = Selection.GetTransforms(SelectionMode.TopLevel);
				if (topLevelSelectedTransforms != null)
				{
					int n = topLevelSelectedTransforms.Length;
					if (n > 0)
					{
						GameObject result = new GameObject();
						Undo.RegisterCreatedObjectUndo(result, "Parent selected to a new empty GameObject");

						// Ensure that the new GO is created in the same scope as the top level of selected GOs.
						if (topLevelSelectedTransforms[0] != null && topLevelSelectedTransforms[0].parent != null)
						{
							Undo.SetTransformParent(result.transform,
								topLevelSelectedTransforms[0].transform.parent, "Parent selected to a new empty GameObject");
							result.transform.localPosition = Vector3.zero;
							result.transform.localEulerAngles = Vector3.zero;
							result.transform.localScale = Vector3.one;
						}

						// Reparent GOs
						while (--n > -1)
							Undo.SetTransformParent(topLevelSelectedTransforms[n], result.transform, "Parent selected to a new empty GameObject");
						Selection.activeObject = null;
						Selection.activeObject = result;

						TargetGameTimeToStartRenameOp = EditorApplication.timeSinceStartup + 0.2f;
						GOToExpand = result;
						DelayedAction = DelayedStartRenameOperation;
					}
				}
			}
		}

		/// <summary>Simply toggles values of 'enabled' states of selected objects ON or OFF. If multiple objects are selected,
		/// the active object's current 'enabled' state will be used as a guide. For instance: 4 objects are selected, 2 are enabled,
		/// 2 are disabled. If the active object is enabled - all objects get disabled, if the active object is disabled - all
		/// objects get enabled.</summary>
		void ToggleEnableDisableGOs()
		{
			if (Selection.activeGameObject != null)
			{
				Undo.RecordObjects(Selection.gameObjects, "Toggle enable/disable GameObject");
				int n = Selection.gameObjects.Length;
				bool newEnabled = true;
				if (Selection.activeGameObject.activeSelf)
					newEnabled = false;
				while (--n > -1)
					Selection.gameObjects[n].SetActive(newEnabled);
			}
		}

		/// <summary>Performs a "Redo" command.</summary>
		void PerformRedo()
		{
			Undo.PerformRedo();
		}

		/// <summary>A fail-safe devise. If you saved your scene with a number of objects hidden, you can use this to 
		/// unhide all MeshRenderers.</summary>
		void UnhideAllRenderers()
		{
			foreach (var go in Selection.gameObjects)
			{
				MeshRenderer renderer = go.GetComponent<MeshRenderer>();
				if (renderer != null)
				{
					Undo.RecordObject(renderer, "Unhide All.");
					renderer.enabled = true;
				}

				SkinnedMeshRenderer skinnedRenderer = go.GetComponent<SkinnedMeshRenderer>();
				if (skinnedRenderer != null)
				{
					Undo.RecordObject(skinnedRenderer, "Unhide All.");
					skinnedRenderer.enabled = true;
				}
			}


			if (CurrentEvent.type != EventType.Layout && CurrentEvent.type != EventType.Layout)
				CurrentEvent.Use();
		}

		#endregion
	}


	public enum Actions
	{
		HoldToSet3DCursorPosition,
		ToggleMove3DCursorToLastTranslationVertexSnapPoint,
		Reset3DCursorPosition,
		ToggleHideGizmo,
		Use3DCursorAsPivotForRotationAndScale,
		UseModelPivotForRotationAndScale,
		Rotate90Degrees,
		ResetTranslation,
		ResetRotation,
		ResetScale,
		ResetAllSelectedPrefabs,
		ApplyAllSelectedPrefabs,
		DuplicateSelectionAndTranslate,
		StartTranslateAction,
		StartRotateAction,
		StartScaleAction,
		TogglePerspective,
		SetFrontCamera,
		SetBackCamera,
		SetRightCamera,
		SetLeftCamera,
		SetTopCamera,
		SetBottomCamera,
		TransformSetXAxis,
		TransformSetYAxis,
		TransformSetZAxis,
		TransformSetYZPlane,
		TransformSetXYPlane,
		TransformSetXZPlane,
		ApplyTransformations,
		CancelTransformations,
		HoldForVertexSnap,
		ToggleNumericSnap,
		HoldToSlowDownTransformations,
		HideUnselected,
		HideSelected,
		UnhideHidden,
		DeselectAll,
		CreateBoxColliderGO,
		CreateSphereColliderGO,
		CreateCapsuleColliderGO,
		ParentSelectedToNewEmptyGameObject,
		CreateEmptyGameObject,
		ToggleEnableDisableGO,
		PerformRedo,
		UnhideAllRenderers,
	}
	

	/// <summary>Helps prevent hotkey action clashes by introducing
	/// groups of actions that cannot be run simultaneously by one hotkey</summary>
	public enum ActionType
	{
		None,
		/// <summary>If clash, launches instead of TransformActivators.</summary>
		OverrideTransforms,
		TransformActivators,
		OnlyInTransformMode,
		/// <summary>If clash and there is an active transform action - ignore this action.</summary>
		IgnoreDuringTransforms
	}

	/// <summary>Helps display the user which groups of hotkeys have been deactivated with a menu checkbox</summary>
	public enum HotkeyType
	{
		None,
		TransformActivators,
		TransformResets,
		ColliderCreators,
		CameraControls,
	}


}


// %%% Highlight selected GameObjects that have no meshes.
// %%% CHECK IF NUMERIC SNAP WORKS!
// %%% Capsule colliders should change hight when u scale them, depending on their axis.
// %%% If we are scaling a capsule, any 'axis lock' hotkey should ONLY activate the axis the capsule is aligned to.

/// %%% Implement the tutorial mode!