// Dialogue Player Script by SeleniumSoul for DREditor (SeleniumSoul Version)
// Updated at 08/21/2021
// Handles all events for dialogues. Class Trial is not supported at this time.

// If desired, this class' Debug Logs should be <color=blue> for color-coding.

using DREditor.Characters;
using DREditor.Dialogues.Events;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using DiaRunData = DREditor.Dialogues.DialogueRuntimeData;

/// <summary>
/// Component service used for dialogue rendering. Use this as a component on the main SYSTEM gameObject.
/// </summary>
namespace DREditor.Dialogues
{
	public class DialoguePlayer : MonoBehaviour, IModule
	{
		[Module] private ProjectwidePlayerInput playerInput;
		[SerializeField] private float AutoWaitTime = 1f;
		[SerializeField] private float SkipWaitTime = 0.4f;

		private Coroutine AutoRoutine;
		private CharacterCluster CurrentCluster;
		private Dictionary<bool, GameObject> TemporaryLoadedCharacters;

		[Header("Controls")]
		[SerializeField] private InputActionReference LookAction;
		[SerializeField] private InputActionReference AcceptAction;
		[SerializeField] private InputActionReference CancelAction;

		private GameObject currentCG;

		public Line GetLine() => DiaRunData.CurrentLine;
		public Character GetCurrentCharacter() => DiaRunData.CurrentLine.Speaker;

		public Task Init() { return Task.CompletedTask; }

		private void OnEnable()
		{
			DRModuleSystem.Inject(this);

			playerInput.RegisterInput(AcceptAction, OnConfirm);
            playerInput.RegisterInput(CancelAction, OnSkip);
            //playerInput.RegisterInput(AcceptAction, OnConfirm);
   //         AcceptAction.action.started += OnConfirm;
			//CancelAction.action.performed += OnSkip;
			//AcceptAction.action.actionMap.Enable();

            DiaRunData.DialogueStarted += StartDialogue;
            DialogueEventSystem.RegisterEvents(this);
        }

		private void OnDestroy()
		{
			//AcceptAction.action.started -= OnConfirm;
			//CancelAction.action.performed -= OnSkip;

			DiaRunData.DialogueStarted -= StartDialogue;
			DialogueEventSystem.DeregisterEvents(this);
		}

		public void OnConfirm(InputAction.CallbackContext ctx)
		{
			//Debug.Log($"DREditor: {AcceptAction.action.actionMap.enabled} iswriting:{DiaRunData.IsWriting} onauto: {DiaRunData.OnAuto} onskip: {DiaRunData.OnSkip}");
			if (!DiaRunData.InDialog || DiaRunData.InMenu || !DiaRunData.Advance)
				return;

			if (!DiaRunData.IsWriting)
			{
				if (DiaRunData.dialogueState != DiaRunData.DialogueState.Skip)
				{
                    DiaRunData.SetDialogueState(DiaRunData.DialogueState.Normal);

                    ContinueDialogue();
				}
				else
				{
					DiaRunData.SetDialogueState(DiaRunData.DialogueState.Normal);

					if (AutoRoutine != null)
						StopCoroutine(AutoRoutine);
				}
			}
			else
			{
				Debug.Log("Finishing Text.");
				_ = DialogueEventSystem.TriggerEvent("FinishText");
			}
		}
		public void OnSkip(InputAction.CallbackContext ctx)
		{
			//skip dialogue
			if (!DiaRunData.InDialog || DiaRunData.InMenu || !DiaRunData.Advance)
				return;

			if (DiaRunData.dialogueState != DiaRunData.DialogueState.Skip)
			{
				DiaRunData.SetDialogueState(DiaRunData.DialogueState.Skip);
				AutoRoutine = StartCoroutine(AutoCoroutine(SkipWaitTime, true));
			}
			else
			{
				DiaRunData.SetDialogueState(DiaRunData.DialogueState.Normal);
				if (AutoRoutine != null)
					StopCoroutine(AutoRoutine);
			}
		}

		private async void StartDialogue(Dialogue dialogueAsset)
		{
			Debug.Log("DREditor (DialoguePlayer): Starting Dialogue.");

			if (DiaRunData.DialogueAsset.StartEvents.Count > 0)
			{
				DiaRunData.Advance = false;
				Debug.Log("DREditor (DialoguePlayer): Triggering Start Events.");
				var diaevents = DiaRunData.DialogueAsset.StartEvents.Select(ev => ((IDialogueEvent)ev).TriggerEvent());
				await Task.WhenAll(diaevents);
				DiaRunData.Advance = true;
			}

			switch (dialogueAsset.DialogueMode)
			{
				case BoxMode.Normal: //Normal
					//if (dialogueAsset.ShowDialoguePanel) // With Dialogue Panels
					//{
					//	SetPanelsActive(true);
					//}
					//else // Without Dialogue Panels
					//{
						await DialogueEventSystem.TriggerEvent("LinePass", DiaRunData.CurrentLineIndex);
					//}
					break;
				case BoxMode.CG: //CG - without Dialogue Panels
					await DialogueEventSystem.TriggerEvent("LinePass", DiaRunData.CurrentLineIndex);
					break;
				default:
					Debug.Log("DREditor (DialoguePlayer): Unable to identify dialogue mode. Defaulting to No Panel.");
					await DialogueEventSystem.TriggerEvent("LinePass", DiaRunData.CurrentLineIndex);
					break;
			}
		}

		[DialogueEvent("ContinueDialogue")]
		public void ContinueDialogue()
		{
			var asset = DiaRunData.DialogueAsset;

			if (asset == null)
			{
                _ = DialogueEventSystem.TriggerEvent("EndDialogue");
                return;
            }

			DiaRunData.CurrentLineIndex++;

			if (DiaRunData.CurrentLineIndex <= DiaRunData.DialogueLength - 1)
			{
				//if (DiaRunData.DialogueAsset.DialogueMode == BoxMode.Normal)
				//	SetPanelsActive(true);

				ProcessLine(DiaRunData.CurrentLineIndex);

				//skip/wait autoadvance waittime
				SetAutoAdvance();
			}
			else
			{
				if (asset.DirectTo.Enabled)
				{
					DirectTo();
					SetAutoAdvance();
				}
				else if (asset.Choices.Count != 0)
				{
					//Insert Choice code here.
					DiaRunData.Advance = false;
					_ = DialogueEventSystem.TriggerEvent("ShowChoices", asset.Choices);
                }
				else
				{
					EndDialogue();
				}
			}
		}

		[DialogueEvent("LinePass")]
		public async void ProcessLine(int lineIndex)
		{
			Line line;
			try
			{
				line = DiaRunData.CurrentLine = DiaRunData.DialogueAsset.Lines[(int)lineIndex];
			}
			catch
			{
				return;
			}

			if (line.Speaker != null)
			{
				string charname = line.Speaker.FirstName + " " + line.Speaker.LastName;

				Debug.Log("DREditor (DialoguePlayer): Processing " + charname + $"'s line. \"{line.Text}\"");
			}

			//Enable DialogueBox
			//SetSayboxActive(true);

			//Voices
			if (line.VoiceSFX != null && !DiaRunData.OnSkip)
			{
				_ = DialogueEventSystem.TriggerEvent("PlayVoice", DiaRunData.CurrentLine.VoiceSFX);
			}

			//SFX
			if (line.SFX.Count > 0 && !DiaRunData.OnSkip)
			{
				foreach (AudioClip aud in DiaRunData.CurrentLine.SFX)
				{
					if (aud != null)
						_ = DialogueEventSystem.TriggerEvent("PlaySFX", aud);
				}
			}

			//Sprites
			if (line.Expression?.Sprite != null)
			{
				_ = DialogueEventSystem.TriggerEvent("ChangeExpression", (line.Speaker.FullName(false), line.Expression?.Sprite));
			}


			//If there's no text inside the line, just do the dialogue events and automatically go to the next line.
			if (line.Text == "")
			{
                _ = Task.WhenAll(line.DiaEvents.Select(ev => ((IDialogueEvent)ev).TriggerEvent()));
                ContinueDialogue();
				return;
			}
			else if (line.Text.Contains("[WAIT]"))
			{
				if (line.DiaEvents.Count > 0)
				{
                    DiaRunData.Advance = false;
                    Debug.Log("Evoking [WAIT] Dialogue Events");
					await Task.WhenAll(line.DiaEvents.Select(ev => ((IDialogueEvent)ev).TriggerEvent()));
					Debug.Log("Continuing Dialogue");
                    DiaRunData.Advance = true;
                    ContinueDialogue();
                }
				return;
			}

			//Dialogue Events
			if (line.DiaEvents.Count > 0)
			{
				Debug.Log("Evoking Dialogue Events");
				await Task.WhenAll(line.DiaEvents.Select(ev => ((IDialogueEvent)ev).TriggerEvent()));
            }

            DiaRunData.Advance = true;

            //Dialogue Text
			_ = DialogueEventSystem.TriggerEvent<Line>("PrintDialogue", line);

			//Pass Dialogue Line to history
			DiaRunData.AddHistory(line);
		}

		private void EndDialogue()
		{
			(bool, CharacterSpot) panel = new(false, null);

			Debug.Log("DREditor (DialoguePlayer): Ending Dialogue");

			DiaRunData.EndDialogue();

			SetPanelsActive(false);
			SetSayboxActive(false);

			//CurrentCharacter = null;
			Resources.UnloadUnusedAssets();
		}

		private void DirectTo()
		{
			var directTo = DiaRunData.DialogueAsset.DirectTo;

			DiaRunData.ContinueWithNewDialogue(directTo.NewDialogue);
			ProcessLine(DiaRunData.CurrentLineIndex);
		}

		private IEnumerator AutoCoroutine(float WaitTime, bool forceTime = false)
		{
			float _linelength = DiaRunData.CurrentLine.Text.Length;
			float _waitTime = WaitTime;

			if (!forceTime)
			{
				if (WaitTime >= 0.5f)
				{
					_waitTime = (_linelength * 0.05f * 2f) + WaitTime;
					_waitTime = Mathf.Clamp(_waitTime, 2f, 100f);
				}
				else
				{
					_waitTime = WaitTime * Time.deltaTime * 50f;
				}
			}

			Debug.Log($"Dialogue will wait for {_waitTime} seconds.");
			yield return new WaitForSeconds(_waitTime);

			ContinueDialogue();

			yield break;
		}

		//private async Task AutoCoroutine(float WaitTime)
		//{
		//	float _linelength = DiaRunData.CurrentLine.Text.Length;
		//	float _waitTime;

		//	if (WaitTime >= 0.5f)
		//	{
		//		_waitTime = (_linelength * 0.05f * 2f) + WaitTime;
		//		_waitTime = Mathf.Clamp(_waitTime, 2f, 100f);
		//	}
		//	else
		//	{
		//		_waitTime = WaitTime * Time.deltaTime * 50f;
		//	}

		//	Debug.Log($"Dialogue will wait for {_waitTime} seconds.");
		//	await Awaitable.WaitForSecondsAsync(_waitTime);

		//	ContinueDialogue();
		//}

		private void SetPanelsActive(bool active)
		{
			_ = DialogueEventSystem.TriggerEvent("PanelActive", active);
		}

		private void SetSayboxActive(bool active) => 
			_ = DialogueEventSystem.TriggerEvent<bool>("SetSayboxActive", active);

		private void SetAutoAdvance()
		{
			if (DiaRunData.CurrentLine.Text.Contains("[WAIT]"))
				return;

			var state = DiaRunData.dialogueState;
			float waitTime;
			bool forceTime = false;

			switch (state)
			{
				case DiaRunData.DialogueState.Skip:
					waitTime = SkipWaitTime;
					forceTime = true;
					break;
				case DiaRunData.DialogueState.Auto:
					waitTime = AutoWaitTime;
					break;
				default:
					waitTime = 1f;
					return;
			}

			AutoRoutine = StartCoroutine(AutoCoroutine(waitTime, forceTime));
		}

		//Dialogue Events
		/// <summary>
		/// Dialogue Event: Show a CG on the screen.
		/// </summary>
		/// <param name="values"></param>
		[DialogueEvent("ShowCG")]
		private async Task ShowCG(GameObject values)
		{
			//GameObject _CG = (GameObject)values;

			if (currentCG != null)
				Destroy(currentCG);

			currentCG = Instantiate(values);

			if (currentCG.TryGetComponent<ICG>(out var cg))
			{
				await cg.Initialize();
			}
		}

		/// <summary>
		/// Dialogue Event: Show a sequence where a character leaves in the middle of the conversation.
		/// </summary>
		/// <param name="values"></param>
		private void LeaveSequence(object values = null)
		{
			//CharacterSpot _cs = CurrentCharacter.transform.parent.parent.GetComponent<CharacterSpot>();
			DiaRunData.Advance = false;

			DiaRunData.SetDialogueState(DiaRunData.DialogueState.Normal);

			SetSayboxActive(true);
			SetPanelsActive(false);

			//StartCoroutine(Waiting());
			//StartCoroutine(LeaveSequenceRoutine(_cs));
		}
		private IEnumerator LeaveSequenceRoutine(CharacterSpot _cs)
		{
			yield return new WaitForSeconds(3f);

			_cs.Disable();

			yield return new WaitForSeconds(1f);

			ContinueDialogue();
			yield return null;
		}


	}
}