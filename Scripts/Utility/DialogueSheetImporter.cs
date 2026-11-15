using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace DREditor
{
    public class DialogueSheetImporter : EditorWindow
    {
        [Serializable]
        public class SheetPage
        {
            public string name;
            public string url;
        }

        private struct HelpMessage
        {
            public string message;
            public MessageType type;
        }

        private const string baseURL = "https://sheets.googleapis.com/v4/spreadsheets/";

        private List<SheetPage> sheetPages = new();
        private string apiKey = "AIzaSyAmkXk3mcnX37x4Lj_u4BmSeNXQoGH6ntk";

        private string sheetURL;
        private string saveFolder = "Assets/";

        private bool showHelp = false;
        private HelpMessage help;

        [MenuItem("Tools/DREditor/DialogueSheetImporter")]
        public static void ShowWindow() => 
            GetWindow<DialogueSheetImporter>("Dialogue Sh*t Importer");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Shit Importer");
            
            apiKey = EditorGUILayout.TextField("API Key", apiKey);

            using (new EditorGUILayout.HorizontalScope())
            {
                saveFolder = EditorGUILayout.TextField("Target Folder", saveFolder);

                if (GUILayout.Button("Path", GUILayout.Width(100)))
                    saveFolder = EditorUtility.OpenFolderPanel("Select where to save the dialogue files", "Assets/", "");
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                sheetURL = EditorGUILayout.TextField("Sheet URL", sheetURL);

                if (showHelp)
                {
                    EditorGUILayout.HelpBox(help.message, help.type);
                }

                if (GUILayout.Button("Fetch Tabs", GUILayout.Width(100)))
                {
                    if (string.IsNullOrEmpty(sheetURL) || string.IsNullOrEmpty(apiKey))
                    {
                        //showHelp
                        EditorGUILayout.HelpBox("Empty URL or API key!", MessageType.Error);
                        return;
                    }

                    string fullURL = $"{baseURL}{sheetURL}?key={apiKey}";
                    var request = UnityWebRequest.Get(fullURL);
                    request.SendWebRequest();
                    while (!request.isDone)
                    {

                    }

                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        EditorGUILayout.HelpBox("Empty URL or API key!", MessageType.Error);
                        return;
                    }
                }


            }

        }

        private void FetchTabs()
        {

        }

        //private List<SheetPage> GetSheetList()
        //{
        //    var pages = new List<SheetPage>();

        //    foreach (var page in sheetPages) {
        //}

    }
}
