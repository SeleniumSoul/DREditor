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
	public class LoadRoom : IDialogueEvent
	{
		public string scenePath;

		public async Task TriggerEvent()
		{
			await DialogueEventSystem.TriggerEvent("LoadRoom", scenePath);
		}

	#if UNITY_EDITOR
		private bool showHelp = false;
		public void EditorUI()
		{
            scenePath = EditorGUILayout.TextField(new GUIContent("Scene Path", "Use the ADDRESSABLES scene path."), scenePath);
		}

		public void ToggleHelpBox()
		{
			showHelp = !showHelp;
		}

		public void ShowHelpBox()
		{
			if (showHelp) 
				EditorGUILayout.HelpBox("Load to a new room.", MessageType.Info, true);
		}
	#endif
	}
}