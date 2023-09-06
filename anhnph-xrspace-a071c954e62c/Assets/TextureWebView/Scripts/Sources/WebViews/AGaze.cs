using UnityEngine;

namespace TWV
{
    public abstract class AGaze : MonoBehaviour
    {
        private const float DEFAULT_SUBMIT_TIME = 1f;

        public abstract float SubmitTime { get; }
        public abstract void Show(Vector3 position, Vector3 normal);
        public abstract void SubmitAnimation(float progress);
    }
}
