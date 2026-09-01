//StartScene

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;

namespace DREditor
{
    public enum SceneType
    {
        PointAndClick,
        FirstPerson,
        ClassTrial
    }

    /// <summary>
    /// Assistant Directors are per scene/map.
    /// </summary>
    public class MapSetup : MonoBehaviour
    {
        static public MapSetup current;
        static public bool PlayingAnimation;

        [Header("Parameters")]
        [SerializeField] private SceneType sceneType;
        [SerializeField] private SceneParameters sceneParameters;

        [Header("Character Spots")]
        [SerializeField] private CharacterCluster[] clusters;

        public SceneType SceneType { get { return sceneType; } }

        private void Awake()
        {
            if (current == null) current = this;
            else Destroy(this);

            DialogueEventSystem.StartListening("SetupScene", SetupScene);   
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            if (SceneType == SceneType.PointAndClick)
                DialogueEventSystem.StartListening("ShowMap", PlayDirector);

            ScreenSetup();
            
            if (sceneParameters != null)
                SetupScene();
        }

        private void OnDestroy()
        {
            DialogueEventSystem.StopListening("SetupScene", SetupScene);
            DialogueEventSystem.StopListening("ShowMap", PlayDirector);
        }

        private void ScreenSetup()
        {
            switch (SceneType)
            {
                case SceneType.PointAndClick:
                    DRScreenManager.current.SetGroupActive(1, false);
                    DRScreenManager.current.SetGroupActive(2, false);
                    DRScreenManager.current.SetGroupActive(3, false);
                    DialoguePlayer.current.DRCursor = DRScreenManager.current.PointAndClickCursor;
                    break;
                case SceneType.FirstPerson:
                    DRScreenManager.current.SetGroupActive(1, true);
                    DRScreenManager.current.SetGroupActive(2, true);
                    DRScreenManager.current.SetGroupActive(3, true);
                    DialoguePlayer.current.DRCursor = DRScreenManager.current.FirstPersonCursor;
                    break;
                default:
                    DRScreenManager.current.SetGroupActive(1, true);
                    DRScreenManager.current.SetGroupActive(2, true);
                    DRScreenManager.current.SetGroupActive(3, true);

                    DialoguePlayer.current.DRCursor = DRScreenManager.current.FirstPersonCursor;
                    break;
            }
        }

        private void SetupScene()
        {
            for (int i = 0; i < sceneParameters.Clusters.Length; i++)
            {
                if (i >= clusters.Length) return;
                clusters[i].LoadInitialCharacters(i, sceneParameters.Clusters[i]);
            }

            if (sceneParameters.StartDialogue != null)
            {
                DialogueRuntimeData.DialogueAsset = sceneParameters.StartDialogue;
                DialogueEventSystem.TriggerEvent("StartDialogue");
            }
        }

        public void PlayDirector(object unused = null)
        {
            SetControlActive(false);
        }

        public void SetControlActive(bool active)
        {
            DRScreenManager.current.SetGroupActive(1, active);
            DRScreenManager.current.SetGroupActive(2, active);
            DRScreenManager.current.SetGroupActive(3, active);
        }

        public CharacterCluster GetCluster(int id)
        {
            return clusters[id];
        }
    }

}
