using DREditor;
using DREditor.Dialogues.Events;
using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour, IModule
{
    public Action OnLoadStarted;
    public Action OnLoadFinished;

    public string PreviousSceneID { get; private set; }
    public Task Init()
    {
        DialogueEventSystem.RegisterEvents(this);
        return Task.CompletedTask;
    }

    [DialogueEvent("LoadRoom")]
    private void LoadRoom(string addressablePath)
    {
        LoadScene(addressablePath, null);
    }

    public async void LoadScene(AssetReference sceneReference, Action<AsyncOperationHandle<SceneInstance>> onLoadCallback)
    {
        //if (SceneManager.GetActiveScene().name != "System")
        //    SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

        //transition simulation
        await Awaitable.WaitForSecondsAsync(0.5f);

        var currentSceneHandle = Addressables.LoadSceneAsync(sceneReference);
        currentSceneHandle.Completed += async ctx =>
        {
            onLoadCallback?.Invoke(ctx);

            SceneManager.SetActiveScene(ctx.Result.Scene);
            Debug.Log($"DREditor <color=blue>(SceneLoader) </color>: Loaded {ctx.Result.Scene.name}.");
            //transition simulation
            await Awaitable.WaitForSecondsAsync(0.5f);
            InitializeRoom();
            OnLoadFinished?.Invoke();
        };

        OnLoadStarted?.Invoke();
    }

    public async void LoadScene(string path, Action<AsyncOperationHandle<SceneInstance>> onLoadCallback)
    {
        OnLoadStarted?.Invoke();

        //transition simulation
        await Awaitable.WaitForSecondsAsync(0.5f);

        var currentSceneHandle = Addressables.LoadSceneAsync(path);
        currentSceneHandle.Completed += async ctx =>
        {
            onLoadCallback?.Invoke(ctx);

            SceneManager.SetActiveScene(ctx.Result.Scene);
            Debug.Log($"DREditor <color=blue>(SceneLoader) </color>: Loaded {ctx.Result.Scene.name}.");
            //transition simulation
            await Awaitable.WaitForSecondsAsync(0.5f);
            InitializeRoom();
            OnLoadFinished?.Invoke();
        };

    }

    private void InitializeRoom()
    {
        var room = GameObject.FindGameObjectWithTag("Room");
        if (room != null && room.TryGetComponent<DRRoomSetup>(out var setup))
        {
            setup.SetupScene();
        }
    }
}