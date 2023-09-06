using UnityEngine;
using UnityEngine.UI;

namespace TWV
{
    public class Gaze : AGaze
    {
        // Special offset that give possibility to solve issues when one object entry to other
        private const float DEFAULT_OFFSET_DISTANSE = 0.01f;

        [SerializeField]
        private GameObject _gazeObject = null;

        [SerializeField]
        private RawImage _fadedCircle = null;

        [SerializeField]
        private float _submitTime = 1;

        [SerializeField]
        private Color _activeColor = Color.black;

        [SerializeField]
        AnimationCurve _loadingCurve = null;

        public override float SubmitTime
        {
            get { return _submitTime; }
        }

        public override void Show(Vector3 position, Vector3 normal)
        {
            _gazeObject.transform.position = position - (Vector3.forward * DEFAULT_OFFSET_DISTANSE);
            _gazeObject.transform.rotation = Quaternion.FromToRotation(Vector3.forward, normal);
        }

        public override void SubmitAnimation(float progress)
        {
            _fadedCircle.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, _loadingCurve.Evaluate(progress));
            _fadedCircle.color = Color.Lerp(Color.clear, _activeColor, _loadingCurve.Evaluate(progress));

            if (progress >= 1)
                _fadedCircle.transform.localScale = Vector3.zero;
        }
    }
}
