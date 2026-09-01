// DR Game Manager by SeleniumSoul for DREditor.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace DREditor
{
    public enum AreaState
    {
        PointAndClick, FirstPerson
    }

    public enum PlayerState
    {
        InGame, InUserInterface
    }

    public class DRGameManager : MonoBehaviour
    {
        public static DRGameManager current;

        [SerializeField] private string Protagonist;

        public GameState CurrentGameState => currentGameState;
        private GameState currentGameState;

        //private AreaData currentAreaData;

        [Header("Modules")]
        private SceneLoader sceneLoader;

        private void Awake()
        {
            if (current == null)
                current = this;
            else
                Destroy(this);

            sceneLoader = GetComponent<SceneLoader>();

        }

        private void ChangeGameState(GameState newState, bool initialize = true)
        {
            currentGameState = newState;
            if (initialize)
                currentGameState.Initialize();
        }

        /// <summary>
        /// Load a new area.
        /// </summary>
        /// <param name="newArea">The new area to load.</param>
        public void LoadArea(AssetReference id, float transition = 0.5f)
        {

        }
    }

}
