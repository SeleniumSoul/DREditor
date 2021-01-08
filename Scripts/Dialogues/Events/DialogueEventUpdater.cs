using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace DREditor.Dialogues.Events
{
    public enum DialogueEventType
    {
        ChangePanelPositions, ChangeCharacterCameraFocus, ChangeMusic, Flash, Emotions, Custom
    }
    
    public class DialogueEventUpdater
    {
        //public Dictionary<string, DialogueEvent> EventList;
        public List<string> EventList;

        public void UpdateList(){    
            Type parentType = typeof(IDialogueEvent);
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] types = assembly.GetTypes();

            IEnumerable<Type> subclasses = types.Where(t => t.BaseType == parentType);
    
            foreach (Type type in subclasses)
            {
                EventList.Add(type.Name);
            }
        }
    }
}