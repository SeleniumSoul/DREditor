//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace DREditor.Dialogues.Events
{
	[Serializable]
	public struct CWPTuple
	{
		public string PatternName;
		public int _ParameterIndex;
	}

	[Serializable]
	public class ChangeWindowPattern : IDialogueEvent
	{
		public CWPTuple CWPValue;
		public bool hidePanels = false;

		public async Task TriggerEvent()
		{
			if (!hidePanels)
				await DialogueEventSystem.TriggerEvent("ChangeWindowPattern", CWPValue);
			else
				await DialogueEventSystem.TriggerEvent("PanelActive", false);
		}

		#if UNITY_EDITOR
        public bool showHelp = false;
        private AnimatorController panelAnimator;
		private string[] parameters;

		public void EditorUI()
		{
			if (panelAnimator == null)
			{
				string animPath = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("DialoguePanels t:AnimatorController")[0]);
				panelAnimator = AssetDatabase.LoadAssetAtPath<AnimatorController>(animPath);
			}
			panelAnimator = (AnimatorController)EditorGUILayout.ObjectField("Animator", panelAnimator, typeof(AnimatorController), true);
            hidePanels = EditorGUILayout.Toggle(new GUIContent("Hide", "Hide the dialogue panels."), hidePanels);

            if (panelAnimator)
			{
				if (hidePanels)
					return;

                parameters = GetAnimatorParameters();
                CWPValue._ParameterIndex = EditorGUILayout.Popup("Position", CWPValue._ParameterIndex, parameters);
                CWPValue.PatternName = parameters[CWPValue._ParameterIndex];
                EditorGUILayout.LabelField($"Showing {CWPValue.PatternName} at this line.");
			}
			else
			{
				CWPValue.PatternName = EditorGUILayout.DelayedTextField("Position", CWPValue.PatternName);
				EditorGUILayout.HelpBox("Please link the AnimatorController of your Dialogue Panel on the Animator Field. Or if unavailable, specify the parameter in the field above.", MessageType.Warning, true);
			}
		}

		public void ToggleHelpBox()
		{
			showHelp = !showHelp;
		}

		public void ShowHelpBox()
		{
			if (showHelp) EditorGUILayout.HelpBox("Change the positions of the panel windows when talking to somebody.", MessageType.Info, true);
		}

		private string[] GetAnimatorParameters()
		{
			int indexAdjust = 2;
			string[] parameterlist = new string[panelAnimator.parameters.Length - indexAdjust];

			for (int i = indexAdjust; i < panelAnimator.parameters.Length; i++)
			{
				parameterlist[i - indexAdjust] = panelAnimator.parameters[i].name;
			}

			return parameterlist;
		}
		#endif
	}
}
