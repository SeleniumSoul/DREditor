//Dialogue Event script by SeleniumSoul for DREditor.
//Adapted from PuppetPlay

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

#nullable enable

namespace DREditor.Dialogues.Events
{
	public interface IDialogueEvent
	{
		Task TriggerEvent();

		#if UNITY_EDITOR
		void EditorUI();
		void ToggleHelpBox();
		void ShowHelpBox();
		#endif
	}

	[AttributeUsage(AttributeTargets.Method)]
	public sealed class DialogueEventAttribute : Attribute
	{
		public string EventName { get; private set; }

		public DialogueEventAttribute(string eventName)
		{
			EventName = eventName;
		}
	}

	public sealed class DialogueEventSystem
	{
		private const BindingFlags bindFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
        private static readonly Dictionary<(string, Type), List<Delegate>> eventGenericDictionary = new();

        //ACHTUNG! Wait, that's german. CAUTION! This does not support inheritance!
        public static void RegisterEvents(object target)
		{
			if (target == null)
				throw new ArgumentException($"No {nameof(target)} found.");

            var startListeningMethod = typeof(DialogueEventSystem).GetMethod(nameof(StartListening), bindFlags);

			Process(startListeningMethod, target.GetType(), target);
		}

        public static void RegisterEvents(Type type)
        {
            var startListeningMethod = typeof(DialogueEventSystem).GetMethod(nameof(StartListening), bindFlags);

            Process(startListeningMethod, type);
        }

        public static void DeregisterEvents(object target)
		{
			if (target == null)
				throw new ArgumentException($"No {nameof(target)} found.");

            var stopListeningMethod = typeof(DialogueEventSystem).GetMethod(nameof(StopListening), bindFlags);
            
            Process(stopListeningMethod, target.GetType(), target);
        }

		private static void Process(MethodInfo method, Type type, object? target = null)
		{
            var eventMethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
			string processString;

			switch (method.Name)
			{
				case "StartListening":
					processString = "Registering";
					break;
				case "StopListening":
					processString = "Deregistering";
					break;
				default:
					processString = "You cannot grasp the method in";
					break;
			}

			foreach (var meth in eventMethods)
			{
				if (!Attribute.IsDefined(meth, typeof(DialogueEventAttribute)))
					continue;

				var parameters = meth.GetParameters();
				var evName = meth.GetCustomAttribute<DialogueEventAttribute>().EventName;
				Delegate? listener;
				Type? delegateType;

				if (meth.ReturnType == typeof(Task))
					delegateType = parameters.Length == 0 ? typeof(Func<Task>) : typeof(Func<,>).MakeGenericType(parameters[0].ParameterType, typeof(Task));
				else
					delegateType = parameters.Length == 0 ? typeof(Action) : typeof(Action<>).MakeGenericType(parameters[0].ParameterType);

				listener = Delegate.CreateDelegate(delegateType, target, meth);

				method.MakeGenericMethod(parameters.Length > 0 ? parameters[0].ParameterType : typeof(object)).Invoke(null, new object[] { evName, listener });
				Debug.Log($"DREditor <color=blue>(DialogueEventSystem)</color>: {processString} {evName} Task event using {(parameters.Length > 0 ? parameters[0].ParameterType.Name : "No Parameters")} with {meth.Name} from {type.Name}.");
			}
		}

        private static void StartListening<T>(string eventName, Delegate listener)
		{
			var tupleKey = (eventName, typeof(T));

			if (eventGenericDictionary.TryGetValue(tupleKey, out var thisEvent))
				thisEvent.Add(listener);
			else
				eventGenericDictionary.Add(tupleKey, new List<Delegate>() { listener });
		}

		private static void StopListening<T>(string eventName, Delegate listener)
		{
			var tupleKey = (eventName, typeof(T));

			if (!eventGenericDictionary.TryGetValue(tupleKey, out var thisEvent))
				return;

			thisEvent.Remove(listener);

            if (thisEvent.Count <= 0)
                eventGenericDictionary.Remove(tupleKey);
		}

		public static async Task TriggerEvent(string eventName)
		{
			await TriggerEvent<object>(eventName, null);
		}

		public static async Task TriggerEvent<T>(string eventName, T? value)
		{
			if (eventGenericDictionary.TryGetValue((eventName, typeof(T)), out var genericEventList))
			{
                Debug.Log($"DREditor <color=blue>(DialogueEventSystem)</color>: Triggering {eventName}({typeof(T).Name}) event.");
                var invokeList = new List<Task>();

				foreach (var genericEvent in genericEventList)
				{
					if (genericEvent is Func<T, Task> taskValueEvent)
						invokeList.Add(taskValueEvent(value!));
					else if (genericEvent is Func<Task> taskEvent)
						invokeList.Add(taskEvent());
                    else if (genericEvent is Action<T> actionValueEvent)
						actionValueEvent.Invoke(value!);
                    else if (genericEvent is Action actionEvent)
						actionEvent.Invoke();
					else
						Debug.Log($"DREditor <color=blue>(DialogueEventSystem)</color>: {genericEvent.Method.Name}({typeof(T).Name}) is not a compatible type!");
                }
                await Task.WhenAll(invokeList);

				//await Task.WhenAll(genericEventList.GetInvocationList().Select(listener => (Task)listener.DynamicInvoke(value) as Task ?? Task.CompletedTask));
			}
			else
			{
				Debug.LogError($"DREditor <color=blue>(DialogueEventSystem)</color>: Unable to find {eventName}({typeof(T).Name}) event.");
			}
		}
	}
}