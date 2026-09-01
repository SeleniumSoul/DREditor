using UnityEngine;

namespace DREditor
{
    public abstract class GameState
    {
        public virtual void Initialize() { }
    }

    public class TitleScreen : GameState
    {

    }

    public class RoamState : GameState
    {
        public enum RoamType
        {
            PointAndClick, FirstPerson
        }

        public RoamType currentRomeType { get; private set; }

        public void SetRoamType(RoamType roamType)
        {
            currentRomeType = roamType;
        }
    }

    //public class Trial : GameState
    //{

    //}
}
