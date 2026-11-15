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
	public struct FTBTuple
	{
		public ScreenTransitions.FadeChoices FadeChoice;
		public float FadeSpeed;
	}

	[Serializable]
	public class ScreenTransitions : IDialogueEvent
	{
        public enum FadeChoices { FadeIn, FadeOut }
        public FTBTuple FTBValue;

		public async Task TriggerEvent()
		{
			switch (FTBValue.FadeChoice)
			{
				case FadeChoices.FadeIn:
					await DialogueEventSystem.TriggerEvent("FadeOut", FTBValue.FadeSpeed);
					break;
				case FadeChoices.FadeOut:
					await DialogueEventSystem.TriggerEvent("FadeToBlack", FTBValue.FadeSpeed);
					break;
				default:
					Debug.LogError("DREditor (ScreenTransitions): Unable to recognize transition! Please check the transition option in the Dialogue Event if it is empty.");
					break;
			}
		}

#if UNITY_EDITOR
        private bool showHelp = false;
        public void EditorUI()
		{
			FTBValue.FadeChoice = (FadeChoices)EditorGUILayout.EnumPopup(new GUIContent("Transition", "Choose which transition to do."), FTBValue.FadeChoice);
			FTBValue.FadeSpeed = EditorGUILayout.DelayedFloatField(new GUIContent("At what speed? (in seconds)", "Time in seconds in which the transition will play."), FTBValue.FadeSpeed);
		}
		public void ToggleHelpBox()
		{
			showHelp = !showHelp;
		}

		public void ShowHelpBox()
		{
			if (showHelp) EditorGUILayout.HelpBox("Make the whole screen fade to black.", MessageType.Info, true);
		}
#endif
	}
}
