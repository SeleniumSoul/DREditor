using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using DREditor.Characters;
using DREditor.Dialogues;

[Serializable]
public struct CharacterPlacement
{
    public Character character;
    public Dialogue[] dialogues;
}

[Serializable]
public struct CharacterClusterSetup
{
    public CharacterPlacement[] placements;

    public List<AssetReferenceT<GameObject>> GetAllCharacterAssetReferences()
    {
        var refList = new List<AssetReferenceT<GameObject>>();

        foreach (var p in placements)
        {
            try
            {
                refList.Add(p.character.CharacterPrefab);
            }
            catch { continue; }
        }

        return refList;
    }
}

[CreateAssetMenu(fileName = "SceneParameters", menuName = "DREditor/Scene Parameters")]
public class SceneParameters : ScriptableObject
{
    [SerializeField] public CharacterClusterSetup[] Clusters;
    [SerializeField] public Dialogue StartDialogue;
}
