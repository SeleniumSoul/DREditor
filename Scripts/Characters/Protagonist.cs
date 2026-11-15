using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace DREditor.Characters
{
    [Obsolete("Use a student card and tick the IsProtagonist value.")]
    [Serializable]
    [CreateAssetMenu(menuName = "DREditor/Characters/Protagonist", fileName = "New Protagonist")]
    public class Protagonist : Character
    {
        public Color ProtagonistColor;
    }

}

