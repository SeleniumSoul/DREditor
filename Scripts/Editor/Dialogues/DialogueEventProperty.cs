using DREditor.Dialogues.Events;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(IDialogueEvent))]
public class DialogueEventProperty : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        IDialogueEvent diaEvent = property.managedReferenceValue as IDialogueEvent;

        diaEvent.EditorUI();
    }
}