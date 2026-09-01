using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace DREditor.Characters
{
	[Serializable]
	public class Alias
	{
		public string Name;
		public Sprite Nameplate;
	}

    [Serializable]
    public class Character : ScriptableObject, IEquatable<Character>
    {
        [Tooltip("The prefab used for the characters.")]
        public AssetReferenceT<GameObject> CharacterPrefab;
        public string CharacterID;
        public string translationKey;
        public string LastName = "";
        public string FirstName = "";
        [Tooltip("Currently used for Splash Arts.")]
        public Texture2D DefaultSprite;
        public Texture2D TrialPortrait;
#if UNITY_EDITOR
        public List<Expression> Expressions = new List<Expression>();
#endif
        public List<Alias> Aliases = new List<Alias>();
        public Sprite Nameplate;
        public Texture2D Headshot;
        public Texture2D TrialNameplate;
        public float TrialHeight = 7.88f;

        public bool Equals(Character other)
        {
            return CharacterID == other.CharacterID;
        }

        /// <summary>
        /// Returns the full name of the character.
        /// </summary>
        /// <param name="jp">If true, return name in Japanese (Last Name, First Name) basis.</param>
        /// <returns></returns>
        public string FullName(bool jp)
        {
            string name;
            if (!string.IsNullOrWhiteSpace(LastName))
            {
                name = jp ? $"{LastName} {FirstName}" : $"{FirstName} {LastName}";
            }
            else
            {
                name = FirstName;
            }
            return name;
        }
    }
}
