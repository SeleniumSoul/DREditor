//Show CG Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;

namespace DREditor.Dialogues.Events
{
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