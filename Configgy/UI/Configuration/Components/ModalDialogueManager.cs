using Configgy.Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Configgy
{
    public class ModalDialogueManager : MonoBehaviour
    {
        [SerializeField] private GameObject blocker;
        [SerializeField] private GameObject dialogueBox;

        [SerializeField] private Text titleText;
        [SerializeField] private Text messageText;
        [SerializeField] private RectTransform buttonContainer;


        private Queue<ModalDialogueEvent> dialogueEvent = new Queue<ModalDialogueEvent>();
        private List<GameObject> spawnedButtons = new List<GameObject>();

        internal static ModalDialogueManager Instance { get; private set; }
        private bool runningCoroutine = false;

        private void Awake()
        {
            Instance = this;
            blocker.SetActive(false);
            dialogueBox.SetActive(false);
            StartCoroutine(SlowUpdate());
        }

        private IEnumerator SlowUpdate()
        {
            while (true)
            {
                transform.SetAsLastSibling();
                yield return new WaitForSecondsRealtime(2f);
            }
        }

        internal void RaiseDialogue(ModalDialogueEvent modalDialogueEvent)
        {
            if(modalDialogueEvent == null)
                throw new ArgumentNullException("modalDialogueEvent is null!");

            dialogueEvent.Enqueue(modalDialogueEvent);
            if (!runningCoroutine)
            {
                runningCoroutine = true;
                StartCoroutine(DialogueCoroutine());
            }
        }

        private void ShowDialogue(ModalDialogueEvent modalDialogueEvent)
        {
            inDialogue = true;
            dialogueBox.SetActive(true);

            if (string.IsNullOrEmpty(modalDialogueEvent.Title))
                titleText.text = "";
            else
                titleText.text = $"-- {modalDialogueEvent.Title} --";

            messageText.text = modalDialogueEvent.Message;

            for (int i = 0; i < modalDialogueEvent.Options.Length; i++)
            {
                DialogueBoxOption option = modalDialogueEvent.Options[i];
                GameObject buttonObject = GameObject.Instantiate(PluginAssets.ButtonPrefab, buttonContainer);
                Button button = buttonObject.GetComponent<Button>();
                Text text = buttonObject.GetComponentInChildren<Text>();
                Image image = buttonObject.GetComponentsInChildren<Image>().Where(x => x.name == "Border").FirstOrDefault();

                spawnedButtons.Add(buttonObject);

                text.color = option.Color;
                text.text = option.Name;
                image.color = option.Color;

                button.onClick.AddListener(() =>
                {
                    EndDialogue();
                    option.OnClick?.Invoke();
                });
            }
        }

        private void EndDialogue()
        {
            inDialogue = false;

            dialogueBox.SetActive(false);

            for (int i = 0; i < spawnedButtons.Count; i++)
            {
                if (spawnedButtons[i] == null)
                    continue;

                GameObject button = spawnedButtons[i];
                spawnedButtons[i] = null;
                GameObject.Destroy(button);
            }

            spawnedButtons.Clear();
        }

        private bool inDialogue = false;

        private IEnumerator DialogueCoroutine()
        {
            blocker.SetActive(true);
            Pauser.Pause(blocker);

            while (dialogueEvent.Count > 0)
            {
                ModalDialogueEvent dialogue = dialogueEvent.Dequeue();
                ShowDialogue(dialogue);
                while (inDialogue)
                {
                    yield return null;
                }
            }

            blocker.SetActive(false);
            runningCoroutine = false;
            Pauser.Unpause();
        }
    }

    public struct DialogueBoxOption
    {
        public string Name;
        public Action OnClick;
        public Color Color;
    }

    public class ModalDialogueEvent
    {
        public string Title;
        public string Message;
        public DialogueBoxOption[] Options;
    }
}
