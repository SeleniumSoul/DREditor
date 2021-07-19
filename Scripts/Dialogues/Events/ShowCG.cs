//Show CG Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;

namespace DREditor.Dialogues.Events
{
    [Serializable]
    public struct SCGTuple
    {
        public GameObject CG;
    }

    [Serializable]
    public class ShowCG : IDialogueEvent
    {
        public SCGTuple SCGValue;
    }
}