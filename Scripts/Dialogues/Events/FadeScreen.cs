//Fade Screen Dialogue Event script by SeleniumSoul for DREditor.
using System;
using UnityEngine;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public class FadeScreen : IDialogueEvent
	{
		public bool active = true;
		public float fadeTime = 0f;
		public Color fadeColor = Color.black;

		public async Task TriggerEvent()
		{
			(bool, float, Color) value = new(active, fadeTime, fadeColor);
			await DialogueEventSystem.TriggerEvent("FadeScreen", value);
		}

	#if UNITY_EDITOR
		private bool showHelp = false;
		public void EditorUI()
		{
			fadeTime = EditorGUILayout.FloatField(new GUIContent("Fade Time", "Amount in seconds the screen will fade."), fadeTime);
		}

		public void ToggleHelpBox()
		{
			showHelp = !showHelp;
		}

		public void ShowHelpBox()
		{
			if (showHelp) 
				EditorGUILayout.HelpBox("Show CG pictures. \n\nNote: CGs should be a GameObject Prefab.", MessageType.Info, true);
		}
	#endif
	}
}