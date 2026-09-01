//DialogueBox script by SeleniumSoul for DR:Distrust

using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DREditor.Trial;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using DREditor.Characters;
using UnityEngine.InputSystem;

/// <summary>
/// UI Script that handles the dialogue box for the story text.
/// </summary>
public class DRDialogueBox : MonoBehaviour
{
	[Header("Properties")]
    //Note: This will be changed after the protagonist asset in DREditor is finished.
    [SerializeField] private string Protagonist = "Rantaro Amami";
    [Tooltip("Check if this dialogue box is used for CG.")]
	[SerializeField] private bool IsCGDialogueBox;
	[Tooltip("Amount of time the text will finish writing. (Not in seconds)")]
	[SerializeField] [Range(1f, 15f)] private float WriteSpeed;
    [Tooltip("The speed of the gradient change.")]
    [SerializeField] private float GradientLerpSpeed = 1;
    [SerializeField] private float GradientLerpLength = 1;

    [Header("Sounds")]
    [SerializeField] private AudioClip _nextLineSound;
    [SerializeField] private AudioClip _TruthBulletShowSound;
    [SerializeField] private AudioClip _TruthBulletHideSound;

	[Header("References")]
	[SerializeField] private Image NBImage;
	[SerializeField] private CanvasGroup NBTextBoxCG;
	[SerializeField] private TextMeshProUGUI NBText, TBText;
	[SerializeField] private RectTransform NBTextTransform;
	[SerializeField] private Image TruthBulletImage;

	[SerializeField] private Material TextboxMaterial;

    //private DRInput controls;
    private Animator boxAnimate;

	private bool IsProtagonist;
	private Coroutine typeTextCoroutine;
	private Coroutine boxGradientLerpCoroutine;

	private InputAction confirmAction;
	private InputAction cancelAction;

	void Awake()
	{
		if (!boxAnimate) boxAnimate = GetComponent<Animator>();
		confirmAction = InputSystem.actions.FindAction("Confirm");
        confirmAction = InputSystem.actions.FindAction("Cancel");
    }

	private void Start()
	{
		//DialogueEventSystem.StartListening("LinePass", ProcessLine);
        DialogueEventSystem.StartListening("CharacterLeave", OnCharacterLeave);
        gameObject.SetActive(false);
	}
    private void OnDestroy()
    {
        DialogueEventSystem.StopListening("CharacterLeave", OnCharacterLeave);
    }
    private void OnEnable()
	{
		DialogueEventSystem.StartListening("TruthBulletDisplay", TruthBulletDisplay);

		Auto(DialogueRuntimeData._auto);

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
		DialogueEventSystem.StopListening("TruthBulletDisplay", TruthBulletDisplay);

		DialogueRuntimeData.IsWriting = false;
	}

	private void Update()
	{
		Auto(DialogueRuntimeData._auto || DialogueRuntimeData._skip);
	}
    public void Auto(bool _on)
    {
		boxAnimate.SetBool("Auto", _on);
	}
	
	private void ProcessLine(object lineObject)
	{
		Line line = (Line)lineObject;
		Sprite nameplate = line.Speaker.Nameplate;

        if (typeTextCoroutine != null) StopCoroutine(typeTextCoroutine);
        if (boxGradientLerpCoroutine != null) StopCoroutine(boxGradientLerpCoroutine);
        if (IsCGDialogueBox) DialogueRuntimeData.Advance = true;

        boxGradientLerpCoroutine = StartCoroutine(BoxGradientLerp(GradientLerpSpeed, GradientLerpLength));

        if (line.AliasNumber > 0 && line.Speaker != null)
        {
            IsProtagonist = (line.Speaker.FullName(false) == Protagonist);
            nameplate = line.Speaker.Aliases[line.AliasNumber - 1].Nameplate;
        }

        SetSpeakerName(nameplate);
        DialogueEventSystem.TriggerEvent("PlaySystemSFX", _nextLineSound);
        boxAnimate.SetBool("Finish", false);

        typeTextCoroutine = StartCoroutine(TypeText(line.Text));
    }

    public void UseText(string speakerName, Sprite nameplate, string line, bool start = false)
    {
        if (typeTextCoroutine != null) StopCoroutine(typeTextCoroutine);
		if (boxGradientLerpCoroutine != null) StopCoroutine(boxGradientLerpCoroutine);
        if (IsCGDialogueBox) DialogueRuntimeData.Advance = true;

        IsProtagonist = (speakerName == Protagonist);
		
		boxGradientLerpCoroutine = StartCoroutine(BoxGradientLerp(GradientLerpSpeed, GradientLerpLength));

        SetSpeakerName(nameplate);
        DialogueEventSystem.TriggerEvent("PlaySystemSFX", _nextLineSound);
        boxAnimate.SetBool("Finish", false);

        if (!start)
			boxAnimate.SetTrigger("Next");

        typeTextCoroutine = StartCoroutine(TypeText(line));
    }

    public void SetSpeakerName(string name, float xpos = 678f)
	{
        NBText.text = name;
        if (name == Protagonist)
		{
			//NBTextBoxCG.alpha = 1;
			//NBTextTransform.anchoredPosition = new Vector2(xpos, NBTextTransform.anchoredPosition.y);
			//NBTextTransform.localScale = new Vector3(-1, 1, 1);
			//NBTextTransform.GetChild(0).GetComponent<RectTransform>().localScale = new Vector3(-1, 1, 1);
		}
		else if (name == " " || name == "")
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

    public void SetSpeakerName(Sprite nameplate, float xpos = 678f)
    {
		NBImage.color = nameplate == null ? Color.clear : Color.white;
        NBImage.sprite = nameplate;
    }

    //Tags:
    //<emp> </emp> = <material="Nunito_Emphasis"> = Emphasis
    //<nar> </nar> = <material="Nunito_Narration"> = Narration
    //<tut> </tut> = <material="Nunito_Tutorial"> = Tutorial

    IEnumerator TypeText(string text) {
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

	private void TruthBulletDisplay(object bullet)
	{
		DTBTuple _bulletObject = (DTBTuple)bullet;
		switch (_bulletObject.TBChoice)
		{
			case DTBChoice.Show:
				TruthBulletImage.sprite = _bulletObject.TB.Picture;
				DialogueEventSystem.TriggerEvent("PlaySystemSFX", _TruthBulletShowSound);
				boxAnimate.SetTrigger("ShowTruthBullet");
				break;
			case DTBChoice.Hide:
				DialogueEventSystem.TriggerEvent("PlaySystemSFX", _TruthBulletHideSound);
				boxAnimate.SetTrigger("HideTruthBullet");
				break;
			default:
				break;
		}
	}

	private IEnumerator BoxGradientLerp(float speed, float length)
	{
		float _elapsedTime = 0;
		float _lerpStart = IsProtagonist ? 0 : 1;
		float _lerpEnd = IsProtagonist ? 1 : 0;

		while (_elapsedTime < length)
		{
			TextboxMaterial.SetFloat("_State", Mathf.Lerp(TextboxMaterial.GetFloat("_State"), _lerpEnd, _elapsedTime * speed));
			_elapsedTime += Time.deltaTime;

			yield return null;
		}

        TextboxMaterial.SetFloat("_State", _lerpEnd);
		yield return null;
    } 

	private void OnCharacterLeave()
	{

	}
}
