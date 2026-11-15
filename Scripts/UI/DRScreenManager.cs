//DR Screen Manager by SeleniumSoul for DREditor
//March 2020

using DREditor;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class DRScreenManager : MonoBehaviour, IModule
{
	private const string UIContainerName = "UIContainer";

	[Module] private SceneLoader loader;

	[Header("HUD")]
	[SerializeField] private AssetReferenceGameObject ChapterDisplay;
	[SerializeField] private AssetReferenceGameObject MusicVisualizer;
	[SerializeField] private AssetReferenceGameObject Cursor;
	
	[Header("Dialogue")]
	[SerializeField] private AssetReferenceGameObject SayBox;
	[SerializeField] private AssetReferenceGameObject DialoguePanels;

	[Header("Fullscreen")]
	[SerializeField] private AssetReferenceGameObject MainMenu;

	[Header("Misc")]
	[SerializeField] private AssetReferenceGameObject LoadingScreen;
	[SerializeField] private AssetReferenceGameObject BlackScreen;
    [SerializeField] private AssetReferenceGameObject DebugProfiler;
    [SerializeField] private Material FadeMaterial;

	private readonly Dictionary<string, AsyncOperationHandle<GameObject>> LoadedUI = new();
	private GameObject uiContainer;
	private Coroutine FadeRoutine;

	//Toggle Parameters
	private bool blcheck = false;
	private bool hlpcheck = false;

	public async Task Init()
	{
        DialogueEventSystem.RegisterEvents(this);
		DRModuleSystem.Inject(this);

		uiContainer = new GameObject(UIContainerName);
		uiContainer.transform.SetParent(transform);

        await SetDialogueUIActive(true);
        await LoadUI("BlackScreen", BlackScreen);
    }

	private void OnEnable()
	{
		DialogueRuntimeData.DialogueStarted += OnStartDialogue;
		DialogueRuntimeData.DialogueEnded += OnEndDialogue;

		loader.OnLoadStarted += OnSceneLoad;
        loader.OnLoadFinished += OnSceneLoadFinished;

        _ = LoadUI("LoadScreen", LoadingScreen, false);
    }

    private void OnSceneLoad()
    {
		_ = LoadUI("LoadScreen", LoadingScreen);
    }

    private void OnSceneLoadFinished()
    {
        if (LoadedUI.TryGetValue("LoadScreen", out var cd))
            cd.Result.SetActive(false);
    }

    private void OnStartDialogue(Dialogue dialogue)
	{
		SetCursorActive(false);
	}
	private void OnEndDialogue()
	{
		SetCursorActive(true);
	}

	private void Start()
	{
        //TODO: Remove this from start
        _ = LoadUI("FPSDisplay", DebugProfiler);
    }

	private void OnHelp()
	{
		if (!blcheck)
		{
			hlpcheck = !hlpcheck;
			DialogueRuntimeData.InMenu = hlpcheck;
			//if (hlpcheck) Time.timeScale = 0f;
			//else Time.timeScale = 1f;
			//help?.SetActive(hlpcheck);
		}
	}

	private void OnBacklog()
	{
		if (hlpcheck)
		{
			blcheck = !blcheck;
			DialogueRuntimeData.InMenu = blcheck;
			//if (blcheck) Time.timeScale = 0f;
			//else Time.timeScale = 1f;
			//backlog?.SetActive(blcheck);
		}
	}

	public async void OnMenu()
	{
		await LoadFullScreenUI("MainMenu", MainMenu);
	}

	public void SetActiveAll(bool active)
	{
		foreach (var ui in LoadedUI)
		{
			ui.Value.Result.SetActive(active);
		}
	}

	/// <summary>
	/// Sets the Free Time HUD activation. 
	/// </summary>
	[DialogueEvent("SetFreeRoamHUDActive")]
	private async Task SetFreeRoamHUDActive(bool active)
	{
		string[] ToSetActive = new string[] { "ChapterDisplay", "MusicVisualizer", "SayBox" };

		if (active)
		{
			await LoadUI(ToSetActive[0], ChapterDisplay);
			await LoadUI(ToSetActive[1], MusicVisualizer);
		}
		else
		{
			foreach (string ui in ToSetActive)
			{
				if (LoadedUI.TryGetValue(ui, out var cd))
					cd.Result.SetActive(false);
			}
		}
	}

    [DialogueEvent("SetDialogueUIActive")]
    private async Task SetDialogueUIActive(bool active)
	{
        string[] ToSetActive = new string[] { "SayBox", "DialoguePanels" };

        if (active)
        {
            await LoadUI(ToSetActive[0], SayBox);
            await LoadUI(ToSetActive[1], DialoguePanels);
        }
        else
        {
            foreach (string ui in ToSetActive)
            {
                if (LoadedUI.TryGetValue(ui, out var cd))
                    cd.Result.SetActive(false);
            }
        }

	}

	/// <summary>
	/// Sets the Trial HUD activation. 
	/// </summary>
	public void SetTrialHUDActive(bool active)
	{
		//TODO: Run MusicPlayer DiaEvent here
		//TODO: Run ChapterDisplay DiaEvent here
	}

	private async Task LoadUI(string name, AssetReferenceT<GameObject> ui)
	{
        if (LoadedUI.TryGetValue(name, out var cd))
		{
            cd.Result.SetActive(true);
            return;
        }

		var uiInstance = Addressables.InstantiateAsync(ui, uiContainer.transform);
		await uiInstance.Task;

		if (uiInstance.IsValid() && uiInstance.Status == AsyncOperationStatus.Succeeded)
		{
			LoadedUI.Add(name, uiInstance);
		}
	}

    private async Task LoadUI(string name, AssetReferenceT<GameObject> ui, bool enabled)
    {
        if (LoadedUI.TryGetValue(name, out var cd))
        {
            cd.Result.SetActive(true);
            return;
        }

        var uiInstance = Addressables.InstantiateAsync(ui, uiContainer.transform);
        await uiInstance.Task;

        if (uiInstance.IsValid() && uiInstance.Status == AsyncOperationStatus.Succeeded)
        {
			uiInstance.Result.SetActive(enabled);
            LoadedUI.Add(name, uiInstance);
        }
    }

    public async Task LoadFullScreenUI(string name, AssetReferenceT<GameObject> asset)
	{
		SetActiveAll(false);

		if (!LoadedUI.ContainsKey(name))
		{
			await LoadUI(name, asset);
			return;
		}
		
		if (LoadedUI.TryGetValue(name, out var ui))
		{
			ui.Result.SetActive(true);
		}
	}

    [DialogueEvent("SetCursorActive")]
    private void SetCursorActive(bool active)
	{
		string cursorKey = "Cursor";

		if (active)
			_ = LoadUI(cursorKey, Cursor);
		else
		{
			if (LoadedUI.TryGetValue(cursorKey, out var cd))
				cd.Result.SetActive(active);
		}
	}

	[DialogueEvent("FadeScreen")]
	private void SetFade((bool active, float time, Color color) value)
	{
		//(bool active, float time, Color color) = (ValueTuple<bool, float, Color>)value;

		FadeMaterial.color = value.color;

		if (FadeRoutine != null)
			StopCoroutine(FadeRoutine);

		if (value.time == 0)
			FadeMaterial.SetFloat("_Opacity", value.active ? 1f : 0f);
		else
			FadeRoutine = StartCoroutine(FadeAnimation(value.time, value.active ? 1f : 0f));
	}

	IEnumerator FadeAnimation(float duration, float endalpha)
	{
		float _startTime = 0f;
		float _startOpacity = FadeMaterial.GetFloat("_Opacity");

		while (_startTime < duration)
		{
			FadeMaterial.SetFloat("_Opacity", Mathf.Lerp(_startOpacity, endalpha, _startTime / duration));
			_startTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}
}
