using DREditor.Characters;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace DREditor
{
    public static class DatabaseUtility
    {
        public static CharacterDatabase GetCharacterDatabase()
        {
            string[] _databaseguids = AssetDatabase.FindAssets("t:CharacterDatabase");
            if (_databaseguids.Length == 1)
            {
                var _databasepath = AssetDatabase.GUIDToAssetPath(_databaseguids[0]);
                return AssetDatabase.LoadAssetAtPath<CharacterDatabase>(_databasepath);
            }
            else if (_databaseguids.Length > 1)
            {
                var sb = new StringBuilder();
                sb.Append("There are more than 1 character databases in your project.\n");
                for (int i = 0; i < _databaseguids.Length; i++)
                {
                    string _path = AssetDatabase.GUIDToAssetPath(_databaseguids[i]);
                    sb.Append($"	• {_path}\n");
                }
                sb.Append("Only one CharacterDatabase is allowed. Please delete all the duplicates until one remains.");
                Debug.LogError(sb.ToString());
                return null;
            }
            else
            {
                var sb = new StringBuilder();
                sb.Append("There are no CharacterDatabase Asset found in your project.\n");
                sb.Append("Please create one by right clicking in the Project Window and navigating to: \n[Create > DREditor > Characters > Character Database]\n");
                sb.Append("Do not create more than one. Only one CharacterDatabase is allowed.");
                Debug.LogError(sb.ToString());
                return null;
            }
        }

    }
}
