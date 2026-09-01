using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace DREditor.Characters
{
    [Serializable]
    [CreateAssetMenu(menuName = "DREditor/Characters/Protagonist", fileName = "New Protagonist")]
    public class Protagonist : Character
    {
        public Color ProtagonistColor;
    }

}

