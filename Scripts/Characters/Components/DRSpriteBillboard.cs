using DREditor;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class DRSpriteBillboard : MonoBehaviour
{
	[Module] private readonly DRRoomSetup roomSetup;
	[Module] private readonly DialoguePlayer player;

	[SerializeField] private bool UseAmbientLight;
	[SerializeField] private Color AmbientLight = Color.gray;
	[SerializeField] private bool DisableRotation;

	private Transform mainCamera;
	private Renderer charRenderer;
	private DRSpriteDepth charDepth;

	private DRBillboardProperties properties;

	private void Awake()
	{
		properties = GetComponent<DRBillboardProperties>();
		DRModuleSystem.Inject(this);
	}

	private void Start()
	{
		if (roomSetup != null)
			DisableRotation = roomSetup.SceneType == SceneType.PointAndClick;

		charRenderer = GetComponent<Renderer>();
		charDepth = GetComponent<DRSpriteDepth>();
		if (!mainCamera && Camera.main) mainCamera = Camera.main.transform;

		if (UseAmbientLight)
		{
			AmbientLight = RenderSettings.ambientLight;

			Color.RGBToHSV(AmbientLight, out var h, out var s, out var v);
			var bright = Color.HSVToRGB(h, s, 1f);

			charRenderer.material.color = bright;
		}
	}

	private void Update()
	{
		if (!DialogueRuntimeData.InDialog && mainCamera && !DisableRotation)
		{
			transform.localRotation = Quaternion.Euler(new Vector3(0f, mainCamera.transform.localRotation.eulerAngles.y + 180f, 0f));
		}
	}

	private void OnEnable()
	{
		DialogueEventSystem.RegisterEvents(this);
	}

	private void OnDisable()
	{
		DialogueEventSystem.DeregisterEvents(this);
	}

	/// <summary>
	/// Used to change the main Texture of the Sprite Billboard.
	/// </summary>
	/// <param name="convert"></param>
	[DialogueEvent("ChangeExpression")]
	public void ChangeExpression((string name, Material mat) expressionMaterial)
	{
		//(string name, Material mat) convert = ((string name, Material mat))expressionMaterial;

		if (properties == null || expressionMaterial.name != properties.GetName())
			return;

		charRenderer.material = expressionMaterial.mat;
		if (charDepth != null)
		{
			StartCoroutine(CharShadowCoroutine(expressionMaterial.mat));
		}
	}

	private IEnumerator CharShadowCoroutine(Material mat)
	{
		yield return new WaitForSeconds(1.5f * Time.deltaTime);
		//CharacterDepth.mainTexture = CharTex.material.mainTexture;
		charDepth.UpdateMat(mat);
	}

	public void Focus(bool check)
	{
		if (charRenderer == null)
			charRenderer = GetComponent<Renderer>();
		
		charRenderer.material.color = check ? Color.white : AmbientLight;
	}

	[DialogueEvent("DisableCharacterHighlight")]
	private void DisableFocusHighlight()
	{
		Focus(false);
	}

	/// <summary>
	/// Flash the texture for a split second.
	/// </summary>
	/// <param name="intensity"></param>
	[DialogueEvent("Flash")]
	public async Task Flash()
	{
		//if (aud != null)
		//	_ = DialogueEventSystem.TriggerEvent("PlaySystemSFX", aud);

		if (player.GetCurrentCharacter().FullName(false) != properties.GetName())
			return;

		var baseColor = charRenderer.material.color;
		var intensity = 5f;
		var elapsedTime = 0f;
		var endTime = 0.2f;
		var flashColor = new Color(intensity, intensity, intensity);

		while (elapsedTime < endTime)
		{
			var c = Mathf.Sin(elapsedTime / endTime * Mathf.PI);
			charRenderer.material.color = Color.Lerp(baseColor, flashColor, c);
			elapsedTime += Time.deltaTime;
			await Awaitable.EndOfFrameAsync();
		}

		charRenderer.material.color = baseColor;
	}
}
