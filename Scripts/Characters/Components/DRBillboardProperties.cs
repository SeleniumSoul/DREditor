using System.Collections.Generic;
using UnityEngine;
using DREditor.Dialogues;

public class DRBillboardProperties : MonoBehaviour
{
    public int ClusterID;
    [SerializeField] private Vector3 CameraFocusPosition;
    [SerializeField] private List<Dialogue> DialogThread = new List<Dialogue>();

    private int DialogCounter = 0;

    public Vector3 CameraPosition { get { return CameraFocusPosition; } }
    public List<Dialogue> DialogThreadList { get { return DialogThread; } }
    public Dialogue DialogThreadPass()
    {
        if (DialogCounter < DialogThread.Count - 1) {
            DialogCounter++;
            return DialogThread[DialogCounter - 1];
        }
        else if(DialogCounter >= DialogThread.Count)
        {
            DialogCounter = DialogThread.Count;
            return DialogThread[DialogCounter];
        }
        else
        {
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
