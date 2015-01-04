/* The GUI class of THCA.
 * Author: Quinten Feys & Willem van Vliet
 * License: BY: Attribution-ShareAlike 3.0 Unported (CC BY-SA 3.0): http://creativecommons.org/licenses/by-sa/3.0/
 */
using System;
using UnityEngine;
using KSP.IO;
using KSP;

namespace ThrottleControlledAvionics
{
	public class TCAGui
	{

		private bool showEngines;
		protected Rect windowPos;
		protected Rect windowPosHelp;
		private bool showHelp;
		private bool showDebug;
		private bool showDirectionSelector = true;
		private bool showHUD = false;
		private ApplicationLauncherButton button;
		private ComboBox directionListBox;
		private Rect windowDirectionPos;
		private Vector2 positionScrollViewEngines;
		private GUIContent[] directionList;   
		private GUIContent[] saveList;
		private ComboBox saveListBox;    

		private Texture textureOn;
		private Texture textureOff;
		private Texture textureReverse;
		private Texture textureNoCharge;

		private ThrottleControlledAvionics main;

		public TCAGui (ThrottleControlledAvionics _main)
		{
			main = _main; //refference to the main class

			windowPos = new Rect(50, 50, 400, 200);
			windowPosHelp = new Rect(500, 100, 400, 50);
			showEngines = false;
			showHelp = false;
			showDebug = false;
			directionList = new GUIContent[3];
			directionList[0] = new GUIContent("up");
			directionList[1] = new GUIContent("mean");
			directionList[2] = new GUIContent("custom");
			directionListBox = new ComboBox();

			saveList = new GUIContent[3];
			saveList[0] = new GUIContent(main.save.GetName(0));
			saveList[1] = new GUIContent(main.save.GetName(1));
			saveList[2] = new GUIContent(main.save.GetName(2));
			saveListBox = new ComboBox();

			textureOn = GameDatabase.Instance.GetTexture("ThrottleControlledAvionics/textures/icon_button_on", false);
			textureOff = GameDatabase.Instance.GetTexture("ThrottleControlledAvionics/textures/icon_button_off", false);
			textureReverse = GameDatabase.Instance.GetTexture("ThrottleControlledAvionics/textures/icon_button_R", false);
			textureNoCharge = GameDatabase.Instance.GetTexture("ThrottleControlledAvionics/textures/icon_button_noCharge", false);

			//Enlist to the toolbar
			try
			{
				GameEvents.onGUIApplicationLauncherReady.Add (this.OnGuiAppLauncherReady);
				GameEvents.onGameSceneLoadRequested.Add (this.OnGameSceneLoadRequestedForAppLauncher);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			//Debug.Log("[TCA] GUI created.");
		}

		//called from main
		public void OnGUI()
		{
			if (showHUD) drawGUI();
			UpdateToolbarIcon();
		}

		//called from main
		public void Destroy()
		{
			try
			{
				GameEvents.onGUIApplicationLauncherReady.Remove (this.OnGuiAppLauncherReady);
				GameEvents.onGameSceneLoadRequested.Remove (this.OnGameSceneLoadRequestedForAppLauncher);
				if(this.button) ApplicationLauncher.Instance.RemoveModApplication(this.button);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			//Debug.Log("[TCA] GUI destroyed.");
		}

		private void OnGameSceneLoadRequestedForAppLauncher(GameScenes SceneToLoad)
		{
			if (this.button != null)
			{
				//ApplicationLauncherButton[] lstButtons = KerbalAlarmClock.FindObjectsOfType<ApplicationLauncherButton>();
				//LogFormatted("AppLauncher: Destroying Button-Button Count:{0}", lstButtons.Length);
				ApplicationLauncher.Instance.RemoveModApplication(this.button);
				this.button = null;
			}
		}

		private void OnGuiAppLauncherReady()
		{
			if (this.button)
				return;
			try
			{
				this.button = ApplicationLauncher.Instance.AddModApplication(
					this.OnTrue,
					this.OnFalse,
					this.OnHover,
					this.OnHoverOut,
					null,
					null,
					ApplicationLauncher.AppScenes.FLIGHT, //just the flight scene
					textureOff);
				//this.actionMenuGui = this.button.gameObject.AddComponent<ActionMenuGui>();
				//this.actionMenuGui.transform.parent = this.button.transform;
				//ApplicationLauncher.Instance.EnableMutuallyExclusive(this.button);
				GameEvents.onHideUI.Add(this.OnHide);
				GameEvents.onShowUI.Add(this.OnShow);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
		
		private void OnHide()
		{
			showHUD = false;
		}
		
		private void OnHover()
		{
			//
		}
		
		private void OnHoverOut()
		{
			//
		}
		
		private void OnShow()
		{
			showHUD = true;
		}
		
		private void OnTrue()
		{
			showHUD = true;
		}
		
		private void OnFalse()
		{
			showHUD = false;
		}
		
		private void drawGUI()
		{
			windowPos = GUILayout.Window(1, windowPos, WindowGUI, "Throttle Controlled Avionics");
			
			if (showHelp) { windowPosHelp = GUILayout.Window(2, windowPosHelp, windowHelp, "Instructions"); }
			if (showDebug) { main.DrawDebugLines(); }
			if (main.direction == ThrottleControlledAvionics.thrustDirections.custom && showDirectionSelector)
			{
				if (windowDirectionPos.xMin <= 1) { windowDirectionPos = new Rect(windowPos.xMax, windowPos.yMin + 50, 120,50); }
				windowDirectionPos = GUILayout.Window(3, windowDirectionPos, WindowDirectionSelector, "Direction Selector");
			}
		}
		
		private void WindowGUI(int windowID)
		{
			if (GUI.Button(new Rect(windowPos.width - 23f, 2f, 20f, 18f), "?"))
			{
				showHelp = !showHelp;
			}
			
			GUILayout.BeginVertical();
			
			if (!main.ElectricChargeAvailible(main.vessel)) { GUILayout.Label("WARNING! Electric charge has run out!"); }

			main.isActive = GUILayout.Toggle(main.isActive, "Toggle TCA");
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Settings: ", GUILayout.ExpandWidth(true));
			InsertDropdownboxSave();
			GUILayout.EndHorizontal();
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Sensitivity: ", GUILayout.ExpandWidth(true));
			GUILayout.Label("" + main.save.GetActiveSensitivity(), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
			main.save.SetActiveSensitivity(Mathf.Exp(GUILayout.HorizontalSlider(Mathf.Log(main.save.GetActiveSensitivity()), -2.0f, 2.0f)));
			
			GUILayout.BeginHorizontal();
			GUILayout.Label("Mean thrust: ", GUILayout.ExpandWidth(true));
			GUILayout.Label("" + main.save.GetActiveMeanThrust(), GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
			main.save.SetActiveMeanThrust(GUILayout.HorizontalSlider(main.save.GetActiveMeanThrust(), 80f, 120f));
			
			main.detectStearingThrusters = GUILayout.Toggle(main.detectStearingThrusters, "Detect reaction control thrusters");
			if (main.detectStearingThrusters)
			{
				GUILayout.Label("Stearing Threshold: " + (Math.Acos(main.minEfficiency) * 180 / Math.PI) + "°");
				main.minEfficiency = Mathf.Cos(GUILayout.HorizontalSlider(Mathf.Acos(main.minEfficiency), 0f, Mathf.PI / 2));
				GUILayout.BeginHorizontal();
				GUILayout.Label("Direction");
				InsertDropdownboxDirection();
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("");
				if (main.direction == ThrottleControlledAvionics.thrustDirections.custom)
				{
					if (GUILayout.Button("Open/Close Direction Selector")) { showDirectionSelector = !showDirectionSelector; }
				}
				GUILayout.EndHorizontal();
				showDebug = GUILayout.Toggle(showDebug, "Show directions");
				main.vtolMode = GUILayout.Toggle (main.vtolMode, "Zero velocity (WIP)"); //VTOL mode
			}
			//contUpdate = GUILayout.Toggle(contUpdate, "Continuous engine update");
			//if (!contUpdate)
			//{
			//    if (GUILayout.Button("recalculate engine torque"))
			//    {
			//        enginesCounted = false;
			//    }
			//}
			
			showEngines = GUILayout.Toggle(showEngines, "show/hide engine information");
			if (showEngines)
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label("Vessel situation: ", GUILayout.ExpandWidth(true));
				GUILayout.Label("" + main.vessel.situation.ToString(), GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("torque demand: ", GUILayout.ExpandWidth(true));
				GUILayout.Label("" + main.demand.ToString(), GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Thrust Axis: ", GUILayout.ExpandWidth(true));
				GUILayout.Label("" + main.mainThrustAxis.ToString(), GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Surface Velocity: ", GUILayout.ExpandWidth(true));
				GUILayout.Label("" + main.vessel.GetSrfVelocity().normalized.ToString(), GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal();
				GUILayout.Label("Forward Vector: ", GUILayout.ExpandWidth(true));
				GUILayout.Label("" + main.vessel.GetFwdVector().ToString(), GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();
				//GUILayout.BeginHorizontal();
				//GUILayout.Label("ReferenceTransformPart: ", GUILayout.ExpandWidth(true));
				//GUILayout.Label("" + vessel.GetReferenceTransformPart(), GUILayout.ExpandWidth(false));
				//GUILayout.EndHorizontal();
				//GUILayout.BeginHorizontal();
				//GUILayout.Label("ReferenceTransformPartID: ", GUILayout.ExpandWidth(true));
				//GUILayout.Label("" + vessel.referenceTransformId, GUILayout.ExpandWidth(false));
				//GUILayout.EndHorizontal();
				//GUILayout.BeginHorizontal();
				//GUILayout.Label("ReferenceTransformRot: ", GUILayout.ExpandWidth(true));
				//GUILayout.Label("" + (vessel.GetReferenceTransformPart().orgRot * Vector3.down).ToString(), GUILayout.ExpandWidth(false));
				//GUILayout.EndHorizontal();
				positionScrollViewEngines = GUILayout.BeginScrollView(positionScrollViewEngines, GUILayout.Height(300));
				foreach (EngineWrapper eng in main.engineTable)
				{
					GUILayout.Label(eng.getName() + " \r\n" +
					                "steering vector: " + eng.steeringVector + "\r\n" +
					                "thrustvector: " + eng.thrustVector + "\r\n" +
					                "Steering: " + eng.steering +
					                " Efficiancy: " + eng.efficiency.ToString("0.00") + "\r\n" +
					                "Thrust: " + eng.thrustPercentage.ToString()
					                );
				}
				GUILayout.EndScrollView();
			}
			else { GUILayout.Label("."); }
			
			GUILayout.EndVertical();
			GUI.DragWindow();
			
			
		}
		
		private void WindowDirectionSelector(int windowID)
		{
			GUILayout.BeginVertical();
			GUILayout.Label("CutsonDirection: " + main.customDirectionVector.ToString("0.0") );
			GUILayout.Label("MainThrust: " + main.mainThrustAxis.ToString("0.0") );
			GUILayout.Label("pitch: " + Mathf.RoundToInt(main.customDirectionVector.x) + "°");
			GUILayout.BeginHorizontal();
			for (int a = -90; a < 200; a += 90) { if (GUILayout.Button(a.ToString() + "°")) { main.customDirectionVector.x = a; } }
			GUILayout.EndHorizontal();
			main.customDirectionVector.x = GUILayout.HorizontalSlider(main.customDirectionVector.x, -180f, 180f);
			GUILayout.Label("yaw: " + Mathf.RoundToInt(main.customDirectionVector.y) + "°");
			GUILayout.BeginHorizontal();
			for (int a = -90; a < 200; a += 90) { if (GUILayout.Button(a.ToString() + "°")) { main.customDirectionVector.y = a; } }
			GUILayout.EndHorizontal();
			main.customDirectionVector.y = GUILayout.HorizontalSlider(main.customDirectionVector.y, -180f, 180f);
			GUILayout.EndVertical();
			GUI.DragWindow();
		}
		
		/// <summary>
		/// Makes sure the toolbar icon resembles the present situation
		/// TODO: update toolbaar button status
		/// </summary>
		private void UpdateToolbarIcon() {
            if (main.isActive) {
				if (main.ElectricChargeAvailible (main.vessel)) {
					if (main.vtolMode)
						this.button.SetTexture(textureReverse);
					else
						this.button.SetTexture(textureOn);
				} else {
					this.button.SetTexture( textureNoCharge);
				}
			} else
				this.button.SetTexture(textureOff);
		}
		
		/// <summary>
		/// Inserts a dropdownbox to select the deired saved settings from
		/// </summary>
		private void InsertDropdownboxSave()
		{
			int i = saveListBox.GetSelectedItemIndex();
			i = saveListBox.List( saveList[i].text, saveList, "Box");
			main.save.SetActiveSave(i);
		}
		
		/// <summary>
		/// Inserts a dropdownbox to select the desired direction
		/// </summary>
		private void InsertDropdownboxDirection()
		{
			int i = directionListBox.GetSelectedItemIndex();
			i = directionListBox.List(directionList[i].text, directionList, "Box");
			switch(i)
			{
			case 0:
				main.direction = ThrottleControlledAvionics.thrustDirections.up;
				break;
			case 1:
				main.direction = ThrottleControlledAvionics.thrustDirections.mean;
				break;
			case 2:
				main.direction = ThrottleControlledAvionics.thrustDirections.custom;
				
				break;
			default:
				Debug.LogError("Invalid direction given");
				break;
				
			}
		}
		
		private void windowHelp(int windowID)
		{
			String instructions = "Welcome to the instructions manual.\nFor simple use:\n\t 1)Put TCA on ('y'),\n\t 2)Put SAS on ('t'), \n\t 3) Launch \n\n"+
				"For more advanced use:\n\t -The sensitivity determines the amount of thrust differences TCA will utilise. A high value will give a very fast and abrupt respose, a low value will be a lot smoother"+
					", but might not be as fast.\n\t -Mean thrust is the virtual average thrust. A value below 100 means that the engines will be started throttled down and "+
					"will correct by both throttling up and down. A value above 100 means that they will wait untill the deviation becomes rather big. This might be good if "+
					"you think that the standard avionics are strong enough to handle the load. \n\t -3 different settings can be saved at the same time. Saving happens automaticly."+
					" \n\t -Detect reaction control thrusters will cause engines that are not sufficiently aligned with the direction you want to go in to only be used as reaction control engines. " +
					"This direction can be chosen as up, where normal rockets or planes want to go to; mean, which takes the weighted avarage of all the engines and tries to go that way, "+
					"or you can chose the direction yourself with custom. The stearing threshold is the maximum angle between an engine and the desired direction so that that engine will fire "+
					"(near) full throttle. A higher angle will result in more engines firing, but with potential less efficiency" +
					"\n\nWarning: \n\t -TCA assumes that the engine bells are aligned allong the y-axis of the engine parts. This is the standard orientation for most of them. "+
					"If you possess engines that can change the orientation of their engine bells, like some form 'Ferram aerospace' please make sure that they are alligned allong"+
					" the right axis before enabeling TCA. \n\t -Note that while jet engines can be throttled, they have some latancy, what might controling VTOL's based on these"+
					" rather tricky. \n\t SRB's can't be controlled. That's because they're SRB's";
			GUILayout.Label(instructions, GUILayout.MaxWidth(400));
			GUI.DragWindow();
		}

	}
}

