using DREditor.Dialogues.Events;
using System.Collections.Generic;
using UnityEngine;

namespace DREditor
{
    public interface IExaminable
    {
        public void Examine();
        public string GetName();
    }

    public class Examinable : MonoBehaviour, IExaminable
    {
        [SerializeField] private string Name;
        [SerializeReference] private List<IDialogueEvent> events;

        private void Start()
        {
            events.Add(new ChangeCharacterFocus());
        }
        public string GetName() => Name;
        public void Examine()
        {
            foreach(var diaevent in events)
            {
                diaevent.TriggerEvent();
            }
        }
    }
}
