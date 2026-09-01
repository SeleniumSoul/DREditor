using System;
using UnityEngine;

namespace DREditor.Characters
{
    [Serializable]
    [CreateAssetMenu(menuName = "DREditor/Characters/Student", fileName = "New Student")]
    public class Student : Character
    {
        public StudentCard StudentCard;
    }
}

