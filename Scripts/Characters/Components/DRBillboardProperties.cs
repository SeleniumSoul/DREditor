using DREditor;
using DREditor.Dialogues;
using DREditor.Dialogues.Events;
using System.Collections.Generic;
using UnityEngine;

public class DRBillboardProperties : MonoBehaviour, IExaminable
{
	public int ClusterID;
	[SerializeField] private string characterName;
	[SerializeField] private Vector3 CameraFocusPosition;
    [SerializeField] private Quaternion CameraFocusRotation = Quaternion.identity;
    [SerializeField] private List<Dialogue> DialogThread = new();

	private int DialogCounter = 0;

	public Vector3 CameraPosition => CameraFocusPosition;
    public Quaternion CameraRotation => CameraFocusRotation;
    public List<Dialogue> DialogThreadList => DialogThread;
	public string GetName() => characterName;

	public async void Examine()
	{
		var dialogue = DialogThreadPass();

		if (dialogue != null)
			await DialogueEventSystem.TriggerEvent("StartDialogue", dialogue);
	}

	private Dialogue DialogThreadPass()
	{
		if (DialogThread.Count <= 0)
		{
			Debug.LogError($"No dialogues available for {characterName}.");
			return null;
		}
		if (DialogCounter < DialogThread.Count - 1) {
			DialogCounter++;
			Debug.Log($"{DialogThread[DialogCounter - 1].name}");
			return DialogThread[DialogCounter - 1];
		}
		else if(DialogCounter >= DialogThread.Count)
		{
			DialogCounter = DialogThread.Count;
			Debug.Log($"{DialogThread[DialogCounter].name}");
			return DialogThread[DialogCounter];
		}
		else
		{
			Debug.Log($"{DialogThread[DialogCounter].name}");
			return DialogThread[DialogCounter];
		}
	}

	public void AddThreads(Dialogue[] threads)
	{
		foreach (Dialogue thread in threads)
		{
			DialogThread.Add(thread);
		}
	}
}
