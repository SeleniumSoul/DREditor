using DREditor.Dialogues;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class DRTextBox : MonoBehaviour
{
    [SerializeField] private Material TextboxMaterial;
    [Tooltip("The speed of the gradient change.")]
    [SerializeField] private float GradientLerpSpeed = 1;
    [SerializeField] private float GradientLerpLength = 1;

    [Header("Special Text Materials")]
    [SerializeField] private string TextEmphasis = "Nunito_Emphasis";
    [SerializeField] private string TextNarration = "Nunito_Narration";
    [SerializeField] private string TextTutorial = "Nunito_Tutorial";

    [Header("References")]
    [SerializeField] private TextMeshProUGUI TBText;

    public bool IsWriting { get; private set; }

    private Coroutine typeTextCoroutine, boxGradientLerpCoroutine;

    //Tags:
    //<emp> </emp> = <material="Nunito_Emphasis"> = Emphasis
    //<nar> </nar> = <material="Nunito_Narration"> = Narration
    //<tut> </tut> = <material="Nunito_Tutorial"> = Tutorial
    public void ProcessLine(string lineText, bool isProtagonist, float writeSpeed = 0)
    {
        if (typeTextCoroutine != null)
        {
            TypeWhole();
            return;
        }

        if (boxGradientLerpCoroutine != null) 
            StopCoroutine(boxGradientLerpCoroutine);

        float _lerpStart = isProtagonist ? 0 : 1;
        float _lerpEnd = isProtagonist ? 1 : 0;
        boxGradientLerpCoroutine = StartCoroutine(GradientLerpRoutine(_lerpEnd, GradientLerpSpeed, GradientLerpLength));

        lineText = Regex.Replace(lineText, "<emp>", $"<font=\"Nunito\" material=\"{TextEmphasis}\">");
        lineText = Regex.Replace(lineText, "</emp>", "</font>");
        lineText = Regex.Replace(lineText, "<nar>", $"<font=\"Nunito\" material=\"{TextNarration}\">");
        lineText = Regex.Replace(lineText, "</nar>", "</font>");
        lineText = Regex.Replace(lineText, "<tut>", $"<font=\"Nunito\" material=\"{TextTutorial}\">");
        lineText = Regex.Replace(lineText, "</tut>", "</font>");

        TBText.text = lineText;
        string _notag = Regex.Replace(lineText, "<.*?>", string.Empty);

        if (writeSpeed > 0)
            typeTextCoroutine = StartCoroutine(TypeTextRoutine(_notag, writeSpeed));
        else
            TypeWhole();
    }

    private IEnumerator GradientLerpRoutine(float end, float speed, float length)
    {
        float _elapsedTime = 0;

        while (_elapsedTime < length)
        {
            TextboxMaterial.SetFloat("_State", Mathf.Lerp(TextboxMaterial.GetFloat("_State"), end, _elapsedTime * speed));
            _elapsedTime += Time.deltaTime;

            yield return null;
        }

        TextboxMaterial.SetFloat("_State", end);
        yield return null;
    }

    private IEnumerator TypeTextRoutine(string text, float writeSpeed)
    {
        float visible = 0f;
        DialogueRuntimeData.IsWriting = true;

        for (int x = 0; x <= text.Length;)
        {
            visible += writeSpeed * Time.deltaTime;

            x = Mathf.FloorToInt(visible);
            TBText.maxVisibleCharacters = x;

            yield return new WaitForEndOfFrame();
        }

        TBText.maxVisibleCharacters = text.Length;
        DialogueRuntimeData.IsWriting = false;
        typeTextCoroutine = null;
        yield return null;
    }

    public void TypeWhole()
    {
        if (typeTextCoroutine != null)
        {
            StopCoroutine(typeTextCoroutine);
            typeTextCoroutine = null;
        }

        TBText.maxVisibleCharacters = TBText.text.Length;
        DialogueRuntimeData.IsWriting = false;
    }
}
