//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;

namespace DREditor.Dialogues.Events
{
    [Serializable]
    public struct CWPTuple
    {
        public string PatternName;
    }

    [Serializable]
    public class ChangeWindowPattern : IDialogueEvent
    {
        //public ValueTuple<string> CWPValue;

        public CWPTuple CWPValue;
    }
}
