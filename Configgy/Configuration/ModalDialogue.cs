using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Configgy
{
    public static class ModalDialogue
    {
        public static void ShowSimple(string title, string message, Action<bool> onComplete, string confirm = "Yes", string deny = "No")
        {
            if (ModalDialogueManager.Instance == null)
            {
                Debug.LogError("No ModalDialogueManager found in scene.");
                return;
            }

            ModalDialogueEvent dialogueEvent = new ModalDialogueEvent();
            dialogueEvent.Title = title;
            dialogueEvent.Message = message;
            dialogueEvent.Options = new DialogueBoxOption[]
            {
                new DialogueBoxOption()
                {
                    Name = confirm,
                    Color = Color.white,
                    OnClick = () => onComplete?.Invoke(true)
                },
                new DialogueBoxOption()
                {
                    Name = deny,
                    Color = Color.white,
                    OnClick = () => onComplete?.Invoke(false)
                }
            };

            ModalDialogueManager.Instance.RaiseDialogue(dialogueEvent);
        }

        public static void ShowDialogue(ModalDialogueEvent modalDialogueEvent)
        {
            if (ModalDialogueManager.Instance == null)
            {
                Debug.LogError("No ModalDialogueManager found in scene.");
                return;
            }

            ModalDialogueManager.Instance.RaiseDialogue(modalDialogueEvent);
        }

        public static void ShowComplex(string title, string message, params DialogueBoxOption[] options)
        {

        }
    }
}
