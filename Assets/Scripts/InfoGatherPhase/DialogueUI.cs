using System.Collections;
using UnityEngine;
using TMPro;

namespace InfoGatherPhase
{
    public class DialogueUI : MonoBehaviour
    {
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI lineText;
        [SerializeField] private float charsPerSecond = 40f;

        public bool IsTyping { get; private set; }

        private Coroutine typeRoutine;

        public void Show(string speaker, string text)
        {
            nameText.text = speaker;
            lineText.text = text;
            dialoguePanel.SetActive(true);
            lineText.ForceMeshUpdate();

            if (typeRoutine != null)
                StopCoroutine(typeRoutine);
            typeRoutine = StartCoroutine(TypeText());
        }

        public void CompleteTyping()
        {
            if (!IsTyping) return;

            if (typeRoutine != null)
                StopCoroutine(typeRoutine);
            typeRoutine = null;

            lineText.maxVisibleCharacters = lineText.textInfo.characterCount;
            IsTyping = false;
        }

        public void Hide()
        {
            if (typeRoutine != null)
            {
                StopCoroutine(typeRoutine);
                typeRoutine = null;
            }
            IsTyping = false;
            dialoguePanel.SetActive(false);
        }

        private IEnumerator TypeText()
        {
            IsTyping = true;
            int totalChars = lineText.textInfo.characterCount;
            lineText.maxVisibleCharacters = 0;

            float timer = 0f;
            float interval = 1f / charsPerSecond;

            for (int i = 0; i < totalChars; i++)
            {
                while (timer < interval)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
                timer -= interval;
                lineText.maxVisibleCharacters = i + 1;
            }

            typeRoutine = null;
            IsTyping = false;
        }
    }
}
