using UnityEngine;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using System.Threading.Tasks;

namespace DREditor
{
	public enum SceneType
	{
		PointAndClick,
		FirstPerson,
		ClassTrial
	}

	/// <summary>
	/// RoomSetup are per scene/room. Do NOT duplicate.
	/// </summary>
	[DefaultExecutionOrder(-100)]
	public class DRRoomSetup : MonoBehaviour
	{
		[SerializeField] private Animator RoomAnimator;

		[Header("Parameters")]
		[SerializeField] private string roomName;
		[SerializeField] private SceneType sceneType;
		[SerializeField] private Scenario scenario;

		[Header("Character Spots")]
		[SerializeField] private CharacterCluster[] clusters;

		public string RoomName => roomName;
		public SceneType SceneType => sceneType;

		private void Awake()
		{
			DialogueEventSystem.RegisterEvents(this);
			DRModuleSystem.RegisterModule(this);
		}

		private void OnDestroy()
		{
			DialogueEventSystem.DeregisterEvents(this);
			DRModuleSystem.UnregisterModule(this);
		}

		public void SetupScene()
		{
			if (scenario != null && scenario.StartDialogue != null)
				_ = DialogueEventSystem.TriggerEvent("StartDialogue", scenario.StartDialogue);
			else
				_ = PlayRoomIntro();
		}

		[DialogueEvent("ShowMap")]
		public async Task PlayRoomIntro()
		{
			await DialogueEventSystem.TriggerEvent("SetSayboxActive", false);
			
			if (RoomAnimator != null)
			{
				Debug.Log("Playing Room Animation...");
				RoomAnimator.Play("intro");
				while (!RoomAnimator.GetCurrentAnimatorStateInfo(0).IsName("Base Layer.intro") ||
					   RoomAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
					await Task.Yield();
			}

			if (scenario != null)
			{
				//_ = DialogueEventSystem.TriggerEvent("PlayBGM", new CMTuple { MusicNum = scenario.MusicIndex });
				await SetupClusters();
			}

			await DialogueEventSystem.TriggerEvent<bool>("SetFreeRoamHUDActive", true);

			if (!DialogueRuntimeData.InDialog)
			{
				_ = DialogueEventSystem.TriggerEvent("SetCursorActive", true);
			}
		}

		public async Task SetupClusters()
		{
			for (int i = 0; i < scenario.Clusters.Length; i++)
			{
				if (i >= clusters.Length) return;
				await clusters[i].SetupCluster(i, scenario.Clusters[i]);
			}
		}

		public CharacterCluster GetCluster(int id)
		{
			return clusters[id];
		}
	}

}
