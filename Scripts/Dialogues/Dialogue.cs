using System;
using System.Collections.Generic;
using UnityEngine;

using DREditor.Characters;
using DREditor.Dialogues.Events;

namespace DREditor.Dialogues
{
	/// <summary>
	/// Identifies if the dialogue will be used normally, CG, or in Trial. Used primarily for the game to know what type of dialogue screen to use.
	/// </summary>
	public enum BoxMode
	{
		Normal,
		CG,
		Trial
	}

	public abstract class DialogueBase : ScriptableObject
	{
		public BoxMode DialogueMode;
		public string translationKey;
		public string DialogueName = "";
		public Color Color = Color.white;
		public bool ShowDialoguePanel = true;
		public Variable Variable = new Variable();
		public DirectTo DirectTo;
		public SceneTransition SceneTransition = new SceneTransition();

#if UNITY_EDITOR
		[HideInInspector]
		public CharacterDatabase Speakers;
		
		public string[] GetCharacterNames()
		{
			if (Speakers == null)
			{
				Speakers = Resources.Load<CharacterDatabase>("Characters/CharacterDatabase");

			}
			return Speakers?.GetNames().ToArray();
		}

		public Alias[] GetCharacterAliases(Character cha)
		{
			return cha.Aliases.ToArray();
		}

		public int[] GetNamesIntValues()
		{
			int[] values = new int[Speakers.GetNames().ToArray().Length];
			for (int i = 0; i < values.Length; i++)
			{
				values[i] = i;
			}
			return values;
		}

		public int[] GetExpressionIntValues(Character cha)
		{

			int[] values = new int[cha.Expressions.Count + 1];
			for (int i = 0; i < values.Length; i++)
			{
				values[i] = i;
			}
			return values;
		}

		public int[] GetAliasesIntValues(Character cha)
		{
			int[] values = new int[cha.Aliases.Count + 1];
			for (int i = 0; i < values.Length; i++)
			{
				values[i] = i;
			}
			return values;
		}
#endif
	}

	[Serializable]
	[CreateAssetMenu(menuName = "DREditor/Dialogues/Dialogue", fileName = "New Dialogue")]
	public class Dialogue : DialogueBase
	{
		public List<Line> Lines = new List<Line>();
		public List<Choice> Choices = new List<Choice>();
		[SerializeReference] public List<IDialogueEvent> StartEvents = new List<IDialogueEvent>();

		public List<Character> GetAllCharactersInDialogue()
		{
			var result = new List<Character>();

			foreach (Line line in Lines)
			{
				if (!result.Contains(line.Speaker))
					result.Add(line.Speaker);
			}

			return result;
		}
	}

	[Serializable]
	public class Line
	{
		public string translationKey;
		public Character Speaker;
		public int SpeakerNumber;
		public string Text;
		public AudioClip VoiceSFX;
		public List<AudioClip> SFX = new List<AudioClip>();
		[SerializeReference] public List<IDialogueEvent> DiaEvents = new();
		public float TimeToNextLine;
		public bool AutomaticLine;
		public Expression Expression;
		public int ExpressionNumber;
		public int AliasNumber;

		#if UNITY_EDITOR
		public bool ShowDiaEvents;
		#endif
	}

	[Serializable]
	public class Choice
	{
		public string ChoiceText;
		public Dialogue NextDialogue;
		public int NextIndexInDialogue;

        [SerializeReference] public List<IDialogueEvent> DiaEvents = new();
    }

	[Serializable]
	public class Variable
	{
		public bool Enabled;
		public Dialogue NextDialogueTrue;
		public int NextIndexInDialogueTrue;
		public Dialogue NextDialogueFalse;
		public int NextIndexInDialogueFalse;
	}

	[Serializable]
	public class DirectTo
	{
		public bool Enabled;
		public Dialogue NewDialogue;
		public int NewDialogueIndex;
	}

	[Serializable]
	public class SceneTransition
	{
		public bool Enabled;
		public string Scene;
	}
}
