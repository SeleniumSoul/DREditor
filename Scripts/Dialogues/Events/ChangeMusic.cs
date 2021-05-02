//Change Character Focus Dialogue Event script by SeleniumSoul for DREditor.

using System;

namespace DREditor.Dialogues.Events
{
    [Serializable]
    public struct CMTuple
    {
        public int MusicNum;
    }

    [Serializable]
    public class ChangeMusic : IDialogueEvent
    {
        //Commented code below will be used instead if ever Unity supports
        //serializing ValueTuples. I'm going to use the h**l out of it. 
        //                                                -SeleniumSoul
        
        //public ValueTuple<int> CMValue;

        public CMTuple CMValue;
    }
}