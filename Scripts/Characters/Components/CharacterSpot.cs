using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DREditor;
using DREditor.Characters;
using DREditor.Dialogues;
using UnityEngine.AddressableAssets;
using System;

[RequireComponent(typeof(Animation))]
public class CharacterSpot : MonoBehaviour
{
    private const string CHAR_ENABLE = "Char_Enable";
    private const string CHAR_DISABLE = "Char_Leave";

    private MapSetup AD;
    private Animation Anim;

    [SerializeField] private int SpotID;
    [SerializeField] private GameObject Character;

    private bool tempCharacter;

    private void Awake()
    {
        if (AD == null) AD = GameObject.FindGameObjectWithTag("Director").GetComponent<MapSetup>();
        Anim = GetComponent<Animation>();
    }

    public bool AvailableSlot => Character == null;
    public GameObject CurrentCharacter => Character;
    public bool TemporaryCharacter => tempCharacter;

    public void LoadCharacter(int clusterID, Character character, Dialogue[] dialogues, bool temp, Action<GameObject> finishcallback = null)
    {
        //if (CharacterLog.LoadedCharacters.ContainsKey(character))
        //{
        //    Debug.Log($"{character.FullName(false)} is already loaded.");
        //    return;
        //}

        var handle = Addressables.LoadAssetAsync<GameObject>(character.CharacterPrefab);
        handle.Completed += ctx =>
        {
            var loaded = Instantiate(handle.Result, transform.GetChild(0));
            var loadedProperties = loaded.GetComponent<DRBillboardProperties>();

            tempCharacter = temp;
            loaded.name = handle.Result.name;
            loaded.transform.SetParent(transform.GetChild(0));
            loadedProperties.ClusterID = clusterID;
            loaded.GetComponent<DRBillboardProperties>().AddThreads(dialogues);

            switch (AD.SceneType)
            {
                case SceneType.PointAndClick:
                    Anim.Play(CHAR_ENABLE);
                    break;
                default:
                    break;
            }

            CharacterLog.LoadedCharacters.Add(character, loaded);
            finishcallback?.Invoke(handle.Result);
            Addressables.Release(handle);
        };
        
        //StartCoroutine(loadchara(clusterID, character, dialogues, finishcallback));
    }

    public void ChangeCharacter(GameObject chara)
    {
        Character = chara;
        Anim.Play(CHAR_ENABLE);
    }

    public void Disable()
    {
        StartCoroutine(DisableAnim());
    }

    private IEnumerator DisableAnim()
    {
        if (Character == null) Character = gameObject.transform.GetChild(0).GetChild(0).gameObject;

        Anim.Play(CHAR_DISABLE);

        yield return new WaitForSeconds(Anim.GetClip(CHAR_DISABLE).length);

        Destroy(Character);
        yield return null;
    }
}