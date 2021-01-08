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
        //Commented code below will be used instead if ever Unity supports
        //serializing ValueTuples. I'm going to use the h**l out of it. 
        //                                                -SeleniumSoul

        //public ValueTuple<int, Vector3> CCFValue;

        public CCFTuple CCFValue;
    }
}