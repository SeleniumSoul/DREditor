//CustomEvent Dialogue Event script by SeleniumSoul for DREditor.
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DREditor.Dialogues.Events
{
    [Serializable]
    public struct CETuple
    {
        public string EventName;
    }

    [Serializable]
    public class CustomEvent : IDialogueEvent
    {
        public CETuple CEValue;
    }
}
