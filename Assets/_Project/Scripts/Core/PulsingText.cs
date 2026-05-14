using UnityEngine;
using TMPro;

namespace SpaceMining
{
    [RequireComponent(typeof(TMP_Text))]
    public class PulsingText : MonoBehaviour
    {
        public float minScale = 0.97f;
        public float maxScale = 1.03f;
        public float speed = 1.5f;

        private Transform _t;

        void Awake() { _t = transform; }

        void Update()
        {
            float a = 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * speed);
            float s = Mathf.Lerp(minScale, maxScale, a);
            _t.localScale = new Vector3(s, s, 1f);
        }
    }
}
