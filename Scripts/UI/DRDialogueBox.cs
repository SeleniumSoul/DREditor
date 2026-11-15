//DialogueBox script by SeleniumSoul for DR:Distrust

using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using System;
using DREditor;

/// <summary>
/// UI Script that handles the dialogue box for the story text.
/// </summary>
public class DRDialogueBox : MonoBehaviour
{
	[Module] private AudioPlayer audioPlayer;

	[Header("Properties")]
	[Obsolete("This will be changed after the protagonist asset in DREditor is finished.")]
	[SerializeField] private string Protagonist = "Rantaro Amami";
	[Tooltip("Check if this dialogue box is used for CG.")]
	[SerializeField] private bool IsCGDialogueBox;
	[Tooltip("The speed of how the text will be shown in letters per second.")]
	[SerializeField] [Range(50f, 100f)] private float WriteSpeed;
	[SerializeField] private Sprite NullNameplate;

	[Header("Sounds")]
	[SerializeField] private AudioClip _nextLineSound;
	[SerializeField] private AudioClip _TruthBulletShowSound;
	[SerializeField] private AudioClip _TruthBulletHideSound;

	[Header("References")]
	[SerializeField] private DRTextBox NormalStyle;
	[SerializeField] private DRTextBox CGStyle;

	[SerializeField] private Image NBImage;
	[SerializeField] private CanvasGroup NBTextBoxCG;
	[SerializeField] private TextMeshProUGUI NBText, TBText;
	[SerializeField] private RectTransform NBTextTransform;
	[SerializeField] private Image TruthBulletImage;

	[SerializeField] private Material TextboxMaterial;

	private Animator boxAnimate;

	void Awake()
	{
		DRModuleSystem.Inject(this);
        DialogueEventSystem.RegisterEvents(this);

        if (!boxAnimate)
			boxAnimate = GetComponent<Animator>();

		DialogueRuntimeData.OnChangeState += OnChangeDialogueState;
	}

	private void Start()
	{
		gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		DialogueRuntimeData.OnChangeState -= OnChangeDialogueState;
		DialogueEventSystem.DeregisterEvents(this);
	}

	private void OnEnable()
	{
		Auto(DialogueRuntimeData.OnAuto);

		foreach (var trgr in boxAnimate.parameters)
		{
			if (trgr.type == AnimatorControllerParameterType.Trigger)
			{
				boxAnimate.ResetTrigger(trgr.name);
			}
		}
	}

	private void OnDisable()
	{
		DialogueRuntimeData.IsWriting = false;
	}

	private void OnChangeDialogueState(DialogueRuntimeData.DialogueState obj)
	{
		Auto(DialogueRuntimeData.OnAuto);
	}

	[DialogueEvent("SetSayboxActive")]
	public void OnSetSayboxActive(bool active)
	{
		gameObject.SetActive(active);
	}


	[DialogueEvent("PrintDialogue")]
	public void UseText(Line line)
	{
		if (!gameObject.activeSelf)
			gameObject.SetActive(true);

		bool isProtagonist = line.Speaker != null && line.Speaker.IsProtagonist;
		var diaMode = DialogueRuntimeData.DialogueAsset.DialogueMode;

		//string charname = "";
		Sprite nameplate = line.Speaker != null ? line.Speaker.Nameplate : null;

		if (line.AliasNumber > 0 && line.Speaker != null)
		{
			//charname = line.Speaker.Aliases[line.AliasNumber - 1].Name;
			nameplate = line.Speaker.Aliases[line.AliasNumber - 1].Nameplate;
		}

		SetSpeakerName(nameplate);

		NormalStyle.gameObject.SetActive(diaMode == BoxMode.Normal);
		CGStyle.gameObject.SetActive(diaMode == BoxMode.CG);
		
		switch (diaMode) {
			case BoxMode.Normal:
				NormalStyle.ProcessLine(line.Text, isProtagonist, WriteSpeed);
				break;
			case BoxMode.CG:
				CGStyle.ProcessLine(line.Text, isProtagonist, WriteSpeed);
				break;
		}

		if (audioPlayer != null)
			audioPlayer.OnPlaySystemSFX(_nextLineSound);

		boxAnimate.SetBool("Finish", false);
	}

    [DialogueEvent("FinishText")]
    public void FinishText()
	{
		switch (DialogueRuntimeData.DialogueAsset.DialogueMode)
		{
			case BoxMode.Normal:
				NormalStyle.TypeWhole();
				break;
			case BoxMode.CG:
				CGStyle.TypeWhole();
				break;
		}
	}

    public void Auto(bool _on)
    {
        boxAnimate.SetBool("Auto", _on);
    }

    private void SetSpeakerName(string name, float xpos = 678f)
	{
		NBText.text = name;
		//if (name == Protagonist)
		//{
		//	NBTextBoxCG.alpha = 1;
		//	NBTextTransform.anchoredPosition = new Vector2(xpos, NBTextTransform.anchoredPosition.y);
		//	NBTextTransform.localScale = new Vector3(-1, 1, 1);
		//	NBTextTransform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
		//}
		if (name == " " || name == "")
		{
			NBTextBoxCG.alpha = 0;
		}
		else
		{
			NBTextBoxCG.alpha = 1;
			NBTextTransform.anchoredPosition = new Vector2(-xpos, NBTextTransform.anchoredPosition.y);
			NBTextTransform.localScale = Vector3.one;
			NBTextTransform.GetChild(0).GetComponent<RectTransform>().localScale = Vector3.one;
		}
	}

	private void SetSpeakerName(Sprite nameplate, float xpos = 678f)
	{
		NBImage.color = nameplate == null ? Color.clear : Color.white;
		NBImage.sprite = nameplate;
	}

	//Tags:
	//<emp> </emp> = <material="Nunito_Emphasis"> = Emphasis
	//<nar> </nar> = <material="Nunito_Narration"> = Narration
	//<tut> </tut> = <material="Nunito_Tutorial"> = Tutorial

	private IEnumerator TypeText(string text) {
		text = Regex.Replace(text, "<emp>", "<material=\"Nunito_Emphasis\">");
		text = Regex.Replace(text, "</emp>", "</material>");
		text = Regex.Replace(text, "<nar>", "<material=\"Nunito_Narration\">");
		text = Regex.Replace(text, "</nar>", "</material>");
		text = Regex.Replace(text, "<tut>", "<material=\"Nunito_Tutorial\">");
		text = Regex.Replace(text, "</tut>", "</material>");

		DialogueRuntimeData.IsWriting = true;

		TBText.text = text;
		string _notag = Regex.Replace(text, "<.*?>", string.Empty);

		for (int x = 0; x <= _notag.Length; x++)
		{
			TBText.maxVisibleCharacters = x;

			//if (TBText.maxVisibleCharacters > 1 && (confirmAction.triggered || cancelAction.triggered) && DialogueHandler.IsWriting)
			//{
			//	TypeWhole(_notag.Length);
			//	yield break;
			//}

			yield return new WaitForSeconds(0.05f / WriteSpeed * Time.deltaTime);
		}
		boxAnimate.SetBool("Finish", true);

		DialogueRuntimeData.IsWriting = false;
		yield break;
	}

	private void TypeWhole(int length)
	{
		TBText.maxVisibleCharacters = length;
		DialogueRuntimeData.IsWriting = false;
		boxAnimate.SetBool("Finish", true);
	}

	[DialogueEvent("TruthBulletDisplay")]
	private void TruthBulletDisplay(DTBTuple bullet)
	{
		switch (bullet.TBChoice)
		{
			case DTBChoice.Show:
                boxAnimate.SetTrigger("ShowTruthBullet");
				break;
			case DTBChoice.Hide:
				boxAnimate.SetTrigger("HideTruthBullet");
				break;
			default:
				break;
		}
	}

    [DialogueEvent("CharacterLeave")]
    private void OnCharacterLeave()
	{

	}
}
