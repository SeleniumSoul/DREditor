//Dialogue Panel script by SeleniumSoul for DR:Distrust

using DREditor;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

/// <summary>
/// UI Script that handles the comicstrip-like view when in a dialogue.
/// </summary>
public class DRDialoguePanels : MonoBehaviour
{
	[SerializeField] private DRPanelCamera[] PanelCameras;
	[SerializeField] private float TalkFOV;
	[SerializeField] private Material BGFilterMaterial;
	[SerializeField] private UniversalRendererData RenderData;
	[SerializeField] private bool Immediate;

    public Camera MainCam { get; private set; }
    private Animator panelAnimator;
	private CanvasGroup panelCanvasGroup;

	private void Awake()
	{
		SceneManager.sceneLoaded += Reinitialize;
		panelAnimator = GetComponent<Animator>();
		panelCanvasGroup = GetComponent<CanvasGroup>();

		DialogueEventSystem.RegisterEvents(this);
		DialogueRuntimeData.DialogueEnded += () => _ = OnPanelActive(false);
		gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
        SceneManager.sceneLoaded -= Reinitialize;
        DialogueEventSystem.DeregisterEvents(this);
	}

	private void Reinitialize(Scene load, LoadSceneMode mode)
	{
		MainCamNullCheck();
	}
    private bool MainCamNullCheck()
    {
        try
        {
            if (MainCam == null)
                MainCam = Camera.main;

            return (MainCam);
        }
        catch
        {
            return false;
        }
    }
    private GameObject GetFocusTarget(string id)
    {
        var target = new List<GameObject>();

        target.AddRange(GameObject.FindGameObjectsWithTag("Character"));
        target.AddRange(GameObject.FindGameObjectsWithTag("Prop"));

        foreach (var c in target)
        {
            if (c.TryGetComponent<IExaminable>(out IExaminable sb))
            {
                if (sb.GetName() == id)
                {
                    Debug.Log($"Character {sb.GetName()} found as focus target.");
                    return c;
                }
            }
        }

        Debug.Log("Target not Found");
        return null;
    }
    private void ResetAllTriggers()
    {
        foreach (var param in panelAnimator.parameters)
        {
            if (param.type == AnimatorControllerParameterType.Trigger)
            {
                panelAnimator.ResetTrigger(param.name);
            }
        }
    }
    private void ReturnCamToOrigParent()
    {
        foreach (var pc in PanelCameras)
        {
            pc.transform.SetParent(transform);
            pc.GetComponent<Camera>().fieldOfView = MainCam.fieldOfView;
        }
    }

    private void OnEnable()
	{
		//foreach (var pc in PanelCameras)
		//{
		//	var cam = pc.GetComponent<Camera>();

		//	if (MainCamNullCheck())
		//	{
		//		pc.transform.SetPositionAndRotation(MainCam.transform.position, MainCam.transform.rotation);
		//		cam.fieldOfView = MainCam.fieldOfView;
		//	}
		//}

		//MainCam.enabled = false;
		//BGInitialize();
	}

	private void OnDisable()
	{
		if (MainCamNullCheck())
			MainCam.enabled = true;
	}

	//private void BGInitialize()
	//{
	//	foreach (var pc in PanelCameras)
	//	{
	//		var cam = pc.GetComponent<Camera>();

	//		if (MainCamNullCheck())
	//		{
	//			pc.transform.SetPositionAndRotation(MainCam.transform.position, MainCam.transform.rotation);
	//			cam.fieldOfView = MainCam.fieldOfView;
	//		}
	//	}

	//	MainCam.enabled = false;

	//	panelAnimator.SetBool("Immediate", Immediate);
	//	BGFilter(true);
	//}

	[DialogueEvent("PanelActive")]
	public async Task OnPanelActive(bool active)
	{
		if (active)
		{
            gameObject.SetActive(true);
            foreach (var pc in PanelCameras)
            {
                var cam = pc.GetComponent<Camera>();

                if (MainCamNullCheck())
                {
                    pc.transform.SetPositionAndRotation(MainCam.transform.position, MainCam.transform.rotation);
                    cam.fieldOfView = MainCam.fieldOfView;
                }
            }

            MainCam.enabled = false;

            panelAnimator.SetBool("Immediate", Immediate);
            await BGFilter(true);
        }
		else
		{
			Immediate = false;
            await BGFilter(false);
        }

		//ReturnCamToOrigParent();
		//for (int i = 0; i < Focused.Length; i++)
		//{
		//	if (Focused[i] == chara.CurrentCharacter)
		//	{
		//		ReturnCamToOrigParent(i);
		//	}
		//}
	}

	[DialogueEvent("EndDialogue")]
	private async Task OnEndDialogue()
	{
		await BGFilter(false, true);
	}

	[DialogueEvent("ChangeWindowPattern")]
	private async Task OnChangeWindowPattern(CWPTuple state)
	{
		if (!gameObject.activeSelf)
			await OnPanelActive(true);

		if (state is CWPTuple _CWPValue)
		{
			Debug.Log("DREditor (DialoguePanels): Changing pattern to " + _CWPValue.PatternName);

			//panelAnimator.CrossFade(_CWPValue.PatternName, 0.1f);
            panelAnimator.SetTrigger(_CWPValue.PatternName);

            //while (panelAnimator.IsInTransition(0) == false)
                //await Task.Yield();

            while (panelAnimator.IsInTransition(0))
                await Task.Yield();
        }
    }

	/// <summary>
	/// Dialogue Event: Change the focus of the panel camera to the current character.
	/// </summary>
	/// <param name="values"></param>
	[DialogueEvent("ChangeCharacterFocus")]
	private void OnChangeCharacterFocus((string ID, int PanelNum, Vector3 CT, bool immediate) value)
	{
		//(string ID, int PanelNum, Vector3 CT, bool immediate) convert = (ValueTuple<string, int, Vector3, bool>)value;

		GameObject target = GetFocusTarget(value.ID);
		int camNum = value.PanelNum;
		bool cameraActive = PanelCameras[camNum].GetComponent<Camera>().isActiveAndEnabled;

		if (target == null)
		{
			Debug.LogWarning($"Cannot find {value.ID} to focus on!");
			return;
		}

		if (DialogueRuntimeData.OnSkip || !cameraActive)
			PanelCameras[camNum].SwitchFocus(target.transform, true, TalkFOV, value.CT);
		else
			PanelCameras[camNum].SwitchFocus(target.transform, value.immediate, TalkFOV, value.CT);

	}

	#region BGFilter - Used to enable/disable the filter of the background
	/// <summary>
	/// Do stuff on the BG capture.
	/// </summary>
	/// <param name="active">Set if the BG is active.</param>
	/// <param name="dialogueend">Used only when the dialogue actually ends.</param>
	/// <param name="value">The speed of the lerps.</param>
	//private void BGFilter(bool active, bool dialogueend = false, float value = 7f)
	//{
	//	if (active)
	//	{
	//		StartCoroutine(EnableBGFilter());
	//	}
	//	else
	//	{
	//		StartCoroutine(DisableBGFilter(dialogueend));
	//	}
	//}
	//private IEnumerator EnableBGFilter()
	//{
	//	DialogueRuntimeData.OnTransition = true;
	//	panelCanvasGroup.alpha = 1f;

	//	//BGFilterMaterial.SetFloat("_Opacity", 0.744f);
	//	//RenderData.rendererFeatures[0].SetActive(true);
	//	if (panelAnimator.GetCurrentAnimatorClipInfo(0).Length > 0)
	//	{
	//		yield return new WaitForSeconds(panelAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
	//	}

	//	//DialogueEventSystem.TriggerEvent("LinePass", DialogueRuntimeData.CurrentLineIndex);
	//	yield break;
	//}
	//private IEnumerator DisableBGFilter(bool enddialogue)
	//{
	//	DialogueRuntimeData.OnTransition = true;
	//	ResetAllTriggers();
	//	panelAnimator.SetTrigger("DialogueEnd");
	//	yield return new WaitForSeconds(1f);

	//	ReturnCamToOrigParent();
	//	DialogueEventSystem.TriggerEvent("DisableCharacterHighlight");

	//	//float opacityValue = 0.744f;

	//	//BGFilterMaterial.SetFloat("_Opacity", 0.744f);
	//	//do
	//	//{
	//	//	opacityValue -= value * Time.deltaTime;
	//	//	BGFilterMaterial.SetFloat("_Opacity", opacityValue);
	//	//	yield return new WaitForEndOfFrame();
	//	//} while (opacityValue > 0f);
	//	//BGFilterMaterial.SetFloat("_Opacity", 0f);

	//	//yield return new WaitForSeconds(0.5f);

	//	//float BGOpacity = 0.744f;
	//	//do
	//	//{
	//	//	BGOpacity -= value * Time.deltaTime;
	//	//	Opacity.SetFloat("_MainAlpha", BGOpacity);
	//	//	yield return new WaitForSeconds(0.01f);
	//	//} while (Opacity.GetFloat("_MainAlpha") > 0f);
	//	//Opacity.SetFloat("_MainAlpha", 0f);
	//	//PanelCG.alpha = 0f;

	//	//MainCam.gameObject.SetActive(true);

	//	if (enddialogue)
	//	{
	//		DialogueRuntimeData.EndDialogue();
	//	}
	//	DialogueRuntimeData.OnTransition = false;
	//	this.gameObject.SetActive(false);
	//	yield break;
	//}

    private async Task BGFilter(bool active, bool dialogueend = false, float value = 7f)
    {
        if (active)
			await EnableBGFilter();
        else
			await DisableBGFilter(dialogueend);
    }
	private async Task EnableBGFilter()
	{
		//DialogueRuntimeData.OnTransition = true;
		panelCanvasGroup.alpha = 1f;
		//BGFilterMaterial.SetFloat("_Opacity", 0.744f);
		//RenderData.rendererFeatures[0].SetActive(true);

		//while (panelAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
		//    await Task.Yield();

		//      if (panelAnimator.GetCurrentAnimatorClipInfo(0).Length > 0)
		//{
		//	await Awaitable.WaitForSecondsAsync(panelAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
		//}

		//var stateInfo = panelAnimator.GetCurrentAnimatorStateInfo(0);
		//      while (stateInfo.normalizedTime < 1f)
		//      {
		//	await Task.Yield();
		//      }

		// Give the animator one frame to register the state change / transition
		//await Awaitable.NextFrameAsync();

		// Ensure it's actually in the state and not stuck on a finished normalizedTime of 1.0 from before
		//AnimatorStateInfo stateInfo = panelAnimator.GetCurrentAnimatorStateInfo(0);

		// Wait until it's finished playing (and ensure it isn't caught in a transition glitch)
		//while ((stateInfo.normalizedTime < 1f || panelAnimator.IsInTransition(0)))
		//{
		//    // If the state hasn't even updated yet, protect against an infinite loop
		//    if (!panelAnimator.IsInTransition(0) && stateInfo.normalizedTime >= 1f)
		//        break;

		//    stateInfo = panelAnimator.GetCurrentAnimatorStateInfo(0);
		//    await Awaitable.NextFrameAsync();

		//}
		_ = DialogueEventSystem.TriggerEvent("SetSayboxActive", false);
		await Awaitable.WaitForSecondsAsync(0.3f);
    }
	private async Task DisableBGFilter(bool enddialogue)
	{
        //DialogueRuntimeData.OnTransition = true;
        _ = DialogueEventSystem.TriggerEvent("SetSayboxActive", false);
        ResetAllTriggers();
		panelAnimator.SetTrigger("DialogueEnd");
		//await Awaitable.NextFrameAsync();

        //AnimatorStateInfo stateInfo = panelAnimator.GetCurrentAnimatorStateInfo(0);
        //if (panelAnimator.GetCurrentAnimatorClipInfo(0).Length > 0)
        //{
        //}
        //while (stateInfo.normalizedTime < 1f || panelAnimator.IsInTransition(0))
        //{
        //	//await Awaitable.WaitForSecondsAsync(panelAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        //	stateInfo = panelAnimator.GetCurrentAnimatorStateInfo(0);
        //	await Awaitable.NextFrameAsync();
        //}
        //ReturnCamToOrigParent();

        await Awaitable.WaitForSecondsAsync(0.5f);
        _ = DialogueEventSystem.TriggerEvent("DisableCharacterHighlight");

		float opacityValue = 0.744f;

		//BGFilterMaterial.SetFloat("_Opacity", 0.744f);
		//do
		//{
		//	opacityValue -= value * Time.deltaTime;
		//	BGFilterMaterial.SetFloat("_Opacity", opacityValue);
		//	yield return new WaitForEndOfFrame();
		//} while (opacityValue > 0f);
		//BGFilterMaterial.SetFloat("_Opacity", 0f);

		//yield return new WaitForSeconds(0.5f);

		//float BGOpacity = 0.744f;
		//do
		//{
		//	BGOpacity -= value * Time.deltaTime;
		//	Opacity.SetFloat("_MainAlpha", BGOpacity);
		//	yield return new WaitForSeconds(0.01f);
		//} while (Opacity.GetFloat("_MainAlpha") > 0f);
		//Opacity.SetFloat("_MainAlpha", 0f);
		//PanelCG.alpha = 0f;

		MainCam.enabled = true;

		//DialogueRuntimeData.OnTransition = false;
		gameObject.SetActive(false);
	}
	#endregion


	#region Shake - Shake the panel camera (Event)
	/// <summary>
	/// Shake the panel camera
	/// </summary>
	/// <param name="camnum"></param>
	private void Shake(int camnum)
	{
		Camera cam = PanelCameras[camnum].GetComponent<Camera>();
		StartCoroutine(Shakey(cam));
	}
	private IEnumerator Shakey(Camera cam)
	{
		float magnitude = 0.015f;
		float duration = 0.1f;
		float elapsed = 0.0f;

		Vector3 originalCamPos = cam.transform.localPosition;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;

			float percentComplete = elapsed / duration;
			float damper = 1.0f - Mathf.Clamp(4.0f * percentComplete - 3.0f, 0.0f, 1.0f);

			float x = UnityEngine.Random.value * 2.0f - 1.0f;
			float y = UnityEngine.Random.value * 2.0f - 1.0f;
			x *= magnitude * damper;
			y *= magnitude * damper;

			cam.transform.localPosition = cam.transform.localPosition + new Vector3(x, y);

			if (Input.GetButton("Cancel"))
				yield break;

			yield return null;
		}

		cam.transform.localPosition = originalCamPos;
	}
    #endregion
}