// Dialogue Player Script by SeleniumSoul for DREditor (SeleniumSoul Version)
// Updated at 08/21/2021
// Handles all events for dialogues. Class Trial is not supported at this time.

// If desired, this class' Debug Logs should be <color=blue> for color-coding.

using DREditor.Characters;
using DREditor.Dialogues.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Component used for dialogue rendering. Use this as a component on the main SYSTEM gameObject.
/// </summary>
namespace DREditor.Dialogues
{
	public class DialoguePlayer : MonoBehaviour
	{
		public static DialoguePlayer current;
		public static Action DialogueStarted;
		public static Action DialogueEnded;

		public bool PropMode = false;

		public float AutoWaitTime = 1f;
		private float SkipWaitTime = 0.4f;

		public DialogueBoxManager SayBoxManager;
		public GameObject DiaPanels, DRCursor;
		public GameObject CurrentCharacter;

		[SerializeField] private AudioClip FlashSFX;

		private UnityEngine.Camera CurrentMainCamera;
		private DRDialogueBox SayBox;

		private Coroutine AutoC;

        private CharacterCluster CurrentCluster;
        private Dictionary<bool, GameObject> TemporaryLoadedCharacters;

		private InputAction confirmAction;
		private InputAction cancelAction;
		private InputAction autoAction;
		private InputAction skipAction;
        private void Awake()
		{
			if (current == null) current = this;
			else Destroy(this);

			if (!CurrentMainCamera) CurrentMainCamera = UnityEngine.Camera.main;

			confirmAction = InputSystem.actions.FindAction("Confirm");
			cancelAction = InputSystem.actions.FindAction("Cancel");
			autoAction = InputSystem.actions.FindAction("Auto");
			skipAction = InputSystem.actions.FindAction("Skip");

			confirmAction.started += ctx =>
			{
				if (DialogueRuntimeData._skip || DialogueRuntimeData._auto)
				{
					DialogueRuntimeData._skip = false;
					DialogueRuntimeData._auto = false;
					if (AutoC != null)
					{
						StopCoroutine(AutoC);
					}
					SayBox.Auto(DialogueRuntimeData._auto);
					return;
				}

				OnConfirm();
			};
			cancelAction.performed += ctx =>
			{
				if (DialogueRuntimeData.InDialog && !DialogueRuntimeData.InMenu && DialogueRuntimeData.Advance)
				{
					if (DialogueRuntimeData._auto) DialogueRuntimeData._auto = false;
					DialogueRuntimeData._skip = !DialogueRuntimeData._skip;
					SayBox.Auto(DialogueRuntimeData._skip);

					if (DialogueRuntimeData._skip)
					{
						AutoC = StartCoroutine(AutoCoroutine(SkipWaitTime));
					}
					else
					{
						if (AutoC != null)
						{
							StopCoroutine(AutoC);
						}
					}
				}
			};
			skipAction.performed += ctx =>
			{
				if (DialogueRuntimeData._skip)
				{
					DialogueRuntimeData._skip = false;
					DialogueRuntimeData._auto = false;
					return;
				}

				if (DialogueRuntimeData.InDialog && !DialogueRuntimeData.InMenu && DialogueRuntimeData.Advance)
				{
					DialogueRuntimeData._auto = !DialogueRuntimeData._auto;
					SayBox.Auto(DialogueRuntimeData._auto);

					if (DialogueRuntimeData._auto)
					{
						AutoC = StartCoroutine(AutoCoroutine(AutoWaitTime));
					}
					else
					{
						if (AutoC != null)
						{
							StopCoroutine(AutoC);
						}
					}
				}
			};
		}

		private void Start()
		{
			if (!CurrentMainCamera) CurrentMainCamera = UnityEngine.Camera.main;
		}

		private void OnEnable()
		{
			DialogueEventSystem.StartListening("StartDialogue", StartDialogue);
			DialogueEventSystem.StartListening("ContinueDialogue", ContinueDialogue);
			DialogueEventSystem.StartListening("LinePass", ProcessLine);

			//Special Events
			DialogueEventSystem.StartListening("ShowCG", ShowCG);
			DialogueEventSystem.StartListening("CharacterLeave", LeaveSequence);
		}

		private void OnDisable()
		{
			DialogueEventSystem.StopListening("StartDialogue", StartDialogue);
            DialogueEventSystem.StopListening("ContinueDialogue", ContinueDialogue);
            DialogueEventSystem.StopListening("LinePass", ProcessLine);

			//Special Events
			DialogueEventSystem.StopListening("ShowCG", ShowCG);
			DialogueEventSystem.StopListening("CharacterLeave", LeaveSequence);
		}

		public void OnConfirm()
		{
			if (DialogueRuntimeData.InDialog && !DialogueRuntimeData.InMenu && DialogueRuntimeData.Advance && !DialogueRuntimeData.OnTransition)
			{
				if (!DialogueRuntimeData.IsWriting)
				{
					if (!DialogueRuntimeData._auto)
					{
						ContinueDialogue();
					}
					else
					{
						DialogueRuntimeData._auto = false;
						SayBox.Auto(DialogueRuntimeData._auto);

						if (AutoC != null)
						{
							StopCoroutine(AutoC);
						}
					}
				}
			}
		}

		private void CamNullCheck()
		{
			if (!CurrentMainCamera) CurrentMainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<UnityEngine.Camera>();
		}

		private void StartDialogue(object value = null)
		{
			CamNullCheck();
			if (DRCursor) DRCursor.SetActive(false);

			//DialogueHandler.Skippable = true;
			DialogueRuntimeData.InDialog = true;

			DialogueRuntimeData._dialogueLength = DialogueRuntimeData.DialogueAsset.Lines.Count;
			DialogueRuntimeData._currentLineNum = 0;

			switch ((int)DialogueRuntimeData.DialogueAsset.DialogueMode)
			{
				case 0: //Normal
					SayBox = SayBoxManager.DialogueBox.GetComponent<DRDialogueBox>();
					if (DialogueRuntimeData.DialogueAsset.ShowDialoguePanel) // With Dialogue Panels
					{
						//CurrentMainCamera.enabled = false;
						if (DiaPanels) DiaPanels.SetActive(true);
						//DialogueEventSystem.TriggerEvent("DiaPanelInitialize");
					}
					else // Without Dialogue Panels
					{
						SayBox.gameObject.SetActive(true);
						DialogueRuntimeData.Advance = true;
						DialogueEventSystem.TriggerEvent("LinePass", DialogueRuntimeData._currentLineNum);
					}
					break;
				case 1: //CG - without Dialogue Panels
					SayBox = SayBoxManager.DialogueBox_CG.GetComponent<DRDialogueBox>();
					DialogueRuntimeData.OnTransition = false;
					SayBox.gameObject.SetActive(true);
					DialogueEventSystem.TriggerEvent("LinePass", DialogueRuntimeData._currentLineNum);
					break;
				default:
					Debug.Log("DREditor (DialoguePlayer): Unable to identify dialogue mode. Defaulting to No Panel.");
					SayBox = SayBoxManager.DialogueBox.GetComponent<DRDialogueBox>();
					SayBox.gameObject.SetActive(true);
					DialogueEventSystem.TriggerEvent("LinePass", DialogueRuntimeData._currentLineNum);
					break;
			}

			if (DialogueRuntimeData.DialogueAsset.StartEvents.Count > 0)
			{
				foreach (var ev in DialogueRuntimeData.DialogueAsset.StartEvents)
				{
					ev.TriggerDialogueEvent();
				}
			}
		}

		private void ContinueDialogue()
		{
			DialogueRuntimeData._currentLineNum++;

			if (DialogueRuntimeData._currentLineNum <= DialogueRuntimeData._dialogueLength - 1)
			{
				EnableDiaPanels();

				ProcessLine(DialogueRuntimeData._currentLineNum);

				if (DialogueRuntimeData._auto && !DialogueRuntimeData._currentLine.Text.Contains("[WAIT]"))
				{
					AutoC = StartCoroutine(AutoCoroutine(AutoWaitTime));
				}
				else if (DialogueRuntimeData._skip && !DialogueRuntimeData._currentLine.Text.Contains("[WAIT]"))
				{
					AutoC = StartCoroutine(AutoCoroutine(SkipWaitTime));
				}
			}
			else
			{
				if (DialogueRuntimeData.DialogueAsset.DirectTo.Enabled)
				{
					DirectTo();

					if (DialogueRuntimeData._auto && !DialogueRuntimeData._currentLine.Text.Contains("[WAIT]"))
					{
						AutoC = StartCoroutine(AutoCoroutine(AutoWaitTime));
					}
					else if (DialogueRuntimeData._skip && !DialogueRuntimeData._currentLine.Text.Contains("[WAIT]"))
					{
						AutoC = StartCoroutine(AutoCoroutine(SkipWaitTime));
					}
				}
				else if (DialogueRuntimeData.DialogueAsset.Choices.Count != 0)
				{
					//Insert Choice code here.
				}
				else
				{
					EndDialogue();
				}
			}
		}

		private void ProcessLine(object _DPlinenum)
		{
			bool _firstLine = false;
			DialogueRuntimeData._currentLine = DialogueRuntimeData.DialogueAsset.Lines[(int)_DPlinenum];

			string charname = "";
			Sprite nameplate = null;

			if (DialogueRuntimeData._currentLine.Speaker != null)
			{
				charname = DialogueRuntimeData._currentLine.Speaker.FirstName + " " + DialogueRuntimeData._currentLine.Speaker.LastName;
				nameplate = DialogueRuntimeData._currentLine.Speaker.Nameplate;

				Debug.Log("DREditor (DialoguePlayer): Processing " + charname + "'s line.");
				if ((int)DialogueRuntimeData.DialogueAsset.DialogueMode != 1 && !PropMode)
				{
					CurrentCharacter = GameObject.Find(charname);
					//DRSB = CurrentCharacter.GetComponent<DRSpriteBillboard>();
					//DRBP = CurrentCharacter.GetComponent<DRBillboardProperties>();
				}
			}

			//Enable DialogueBox
			if (SayBox.gameObject.activeSelf == false)
			{
				SayBox.gameObject.SetActive(true);
				_firstLine = true;
			}

			//Voices
			if (DialogueRuntimeData._currentLine.VoiceSFX != null && !DialogueRuntimeData._skip)
			{
				DialogueEventSystem.TriggerEvent("PlayVoice", DialogueRuntimeData._currentLine.VoiceSFX);
			}

			//SFX
			if (DialogueRuntimeData._currentLine.SFX.Count > 0 && !DialogueRuntimeData._skip)
			{
				foreach (AudioClip aud in DialogueRuntimeData._currentLine.SFX)
				{
					if (aud != null) DialogueEventSystem.TriggerEvent("PlaySFX", aud);
				}
			}

			//Dialogue Events
			if (DialogueRuntimeData._currentLine.DiaEvents.Count > 0)
			{
				for (int d = 0; d < DialogueRuntimeData._currentLine.DiaEvents.Count; d++)
				{
					DialogueRuntimeData._currentLine.DiaEvents[d].TriggerDialogueEvent();
				}
			}

			//Sprites
			if (DialogueRuntimeData._currentLine.Expression?.Sprite != null)
			{
                DialogueEventSystem.TriggerEvent("ChangeExpression", (DialogueRuntimeData._currentLine.Speaker.FullName(false), DialogueRuntimeData._currentLine.Expression?.Sprite));
				//if (DRSB) DRSB.ChangeExpression(DialogueHandler._currentLine.Expression?.Sprite);
			}

			//If there's no text inside the line, just do the dialogue events and automatically go to the next line.
			if (DialogueRuntimeData._currentLine.Text == "")
			{
				ContinueDialogue();
				return;
			}
			else if (DialogueRuntimeData._currentLine.Text.Contains("[WAIT]"))
			{
				return;
			}

			//Dialogue Text
			if (DialogueRuntimeData._currentLine.AliasNumber > 0 && DialogueRuntimeData._currentLine.Speaker != null)
			{
				charname = DialogueRuntimeData._currentLine.Speaker.Aliases[DialogueRuntimeData._currentLine.AliasNumber - 1].Name;
				nameplate = DialogueRuntimeData._currentLine.Speaker.Aliases[DialogueRuntimeData._currentLine.AliasNumber - 1].Nameplate;
			}

			SayBox.UseText(charname, nameplate, DialogueRuntimeData._currentLine.Text, _firstLine);

			//Pass Dialogue Lines to a Backlog File
			if (DialogueRuntimeData.BLFile) DialogueRuntimeData.BLFile.AddLine(DialogueRuntimeData._currentLine.Speaker, DialogueRuntimeData._currentLine.Text, DialogueRuntimeData._currentLine.VoiceSFX);
		}

		private void EndDialogue()
		{
			Debug.Log("DREditor (DialoguePlayer): Ending Dialogue");

			CurrentMainCamera.enabled = true;
			DialogueRuntimeData._currentLineNum = 0;
			DialogueRuntimeData._dialogueLength = 0;

			DialogueRuntimeData.Advance = false;
			DialogueRuntimeData.Skippable = false;

			DialogueRuntimeData._skip = false;
			DialogueRuntimeData._auto = false;

			if (!DiaPanels || DiaPanels.activeSelf == false)
			{
				DialogueRuntimeData.InDialog = false;
				SayBox.gameObject.SetActive(false);
				StartCoroutine(Waiting());
				DialogueEventSystem.TriggerEvent("DisableCharacterHighlight");
			}
			else
			{
				DialogueEventSystem.TriggerEvent("DiaPanelTerminate");
				SayBox.gameObject.SetActive(false);
			}

			CurrentCharacter = null;

			DialogueEnded?.Invoke();
			Resources.UnloadUnusedAssets();
		}

		private void DirectTo()
		{
			SayBox.gameObject.SetActive(false);

			DialogueRuntimeData._currentLineNum = DialogueRuntimeData.DialogueAsset.DirectTo.NewDialogueIndex;
			DialogueRuntimeData.DialogueAsset = DialogueRuntimeData.DialogueAsset.DirectTo.NewDialogue;

			DialogueRuntimeData._dialogueLength = DialogueRuntimeData.DialogueAsset.Lines.Count;

			switch ((int)DialogueRuntimeData.DialogueAsset.DialogueMode)
			{
				case 0:
					SayBox = SayBoxManager.DialogueBox.GetComponent<DRDialogueBox>();
					break;
				case 1:
					SayBox = SayBoxManager.DialogueBox_CG.GetComponent<DRDialogueBox>();
					break;
				default:
					SayBox = SayBoxManager.DialogueBox.GetComponent<DRDialogueBox>();
					break;
			}

			SayBox.gameObject.SetActive(true);

			EnableDiaPanels();
			ProcessLine(DialogueRuntimeData._currentLineNum);
		}

		private IEnumerator Waiting()
		{
			yield return new WaitForSeconds(0.1f);
			if (DRCursor) DRCursor.SetActive(true);
			yield break;
		}

		private IEnumerator AutoCoroutine(float WaitTime)
		{
			float _linelength = DialogueRuntimeData._currentLine.Text.Length;
			float _waitTime;

			if (WaitTime >= 0.5f)
			{
				_waitTime = (_linelength * 0.05f * 2f) + WaitTime;
				_waitTime = Mathf.Clamp(_waitTime, 2f, 100f);
			}
			else
			{
				_waitTime = WaitTime * Time.deltaTime * 50f;
			}

			Debug.Log($"Dialogue will wait for {_waitTime} seconds.");
			yield return new WaitForSeconds(_waitTime);

			ContinueDialogue();

			yield break;
		}

		private void EnableDiaPanels()
		{
			if (DiaPanels.activeSelf == false && (int)DialogueRuntimeData.DialogueAsset.DialogueMode == 0 && DialogueRuntimeData.DialogueAsset.ShowDialoguePanel)
			{
				DiaPanels.GetComponent<DRDialoguePanels>().Immediate = true;
				CurrentMainCamera.enabled = false;
				DiaPanels.SetActive(true);
			}
		}

		//Dialogue Events

		/// <summary>
		/// Dialogue Event: Show a CG on the screen.
		/// </summary>
		/// <param name="values"></param>
		private void ShowCG(object values)
		{
			GameObject _CG = (GameObject)values;
			Instantiate(_CG);
		}

		/// <summary>
		/// Dialogue Event: Show a sequence where a character leaves in the middle of the conversation.
		/// </summary>
		/// <param name="values"></param>
		private void LeaveSequence(object values = null)
		{
			CharacterSpot _cs = CurrentCharacter.transform.parent.parent.GetComponent<CharacterSpot>();
			DialogueRuntimeData.Advance = false;

			DialogueRuntimeData._skip = false;
			DialogueRuntimeData._auto = false;

			if (!DiaPanels || DiaPanels.activeSelf == false)
			{
				SayBox.gameObject.SetActive(false);
				StartCoroutine(Waiting());
				DialogueEventSystem.TriggerEvent("DisableCharacterHighlight");
			}
			else
			{
				StartCoroutine(_LeaveSequence(_cs));
				SayBox.gameObject.SetActive(false);
			}
		}
		private IEnumerator _LeaveSequence(CharacterSpot _cs)
		{
			CurrentMainCamera.enabled = true;
			DialogueEventSystem.TriggerEvent("PanelActive", (false, _cs.CurrentCharacter));

			yield return new WaitForSeconds(3f);

			_cs.Disable();

			yield return new WaitForSeconds(1f);

			DialogueRuntimeData.Advance = true;
			ContinueDialogue();
			yield return null;
		}
	}
}