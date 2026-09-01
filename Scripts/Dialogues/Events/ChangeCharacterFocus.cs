//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;
using DREditor.Characters;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct CCFTuple
	{
		public string ID;
		public int PanelNum;
		public Vector3 CamTrans;
	}

	[Serializable]
	public class ChangeCharacterFocus : IDialogueEvent
	{
		public bool _ShowHelp = false;
		public CCFTuple CCFValue;

		public void TriggerDialogueEvent()
		{
			(string, int, Vector3) values;
			values = (CCFValue.ID, CCFValue.PanelNum, CCFValue.CamTrans);

			DialogueEventSystem.TriggerEvent("ChangeCharacterFocus", values);
		}

#if UNITY_EDITOR
		public void EditorUI()
		{
			CCFValue.ID = EditorGUILayout.DelayedTextField("Focus ID:", CCFValue.ID);
			CCFValue.PanelNum = EditorGUILayout.IntPopup("Panel Camera:", CCFValue.PanelNum, new string[] { "Middle Panel", "Right Panel", "Left Panel" }, new int[] { 0, 1, 2 });
			CCFValue.CamTrans = EditorGUILayout.Vector3Field("Camera Position Offset (Optional):", CCFValue.CamTrans);
		}

		public void ToggleHelpBox()
		{
			_ShowHelp = !_ShowHelp;
		}

		public void ShowHelpBox()
		{
			if (_ShowHelp) EditorGUILayout.HelpBox("Choose which panel would change its focused character.\nNote: Changing the Camera Position values to non-zero would force its camera transform.", MessageType.Info, true);
		}
#endif
	}
}