//Dialogue Panel script by SeleniumSoul for DR:Distrust

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DREditor.Characters;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using UnityEngine.Rendering.Universal;

/// <summary>
/// UI Script that handles the comicstrip-like view when in a dialogue.
/// </summary>
public class DRDialoguePanels : MonoBehaviour
{
	[SerializeField] private GameObject[] PanelCameras = new GameObject[3];
	[SerializeField] private float TalkFOV;
	[SerializeField] private Material BGFilterMaterial;
	[SerializeField] private Material Opacity;
	[SerializeField] private UniversalRendererData RenderData;
	[SerializeField] public bool Immediate;

	private Animator Panels;
	private GameObject[] Focused;
	private Camera MainCam;

	private CanvasGroup PanelCG;

	private Coroutine CameraLerp;

	private void Awake()
	{
		SceneManager.sceneLoaded += Reinitialize;
		Panels = GetComponent<Animator>();
		PanelCG = GetComponent<CanvasGroup>();
    }

	private void Start()
	{
        //Subscribe to Events here
        DialogueEventSystem.StartListening("ChangeCharacterFocus", OnChangeCharacterFocus);
        DialogueEventSystem.StartListening("ChangeWindowPattern", OnChangeWindowPattern);
        DialogueEventSystem.StartListening("DiaPanelTerminate", OnDiaPanelTerminate);

        DialogueEventSystem.StartListening("PanelActive", OnPanelActive);

        //DialoguePlayer.current.DiaPanels = gameObject;
		Focused = new GameObject[PanelCameras.Length];

		foreach (GameObject n in PanelCameras)
		{
			n.GetComponent<Camera>().enabled = false;
		}

        gameObject.SetActive(false);
    }
    private void OnDestroy()
    {
        DialogueEventSystem.StopListening("ChangeCharacterFocus", OnChangeCharacterFocus);
        DialogueEventSystem.StopListening("ChangeWindowPattern", OnChangeWindowPattern);
        DialogueEventSystem.StopListening("DiaPanelTerminate", OnDiaPanelTerminate);

        DialogueEventSystem.StopListening("PanelActive", OnPanelActive);
    }

    private void Reinitialize(Scene load, LoadSceneMode mode)
    {
		CamNullCheck();
    }

	private bool CamNullCheck()
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

	private void OnEnable()
	{
        MainCam = Camera.main;
        foreach (GameObject n in PanelCameras)
		{
			if (MainCam)
			{
				n.transform.SetParent(MainCam.transform);
				n.transform.SetPositionAndRotation(MainCam.transform.position, MainCam.transform.rotation);
				n.GetComponent<Camera>().fieldOfView = MainCam.fieldOfView;
				n.GetComponent<Camera>().enabled = true;
			}
		}

		BGInitialize();
	}

    private void BGInitialize()
	{
		Panels.SetBool("Immediate", Immediate);
        BGFilter(true);
    }

	private void OnPanelActive(object value)
    {
		var (open, chara) = ((bool open, CharacterSpot chara))value;
		gameObject.SetActive(open);

		for (int i = 0; i < Focused.Length; i++)
		{
			if (Focused[i] == chara.CurrentCharacter)
			{
				ReturnCamToOrigParent(i);
			}
		}
    }

	private void OnDiaPanelTerminate(object value)
	{
		Immediate = false;
		BGFilter(false, true, true);
	}

	private void ReturnCamToOrigParent()
	{
		foreach (GameObject n in PanelCameras)
		{
            n.transform.SetParent(transform);
            n.GetComponent<Camera>().fieldOfView = MainCam.fieldOfView;
			n.GetComponent<Camera>().enabled = false;
		}
	}
    private void ReturnCamToOrigParent(int index)
    {
        PanelCameras[index].transform.SetParent(transform);
        PanelCameras[index].GetComponent<Camera>().fieldOfView = MainCam.fieldOfView;
        PanelCameras[index].GetComponent<Camera>().enabled = false;
    }
    private void OnChangeWindowPattern(object state)
	{
		if (state is CWPTuple _CWPValue)
		{
			Debug.Log("DREditor (DialoguePanels): Changing pattern to " + _CWPValue.PatternName);
			Panels.SetTrigger(_CWPValue.PatternName);
		}

		else if (state is string _stringstate)
		{
			Panels.SetTrigger(_stringstate);
		}

		else if (state is int _intstate)
		{
			Debug.Log("DREditor (DialoguePanels): Triggering ChangePattern with int " + _intstate);
			Panels.SetTrigger(_intstate);
		}
	}

	private GameObject GetFocusTarget(string id)
	{
        var characters = GameObject.FindGameObjectsWithTag("Character");
        var props = GameObject.FindGameObjectsWithTag("Prop");

		foreach (var c in characters)
		{
			if (c.TryGetComponent<DRSpriteBillboard>(out DRSpriteBillboard sb))
			{
                Debug.Log($"{sb.CharacterName} == {id}");
                if (sb.CharacterName == id)
				{
					Debug.Log("Character Found");
                    return c;
                }
            }
		}

		foreach (var p in props)
		{
			if (p.name == id)
				return p;
		}

        Debug.Log("Target not Found");
        return null;
    }

    /// <summary>
    /// Dialogue Event: Change the focus of the panel camera to the current character.
    /// </summary>
    /// <param name="values"></param>
    private void OnChangeCharacterFocus(object value)
	{
        (string ID, int PanelNum, Vector3 CT) convert = (ValueTuple<string, int, Vector3>)value;

        GameObject target = GetFocusTarget(convert.ID);
        int camNum = convert.PanelNum;
        Vector3 offsetPosition = convert.CT;

		if (target == null) return;
        if (target.TryGetComponent<DRBillboardProperties>(out DRBillboardProperties sb))
        {
            offsetPosition = sb.CameraPosition + convert.CT;
        }

        if (CameraLerp != null)
            StopCoroutine(CameraLerp);

        //Change color of previous character
        if (Focused[camNum] && !DialoguePlayer.current.PropMode)
		{
			Focused[camNum].GetComponent<DRSpriteBillboard>().Focus(false);
		}

		Debug.Log("DREditor <color=purple>(DialoguePanels)</color>: Triggering ChangeCamFocus with camera " + camNum + " on " + target.name + " at " + offsetPosition + " position.");

		Focused[camNum] = target;
		PanelCameras[camNum].transform.SetParent(target.transform);

		if (!DialogueRuntimeData._skip) //If not skipping, lerp.
        {
			CameraLerp = StartCoroutine(LerpPos(PanelCameras[camNum], offsetPosition, 0.5f));
		}
        else //If skipping, just teleport.
        {
			PanelCameras[camNum].GetComponent<Camera>().fieldOfView = TalkFOV;
			PanelCameras[camNum].transform.localPosition = offsetPosition;
			PanelCameras[camNum].transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
		}

		//Change color of character
		if (!DialoguePlayer.current.PropMode)
		{
			Focused[camNum].GetComponent<DRSpriteBillboard>().Focus(true);
		}
	}
	private IEnumerator LerpPos(GameObject camera, Vector3 position, float speed)
	{
		Camera cam = camera.GetComponent<Camera>();
		float InitFOV = cam.fieldOfView;
		float _StartTime = 0;

		while (_StartTime < speed)
		{
			if (!DialoguePlayer.current.PropMode)
				cam.fieldOfView = Mathf.Lerp(InitFOV, TalkFOV, (_StartTime / speed * 2f));

			camera.transform.localPosition = Vector3.Lerp(camera.transform.localPosition, position, (_StartTime / speed));
			camera.transform.localRotation = Quaternion.Lerp(camera.transform.localRotation, Quaternion.Euler(new Vector3(0f, 180f, 0f)), (_StartTime / speed));
			_StartTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		camera.transform.localPosition = position;
		camera.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
	}

	#region BGFilter - Used to enable/disable the filter of the background
	/// <summary>
	/// Do stuff on the BG capture.
	/// </summary>
	/// <param name="active">Set if the BG is active.</param>
	/// <param name="capdestroy">Destroy the capture.</param>
	/// <param name="dialogueend">Used only when the dialogue actually ends.</param>
	/// <param name="value">The speed of the lerps.</param>
	private void BGFilter(bool active, bool capdestroy = false, bool dialogueend = false, float value = 7f)
	{
        if (active)
		{
            StartCoroutine(EnableBGFilter());
        }
		else
		{
            StartCoroutine(DisableBGFilter(value, capdestroy, dialogueend));
        }
	}
	private IEnumerator EnableBGFilter()
	{
		DialogueRuntimeData.OnTransition = true;
		PanelCG.alpha = 1f;

  //      BGFilterMaterial.SetFloat("_Opacity", 0.744f);
  //      RenderData.rendererFeatures[0].SetActive(true);
		//if (Panels.GetCurrentAnimatorClipInfo(0).Length > 0)
		//{
		//	yield return new WaitForSeconds(Panels.GetCurrentAnimatorClipInfo(0)[0].clip.length);
		//}

		DialogueRuntimeData.Advance = true;
		DialogueEventSystem.TriggerEvent("LinePass", DialogueRuntimeData._currentLineNum);
		DialogueRuntimeData.OnTransition = false;
		yield break;
	}
	private IEnumerator DisableBGFilter(float value, bool destroycap, bool enddialogue)
	{
		DialogueRuntimeData.OnTransition = true;
		ResetAllTriggers();
		Panels.SetTrigger("DialogueEnd");
		yield return new WaitForSeconds(1f);

		ReturnCamToOrigParent();
		DialogueEventSystem.TriggerEvent("DisableCharacterHighlight");

		float opacityValue = 0.744f;

        BGFilterMaterial.SetFloat("_Opacity", 0.744f);
		do
		{
            opacityValue -= value * Time.deltaTime;
            BGFilterMaterial.SetFloat("_Opacity", opacityValue);
			yield return new WaitForEndOfFrame();
		} while (opacityValue > 0f);
        BGFilterMaterial.SetFloat("_Opacity", 0f);

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

        //MainCam.gameObject.SetActive(true);

        //RenderData.rendererFeatures[0].SetActive(false);
  //      if (destroycap)
		//{
		//	Destroy(_Capture);
		//	_Capture = null;
		//}
		if (enddialogue)
		{
			DialogueRuntimeData.InDialog = false;
			DialoguePlayer.current.DRCursor.SetActive(true);
			Focused[0] = null;
			Focused[1] = null;
			Focused[2] = null;
			DialogueRuntimeData.EndDialogue();
		}
		DialogueRuntimeData.OnTransition = false;
		this.gameObject.SetActive(false);
		yield break;
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

	private void ResetAllTriggers()
	{
		foreach (var param in Panels.parameters)
		{
			if (param.type == AnimatorControllerParameterType.Trigger)
			{
				Panels.ResetTrigger(param.name);
			}
		}
	}
}