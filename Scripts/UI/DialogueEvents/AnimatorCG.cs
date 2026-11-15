using UnityEngine;
using DREditor.Dialogues.Events;
using DREditor;
using System.Threading.Tasks;

/// <summary>
/// CG pictures. Use LegacyCG if you just want to use legacy Animation object for simplicity.
/// </summary>
[RequireComponent(typeof(Animator))]
public class AnimatorCG : MonoBehaviour, ICG
{
    [SerializeField] private float FadeSpeed = 0.5f;
    [SerializeField] private bool StartBlackScreen;

    [SerializeField] private Material FadeMaterial;
    private CanvasGroup canvasGroup;

    public void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        DialogueEventSystem.RegisterEvents(this);
        if (StartBlackScreen)
        {
            FadeMaterial.color = Color.black;
            FadeMaterial.SetFloat("_Opacity", 1f);
        }
    }

    private void OnDisable()
    {
        DialogueEventSystem.DeregisterEvents(this);
    }

    public async Task Initialize()
    {
        //StartCoroutine(FadeAnimation(FadeSpeed, 0f, 1f));
        await FadeAnimation(FadeSpeed, 0f, 1f);
    }

    [DialogueEvent("CG_TriggerNextState")]
    private void TriggerNextState()
    {
        GetComponent<Animator>().SetTrigger("Next");
    }

    [DialogueEvent("CG_FadeOut")]
    private async Task CG_FadeOut()
    {
        //StartCoroutine(FadeAnimation(FadeSpeed, 1f, 0f));
        await FadeAnimation(FadeSpeed, 1f, 0f);

        if (StartBlackScreen)
        {
            FadeMaterial.color = Color.black;
            FadeMaterial.SetFloat("_Opacity", 0f);
        }
    }

    //private IEnumerator FadeAnimation(float speed, float startalpha, float endalpha)
    //{
    //    float _startTime = 0f;

    //    while (_startTime < speed)
    //    {
    //        _canvasGroup.alpha = Mathf.Lerp(startalpha, endalpha, _startTime / speed);
    //        _startTime += Time.deltaTime;
    //        yield return new WaitForEndOfFrame();
    //    }

    //    if (startalpha == 1f)
    //    {
    //        Destroy(gameObject);
    //    }
    //    else
    //    {
    //        _canvasGroup.alpha = 1f;
    //    }
    //}

    private async Task FadeAnimation(float speed, float startalpha, float endalpha)
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        float _startTime = 0f;

        while (_startTime < speed)
        {
            canvasGroup.alpha = Mathf.Lerp(startalpha, endalpha, _startTime / speed);
            _startTime += Time.deltaTime;
            await Awaitable.EndOfFrameAsync();
        }

        if (startalpha == 1f)
        {
            Destroy(gameObject);
        }
        else
        {
            canvasGroup.alpha = 1f;
        }
    }
}
