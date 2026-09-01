//UI Screen List Script by SeleniumSoul for DREditor
//April 2021

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIContainer : MonoBehaviour
{
    [SerializeField] private string id;
    [SerializeField] private bool LoadAllOnAwake;
    [SerializeField] public List<AssetReference> UIScreens;

    private List<GameObject> LoadedScreens = new();

    private void Awake()
    {
        if (!LoadAllOnAwake) return;

        foreach (var ui in UIScreens)
        {
            LoadUIScreen(ui);
        }
    }

    public void LoadUIScreen(int selection)
    {
        var screenLoad = Addressables.InstantiateAsync(UIScreens[selection]);
        screenLoad.Completed += ctx =>
        {
            var result = ctx.Result;
            result.transform.SetParent(transform, true);
            LoadedScreens.Add(result);
        };
    }

    public void LoadUIScreen(AssetReference reference)
    {
        var screenLoad = Addressables.InstantiateAsync(reference);
        screenLoad.Completed += ctx =>
        {
            var result = ctx.Result;
            result.transform.SetParent(transform, true);
            LoadedScreens.Add(result);
        };
    }
}
