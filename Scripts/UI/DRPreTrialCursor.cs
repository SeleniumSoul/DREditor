using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using System;

public class DRPreTrialCursor : MonoBehaviour
{
    public enum CursorMode { Point, FreeRoam }

    [Header("Cursor")]
    [SerializeField] private CursorMode Mode;

    [SerializeField] private float ReticleSensitivity;
    [SerializeField] private bool cursorCenterLock;
    [SerializeField] private AudioClip[] SFX = new AudioClip[2];

    [Header("Raycast")]
    [SerializeField] private float RaycastDistance = 5f;
    [SerializeField] private LayerMask RaycastMask;

    [Header("References")]
	[SerializeField] private RectTransform Reticle;
	[SerializeField] private TextMeshProUGUI NameText;
	[SerializeField] private CanvasGroup NamePanel;

    [Header("Controls")]
    [SerializeField] private InputActionReference LookAction;
    [SerializeField] private InputActionReference AcceptAction;

    public bool CenterLock
    {
        get { return cursorCenterLock; }
        set
        {
            if (value) reticlePosition = new Vector2(Screen.currentResolution.width / 2f, Screen.currentResolution.height / 2f);
            cursorCenterLock = value;
        }
    }

    private Vector2 rawLook;
    private Vector2 reticlePosition;

    private Ray cursorRay;
    private DRBillboardProperties bbProperties;

    private Camera mainCamera;
    //private Animator cursorAnimator;

    //Input Actions
    private InputActionMap actionMap;

    private void Awake()
	{
        actionMap = InputSystem.actions.FindActionMap(LookAction.action.actionMap.name);
    }

    private void Start()
	{
		//if (DialoguePlayer.current.DRCursor == null)
        //   DialoguePlayer.current.DRCursor = gameObject;

		mainCamera = Camera.main;
		//cursorAnimator = GetComponent<Animator>();
		NamePanel.alpha = 0f;

		//Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

    private void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        if (!DialogueRuntimeData.InDialog && !DialogueRuntimeData.InMenu)
        {
            rawLook = ReticleSensitivity * ctx.ReadValue<Vector2>();
        }
    }
	private void OnLookCanceled(InputAction.CallbackContext ctx) => 
        rawLook = Vector2.zero;

    private void OnEnable()
	{
        if (mainCamera == null)
            mainCamera = Camera.main;

        LookAction.action.performed += OnLookPerformed;
        LookAction.action.canceled += OnLookCanceled;

        reticlePosition = new Vector2(Screen.currentResolution.width / 2f, Screen.currentResolution.height / 2f);
    }

	private void OnDisable()
	{
        LookAction.action.performed -= OnLookPerformed;
        LookAction.action.canceled -= OnLookCanceled;
    }

	private void PassThreadToHandler()
	{
		if (bbProperties) DialogueRuntimeData.DialogueAsset = bbProperties.DialogThreadPass();
		else Debug.LogError("DREditor (PassThreadToHandler): Your code works, but I can't seem to find _properties... :(");
	}

    private void Update()
    {
        if (Mode != CursorMode.FreeRoam)
        {
            reticlePosition += rawLook;
            reticlePosition = new Vector2(Mathf.Clamp(reticlePosition.x, 0f, Screen.currentResolution.width), Mathf.Clamp(reticlePosition.y, 0f, Screen.currentResolution.height));
            Reticle.anchoredPosition = reticlePosition;
        }

        cursorRay = mainCamera.ViewportPointToRay(new Vector3(reticlePosition.x / Screen.currentResolution.width, reticlePosition.y / Screen.currentResolution.height));
        var cursorHit = new RaycastHit[5];
        var amount = Physics.RaycastNonAlloc(cursorRay, cursorHit, Mode == CursorMode.FreeRoam ? RaycastDistance : Mathf.Infinity, RaycastMask);

        if (amount <= 0) return;
        if (cursorHit[0].collider.gameObject.layer == 3) return;

        for (int i = 0; i < cursorHit.Length; i++)
        {
            try
            {
                Renderer rend = cursorHit[i].transform.GetComponent<Renderer>();
                Texture2D tex = rend.material.mainTexture as Texture2D;
                Vector2 pixelUV = cursorHit[i].textureCoord;
                pixelUV.x *= tex.width;
                pixelUV.y *= tex.height;

                if (tex.GetPixel((int)pixelUV.x, (int)pixelUV.y).a != 0f)
                {
                    NameText.text = cursorHit[i].transform.name;
                    bbProperties = cursorHit[i].transform.gameObject.GetComponent<DRBillboardProperties>();
                    NamePanel.alpha = 0.9f;
                    //cursorAnimator.SetBool("Highlight", true);

                    //if (AcceptAction.action.triggered &&
                    //    !DialogueRuntimeData.InDialog &&
                    //    !DialogueRuntimeData.InMenu &&
                    //    !DialogueRuntimeData.IsWriting &&
                    //    !UIFade.OnScreenFade)
                    //{
                    //    DialogueRuntimeData.InDialog = true;
                    //    TriggerSound(0);
                    //    //cursorAnimator.SetTrigger("Click");

                    //    //Pass: Use ClickAnim as Animation Event
                    //}
                    return;
                }
                else
                {
                    NamePanel.alpha = 0f;
                    //cursorAnimator.SetBool("Highlight", false);
                    continue;
                }
            }
            catch (UnityException)
            {
                NamePanel.alpha = 0f;
                //cursorAnimator.SetBool("Highlight", false);
                return;
            }
            catch (NullReferenceException)
            {
                NamePanel.alpha = 0f;
                //cursorAnimator.SetBool("Highlight", false);
                return;
            }
        }

        NamePanel.alpha = 0f;
        //cursorAnimator.SetBool("Highlight", false);
    }

	public void ClickAnim() //Used as Animation Event
	{
		if (bbProperties != null && bbProperties.DialogThreadList[0] != null && bbProperties.DialogThreadList.Count != 0)
		{
			PassThreadToHandler();
			DialogueEventSystem.TriggerEvent("StartDialogue");
		}
		else
		{
			DialogueRuntimeData.InDialog = false;
			Debug.LogError("DREditor (DRPNCCursor): Dialogue File Missing! Please set it up on DR Sprite Properties.");
		}
	}

	private void TriggerSound(int mode)
	{
        DialogueEventSystem.TriggerEvent("PlaySystemSFX", SFX[mode]);
	}
}
