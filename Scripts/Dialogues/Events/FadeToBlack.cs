//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;

namespace DREditor.Dialogues.Events
{
    [Serializable]
    public struct FTBTuple
    {
        public float FadeSpeed;
    }

    [Serializable]
    public class FadeToBlack : IDialogueEvent
    {
        public FTBTuple FTBValue;
    }
}
