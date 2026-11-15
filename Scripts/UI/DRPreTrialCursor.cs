using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using DREditor;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using System;

public class DRPreTrialCursor : MonoBehaviour
{
	[Module] private ProjectwidePlayerInput playerInput;
	[Module] private SceneLoader sceneLoader;
	[Module] private DRRoomSetup roomSetup;
	[Module] private AudioPlayer audioPlayer;

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

	private bool validcheck;
	private Ray cursorRay;
	private IExaminable examinable;

	private Camera mainCamera;
	private Animator cursorAnimator;

	private void Awake()
	{
		DRModuleSystem.Inject(this);

		cursorAnimator = GetComponent<Animator>();

		sceneLoader.OnLoadFinished += OnSceneLoadFinished;
	}

	private void OnSceneLoadFinished()
	{
		DRModuleSystem.Inject(this);
		OnEnable();
	}

	private void Start()
	{
		mainCamera = Camera.main;
		NamePanel.alpha = 0f;

		DialogueRuntimeData.DialogueStarted += OnStartDialogue;
		DialogueRuntimeData.DialogueEnded += OnEndDialogue;

		Cursor.lockState = CursorLockMode.Locked;
	}

	private void OnStartDialogue(Dialogue dia)
	{
		gameObject.SetActive(false);
	}
	
	private void OnEndDialogue()
	{
		gameObject.SetActive(true);
	}

	private void OnLookPerformed(InputAction.CallbackContext ctx)
	{
		if (!DialogueRuntimeData.InDialog)
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

		Debug.Log("DREditor (DRPreTrialCursor): Turning on Cursor!");

		playerInput.RegisterInput(LookAction, OnLookPerformed);
		playerInput.RegisterInput(LookAction, OnLookCanceled, ProjectwidePlayerInput.ActionFlag.cancelled);
		playerInput.RegisterInput(AcceptAction, OnSelect);

		reticlePosition = new Vector2(Screen.currentResolution.width / 2f, Screen.currentResolution.height / 2f);

		switch (roomSetup.SceneType)
		{
			case SceneType.PointAndClick:
				Mode = CursorMode.Point;
				break;
			case SceneType.FirstPerson:
				Mode = CursorMode.FreeRoam;
				break;
		}
	}

	private void OnDisable()
	{
        playerInput.UnregisterInput(LookAction, OnLookPerformed);
        playerInput.UnregisterInput(LookAction, OnLookCanceled, ProjectwidePlayerInput.ActionFlag.cancelled);
        playerInput.UnregisterInput(AcceptAction, OnSelect);
    }

	private void OnDestroy()
	{
		DialogueRuntimeData.DialogueStarted -= OnStartDialogue;
		DialogueRuntimeData.DialogueEnded -= OnEndDialogue;
	}

	private void Update()
	{
		if (mainCamera == null)
			return;

		if (Mode != CursorMode.FreeRoam)
		{
			reticlePosition += rawLook;
			reticlePosition = new Vector2(Mathf.Clamp(reticlePosition.x, 0f, Screen.currentResolution.width), Mathf.Clamp(reticlePosition.y, 0f, Screen.currentResolution.height));
			Reticle.anchoredPosition = reticlePosition;
		}

		cursorRay = mainCamera.ViewportPointToRay(new Vector3(reticlePosition.x / Screen.currentResolution.width, reticlePosition.y / Screen.currentResolution.height));
		var cursorHit = new RaycastHit[5];
		var amount = Physics.RaycastNonAlloc(cursorRay, cursorHit, Mode == CursorMode.FreeRoam ? RaycastDistance : Mathf.Infinity, RaycastMask);

		if (amount <= 0)
		{
			if (examinable == null)
				return;
			
			NamePanel.alpha = 0f;
			cursorAnimator.SetBool("Highlight", false);
			return;
		}

		if (cursorHit[0].collider.gameObject.layer == 3)
			return;

		for (int i = 0; i < cursorHit.Length; i++)
		{
			try
			{
				//bool validcheck;
				try
				{
					Renderer rend = cursorHit[i].transform.GetComponent<Renderer>();
					Texture2D tex = rend.material.mainTexture as Texture2D;
					Vector2 pixelUV = cursorHit[i].textureCoord;
					pixelUV.x *= tex.width;
					pixelUV.y *= tex.height;

					validcheck = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y).a != 0f;
				}
				catch
				{
					validcheck = true;
				}

				if (validcheck)
				{
					examinable = cursorHit[i].transform.gameObject.GetComponent<IExaminable>();

					var name = examinable.GetName();
					NameText.text = name;
					NamePanel.alpha = !string.IsNullOrWhiteSpace(name) ? 0.9f : 0f;

					cursorAnimator.SetBool("Highlight", true);
					return;
				}
				else
				{
					NamePanel.alpha = 0f;
					cursorAnimator.SetBool("Highlight", false);
					continue;
				}
			}
			catch (UnityException)
			{
				NamePanel.alpha = 0f;
				cursorAnimator.SetBool("Highlight", false);
				validcheck = false;
				return;
			}
			catch (NullReferenceException)
			{
				NamePanel.alpha = 0f;
				cursorAnimator.SetBool("Highlight", false);
				validcheck = false;
				return;
			}
		}

		NamePanel.alpha = 0f;
		cursorAnimator.SetBool("Highlight", false);
	}

	private void OnSelect(InputAction.CallbackContext ctx)
	{
		if (!validcheck)
			return;

        if (!DialogueRuntimeData.InDialog && !DialogueRuntimeData.InMenu && !DialogueRuntimeData.IsWriting && !UIFade.OnScreenFade)
        {
            TriggerSound(0);
            cursorAnimator.SetTrigger("Click");

            //Pass: Use ClickAnim as Animation Event
            //ClickAnim();
        }
    }

	public void ClickAnim() //Used as Animation Event
	{
		try
		{
			examinable?.Examine();
		}
		catch (NullReferenceException)
		{
			Debug.LogError("DREditor (DRPreTrialCursor): Inexaminable!");
		}
	}

	private void TriggerSound(int mode)
	{
        audioPlayer.OnPlaySFX(SFX[mode]);
        //_ = DialogueEventSystem.TriggerEvent("PlaySystemSFX", SFX[mode]);
	}
}
