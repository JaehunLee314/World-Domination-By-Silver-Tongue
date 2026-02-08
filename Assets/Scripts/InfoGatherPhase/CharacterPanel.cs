using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace InfoGatherPhase
{
    [Serializable]
    public struct EmotionTexture
    {
        public Emotion emotion;
        public Texture2D texture;
    }

    [RequireComponent(typeof(MeshRenderer))]
    public class CharacterPanel : MonoBehaviour
    {
        [SerializeField] private string characterName;
        [SerializeField] private Emotion defaultEmotion = Emotion.Neutral;
        [SerializeField] private EmotionTexture[] emotions;
        [SerializeField] private Camera cam;
        [SerializeField] private Vector3 speakingOffset = new Vector3(0.4f, -0.2f, 1.5f);

        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propBlock;
        private Vector3 homePosition;
        private Quaternion homeRotation;
        private bool isSpeaking;
        private Vector3 bounceOffset;
        private Tween bounceTween;

        private static readonly Dictionary<string, CharacterPanel> registry = new Dictionary<string, CharacterPanel>();

        public string CharacterName => characterName;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            propBlock = new MaterialPropertyBlock();
            homePosition = transform.position;
            homeRotation = transform.rotation;
            ApplyTexture(defaultEmotion);
        }

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(characterName))
                registry[characterName] = this;
        }

        private void OnDisable()
        {
            if (!string.IsNullOrEmpty(characterName) && registry.TryGetValue(characterName, out var panel) && panel == this)
                registry.Remove(characterName);
        }

        private void LateUpdate()
        {
            if (!isSpeaking || cam == null) return;

            Vector3 worldPos = cam.transform.TransformPoint(speakingOffset);
            transform.position = worldPos + cam.transform.TransformDirection(bounceOffset);
            transform.rotation = cam.transform.rotation;
        }

        public static CharacterPanel Get(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            registry.TryGetValue(name, out var panel);
            return panel;
        }

        public void Show(Emotion emotion)
        {
            ApplyTexture(emotion);
            isSpeaking = true;
            PlayBounce();
        }

        private void PlayBounce()
        {
            bounceTween?.Kill();
            bounceOffset = Vector3.zero;
            bounceTween = DOTween.Punch(() => bounceOffset, x => bounceOffset = x,
                new Vector3(0f, 0.12f, 0f), 0.35f, 6, 0.6f)
                .SetEase(Ease.OutQuad);
        }

        public void Hide()
        {
            bounceTween?.Kill();
            bounceOffset = Vector3.zero;
            transform.position = homePosition;
            transform.rotation = homeRotation;
            ApplyTexture(defaultEmotion);
            isSpeaking = false;
        }

        private void OnDestroy()
        {
            bounceTween?.Kill();
        }

        private Texture2D FindTexture(Emotion emotion)
        {
            if (emotions == null) return null;
            for (int i = 0; i < emotions.Length; i++)
            {
                if (emotions[i].emotion == emotion)
                    return emotions[i].texture;
            }
            return null;
        }

        private void ApplyTexture(Emotion emotion)
        {
            Texture2D tex = FindTexture(emotion);
            if (tex == null && emotion != defaultEmotion)
                tex = FindTexture(defaultEmotion);

            if (tex != null)
            {
                meshRenderer.GetPropertyBlock(propBlock);
                propBlock.SetTexture("_MainTex", tex);
                meshRenderer.SetPropertyBlock(propBlock);
            }
        }
    }
}
