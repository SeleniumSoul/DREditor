using System.Collections.Generic;
using UnityEngine;
using DREditor.Characters;
using UnityEditor;

public class CharacterCluster : MonoBehaviour
{
    [SerializeField] private CharacterSpot[] TemporarySpots;
    [SerializeField] private CharacterSpot[] CharacterSpots;

    private List<GameObject> tempchara = new();

    public void Start()
    {
        
    }

    public void LoadInitialCharacters(int id, CharacterClusterSetup setup)
    {
        for (int i = 0; i < setup.placements.Length; i++)
        {
            CharacterSpots[i].LoadCharacter(id, setup.placements[i].character, setup.placements[i].dialogues, false);
        }
    }

    public void LoadTempCharacter(int id, Character character)
    {
        for (int i = 0; i < CharacterSpots.Length; i++)
        {
            CharacterSpots[i].LoadCharacter(id, character, null, true);
        }
    }

    public void UnloadTempChara()
    {
        foreach (var chara in CharacterSpots)
        {
            //if (chara.)
        }
    }

    #if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        var labelStyle = new GUIStyle()
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 30,
            normal = new GUIStyleState() { textColor = Color.white },
        };

        for (int tmp = 0; tmp < TemporarySpots.Length; tmp++) {
            Handles.Label(TemporarySpots[tmp].transform.position, $"T{tmp}", labelStyle);
        }

        for (int sp = 0; sp < CharacterSpots.Length; sp++)
        {
            Handles.Label(CharacterSpots[sp].transform.position, $"{sp}", labelStyle);
        }
    }
    #endif
}
