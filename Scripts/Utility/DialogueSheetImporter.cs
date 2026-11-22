//Dialogue Google Sheet Importer by SeleniumSoul for DREditor

using DREditor.Characters;
using DREditor.Dialogues;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

		[Serializable]
		private class SpreadsheetResponse
		{
			public SheetWrapper[] sheets;
			public string spreadsheetURL;
		}

		[Serializable]
		private class SheetWrapper
		{
			public SheetProperties properties;
		}

		[Serializable]
		private class SheetProperties
		{
			public int sheetId;
			public string title;
		}

		private struct HelpMessage
		{
			public string message;
			public MessageType type;

			public HelpMessage(string Message, MessageType messageType = MessageType.None)
			{
				message = Message;
				type = messageType;
			}
		}

		private enum Tabs { PageTabs, RawView }
		private const string baseURL = "https://sheets.googleapis.com/v4/spreadsheets/";
		private const string csvBaseURL = "https://docs.google.com/spreadsheets/d/";

		private readonly List<SheetPage> sheetPages = new();
		private string apiKey = "AIzaSyAmkXk3mcnX37x4Lj_u4BmSeNXQoGH6ntk";
		private string saveFolder = "Assets/";
		private string sheetID;
		private string sheetResponse;

		private Tabs tabIndex;
		private Vector2 scrollPosition = new();
		private HelpMessage help;

		[MenuItem("Tools/DREditor/Dialogue Google Sheet Importer")]
		public static void ShowWindow() => 
			GetWindow<DialogueSheetImporter>("Dialogue Google Sheet Importer");

		private void OnGUI()
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				saveFolder = EditorGUILayout.TextField("Target Folder", saveFolder);

				if (GUILayout.Button("Path", GUILayout.Width(100)))
					saveFolder = EditorUtility.OpenFolderPanel("Select where to save the dialogue files", "Assets/", "");
			}

			EditorGUILayout.Space();

			EditorGUILayout.LabelField("URL");
			apiKey = EditorGUILayout.TextField("API Key", apiKey);
			using (new EditorGUILayout.HorizontalScope())
			{
				sheetID = EditorGUILayout.TextField("Sheet ID", sheetID);

				if (GUILayout.Button("Fetch Pages", GUILayout.Width(100)))
				{
					FetchTabs();
				}
			}
			string fullURL = $"{baseURL}{sheetID}?key={apiKey}";
			EditorGUILayout.LabelField("Full URL: " + fullURL);

			EditorGUILayout.Space();

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("Fetched Pages"))
					tabIndex = Tabs.PageTabs;
				if (GUILayout.Button("Raw Response"))
					tabIndex = Tabs.RawView;
			}

			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
			switch (tabIndex)
			{
				case Tabs.PageTabs:
					PageTabsView();
					break;
				case Tabs.RawView:
					RawResponseView();
					break;
				default:
					help = new HelpMessage("Tab Error!", MessageType.Error);
					break;
			}
			EditorGUILayout.EndScrollView();

			if (!string.IsNullOrEmpty(help.message))
			{
				EditorGUILayout.HelpBox(help.message, help.type);
			}
		}

		private void FetchTabs()
		{
			if (string.IsNullOrEmpty(sheetID) || string.IsNullOrEmpty(apiKey))
			{
				help = new HelpMessage("Empty URL or API key!", MessageType.Error);
				return;
			}
			else
				sheetPages.Clear();

			var fullURL = $"{baseURL}{sheetID}?key={apiKey}";
			var request = UnityWebRequest.Get(fullURL);
			request.SendWebRequest();
			while (!request.isDone)
			{
				help = new HelpMessage("Fetching the sheet from the URL...", MessageType.Info);
			}

			if (request.result != UnityWebRequest.Result.Success)
			{
				help = new HelpMessage("Wrong URL or API key!", MessageType.Error);
				return;
			}
			else
			{
				SpreadsheetResponse response = JsonUtility.FromJson<SpreadsheetResponse>(request.downloadHandler.text);

				if (response != null && response.sheets != null)
				{
					sheetResponse = request.downloadHandler.text;

					foreach (var sheet in response.sheets)
					{
						var title = sheet.properties.title;
						var id = sheet.properties.sheetId;
						var csvUrl = $"{csvBaseURL}{sheetID}/export?format=csv&gid={id}";

						sheetPages.Add(new SheetPage
						{
							name = title,
							url = csvUrl
						});
					}
					help = new HelpMessage($"Successfully fetched {sheetPages.Count} tabs via API.", MessageType.Info);
				}
			}
		}

		private string FetchPage(SheetPage page)
		{
			help = new HelpMessage($"Fetching {page.name}...", MessageType.Info);
            var client = new System.Net.WebClient();
            var csvText = client.DownloadString(page.url);

            return csvText;
        }

        private void SavePageAsDialogueFile(string pageName, string csvPage)
		{
			var path = $"{saveFolder}{pageName}.asset";
			Dialogue dialogueFile = AssetDatabase.LoadAssetAtPath<Dialogue>(path);
			if (dialogueFile == null)
			{
				dialogueFile = ScriptableObject.CreateInstance<Dialogue>();
				AssetDatabase.CreateAsset(dialogueFile, path);
			}
			else
			{
				dialogueFile.Lines.Clear();
			}
			
			dialogueFile.DialogueName = pageName;

			string[] rows = csvPage.Split('\n');

			//Legend Column:
			//0 - Line ID (TBD for what use this will be.)
			//1 - Speaker
			//2 - Text
			//3 - Sprite
			//4 - Audio
			//5 - Notes (Will not be saved in the dialogue file)
			for (int i = 1; i < rows.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(rows[i]))
					continue;

                var columns = SliceRow(rows[i]);
				var line = new Line();

                var nameRegex = @"^(?<alias>[^()]+?)\s\((?<realName>[^()]+)\)$";
                var match = Regex.Match(columns[1], nameRegex);

				try
				{
					line.Speaker = match.Success ? ParseCharacter(match.Groups["realName"].Value) : ParseCharacter(columns[1]);
					line.Text = columns[2];
					line.AliasNumber = match.Success ? line.Speaker.GetAliasIndex(match.Groups["alias"].Value) : 0;
					Debug.Log($"Created line for {line.Speaker.FullName(false)} ({line.AliasNumber})");
				}
				catch (NullReferenceException)
				{
					//continue;
				}

				dialogueFile.Lines.Add(line);
			}

			EditorUtility.SetDirty(dialogueFile);
			help = new HelpMessage($"Page saved at {path}!", MessageType.Info);
		}

		private void PageTabsView()
		{
			using (new EditorGUILayout.VerticalScope("Box"))
			{
				if (sheetPages.Count > 0)
				{
					if (GUILayout.Button("Save All Pages as Dialogue"))
					{
						for (int i = 0; i < sheetPages.Count; i++)
						{
                            var progress = (float)i / sheetPages.Count - 1;
                            EditorUtility.DisplayProgressBar("Importing Dialogues", $"Importing page: {sheetPages[i].name}...", progress);

                            SavePageAsDialogueFile(sheetPages[i].name, FetchPage(sheetPages[i]));
                        }
					}

					foreach (var sheetPage in sheetPages)
					{
						PageSelectRow(sheetPage);
					}
				}
				else
				{
					EditorGUILayout.LabelField("No sheet loaded! Press [Fetch Pages] to get the the list of pages from the sheet.");
				}
			}
		}

		private void RawResponseView()
		{
			var style = new GUIStyle(EditorStyles.textArea);
			style.wordWrap = true;
			float height = style.CalcHeight(new GUIContent(sheetResponse), EditorGUIUtility.currentViewWidth);

			EditorGUILayout.SelectableLabel(sheetResponse, style, GUILayout.Height(height));
		}

		private void PageSelectRow(SheetPage page)
		{
			using (new EditorGUILayout.HorizontalScope())
			{
				EditorGUILayout.LabelField(page.name);
				if (GUILayout.Button("Fetch"))
				{
					sheetResponse = FetchPage(page);
					help = new HelpMessage($"Successfully fetched {page.name}.", MessageType.Info);
				}
				if (GUILayout.Button("Save"))
				{
					SavePageAsDialogueFile(page.name, FetchPage(page));
				}
			}
		}

        private List<string> SliceRow(string row)
        {
            var fields = new List<string>();
			var matches = Regex.Matches(row, @"(""[^""]*""|[^,]+)(,|$)");

            foreach (Match match in matches)
            {
                var field = match.Groups[1].Value.Trim();

                if (field.StartsWith("\"") && field.EndsWith("\""))
					field = field[1..^1].Replace("\"\"", "\"");

                fields.Add(field);
            }

            return fields;
        }

		private Character ParseCharacter(string characterID)
		{
            var database = DatabaseUtility.GetCharacterDatabase();

            return database.GetCharacter(characterID);
        }
    }
}