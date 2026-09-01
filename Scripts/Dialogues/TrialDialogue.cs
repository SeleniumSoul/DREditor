using System;
using System.Collections.Generic;

using UnityEngine;

namespace DREditor.Dialogues
{
    [CreateAssetMenu(menuName = "DREditor/Dialogues/TrialDialogue", fileName = "New Trial Dialogue")]
    public class TrialDialogue : DialogueBase
    {
        public List<TrialLine> Lines = new List<TrialLine>();
    }

    [Serializable]
    public class TrialLine : Line
    {
        public int vfxIdx;
        public int camAnimIdx;
    }
}