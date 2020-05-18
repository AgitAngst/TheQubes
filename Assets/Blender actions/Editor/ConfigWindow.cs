//#define U5
#define U5_6
//#define U2017

using UnityEngine;
using UnityEditor;

namespace BlenderActions
{
	public class ConfigWindow : EditorWindow
	{
		/// <summary>Allows communication with the 'Enter hotkey' window.</summary>
		public Hotkey ActiveHotkey = null;
		public static Color BGColor = new Color(0.8f, 0.8f, 0.8f);
		public static Color BGDeactivatedColor = new Color(0.5f, 0.2f, 0.2f);
		public static Color SectionTitleColor = new Color(0.8f, 1f, 1f);
		public static Color ContrastRed = new Color(0.8f, 0.0f, 0.0f);
		public static Color ContrastGreen = new Color(0.0f, 0.8f, 0.0f);

		public static Color DimRed = new Color(0.7f, 0.5f, 0.5f);
		public static Color DimGreen = new Color(0.5f, 0.7f, 0.5f);

		public int FontSize = 14;
		int TitleFontSize = 24;

		[MenuItem("Window/Blender Actions")]
		public static void OpenWindow()
		{
			ConfigWindow Window = (ConfigWindow)GetWindow(typeof(ConfigWindow));
			GUIContent c = new GUIContent();

			string text = "";
			if (SavableEditorPrefs.Language == Language.ENG)
				text = "Blender Actions config";
			else if (SavableEditorPrefs.Language == Language.UA)
				text = "Blender Actions налаштування";
			else if (SavableEditorPrefs.Language == Language.RU)
				text = "Blender Actions настройки";
#if U5
			Window.title = text;
#endif
#if U2017
			c.text = text;
			Window.titleContent = c;
#endif
			Window.minSize = new Vector2(920, 900);
			//Window.maxSize = new Vector2(1920, 1200);
		}

		/// <summary>Last frame user's position in the ScrollView.</summary>
		Vector2 lastScrollViewPosition = Vector2.zero;
		void OnGUI()
		{
			if (Event.current != null && Event.current.type == EventType.KeyDown
				&& Event.current.keyCode == KeyCode.Escape)
				Close();

			string labelText = "";
			GUIStyle s = new GUIStyle(EditorStyles.textField);
			Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
			s.font = font;
			s.fontSize = FontSize;
			s.fontStyle = FontStyle.Bold;
			s.alignment = TextAnchor.MiddleLeft;
			GUIContent c = new GUIContent();
			if (ActiveHotkey != null)
			{
				if (SavableEditorPrefs.Language == Language.ENG)
					labelText = "Waiting for input... Close the 'Enter key' window to return to normal mode...";
				else if (SavableEditorPrefs.Language == Language.UA)
					labelText = "Чекаємо на клавішу... Закрийте вікно 'Натисніть клавішу' щоб повернутись до нормального режиму...";
				else if (SavableEditorPrefs.Language == Language.RU)
					labelText = "Ожидается нажатия клавиши... Закройте окно 'Нажмите клавишу', чтобы вернутся к нормальному режиму...";
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.Space();
				EditorGUILayout.LabelField(labelText, s, GUILayout.Width(900), GUILayout.Height(40));
			}
			else
			{
				string buttonText = "";
				Color labelColor = Color.red;
				Color buttonColor = Color.red;

				#region BlenderActions status label and button.
				if (SavableEditorPrefs.BlenderActionsON)
				{
					if (SavableEditorPrefs.Language == Language.ENG)
						labelText = "BlenderActions is ONLINE";
					else if (SavableEditorPrefs.Language == Language.UA)
						labelText = "BlenderActions ПРАЦЮЄ";
					else if (SavableEditorPrefs.Language == Language.RU)
						labelText = "BlenderActions РАБОТАЕТ";

					labelColor = ContrastGreen;
					buttonColor = ContrastRed;

					if (SavableEditorPrefs.Language == Language.ENG)
						buttonText = "TURN OFF";
					else if (SavableEditorPrefs.Language == Language.UA)
						buttonText = "ВИМКНУТИ";
					else if (SavableEditorPrefs.Language == Language.RU)
						buttonText = "ВЫКЛЮЧИТЬ";
				}
				else
				{
					if (SavableEditorPrefs.Language == Language.ENG)
						labelText = "BlenderActions is OFFLINE";
					else if (SavableEditorPrefs.Language == Language.UA)
						labelText = "BlenderActions ВИМКНЕНО";
					else if (SavableEditorPrefs.Language == Language.RU)
						labelText = "BlenderActions ВЫКЛЮЧЕН";

					labelColor = ContrastRed;
					buttonColor = ContrastGreen;

					if (SavableEditorPrefs.Language == Language.ENG)
						buttonText = "TURN ON";
					else if (SavableEditorPrefs.Language == Language.UA)
						buttonText = "ВВІМКНУТИ";
					else if (SavableEditorPrefs.Language == Language.RU)
						buttonText = "ВКЛЮЧИТЬ";
				}


				EditorGUILayout.BeginHorizontal();
				GUI.backgroundColor = labelColor;
					EditorGUILayout.LabelField(labelText, s, GUILayout.Height(40));
					GUI.backgroundColor = buttonColor;
					if (GUILayout.Button(buttonText, GUILayout.Width(100), GUILayout.Height(40)))
						ToggleBlenderActionsON_OFF();
				EditorGUILayout.EndHorizontal();
				#endregion

				s.fontSize = 12;
				GUI.backgroundColor = new Color(0, 1, 1);
				EditorGUILayout.LabelField("v. 1.0", s, GUILayout.Height(20));

				#region Change language buttons
				EditorGUILayout.BeginHorizontal();

					GUI.backgroundColor = BGColor;
					if (SavableEditorPrefs.Language == Language.ENG)
						GUI.backgroundColor = ContrastGreen;
					if (GUILayout.Button("ENGLISH", GUILayout.Width(100), GUILayout.Height(40)))
						SavableEditorPrefs.Language = Language.ENG;

					GUI.backgroundColor = BGColor;
					if (SavableEditorPrefs.Language == Language.UA)
						GUI.backgroundColor = ContrastGreen;
					if (GUILayout.Button("УКРАЇНСЬКА", GUILayout.Width(100), GUILayout.Height(40)))
						SavableEditorPrefs.Language = Language.UA;

					GUI.backgroundColor = BGColor;
					if (SavableEditorPrefs.Language == Language.RU)
						GUI.backgroundColor = ContrastGreen;
					if (GUILayout.Button("РУССКИЙ", GUILayout.Width(100), GUILayout.Height(40)))
						SavableEditorPrefs.Language = Language.RU;

				EditorGUILayout.EndHorizontal();
				#endregion

				// Add a label field for a cool separator.
				GUI.backgroundColor = BGColor;
				EditorGUILayout.LabelField("", s, GUILayout.Height(2));

				GUI.backgroundColor = BGColor;
				lastScrollViewPosition = EditorGUILayout.BeginScrollView(lastScrollViewPosition);


				GUI.backgroundColor = BGColor;
				EditorGUILayout.BeginVertical();

					#region Welcome section
					s.fontSize = FontSize;
				
					if (SavableEditorPrefs.Language == Language.ENG)
						buttonText = "SHOW TUTORIAL";
					else if (SavableEditorPrefs.Language == Language.UA)
						buttonText = "ПОКАЗАТИ ТЬЮТОРІАЛ";
					else if (SavableEditorPrefs.Language == Language.RU)
						buttonText = "ПОКАЗАТЬ ТЬЮТОРИАЛ";

					if (SavableEditorPrefs.ShowTutorialSectionInCfgWindow)
					{
						if (SavableEditorPrefs.Language == Language.ENG)
							c.text = "Welcome to Blender Actions for Unity!";
						else if (SavableEditorPrefs.Language == Language.UA)
							c.text = "Інструмент Blender Actions для Unity вітає Вас!";
						else if (SavableEditorPrefs.Language == Language.RU)
							c.text = "Плагин Blender Actions для Unity приветствует Вас!";

						EditorGUILayout.LabelField(c, s, GUILayout.Height(40));

						EditorGUILayout.Space();

						if (SavableEditorPrefs.Language == Language.ENG)
							c.text = "Blender Actions is designed to leave zero garbage in your project: it does not require any\n" +
							" components, never creates any GameObjects (unless you specifically order it to)\n" +
							" and can be turned on or off whenever you please. If you would ever want to\n" +
							" delete this plugin from your project, no trace of it would be left and Unity\n" +
							" would think that you were working without this plugin the whole time.";
						else if (SavableEditorPrefs.Language == Language.UA)
							c.text = "Плагін Blender Actions розроблено таким чином, щоб він не створював нічого зайвого у Вашому\n" +
							" проекті: він не потребує компонентів, ніколи не створює GameObject-ів у сцені\n" +
							" (окрім випадків, коли Ви самі дасте йому команду) і може буди включений чи\n" +
							" вимкнений у будь-який момент. Якщо Ви коли небудь захочете повністю видалити\n" +
							" цей плагін з Вашого компьютера, Unity буде вважати, що весь цей час Ви\n" +
							" просто працювали без плагіну.";
						else if (SavableEditorPrefs.Language == Language.RU)
							c.text = "Плагин Blender Actions устроен таким образом, чтобы он не создавал ничего лишнего в Вашем\n" +
							" проекте: он не требует компонентов, никогда не создает GameObject-ы в сцене\n" +
							" (кроме случаев, когда Вы сами дадите ему комманду) и может быть включен либо\n" +
							" выключен в любой момент. Если Вы когда-нибудь захотите полностью удалить\n" +
							" плагин из Вашего проекта, Unity будет думать, что все это время Вы работали\n" +
							" без данного плагина.";

						c.text = c.text.Replace("\n", System.Environment.NewLine);

						EditorGUILayout.LabelField(c, s, GUILayout.Height(140));

						if (SavableEditorPrefs.Language == Language.ENG)
							c.text = "Please turn on the plugin using this menu to start working. The full pdf tutorial is included in\n" +
							" the root folder of the plugin ('Blender Actions/') but you can always find all you need by reading the\n" +
							" hotkey tooltips in this window (start the plugin to see them). The only non-documented thing is turning\n" +
							" ON collider editing mode for any collider - just right click on the collider's component in the inspector\n" +
							" window and choose 'Start Blender Actions'.";
						else if (SavableEditorPrefs.Language == Language.UA)
							c.text = "Будь ласка включіть інструмент за допомогою цього вікна для початку роботи. Повний\n" +
							" pdf тьюторіал знаходиться у корні основної папки плагіну ('Blender Actions/') але Ви може дізнатись\n" +
							" все необхідне просто читаючи підказки для гарячих клавіш у цьому вікні (запустіть плагін щоб\n" +
							" побачити їх). Єдина незадокументована дія - це ввімкненя режиму редагування коллайдерів для будь-\n" +
							" якого коллайдеру: просто натисніть правою кнопкою на компоненті коллайдеру у вікні інспектора та\n" +
							" виберіть пункт 'Start Blender Actions'.";
						else if (SavableEditorPrefs.Language == Language.RU)
							c.text = "Пожалуйста включите плагин, используя это окно, для начала работы. Полный тьюториал\n" +
							" находится в корневой папке плагина ('Blender Actions/'), но Вы можете узнать все необходимое\n" +
							" просто читая подсказки для гарячих клавиш в этом окне (запустите плагин, чтобы\n" +
							" увидить их). Единственное незадокументированное действие - запуск режима редактирования коллайдера\n" +
							" для любого коллайдера: просто нажмите правой кнопкой на компоненте коллайдера в окне инспектора и\n" +
							" выберите пункт 'Start Blender Actions'.";

						c.text = c.text.Replace("\n", System.Environment.NewLine);

						EditorGUILayout.LabelField(c, s, GUILayout.Height(130));

						if (SavableEditorPrefs.Language == Language.ENG)
							c.text = "To open this window again in the future, go to 'Windows/Blender Actions'";
						else if (SavableEditorPrefs.Language == Language.UA)
							c.text = "Щоб знову відкрити це вікно у майбутньому перейдіть по меню 'Windows/Blender Actions'";
						else if (SavableEditorPrefs.Language == Language.RU)
							c.text = "Чтобы снова открыть это окно в будущем, выберите пункт меню 'Windows/Blender Actions'";

						EditorGUILayout.LabelField(c, s, GUILayout.Height(30));

						if (SavableEditorPrefs.Language == Language.ENG)
							c.text = "KEEP IN MIND. The tool reacts to hotkeys only if the 'Scene View' or the 'Hierarchy' window" +
							" is active at the moment. 'Scene View' window is part of the editor, where you see your level with" +
							" all the gizmos (not the 'Game' window), while 'Hierarchy' is the part of the editor, where the scene's" +
							" hierarchy is shown. IF THE TOOL'S HOTKEYS DO NOT RESPOND, it is quite likely that neither of the windows" +
							" is selected. To fix this, just position your mouse over the 'Scene View' window and push the mouse's" +
							" middle button.";
						else if (SavableEditorPrefs.Language == Language.UA)
							c.text = "МАЙТЕ НА УВАЗІ. Даний інструмент реагує на гарячі клавіши лише коли активно вікно" +
							" 'Scene View' або вікно 'Hierarchy'. 'Scene View' це вікно, де Ви бачите Вашу сцену разом із" +
							" усіма гізмо (не сплутайте з вікном 'Game'), 'Hierarchy' це вікно, де відображена ієрархія" +
							" усієї сцени. ЯКЩО ПЛАГІН НЕ РЕАГУЄ НА ГАРЯЧІ КЛАВІШИ, скоріше за все не активно жодне із" +
							" двох вікон. Щоб це поправити, наведіть курсор миші на вікно 'Scene View' та натисніть середню" +
							" кнопку миші.";
						else if (SavableEditorPrefs.Language == Language.RU)
							c.text = "ИМЕЙТЕ В ВИДУ. Данный плагин реагирует на гарячие клавиши только если активно окно" +
							" 'Scene View' или окно 'Hierarchy'. 'Scene View' это окно, где Вы видите Вашу сцену вместе со" +
							" всеми гизмо (не спутайте с окном 'Game'), 'Hierarchy' это окно, где отображена иерархия всей" +
							" сцены. ЕСЛИ ПЛАГИН НЕ РЕАГИРУЕТ НА ГАРЯЧИЕ КЛАВИШИ, скорее всего не активно ни одно из двух" +
							" окон. Чтобы это поправить, наведите курсор мышки на окно 'Scene View' и нажмите среднюю" +
							" кнопку мышки.";

						EditorGUILayout.LabelField(c, s, GUILayout.Height(100));
						
						if (SavableEditorPrefs.Language == Language.ENG)
							buttonText = "HIDE TUTORIAL";
						else if (SavableEditorPrefs.Language == Language.UA)
							buttonText = "СХОВАТИ ТЬЮТОРІАЛ";
						else if (SavableEditorPrefs.Language == Language.RU)
							buttonText = "СПРЯТАТЬ ТЬЮТОРИАЛ";
					}

					if (GUILayout.Button(buttonText, GUILayout.Width(150), GUILayout.Height(40)))
						SavableEditorPrefs.ShowTutorialSectionInCfgWindow = !SavableEditorPrefs.ShowTutorialSectionInCfgWindow;
					#endregion

					if (BlenderActions.Main != null)
					{
						EditorGUILayout.Space();

						#region General settings title
						if (SavableEditorPrefs.Language == Language.ENG)
							labelText = "General settings";
						else if (SavableEditorPrefs.Language == Language.UA)
							labelText = "Загальні налаштування";
						else if (SavableEditorPrefs.Language == Language.RU)
							labelText = "Общие настройки";

						s.fontSize = TitleFontSize;
						GUI.backgroundColor = SectionTitleColor;
						EditorGUILayout.LabelField(labelText, s, GUILayout.Height(60));
						s.fontSize = FontSize;
						#endregion

						EditorGUILayout.Space();
					
						#region Hold Control And Shift To Position 3DC ursor
						if (SavableEditorPrefs.Language == Language.ENG)
						{
							c.text = "Hold 'Control'+'Shift' to position 3D cursor: ";
							c.tooltip = "";
						}
						else if (SavableEditorPrefs.Language == Language.UA)
						{
							c.text = "Затисніть 'Control'+'Shift', щоб перемістити 3D курсор: ";
							c.tooltip = "";
						}
						else if (SavableEditorPrefs.Language == Language.RU)
						{
							c.text = "Зажмите 'Control'+'Shift', чтобы переместить 3D курсор: ";
							c.tooltip = "";
						}

						if (SavableEditorPrefs.HoldControlAndShiftToPosition3DCursor)
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "АКТИВНО";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВКЛЮЧЕНО";

							labelColor = DimGreen;
							buttonColor = DimRed;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВИМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВЫКЛЮЧИТЬ";
						}
						else
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "ВИМКНЕНО";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВЫКЛЮЧЕНО";

							labelColor = DimRed;
							buttonColor = DimGreen;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВВІМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВКЛЮЧИТЬ";
						}

						EditorGUILayout.BeginHorizontal();
							GUI.backgroundColor = labelColor;
							EditorGUILayout.LabelField(c, s, GUILayout.Height(40));
							GUI.backgroundColor = buttonColor;
							if (GUILayout.Button(buttonText, GUILayout.Width(100), GUILayout.Height(40)))
								SavableEditorPrefs.HoldControlAndShiftToPosition3DCursor 
									= !SavableEditorPrefs.HoldControlAndShiftToPosition3DCursor;
						EditorGUILayout.EndHorizontal();
						#endregion
						#region Use middle mouse to rotate
						if (SavableEditorPrefs.Language == Language.ENG)
						{
							c.text = "Use middle mouse to rotate when alt is held: ";
							c.tooltip = "Just in case you want to be able to rotate the editor camera with the middle mouse" +
								" held, instead of the standard left mouse. Does NOT turn OFF rotation with left mouse.";
						}
						else if (SavableEditorPrefs.Language == Language.UA)
						{
							c.text = "Використовувати середню кнопку миші для повороту камери, коли затиснуто 'Alt': ";
							c.tooltip = "На випадок, якщо Вам потрібно мати можливість обертати камеру редактора затиснувши" +
								" середню кнопку миші, замість стандартної лівой кнопки миші. Не вимикає обертання лівою кнопкою.";
						}
						else if (SavableEditorPrefs.Language == Language.RU)
						{
							c.text = "Использовать среднюю кнопку мышки для поворота камеры, когда зажат 'Alt': ";
							c.tooltip = "На случай, если Вы хотите иметь возможность вращать камеру редактора зажав" +
								" среднюю кнопку мышки, вместо стандартной левой. Не выключает вращение левой кнопкой.";
						}

						if (SavableEditorPrefs.UseMiddleMouseToRotate)
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "АКТИВНО";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВКЛЮЧЕНО";

							labelColor = DimGreen;
							buttonColor = DimRed;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВИМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВЫКЛЮЧИТЬ";
						}
						else
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "ВИМКНЕНО";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВЫКЛЮЧЕНО";

							labelColor = DimRed;
							buttonColor = DimGreen;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВВІМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВКЛЮЧИТЬ";
						}

						EditorGUILayout.BeginHorizontal();
							GUI.backgroundColor = labelColor;
							EditorGUILayout.LabelField(c, s, GUILayout.Height(40));
							GUI.backgroundColor = buttonColor;
							if (GUILayout.Button(buttonText, GUILayout.Width(100), GUILayout.Height(40)))
								SavableEditorPrefs.UseMiddleMouseToRotate = !SavableEditorPrefs.UseMiddleMouseToRotate;
						EditorGUILayout.EndHorizontal();
						#endregion
						#region Transform Actions Enabled
						if (SavableEditorPrefs.Language == Language.ENG)
						{
							c.text = "Translate/rotate/scale actions : ";
							c.tooltip = "Set this to 'OFF' if you don't want the translate/rotate/scale actions to ever activate.";
						}
						else if (SavableEditorPrefs.Language == Language.UA)
						{
							c.text = "Інструменти переміщення/обертання/масштабування: ";
							c.tooltip = "Вимкніть цю опцію, якщо Ви хочете, щоб інструменти переміщення/обертання/масштабування" +
								" не працювали.";
						}
						else if (SavableEditorPrefs.Language == Language.RU)
						{
							c.text = "Инструменты перемещения/вращения/скалирования: ";
							c.tooltip = "Отключите эту опцию, если Вы хотите, чтобы инструменты перемещения/вращения/скалирования" +
								" не работали.";
						}

						if (SavableEditorPrefs.TransformActionsEnabled)
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "АКТИВНІ";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВКЛЮЧЕНЫ";

							labelColor = DimGreen;
							buttonColor = DimRed;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВИМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВЫКЛЮЧИТЬ";
						}
						else
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "ВИМКНЕНІ";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВЫКЛЮЧЕНЫ";

							labelColor = DimRed;
							buttonColor = DimGreen;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВВІМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВКЛЮЧИТЬ";
						}

						EditorGUILayout.BeginHorizontal();
							GUI.backgroundColor = labelColor;
							EditorGUILayout.LabelField(c, s, GUILayout.Height(40));
							GUI.backgroundColor = buttonColor;
							if (GUILayout.Button(buttonText, GUILayout.Width(100), GUILayout.Height(40)))
								SavableEditorPrefs.TransformActionsEnabled = !SavableEditorPrefs.TransformActionsEnabled;
						EditorGUILayout.EndHorizontal();
						#endregion
						#region Reset Transforms Enabled
						if (SavableEditorPrefs.Language == Language.ENG)
						{
							c.text = "Reset translation/rotation/scale actions : ";
							c.tooltip = "Set this to 'OFF' if you don't want the 'Reset translate/rotate/scale actions' to ever activate.";
						}
						else if (SavableEditorPrefs.Language == Language.UA)
						{
							c.text = "Інструменти скидання переміщення/обертання/масштабування: ";
							c.tooltip = "Вимкніть цю опцію, якщо Ви хочете, щоб інструменти скидання переміщення/обертання/масштабування" +
								" не працювали.";
						}
						else if (SavableEditorPrefs.Language == Language.RU)
						{
							c.text = "Инструменты сброса перемещения/вращения/скалирования: ";
							c.tooltip = "Отключите эту опцию, если Вы хотите, чтобы инструменты сброса перемещения/вращения/скалирования" +
								" не работали.";
						}

						if (SavableEditorPrefs.ResetTransformsEnabled)
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "АКТИВНІ";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВКЛЮЧЕНЫ";

							labelColor = DimGreen;
							buttonColor = DimRed;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВИМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВЫКЛЮЧИТЬ";
						}
						else
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "ВИМКНЕНІ";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВЫКЛЮЧЕНЫ";

							labelColor = DimRed;
							buttonColor = DimGreen;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВВІМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВКЛЮЧИТЬ";
						}

						EditorGUILayout.BeginHorizontal();
							GUI.backgroundColor = labelColor;
							EditorGUILayout.LabelField(c, s, GUILayout.Height(40));
							GUI.backgroundColor = buttonColor;
							if (GUILayout.Button(buttonText, GUILayout.Width(100), GUILayout.Height(40)))
								SavableEditorPrefs.ResetTransformsEnabled = !SavableEditorPrefs.ResetTransformsEnabled;
						EditorGUILayout.EndHorizontal();
						#endregion
						#region Numeric Snap Enabled By Default
						//if (SavableEditorPrefs.Language == Language.ENG)
						//{
						//	c.text = "Numeric snap (snap to a grid) enabled by default : ";
						//	c.tooltip = "Set this to 'OFF' if you don't want the numeric snap to be enabled every" +
						//		" time you start a transform action.";
						//}
						//else if (SavableEditorPrefs.Language == Language.UA)
						//{
						//	c.text = "Числова прив'язка (прив'язка до сітки) завжди увімкнена : ";
						//	c.tooltip = "Вимкніть цю опцію, якщо Ви не хочете, щоб числова прив'язка завжди була увімкнена," +
						//		" коли Ви активуєте інструмент трансформування.";
						//}
						//else if (SavableEditorPrefs.Language == Language.RU)
						//{
						//	c.text = "Числовая привязка всегда включена : ";
						//	c.tooltip = "Отключите эту опцию, если Вы не хотите, чтобы числовая привязка была всегда включена," +
						//		" когда Вы запускаете инструмент трансформации.";
						//}

						//if (SavableEditorPrefs.NumericSnapEnabledByDefault)
						//{
						//	if (SavableEditorPrefs.Language == Language.ENG)
						//		c.text += "ON";
						//	else if (SavableEditorPrefs.Language == Language.UA)
						//		c.text += "ТАК";
						//	else if (SavableEditorPrefs.Language == Language.RU)
						//		c.text += "ДА";

						//	labelColor = DimGreen;
						//	buttonColor = DimRed;

						//	if (SavableEditorPrefs.Language == Language.ENG)
						//		buttonText = "TURN OFF";
						//	else if (SavableEditorPrefs.Language == Language.UA)
						//		buttonText = "ВИМКНУТИ";
						//	else if (SavableEditorPrefs.Language == Language.RU)
						//		buttonText = "ВЫКЛЮЧИТЬ";
						//}
						//else
						//{
						//	if (SavableEditorPrefs.Language == Language.ENG)
						//		c.text += "OFF";
						//	else if (SavableEditorPrefs.Language == Language.UA)
						//		c.text += "НІ";
						//	else if (SavableEditorPrefs.Language == Language.RU)
						//		c.text += "НЕТ";

						//	labelColor = DimRed;
						//	buttonColor = DimGreen;

						//	if (SavableEditorPrefs.Language == Language.ENG)
						//		buttonText = "TURN ON";
						//	else if (SavableEditorPrefs.Language == Language.UA)
						//		buttonText = "ВВІМКНУТИ";
						//	else if (SavableEditorPrefs.Language == Language.RU)
						//		buttonText = "ВКЛЮЧИТЬ";
						//}

						//EditorGUILayout.BeginHorizontal();
						//GUI.backgroundColor = labelColor;
						//EditorGUILayout.LabelField(c, s, GUILayout.Height(40));
						//GUI.backgroundColor = buttonColor;
						//if (GUILayout.Button(buttonText, GUILayout.Width(100), GUILayout.Height(40)))
						//	SavableEditorPrefs.NumericSnapEnabledByDefault = !SavableEditorPrefs.NumericSnapEnabledByDefault;
						//EditorGUILayout.EndHorizontal();
						#endregion
						#region TranslateSnapIncrement
						//if (SavableEditorPrefs.Language == Language.ENG)
						//{
						//	c.text = "Numeric snap (snap to a grid) enabled by default : ";
						//	c.tooltip = "Set this to 'OFF' if you don't want the numeric snap to be enabled every" +
						//		" time you start a transform action.";
						//}
						//else if (SavableEditorPrefs.Language == Language.UA)
						//{
						//	c.text = "Числова прив'язка (прив'язка до сітки) завжди увімкнена : ";
						//	c.tooltip = "Вимкніть цю опцію, якщо Ви не хочете, щоб числова прив'язка завжди була увімкнена," +
						//		" коли Ви активуєте інструмент трансформування.";
						//}
						//else if (SavableEditorPrefs.Language == Language.RU)
						//{
						//	c.text = "Числовая привязка всегда включена : ";
						//	c.tooltip = "Отключите эту опцию, если Вы не хотите, чтобы числовая привязка была всегда включена," +
						//		" когда Вы запускаете инструмент трансформации.";
						//}

						//if (SavableEditorPrefs.ResetTransformsEnabled)
						//{
						//	if (SavableEditorPrefs.Language == Language.ENG)
						//		c.text += "NO";
						//	else if (SavableEditorPrefs.Language == Language.UA)
						//		c.text += "ТАК";
						//	else if (SavableEditorPrefs.Language == Language.RU)
						//		c.text += "ДА";

						//	labelColor = DimGreen;
						//	buttonColor = DimRed;

						//	if (SavableEditorPrefs.Language == Language.ENG)
						//		buttonText = "TURN OFF";
						//	else if (SavableEditorPrefs.Language == Language.UA)
						//		buttonText = "ВИМКНУТИ";
						//	else if (SavableEditorPrefs.Language == Language.RU)
						//		buttonText = "ВЫКЛЮЧИТЬ";
						//}
						//else
						//{
						//	if (SavableEditorPrefs.Language == Language.ENG)
						//		c.text += "NO";
						//	else if (SavableEditorPrefs.Language == Language.UA)
						//		c.text += "НІ";
						//	else if (SavableEditorPrefs.Language == Language.RU)
						//		c.text += "НЕТ";

						//	labelColor = DimRed;
						//	buttonColor = DimGreen;

						//	if (SavableEditorPrefs.Language == Language.ENG)
						//		buttonText = "TURN ON";
						//	else if (SavableEditorPrefs.Language == Language.UA)
						//		buttonText = "ВВІМКНУТИ";
						//	else if (SavableEditorPrefs.Language == Language.RU)
						//		buttonText = "ВКЛЮЧИТЬ";
						//}

						//EditorGUILayout.BeginHorizontal();
						//GUI.backgroundColor = labelColor;
						//EditorGUILayout.LabelField(c, s, GUILayout.Height(40));
						//GUI.backgroundColor = buttonColor;
						//if (GUILayout.Button(buttonText, GUILayout.Width(100), GUILayout.Height(40)))
						//	SavableEditorPrefs.ResetTransformsEnabled = !SavableEditorPrefs.ResetTransformsEnabled;
						//EditorGUILayout.EndHorizontal();
						#endregion
						#region Camera Controls Enabled
						if (SavableEditorPrefs.Language == Language.ENG)
						{
							c.text = "Camera controls enabled : ";
							c.tooltip = "Set this to 'OFF' if you don't want the camera controls hotkeys to work.";
						}
						else if (SavableEditorPrefs.Language == Language.UA)
						{
							c.text = "Управління камерою дозволено : ";
							c.tooltip = "Вимкніть цю опцію, якщо Ви не хочете, щоб гарячі клавіши керування камерою працювали.";
						}
						else if (SavableEditorPrefs.Language == Language.RU)
						{
							c.text = "Управление камерой разрешено : ";
							c.tooltip = "Отключите эту опцию, если Вы не хотите, чтобы гарячие клавиши управления камерой работали.";
						}

						if (SavableEditorPrefs.CameraControlsEnabled)
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "ТАК";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ДА";

							labelColor = DimGreen;
							buttonColor = DimRed;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВИМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВЫКЛЮЧИТЬ";
						}
						else
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "НІ";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "НЕТ";

							labelColor = DimRed;
							buttonColor = DimGreen;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВВІМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВКЛЮЧИТЬ";
						}

						EditorGUILayout.BeginHorizontal();
						GUI.backgroundColor = labelColor;
						EditorGUILayout.LabelField(c, s, GUILayout.Height(40));
						GUI.backgroundColor = buttonColor;
						if (GUILayout.Button(buttonText, GUILayout.Width(100), GUILayout.Height(40)))
							SavableEditorPrefs.CameraControlsEnabled = !SavableEditorPrefs.CameraControlsEnabled;
						EditorGUILayout.EndHorizontal();
						#endregion
						#region Hold Shift To Slow Down Transforms
						if (SavableEditorPrefs.Language == Language.ENG)
						{
							c.text = "Hold shift to slow down transforms : ";
							c.tooltip = "Set this to 'OFF' if you don't want the 'shift' button to slow down transform actions." +
								" You can then set your own hotkey in the config - these work in parallel.";
						}
						else if (SavableEditorPrefs.Language == Language.UA)
						{
							c.text = "Затисніть 'shift', щоб сповільнити трансформації : ";
							c.tooltip = "Вимкніть цю опцію, якщо Ви не хочете, щоб затискання кнопки 'shift' сповільнювало інструменти" +
								" трансформації. Після цього, Ви можете задати свою кнопку у настройках - ці дві кнопки опції паралельно.";
						}
						else if (SavableEditorPrefs.Language == Language.RU)
						{
							c.text = "Зажмите 'shift', чтобы замедлить трансформации : ";
							c.tooltip = "Отключите эту опцию, если Вы не хотите, чтобы зажатие кнопки 'shift' замедляло инструменты" +
								" трансформаций. После этого, Вы можете задать свою кнопку в настройках - эти две опции работают параллельно.";
						}

						if (SavableEditorPrefs.HoldShiftToSlowDownTransforms)
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "АКТИВНО";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВКЛЮЧЕНО";

							labelColor = DimGreen;
							buttonColor = DimRed;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВИМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВЫКЛЮЧИТЬ";
						}
						else
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "ВИМКНЕНО";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВЫКЛЮЧЕНО";

							labelColor = DimRed;
							buttonColor = DimGreen;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВВІМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВКЛЮЧИТЬ";
						}

						EditorGUILayout.BeginHorizontal();
						GUI.backgroundColor = labelColor;
						EditorGUILayout.LabelField(c, s, GUILayout.Height(40));
						GUI.backgroundColor = buttonColor;
						if (GUILayout.Button(buttonText, GUILayout.Width(100), GUILayout.Height(40)))
							SavableEditorPrefs.HoldShiftToSlowDownTransforms = !SavableEditorPrefs.HoldShiftToSlowDownTransforms;
						EditorGUILayout.EndHorizontal();
						#endregion
						#region Hold Control For Vertex Snap During Transforms
						if (SavableEditorPrefs.Language == Language.ENG)
						{
							c.text = "Hold control for vertex snap during tranforms : ";
							c.tooltip = "Set this to 'OFF' if you don't want the 'control' button to activate vertex snap during transform actions." +
								" You can then set your own hotkey in the config - these work in parallel.";
						}
						else if (SavableEditorPrefs.Language == Language.UA)
						{
							c.text = "Затисніть 'control', для режиму прив'язки до вершин : ";
							c.tooltip = "Вимкніть цю опцію, якщо Ви не хочете, щоб затискання кнопки 'control' включало режим" +
								" прив'язки до вершин під час" +
								" трансформації. Після цього, Ви можете задати свою кнопку у настройках - ці дві кнопки опції паралельно.";
						}
						else if (SavableEditorPrefs.Language == Language.RU)
						{
							c.text = "Зажмите 'control', для режим привязки к вершинам : ";
							c.tooltip = "Отключите эту опцию, если Вы не хотите, чтобы зажатие кнопки 'control' включало режим" +
								" привязки к вершинам во время" +
								" трансформаций. После этого, Вы можете задать свою кнопку в настройках - эти две опции работают параллельно.";
						}

						if (SavableEditorPrefs.HoldControlForVertexSnapDuringTransforms)
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "АКТИВНО";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВКЛЮЧЕНО";

							labelColor = DimGreen;
							buttonColor = DimRed;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВИМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВЫКЛЮЧИТЬ";
						}
						else
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "ВИМКНЕНО";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ВЫКЛЮЧЕНО";

							labelColor = DimRed;
							buttonColor = DimGreen;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВВІМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВКЛЮЧИТЬ";
						}

						EditorGUILayout.BeginHorizontal();
						GUI.backgroundColor = labelColor;
						EditorGUILayout.LabelField(c, s, GUILayout.Height(40));
						GUI.backgroundColor = buttonColor;
						if (GUILayout.Button(buttonText, GUILayout.Width(100), GUILayout.Height(40)))
							SavableEditorPrefs.HoldControlForVertexSnapDuringTransforms 
							= !SavableEditorPrefs.HoldControlForVertexSnapDuringTransforms;
						EditorGUILayout.EndHorizontal();
						#endregion
						#region Create Colliders Enabled
						if (SavableEditorPrefs.Language == Language.ENG)
						{
							c.text = "Creating colliders enabled : ";
							c.tooltip = "Set this to 'OFF' if you don't want the collider creation hotkeys to activate.";
						}
						else if (SavableEditorPrefs.Language == Language.UA)
						{
							c.text = "Створення коллайдерів дозволено : ";
							c.tooltip = "Вимкніть цю опцію, якщо Ви не хочете, щоб гарячі клавіши створення коллайдерів працювали.";
						}
						else if (SavableEditorPrefs.Language == Language.RU)
						{
							c.text = "Создание коллайдеров разрешено : ";
							c.tooltip = "Отключите эту опцию, если Вы не хотите, чтобы гарячие клавиши создания коллайдеров работали.";
						}

						if (SavableEditorPrefs.CreateCollidersEnabled)
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "ТАК";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "ДА";

							labelColor = DimGreen;
							buttonColor = DimRed;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВИМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВЫКЛЮЧИТЬ";
						}
						else
						{
							if (SavableEditorPrefs.Language == Language.ENG)
								c.text += "OFF";
							else if (SavableEditorPrefs.Language == Language.UA)
								c.text += "НІ";
							else if (SavableEditorPrefs.Language == Language.RU)
								c.text += "НЕТ";

							labelColor = DimRed;
							buttonColor = DimGreen;

							if (SavableEditorPrefs.Language == Language.ENG)
								buttonText = "TURN ON";
							else if (SavableEditorPrefs.Language == Language.UA)
								buttonText = "ВВІМКНУТИ";
							else if (SavableEditorPrefs.Language == Language.RU)
								buttonText = "ВКЛЮЧИТЬ";
						}

						EditorGUILayout.BeginHorizontal();
						GUI.backgroundColor = labelColor;
						EditorGUILayout.LabelField(c, s, GUILayout.Height(40));
						GUI.backgroundColor = buttonColor;
						if (GUILayout.Button(buttonText, GUILayout.Width(100), GUILayout.Height(40)))
							SavableEditorPrefs.CreateCollidersEnabled = !SavableEditorPrefs.CreateCollidersEnabled;
						EditorGUILayout.EndHorizontal();
						#endregion

						EditorGUILayout.Space();

						#region Hotkey configuration title
						if (SavableEditorPrefs.Language == Language.ENG)
							labelText = "Hotkey configuration";
						else if (SavableEditorPrefs.Language == Language.UA)
							labelText = "Налаштування гарячих клавіш";
						else if (SavableEditorPrefs.Language == Language.RU)
							labelText = "Настройки горячих клавиш";

						s.fontSize = TitleFontSize;
						GUI.backgroundColor = SectionTitleColor;
						EditorGUILayout.LabelField(labelText, s, GUILayout.Height(60));
						s.fontSize = FontSize;
						

						if (SavableEditorPrefs.Language == Language.ENG)
							labelText = "Mouse buttons are supported.";
						else if (SavableEditorPrefs.Language == Language.UA)
							labelText = "Кнопки миші підтримуются";
						else if (SavableEditorPrefs.Language == Language.RU)
							labelText = "Кнопки мышки поддерживаются";

						EditorGUILayout.LabelField(labelText, s, GUILayout.Height(40));
						EditorGUILayout.Space();
						#endregion
						
						EditorGUILayout.Space();

						#region Hotkey configuration labels and buttons
						// #colreg(swampgreen*2);
						foreach (var hotkey in BlenderActions.Main.AllActions)
						{
							GUI.backgroundColor = BGColor;

							if ((hotkey.Value.HotkeyType == HotkeyType.TransformActivators
									&& !SavableEditorPrefs.TransformActionsEnabled)
								|| (hotkey.Value.HotkeyType == HotkeyType.TransformResets
									&& !SavableEditorPrefs.ResetTransformsEnabled)
								|| (hotkey.Value.HotkeyType == HotkeyType.CameraControls
									&& !SavableEditorPrefs.CameraControlsEnabled)
								|| (hotkey.Value.HotkeyType == HotkeyType.ColliderCreators
									&& !SavableEditorPrefs.CreateCollidersEnabled))
								GUI.backgroundColor = BGDeactivatedColor;
								

							EditorGUILayout.BeginHorizontal();
								EditorGUILayout.BeginVertical();
									s.alignment = TextAnchor.MiddleLeft;
									if (SavableEditorPrefs.Language == Language.ENG)
									{
										c.text = " " + hotkey.Value.DisplayNameENG;
										c.tooltip = hotkey.Value.TooltipENG;
									}
									else if (SavableEditorPrefs.Language == Language.UA)
									{
										c.text = " " + hotkey.Value.DisplayNameUA;
										c.tooltip = hotkey.Value.TooltipUA;
									}
									else if (SavableEditorPrefs.Language == Language.RU)
									{
										c.text = " " + hotkey.Value.DisplayNameRU;
										c.tooltip = hotkey.Value.TooltipRU;
									}
									EditorGUILayout.LabelField(c, s, GUILayout.Width(681), GUILayout.Height(27));
									EditorGUILayout.BeginHorizontal();
										c.text = "Control";
										if (SavableEditorPrefs.Language == Language.ENG)
											c.tooltip = "React to hotkey press only if 'Control' modifier is held down.";
										else if (SavableEditorPrefs.Language == Language.UA)
											c.tooltip = "Сприймати натискання гарячої клавіші лише коли модифікатор 'Control' затиснутий.";
										else if (SavableEditorPrefs.Language == Language.RU)
											c.tooltip = "Реагировать на нажатие горячей клавиши только если модификатор 'Control' зажат.";
										hotkey.Value.Control = EditorGUILayout.ToggleLeft(c,
											hotkey.Value.Control, s, GUILayout.Width(167), GUILayout.Height(27));
										if (hotkey.Value.Control)
											hotkey.Value.Any = false;

										c.text = "Alt";
										if (SavableEditorPrefs.Language == Language.ENG)
											c.tooltip = "React to hotkey press only if 'Alt' modifier is held down.";
										else if (SavableEditorPrefs.Language == Language.UA)
											c.tooltip = "Сприймати натискання гарячої клавіші лише коли модифікатор 'Alt' затиснутий.";
										else if (SavableEditorPrefs.Language == Language.RU)
											c.tooltip = "Реагировать на нажатие горячей клавиши только если модификатор 'Alt' зажат.";
										hotkey.Value.Alt = EditorGUILayout.ToggleLeft(c,
											hotkey.Value.Alt, s, GUILayout.Width(167), GUILayout.Height(27));
										if (hotkey.Value.Alt)
											hotkey.Value.Any = false;

										c.text = "Shift";
										if (SavableEditorPrefs.Language == Language.ENG)
											c.tooltip = "React to hotkey press only if 'Shift' modifier is held down.";
										else if (SavableEditorPrefs.Language == Language.UA)
											c.tooltip = "Сприймати натискання гарячої клавіші лише коли модифікатор 'Shift' затиснутий.";
										else if (SavableEditorPrefs.Language == Language.RU)
											c.tooltip = "Реагировать на нажатие горячей клавиши только если модификатор 'Shift' зажат.";
										hotkey.Value.Shift = EditorGUILayout.ToggleLeft(c,
											hotkey.Value.Shift, s, GUILayout.Width(167), GUILayout.Height(27));
										if (hotkey.Value.Shift)
											hotkey.Value.Any = false;

										c.text = "Any";
										if (SavableEditorPrefs.Language == Language.ENG)
											c.tooltip = "React to hotkey press regardless of modifiers states.";
										else if (SavableEditorPrefs.Language == Language.UA)
											c.tooltip = "Сприймати натискання гарячої клавіші незалежно від стану модифікаторів.";
										else if (SavableEditorPrefs.Language == Language.RU)
											c.tooltip = "Реагировать на нажатие горячей клавиши независимо от состояния модификаторов.";
										hotkey.Value.Any = EditorGUILayout.ToggleLeft(c,
											hotkey.Value.Any, s, GUILayout.Width(167), GUILayout.Height(27));
										if (hotkey.Value.Any)
										{
											hotkey.Value.Control = false;
											hotkey.Value.Alt = false;
											hotkey.Value.Shift = false;
										}
									EditorGUILayout.EndHorizontal();
								EditorGUILayout.EndVertical();

								s.alignment = TextAnchor.MiddleCenter;
								string keyText = "";
								if (SavableEditorPrefs.Language == Language.ENG)
									keyText = "NOT SET";
								else if (SavableEditorPrefs.Language == Language.UA)
									keyText = "ПУСТО";
								else if (SavableEditorPrefs.Language == Language.RU)
									keyText = "ПУСТО";

								if (hotkey.Value.MainKeyCode != KeyCode.Delete)
								{
									keyText = Utils.ProcessKeyCodeDisplayName(hotkey.Value.MainKeyCode);
								}
								if (hotkey.Value.MainMouseButton >= 0)
									keyText = "Mouse" + (hotkey.Value.MainMouseButton + 1).ToString();
								if (GUILayout.Button(keyText, s, GUILayout.Width(100), GUILayout.Height(50)))
								{
									ActiveHotkey = hotkey.Value;
									EnterKey.OpenWindow(this, isMain: true);
								}

								if (SavableEditorPrefs.Language == Language.ENG)
									keyText = "NOT SET";
								else if (SavableEditorPrefs.Language == Language.UA)
									keyText = "ПУСТО";
								else if (SavableEditorPrefs.Language == Language.RU)
									keyText = "ПУСТО";
								if (hotkey.Value.SecondaryKeyCode != KeyCode.Delete)
								{
									keyText = Utils.ProcessKeyCodeDisplayName(hotkey.Value.SecondaryKeyCode);
								}
								if (hotkey.Value.SecondaryMouseButton >= 0)
									keyText = "Mouse" + (hotkey.Value.SecondaryMouseButton + 1).ToString();
								if (GUILayout.Button(keyText, s, GUILayout.Width(100), GUILayout.Height(50)))
								{
									ActiveHotkey = hotkey.Value;
									EnterKey.OpenWindow(this, isMain: false);
								}
							EditorGUILayout.EndHorizontal();
							EditorGUILayout.Space();
						//#endcolreg
					}

					if (SavableEditorPrefs.Language == Language.ENG)
							labelText = "RESET DEFAULTS";
						else if (SavableEditorPrefs.Language == Language.UA)
							labelText = "СКИНУТИ ДО ДЕФОЛТНИХ";
						else if (SavableEditorPrefs.Language == Language.RU)
							labelText = "СБРОСИТЬ ДО ДЕФОЛТНЫХ";

						GUI.backgroundColor = BGColor;
						if (GUILayout.Button(labelText, GUILayout.Width(190), GUILayout.Height(55)))
						{
							if (BlenderActions.Main != null)
								BlenderActions.Main.InitializeHotkeys();
						}
						#endregion
					}
				EditorGUILayout.EndVertical();
				EditorGUILayout.EndScrollView();
				//GUI.backgroundColor = Color.green;
				////Font f = new Font();
				////GUI.skin.label.font = f;

				//if (GUILayout.Button("Change key", GUILayout.Width(100), GUILayout.Height(40)))
				//{
				//	//ActiveHotkey = new Hotkey();
				//	InputKey.OpenWindow(this);
				//}

				//         EditorGUILayout.Space();

				//EditorGUILayout.HelpBox(
				//	"Improved and modified Blender actions for Unity editor.", MessageType.Info);
				//EditorGUILayout.HelpBox(
				//	"To open this window go to Windows/Blender actions.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox(
				//	"Most of the stuff below, only works when the Scene View window has focus - keep that in mind.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("Most hotkeys are hardcoded - search for them in the project to change."
				//	+ " I would advise trying them out first, they are kinda neat :)", MessageType.Info);
				//EditorGUILayout.HelpBox("Unity's Move/Rotate/Scale gizmo is hidden - use [Ctrl]+[Space] to unhide/hide them.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("Press [D] to move objects, [S] to scale, [R] to rotate. When you are done with the tool, " +
				//	"push [Left mouse] to commit changes or [Right mouse] to revert changes",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("While doing any of the latter three actions you can push [X] to limit actions to the X axis,"
				//	+ " push [C] to limit actions to the Z axis and push [Z] to limit actions to the Y axis.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("If you want to limit actions to the respective LOCAL axis, double tap the respective key.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("When limiting actions with hotkeys, hold [Shift] to restrict movement to any axis BUT the selected one. " +
				//	"For instance: push [Shift] + [C] to limit action to the [X-Y] plane.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("When performing actions, hold [Shift] to slow down the tool 10 times: scaling will happen slower," +
				//	" rotation will spin slower, etc.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("When performing rotations or scalings, type the number you need to be applied as an action. E.g. " +
				//	"push [4] + [5] when rotating to rotate the object 45 degrees. Floating point numbers are supported - use [,] or [.] keys.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("Push [Alt]+[1] for Front view, [Alt]+[2] for Top view, [Alt]+[3] for Left view. Holding [Ctrl] " +
				//	"instead of [Alt] will show the opposite view: e.g. Bottom instead of Top.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("Push [5] to change perspective from Orthographic to Perspective.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("Hold [Alt], [Middle mouse button] and move the mouse to orbit the camera.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("Push [Shift] + [R] to quickly rotate the selected objects 90 degrees around the world's Y axis.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("Push [Alt] + [R] to reset rotation, [Alt] + [D] to reset position and [Alt] + [S] to reset scale.",
				//	MessageType.Info);
				//EditorGUILayout.HelpBox("Pushing [Ctrl] + [D] now launches the transform tool, alongside duplicating the object(s) " +
				//	"AND resets all the duplicated prefabs (if any). Takes 3 [Ctrl]+[Z]-s to undo. You can always [Ctrl]+[C], " +
				//	"[Ctrl]+[V] for default duplication functionality.",
				//	MessageType.Info);


				//Data.transformEditingEnabled = EditorGUILayout.BeginToggleGroup("Transform Edit Enabled", Data.transformEditingEnabled);
				//EditorGUILayout.EndToggleGroup();

				//Data.snappingEnabledByDefault = EditorGUILayout.BeginToggleGroup("Snap Enabled by Default", Data.snappingEnabledByDefault);
				//EditorGUILayout.EndToggleGroup();

				//if (Data.snappingEnabledByDefault)
				//{
				//	Data.translateSnapIncrement = EditorGUILayout.FloatField("Translate Snap Increment", Data.translateSnapIncrement);
				//	Data.rotateSnapIncrement = EditorGUILayout.FloatField("Rotate Snap Increment", Data.rotateSnapIncrement);
				//	Data.scaleSnapIncrement = EditorGUILayout.FloatField("Scale Snap Increment", Data.scaleSnapIncrement);
				//}

				//         EditorGUILayout.Space();

				//         Data.useTInstedOfR = EditorGUILayout.BeginToggleGroup("Use 'T' instead of 'R' for Rotation", Data.useTInstedOfR);
				//EditorGUILayout.EndToggleGroup();

				//         Data.enableMouseConfirmCancel = EditorGUILayout.BeginToggleGroup("Mouse Confirm/Cancel Enabled", Data.enableMouseConfirmCancel);
				//EditorGUILayout.EndToggleGroup();

				//EditorGUILayout.Space();

				//         Data.resetTransformsEnabled = EditorGUILayout.BeginToggleGroup("Reset Transform Hot Keys Enabled", Data.resetTransformsEnabled);
				//         EditorGUILayout.EndToggleGroup();

				//         EditorGUILayout.Space();

				//         Data.cameraControlEnabled = EditorGUILayout.BeginToggleGroup("Camera Control Hot Keys Enabled", Data.cameraControlEnabled);
				//         EditorGUILayout.EndToggleGroup();
			}
		}

		void ToggleBlenderActionsON_OFF()
		{
			SavableEditorPrefs.BlenderActionsON = !SavableEditorPrefs.BlenderActionsON;
			if (SavableEditorPrefs.BlenderActionsON)
				BlenderActions.TurnBlenderActionsON();
			else
				BlenderActions.TurnBlenderActionsOFF();
		}

		void OnLostFocus()
		{
			SavableEditorPrefs.Save();
		}
	}

	/// <summary>Waits for user input and sends it to the ConfigWindow class.</summary>
	public class EnterKey : EditorWindow
	{
		public ConfigWindow ConfigWindow;
		public bool IsMain = false;
		GUIContent content = new GUIContent();
		GUIStyle style = new GUIStyle(EditorStyles.textField);

		public static EnterKey OpenWindow(ConfigWindow configWindow, bool isMain)
		{
			EnterKey window = (EnterKey)GetWindow(typeof(EnterKey));
			window.ConfigWindow = configWindow;
			window.IsMain = isMain;
			string text = "";
			if (SavableEditorPrefs.Language == Language.ENG)
				text = "Press a keyboard/mouse button...";
			else if (SavableEditorPrefs.Language == Language.UA)
				text = "Натисніть клавішу на клавіатурі/миші...";
			else if (SavableEditorPrefs.Language == Language.RU)
				text = "Нажмите кнопку на клавиатуре/мышке...";
#if U5
			window.title = text;
#endif
#if U2017
			window.content.text = text;
			window.titleContent = window.content;
#endif

			window.style.fontSize = configWindow.FontSize;
			window.style.fontStyle = FontStyle.Bold;
			window.style.alignment = TextAnchor.MiddleLeft;

			window.minSize = new Vector2(600, 125);
			window.maxSize = new Vector2(600, 125);

			var position = window.position;
			position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
			window.position = position;

			return window;
		}

		void FocusConfigWindow()
		{
			if (ConfigWindow != null)
			{
				ConfigWindow.ActiveHotkey = null;
				ConfigWindow.Focus();
			}
		}

		private void OnDestroy()
		{
			FocusConfigWindow();
		}

		private void OnDisable()
		{
			FocusConfigWindow();
		}


		private void OnLostFocus()
		{
			Close();
			FocusConfigWindow();
		}


		void OnGUI()
		{
			string text = "";
			if (SavableEditorPrefs.Language == Language.ENG)
				text = "   Press a keyboard/mouse button." + System.Environment.NewLine 
					+ "   Press 'DELETE' to clear.";
			else if (SavableEditorPrefs.Language == Language.UA)
				text = "   Натисніть клавішу на клавіатурі/миші." + System.Environment.NewLine 
					+ "   Натисніть 'DELETE', щоб зробити поле пустим.";
			else if (SavableEditorPrefs.Language == Language.RU)
				text = "   Нажмите кнопку на клавиатуре/мышке." + System.Environment.NewLine 
					+ "   Нажмите 'DELETE', чтобы сделать поле пустым.";

			GUI.backgroundColor = ConfigWindow.ContrastGreen;
			if (SavableEditorPrefs.Language == Language.ENG)
				style.fontSize = 18;
			else
				style.fontSize = 18;

#if U5_6
			EditorGUILayout.LabelField(text, style, GUILayout.Width(800), GUILayout.Height(50));
#endif
#if U2017
			content.text = text;
			EditorGUILayout.LabelField(content, style, GUILayout.Width(800), GUILayout.Height(50));
#endif
			if (Event.current != null && 
				(Event.current.type == EventType.KeyDown || Event.current.type == EventType.MouseDown))
			{
				if (Event.current.type == EventType.KeyDown)
				{
					if (IsMain)
					{
						ConfigWindow.ActiveHotkey.MainKeyCode = Event.current.keyCode;
						ConfigWindow.ActiveHotkey.MainMouseButton = -1;
					}
					else
					{
						ConfigWindow.ActiveHotkey.SecondaryKeyCode = Event.current.keyCode;
						ConfigWindow.ActiveHotkey.SecondaryMouseButton = -1;
					}
				}
				else
				{
					if (IsMain)
					{
						ConfigWindow.ActiveHotkey.MainMouseButton = Event.current.button;
						ConfigWindow.ActiveHotkey.MainKeyCode = KeyCode.Delete;
					}
					else
					{
						ConfigWindow.ActiveHotkey.SecondaryMouseButton = Event.current.button;
						ConfigWindow.ActiveHotkey.SecondaryKeyCode = KeyCode.Delete;
					}

				}

				ConfigWindow.ActiveHotkey = null;
				Close();
			}
		}
	}
}