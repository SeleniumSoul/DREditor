using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DREditor.Dialogues.Events;
using DREditor.Dialogues;
using System.Threading.Tasks;

public class UIFade : MonoBehaviour
{
    public static bool OnScreenFade
    {
        get
        {
            return (_OnTransition || screenCovered);
        }
    }
    public Image FadeCover;

    private static bool screenCovered = false;
    private static bool _OnTransition = false;

    private void OnEnable()
    {
        DialogueEventSystem.RegisterEvents(this);
    }

    private void OnDisable()
    {
        DialogueEventSystem.DeregisterEvents(this);
    }

    [DialogueEvent("FadeToBlack")]
    public async Task FadeToBlack(float speed)
    {
        //var speed = (float)FTBValue;
        await Fade(true, speed, Color.black);
    }

    [DialogueEvent("FadeOut")]
    public async Task FadeOut(float speed)
    {
        //var speed = (float)FTBValue;
        await Fade(false, speed, Color.black);
    }

    public async Task Fade(bool enable, float fadeTime, Color color)
    {
        float targetAlpha = enable ? 1f : 0f;

        screenCovered = enable;

        if (fadeTime <= 0f)
        {
            FadeCover.color = new Color(color.r, color.g, color.b, targetAlpha);
            return;
        }

        //TODO: Use await here.
        if (!_OnTransition) 
            StartCoroutine(FadeAnimation(fadeTime, targetAlpha, color));
        else
        {
            StopCoroutine("FadeAnimation");
            StartCoroutine(FadeAnimation(fadeTime, targetAlpha, color));
        }
    }

    public void Flash(object FLSValue)
    {
        StartCoroutine(FadeAnimation(10f, 1f, Color.white));
    }

    IEnumerator FadeAnimation(float speed, float endalpha, Color _color)
    {
        _OnTransition = true;
        float _startTime = 0f;
        float _startalpha = FadeCover.color.a;

        while (_startTime < speed)
        {
            if (speed == 0)
            {
                _startTime = speed;
                continue;
            }
            else
            {
                FadeCover.color = new Color(_color.r, _color.g, _color.b, Mathf.Lerp(_startalpha, endalpha, _startTime / speed));
                _startTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }
        FadeCover.color = new Color(_color.r, _color.g, _color.b, endalpha);
        _OnTransition = false;
    }
}
