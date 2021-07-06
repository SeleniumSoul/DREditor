//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;
using UnityEngine;

namespace DREditor.Dialogues.Events
{
    [Serializable]
    public struct CCFTuple
    {
        public int PanelNum;
        public Vector3 CamTrans;
    }

    [Serializable]
    public class ChangeCharacterFocus : IDialogueEvent
    {
        public CCFTuple CCFValue;
    }
}