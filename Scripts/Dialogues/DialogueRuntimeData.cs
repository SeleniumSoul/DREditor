//Dialogue Handler script by SeleniumSoul for DR:Distrust

using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Houses all the global parameters in a dialogue that can be used by multiple classes.
/// </summary>
namespace DREditor.Dialogues
{
	public static class DialogueRuntimeData
	{
		public static Action DialogueStarted;
		public static Action DialoguePaused;
		public static Action DialogueStopped;

		static public bool InDialog;
		static public bool InMenu;
		static public bool Advance;
		static public bool Skippable;
		static public bool IsWriting;
		static public bool OnTransition;

		static public bool _skip;
		static public bool _auto;

		static public Backlog BLFile;

		static public CharacterCluster CurrentCluster;
		[NonSerialized] static public Dialogue DialogueAsset;
		[NonSerialized] static public int _dialogueLength;
		[NonSerialized] static public int _currentLineNum;
		[NonSerialized] static public Line _currentLine;

		public static void StartDialogue()
		{
            InDialog = true;
            Advance = false;
            Skippable = false;
            IsWriting = false;
        }

		public static void EndDialogue()
        {
			InDialog = false;
			Advance = false;
			Skippable = false;
			IsWriting = false;
			OnTransition = false;
        }
	}
}