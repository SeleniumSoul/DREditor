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

    public readonly List<AssetReferenceT<GameObject>> GetAllCharacterAssetReferences()
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

[CreateAssetMenu(fileName = "Scenario", menuName = "DREditor/Scenario")]
public class Scenario : ScriptableObject
{
    [SerializeField] private CharacterClusterSetup[] clusters;
    [SerializeField] private Dialogue startDialogue;
    [SerializeField] private int musicIndex;

    public CharacterClusterSetup[] Clusters => clusters;
    public Dialogue StartDialogue => startDialogue;
    public int MusicIndex => musicIndex;
}
