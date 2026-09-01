using System.Collections;
using UnityEngine;
using DREditor;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;

public class DRSpriteBillboard : MonoBehaviour
{
	[SerializeField] private string characterName;
	[SerializeField] private bool UseAmbientLight;
	[SerializeField] private Color AmbientLight = Color.gray;

	private bool PnCMode;
	private Transform MainCamera;
	private Renderer CharRenderer;
    private DRSpriteDepth CharDepth;

    private Coroutine FlashC;

	public string CharacterName { get { return characterName; } }

    private void Start()
	{
        if (MapSetup.current)
			PnCMode = MapSetup.current.SceneType == SceneType.PointAndClick;

		CharRenderer = GetComponent<Renderer>();
		CharDepth = GetComponent<DRSpriteDepth>();
		if (!MainCamera && Camera.main) MainCamera = Camera.main.transform;

		if (UseAmbientLight)
		{
            AmbientLight = RenderSettings.ambientLight;
            CharRenderer.material.color = AmbientLight;
        }
	}

	private void Update()
	{
		if (!DialogueRuntimeData.InDialog && MainCamera && !PnCMode)
		{
			transform.localRotation = Quaternion.Euler(new Vector3(0f, MainCamera.transform.localRotation.eulerAngles.y + 180f, 0f));
		}
	}

	private void OnEnable()
	{
		DialogueEventSystem.StartListening("DisableCharacterHighlight", DisableFocusHighlight);
        DialogueEventSystem.StartListening("ChangeExpression", ChangeExpression);
    }

	private void OnDisable()
	{
		DialogueEventSystem.StopListening("DisableCharacterHighlight", DisableFocusHighlight);
        DialogueEventSystem.StopListening("ChangeExpression", ChangeExpression);
    }

    /// <summary>
    /// Used to change the main Texture of the Sprite Billboard.
    /// </summary>
    /// <param name="convert"></param>
    public void ChangeExpression(object expressionMaterial)
	{
        (string name, Material mat) convert = ((string name, Material mat))expressionMaterial;

        if (convert.name != CharacterName)
            return;

        CharRenderer.material = convert.mat;
		if (CharDepth != null)
		{
			StartCoroutine(CharShadowCoroutine(convert.mat));
		}
	}

    public void ChangeExpression(string name, Material expressionMaterial)
    {
        if (name != CharacterName)
            return;

        CharRenderer.material = expressionMaterial;
        if (CharDepth != null)
        {
            StartCoroutine(CharShadowCoroutine(expressionMaterial));
        }
    }

    private IEnumerator CharShadowCoroutine(Material mat)
    {
		yield return new WaitForSeconds(1.5f * Time.deltaTime);
        //CharacterDepth.mainTexture = CharTex.material.mainTexture;
        CharDepth.UpdateMat(mat);
	}

	public void Focus(bool check)
	{
		if (check)
		{
			CharRenderer.material.color = Color.white;
		}
		else
		{
			CharRenderer.material.color = AmbientLight;
		}
	}

	private void DisableFocusHighlight(object value)
	{
		Focus(false);
	}

	/// <summary>
	/// Flash the texture for a split second.
	/// </summary>
	/// <param name="intensity"></param>
	public void Flash(float intensity = 5f, AudioClip aud = null)
	{
		if (aud != null) DialogueEventSystem.TriggerEvent("PlaySystemSFX", aud);
		FlashC = StartCoroutine(FlashCoroutine(intensity));
	}
	
	//Planning to change this into a Sinewave here. But too lazy to do it yet.
	private IEnumerator FlashCoroutine(float intensity)
	{
		float _time = 0.01f;
		float _elaspedtime = 0f;
		Color flash;
		flash.r = intensity;
		flash.g = intensity;
		flash.b = intensity;
		flash.a = 1f;

		do
		{
			CharRenderer.material.color = Color.Lerp(Color.white, flash, _elaspedtime / _time);
			_elaspedtime += _time * 10f * Time.deltaTime;
			yield return new WaitForEndOfFrame();
			if (Input.GetButton("Cancel"))
				yield break;
		} while (_elaspedtime < _time);

		_elaspedtime = 0f;

		do
		{
			CharRenderer.material.color = Color.Lerp(flash, Color.white, _elaspedtime / _time);
			_elaspedtime += _time * 10f * Time.deltaTime;
			yield return new WaitForEndOfFrame();
			if (Input.GetButton("Cancel"))
				yield break;
		} while (_elaspedtime < _time);

		CharRenderer.material.color = Color.white;
		yield break;
	}
}
