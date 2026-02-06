using UnityEngine;

namespace InfoGatherPhase
{
    [RequireComponent(typeof(MeshRenderer))]
    public class SpeakerPanel : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private Vector3 offsetFromCamera = new Vector3(0.4f, -0.2f, 1.5f);

        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock propBlock;
        private bool isVisible;

        private void Awake()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            propBlock = new MaterialPropertyBlock();
            SetVisible(false);
        }

        private void LateUpdate()
        {
            if (!isVisible || cam == null) return;

            Vector3 worldPos = cam.transform.TransformPoint(offsetFromCamera);
            transform.position = worldPos;
            transform.rotation = cam.transform.rotation;
        }

        public void Show(Texture2D portrait)
        {
            if (portrait == null)
            {
                SetVisible(false);
                return;
            }

            meshRenderer.GetPropertyBlock(propBlock);
            propBlock.SetTexture("_BaseMap", portrait);
            meshRenderer.SetPropertyBlock(propBlock);
            SetVisible(true);
        }

        public void Hide()
        {
            SetVisible(false);
        }

        private void SetVisible(bool visible)
        {
            isVisible = visible;
            meshRenderer.enabled = visible;
        }
    }
}
