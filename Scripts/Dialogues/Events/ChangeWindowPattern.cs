//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct CWPTuple
	{
		public string PatternName;
	}

	[Serializable]
	public class ChangeWindowPattern : IDialogueEvent
	{
		public bool _ShowHelp = false;
		public CWPTuple CWPValue;

		public void TriggerDialogueEvent()
		{
			DialogueEventSystem.TriggerEvent("ChangeWindowPattern", CWPValue);
		}

#if UNITY_EDITOR
		public void EditorUI()
		{
			CWPValue.PatternName = EditorGUILayout.DelayedTextField("Position: ", CWPValue.PatternName);
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Change the positions of the panel windows when talking to somebody.", MessageType.Info, true);
		}
#endif
	}
}
