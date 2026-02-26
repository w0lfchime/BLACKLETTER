using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace BLDebug
{
    public class ConsoleManager : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject wholeConsole;
        public TMP_InputField inputField;
        public Button sendButton;
        public Button refreshButton;
        public Button exitButton;
        public ScrollRect consoleScrollRect;
        public Transform consoleContent;
        public TMP_Text consoleMessagePrefab;

        [Header("Settings")]
        public int messageLimit = 100;

        private Queue<GameObject> messageObjects = new Queue<GameObject>();

        [Header("Local Variables")]
        [SerializeField] private bool consoleIsOpen;

        private void Start()
        {
            sendButton.onClick.AddListener(SendCommand);
            refreshButton.onClick.AddListener(ClearConsole);
            exitButton.onClick.AddListener(() => SetConsoleOpen(false));

            // TMP InputField's onSubmit works with Enter key
            inputField.onSubmit.AddListener(_ => SendCommand());

            Logger.OnLog += LogMessage;
        }

        private void Update()
        {
            // Toggle console with ~ key
            if (Input.GetKeyDown(KeyCode.BackQuote)) // ~ key is typically `BackQuote`
            {
                consoleIsOpen = !consoleIsOpen;
                wholeConsole.SetActive(consoleIsOpen);

                if (consoleIsOpen)
                {
                    inputField.ActivateInputField(); // Focus input field when opened
                }
            }
        }


        private void SendCommand()
        {
            string command = inputField.text;
            if (string.IsNullOrWhiteSpace(command)) return;

            inputField.text = "";
            CommandHandler.ExecuteCommand(command);
        }

        private void LogMessage(string message)
        {
            // Enforce message limit
            if (messageObjects.Count >= messageLimit)
            {
                GameObject oldestMessage = messageObjects.Dequeue();
                Destroy(oldestMessage);
            }

            // Create a new message and add it to the queue
            TMP_Text newMessage = Instantiate(consoleMessagePrefab, consoleContent);
            newMessage.text = message;
            messageObjects.Enqueue(newMessage.gameObject);

            // Force layout and scroll updates
            Canvas.ForceUpdateCanvases();
            consoleScrollRect.verticalNormalizedPosition = 0; // Keep scrolled to the bottom
        }

        private void ClearConsole()
        {
            foreach (Transform child in consoleContent)
            {
                Destroy(child.gameObject);
            }

            messageObjects.Clear();
        }

        private void SetConsoleOpen(bool isOpen)
        {
            consoleIsOpen = isOpen;
            wholeConsole.SetActive(false);
        }

        private void OnDestroy()
        {
            Logger.OnLog -= LogMessage;
        }
    }
}