using System.Collections.Generic;
using UnityEngine;
using DREditor.Characters;
using UnityEditor;
using System.Threading.Tasks;

public class CharacterCluster : MonoBehaviour
{
    //[SerializeField] private CharacterSpot[] TemporarySpots;
    [SerializeField] private CharacterSpot[] CharacterSpots;
    [SerializeField] private float ClusterLoadDelay = 0.3f;

    private List<GameObject> tempchara = new();

    public async Task SetupCluster(int id, CharacterClusterSetup setup)
    {
        if (setup.placements.Length > CharacterSpots.Length)
            Debug.LogWarning("There are more characters needed to load than the available spots! Not all characters can be loaded!");

        List<Task> spotTasks = new();

        for (int i = 0; i < setup.placements.Length; i++)
        {
            spotTasks.Add(CharacterSpots[i].LoadCharacter(id, setup.placements[i].character, setup.placements[i].dialogues, false));
            await Awaitable.WaitForSecondsAsync(ClusterLoadDelay);
        }

        await Task.WhenAll(spotTasks);
    }

    public async void LoadTempCharacter(int id, Character character)
    {
        for (int i = 0; i < CharacterSpots.Length; i++)
        {
            _ = CharacterSpots[i].LoadCharacter(id, character, null, true);
            await Task.Delay(500);
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

        //if (TemporarySpots == null)
        //    return;

        //for (int tmp = 0; tmp < TemporarySpots.Length; tmp++) {
        //    Handles.Label(TemporarySpots[tmp].transform.position, $"T{tmp}", labelStyle);
        //}

        if (CharacterSpots == null)
            return;

        for (int sp = 0; sp < CharacterSpots.Length; sp++)
        {
            Handles.Label(CharacterSpots[sp].transform.position, $"{sp}", labelStyle);
        }
    }
    #endif
}
