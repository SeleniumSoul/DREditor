//Dialogue Handler script by SeleniumSoul for DR:Distrust

using DREditor.Dialogues.Events;
using System;
using System.Collections.Generic;

namespace DREditor.Dialogues
{
	/// <summary>
	/// Houses all the global parameters in a dialogue that can be used by multiple classes. DO NOT USE BEHAVIORS IN THIS. I'm looking at you, old me.
	/// </summary>
	public static class DialogueRuntimeData
	{
		private const int LineHistorySize = 50;

		public enum DialogueState
		{
			Normal, Skip, Auto
		}

		public static event Action<Dialogue> DialogueStarted;
		public static event Action DialoguePaused;
		public static event Action DialogueEnded;
		public static event Action<DialogueState> OnChangeState;

		static public bool InDialog { get; private set; }
		[Obsolete("To be migrated to a different state script.")]
		static public bool InMenu;
		/// <summary> Marks the line to auto advance. Useful for transitions/waiting for animation to finish.</summary>
		static public bool Advance;
		/// <summary> Marks the line to be skippable. Useful for skipping.</summary>
		static public bool Skippable;
		static public bool IsWriting;
		static public bool OnTransition;

		static public CharacterCluster CurrentCluster;
		static public Dialogue DialogueAsset { get; private set; }
		[NonSerialized] static public int DialogueLength;
		[NonSerialized] static public int CurrentLineIndex;
		[NonSerialized] static public Line CurrentLine;

		static public DialogueState dialogueState { get; private set; }
		static public Queue<Line> LineHistory { get; private set; } = new();

		public static bool OnSkip => dialogueState == DialogueState.Skip;
		public static bool OnAuto => dialogueState == DialogueState.Auto;
		public static void SetDialogueState(DialogueState state)
		{
			dialogueState = state;
			OnChangeState?.Invoke(state);
		}

		static DialogueRuntimeData()
		{
            Initialize();
        }

		public static void Initialize()
		{
			DialogueEventSystem.RegisterEvents(typeof(DialogueRuntimeData));
		}

		[DialogueEvent("StartDialogue")]
		public static void StartDialogue(Dialogue asset)
		{
			InDialog = true;
			Advance = false;
			Skippable = false;
			IsWriting = false;

			SetDialogueAsset(asset);
			DialogueStarted?.Invoke(asset);
		}

		public static void ContinueWithNewDialogue(Dialogue asset)
		{
			SetDialogueAsset(asset);
		}

		[DialogueEvent("EndDialogue")]
		public static void EndDialogue()
		{
			InDialog = false;
			Advance = false;
			Skippable = false;
			IsWriting = false;
			OnTransition = false;
			dialogueState = DialogueState.Normal;

			SetDialogueAsset(null);
			DialogueEnded?.Invoke();
		}

		public static void AddHistory(Line line)
		{
			while (LineHistory.Count >= LineHistorySize)
				LineHistory.Dequeue();

			LineHistory.Enqueue(line);
		}

		private static void SetDialogueAsset(Dialogue asset, int index = 0)
		{
			DialogueAsset = asset;
			DialogueLength = DialogueAsset != null ? DialogueAsset.Lines.Count : 0;
			CurrentLineIndex = index;
			CurrentLine = null;
		}
	}
}