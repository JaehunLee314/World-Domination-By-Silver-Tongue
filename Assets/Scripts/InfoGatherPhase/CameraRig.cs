using System;
using UnityEngine;
using DG.Tweening;

namespace InfoGatherPhase
{
    public class CameraRig : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float tweenDuration = 0.6f;
        [SerializeField] private Ease tweenEase = Ease.InOutQuad;

        public bool IsTweening { get; private set; }
        public event Action OnTweenComplete;

        public void TweenTo(Vector3 worldPosition, CameraSettings settings)
        {
            if (IsTweening) return;
            IsTweening = true;

            Vector3 targetPos = worldPosition + settings.positionOffset;
            Quaternion targetRot = Quaternion.Euler(settings.rotation);

            Sequence seq = DOTween.Sequence();
            seq.Append(transform.DOMove(targetPos, tweenDuration).SetEase(tweenEase));
            seq.Join(transform.DORotateQuaternion(targetRot, tweenDuration).SetEase(tweenEase));
            seq.Join(cam.DOFieldOfView(settings.fov, tweenDuration).SetEase(tweenEase));
            seq.OnComplete(() =>
            {
                IsTweening = false;
                OnTweenComplete?.Invoke();
            });
        }

        public void SnapTo(Vector3 worldPosition, CameraSettings settings)
        {
            transform.position = worldPosition + settings.positionOffset;
            transform.rotation = Quaternion.Euler(settings.rotation);
            cam.fieldOfView = settings.fov;
        }
    }
}
