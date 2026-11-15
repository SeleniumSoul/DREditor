using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class PanelCamRT : MonoBehaviour
{
	[SerializeField] private bool Screenshot;
	[SerializeField] private Camera PanelCamera;

	[Header("Render Texture")]
	[Range(0.2f, 2f)] public float RenderResolution = 1f;
	[SerializeField] private bool DynamicResolution = true;
	[SerializeField] FilterMode filterMode = FilterMode.Bilinear;

	//Stupid hack because this script is blind af
	[SerializeField] private DRDialoguePanels panels;

	private Camera mainCamera;
	private RenderTexture RT;
	private RectTransform canvasRect;
	private RawImage rawImage;

	private void Awake()
	{
		canvasRect = GetComponent<RectTransform>();
		rawImage = GetComponent<RawImage>();

		mainCamera = panels.MainCam;
	}

	public void OnEnable()
	{
		if (Screenshot)
		{
			PanelCamera.transform.SetPositionAndRotation(mainCamera.transform.position, mainCamera.transform.rotation);
			CreateRT(true);
			UniversalRenderPipeline.SingleCameraRequest request = new();

			if (RenderPipeline.SupportsRenderRequest(PanelCamera, request))
			{
				request.destination = RT;
				RenderPipeline.SubmitRenderRequest(PanelCamera, request);
			}
		}
		else
		{
			CreateRT(false);
			PanelCamera.enabled = true;
		}
	}

	public void OnDisable()
	{
		PanelCamera.enabled = false;
	}

	private void CreateRT(bool fullscreen)
	{
		var resolution = fullscreen ?
			new Vector2Int(Screen.width, Screen.height) :
			new Vector2Int((int)(canvasRect.rect.width * RenderResolution), (int)(canvasRect.rect.height * RenderResolution));

		RT = new RenderTexture(resolution.x, resolution.y, 32)
		{
			useDynamicScale = DynamicResolution,
			filterMode = filterMode
		};

		PanelCamera.targetTexture = RT;
		rawImage.texture = RT;
	}
}
