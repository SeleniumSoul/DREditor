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
        public CMTuple CMValue;
    }
}