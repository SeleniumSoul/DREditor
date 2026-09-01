using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class PanelCamRT : MonoBehaviour
{
	[SerializeField] private bool Screenshot;
	[SerializeField] private Camera _PanelCam;

	[Header("Render Texture")]
    [Range(0.2f, 2f)] public float RenderResolution = 1f;
    [SerializeField] private bool DynamicResolution = true;
	[SerializeField] FilterMode filterMode = FilterMode.Bilinear;
	private RenderTexture RT;
	private RectTransform canvasRect;
	private RawImage rawImage;

	private void Awake()
	{
		canvasRect = GetComponent<RectTransform>();
		rawImage = GetComponent<RawImage>();

        RT = new RenderTexture((int)(canvasRect.rect.width * RenderResolution), (int)(canvasRect.rect.height * RenderResolution), 32)
        {
            useDynamicScale = DynamicResolution,
            filterMode = filterMode
        };

        _PanelCam.targetTexture = RT;
        rawImage.texture = RT;
    }

	public void OnEnable()
	{
        if (!Screenshot) return;

        _PanelCam.transform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);
        UniversalRenderPipeline.SingleCameraRequest request = new();

        if (RenderPipeline.SupportsRenderRequest(_PanelCam, request))
        {
            request.destination = RT;
            RenderPipeline.SubmitRenderRequest(_PanelCam, request);
        }

        rawImage.texture = RT;
    }
}
