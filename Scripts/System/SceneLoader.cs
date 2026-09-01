using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static Action<AsyncOperationHandle<SceneInstance>> OnLoadStarted;

    public void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneAt(0));
    }

    public AsyncOperationHandle<SceneInstance> LoadScene(AssetReference sceneReference, Action<AsyncOperationHandle<SceneInstance>> onLoadCallback)
    {
        if (SceneManager.GetActiveScene().name != "System")
            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());

        var currentSceneHandle = Addressables.LoadSceneAsync(sceneReference, LoadSceneMode.Additive);
        currentSceneHandle.Completed += ctx =>
        {
            onLoadCallback(ctx);
            SceneManager.SetActiveScene(ctx.Result.Scene);
        };

        OnLoadStarted?.Invoke(currentSceneHandle);
        return currentSceneHandle;
    }
}