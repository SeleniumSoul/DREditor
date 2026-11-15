//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;
using System.Threading.Tasks;


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
		public bool Immediate;
	}

	[Serializable]
	public class ChangeCharacterFocus : IDialogueEvent
	{
		public CCFTuple CCFValue;

		public async Task TriggerEvent()
		{
			(string, int, Vector3, bool) values;
			values = (CCFValue.ID, CCFValue.PanelNum, CCFValue.CamTrans, CCFValue.Immediate);
			await DialogueEventSystem.TriggerEvent("ChangeCharacterFocus", values);
		}

#if UNITY_EDITOR
        private bool showHelp = false;
        public void EditorUI()
		{
			CCFValue.ID = EditorGUILayout.DelayedTextField("Focus ID:", CCFValue.ID);
			CCFValue.PanelNum = EditorGUILayout.IntPopup("Panel Camera:", CCFValue.PanelNum, new string[] { "Middle Panel", "Right Panel", "Left Panel" }, new int[] { 0, 1, 2 });
			CCFValue.CamTrans = EditorGUILayout.Vector3Field("Camera Position Offset (Optional):", CCFValue.CamTrans);
            CCFValue.Immediate = EditorGUILayout.Toggle("Force Immediate:", CCFValue.Immediate);
		}

		public void ToggleHelpBox()
		{
			showHelp = !showHelp;
		}

		public void ShowHelpBox()
		{
			if (showHelp)
				EditorGUILayout.HelpBox("Choose which panel would change its focused character.\nNote: Changing the Camera Position values to non-zero would force its camera transform.", MessageType.Info, true);
		}
#endif
	}
}