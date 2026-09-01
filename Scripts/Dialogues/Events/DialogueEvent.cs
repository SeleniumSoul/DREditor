//Dialogue Event script by SeleniumSoul for DREditor.
//
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace DREditor.Dialogues.Events
{
    public interface IDialogueEvent
	{
		void TriggerDialogueEvent();

		#if UNITY_EDITOR
		void EditorUI();
		void ToggleHelpBox();
		void ShowHelpBox();
		#endif
	}

	public class DialogueUnityEventBase : UnityEvent { }
	public class DialogueUnityEventTuple : UnityEvent<object> { }

    static public class DialogueEventSystem
    {
        private static Dictionary<string, DialogueUnityEventBase> eventDictionary = new();
        private static Dictionary<string, DialogueUnityEventTuple> tupleEventDictionary = new();

        public static void StartListening(string eventName, UnityAction listener)
        {
            if (eventDictionary.TryGetValue(eventName, out DialogueUnityEventBase thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new DialogueUnityEventBase();
                thisEvent.AddListener(listener);
                eventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StartListening(string eventName, UnityAction<object> listener)
		{
			if (tupleEventDictionary.TryGetValue(eventName, out DialogueUnityEventTuple thisEvent))
			{
				thisEvent.AddListener(listener);
			}
			else
			{
				thisEvent = new DialogueUnityEventTuple();
				thisEvent.AddListener(listener);
                tupleEventDictionary.Add(eventName, thisEvent);
			}
		}

        public static void StopListening(string eventName, UnityAction listener)
        {
            if (eventDictionary.TryGetValue(eventName, out DialogueUnityEventBase thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public static void StopListening(string eventName, UnityAction<object> listener)
		{
			if (tupleEventDictionary.TryGetValue(eventName, out DialogueUnityEventTuple thisEvent))
			{
				thisEvent.RemoveListener(listener);
			}
		}

		public static void TriggerEvent(string eventName)
		{
			if (eventDictionary.TryGetValue(eventName, out DialogueUnityEventBase thisEvent))
			{
				Debug.Log("DREditor <color=blue>(DialogueEventSystem)</color>: Triggering " + eventName + " event.");
				thisEvent.Invoke();
			}
			else if (tupleEventDictionary.TryGetValue(eventName, out DialogueUnityEventTuple tupleEvent))
			{
                Debug.Log("DREditor <color=blue>(DialogueEventSystem)</color>: Triggering " + eventName + " event.");
                tupleEvent.Invoke(null);
            }
            else
			{
				Debug.LogWarning($"DREditor (DialogueEventSystem): Tryna trigger {eventName}, but it somehow doesn't exist.\nYou need to have to use StartListening first before you invoke the event. This is ignorable if it's intended.");
			}
		}

		public static void TriggerEvent(string eventName, object value = null)
		{
            if (eventDictionary.TryGetValue(eventName, out DialogueUnityEventBase thisEvent))
            {
                Debug.Log("DREditor <color=blue>(DialogueEventSystem)</color>: Triggering " + eventName + " event.");
                thisEvent.Invoke();
            }
            if (tupleEventDictionary.TryGetValue(eventName, out DialogueUnityEventTuple tupleEvent))
			{
				Debug.Log($"DREditor <color=blue>(DialogueEventSystem)</color>: Triggering { eventName } event.");
				tupleEvent.Invoke(value);
			}
			else
			{
				Debug.LogWarning($"DREditor (DialogueEventSystem): Tryna trigger { eventName }, but it somehow doesn't exist.\nYou need to have to use StartListening first before you invoke the event. This is ignorable if it's intended.");
			}
		}
	}
}