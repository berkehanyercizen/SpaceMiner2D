using UnityEngine;
using UnityEngine.UI;

namespace SpaceMining
{
    [RequireComponent(typeof(RectTransform))]
    public class StarfieldGenerator : MonoBehaviour
    {
        public int starCount = 180;
        public float minSize = 2f;
        public float maxSize = 6f;
        public float twinkleSpeed = 1.5f;
        public Color starColor = Color.white;

        private Star[] _stars;
        private Sprite _circleSprite;

        private struct Star
        {
            public Image image;
            public float baseAlpha;
            public float phase;
        }

        void Start()
        {
            _circleSprite = MakeCircleSprite();
            var rt = (RectTransform)transform;
            Vector2 size = rt.rect.size;
            _stars = new Star[starCount];

            for (int i = 0; i < starCount; i++)
            {
                var go = new GameObject($"Star_{i}", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                var srt = (RectTransform)go.transform;
                srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0.5f);
                srt.anchoredPosition = new Vector2(
                    Random.Range(-size.x * 0.5f, size.x * 0.5f),
                    Random.Range(-size.y * 0.5f, size.y * 0.5f));
                float s = Random.Range(minSize, maxSize);
                srt.sizeDelta = new Vector2(s, s);

                var img = go.GetComponent<Image>();
                img.sprite = _circleSprite;
                img.raycastTarget = false;
                float baseAlpha = Random.Range(0.4f, 1f);
                img.color = new Color(starColor.r, starColor.g, starColor.b, baseAlpha);

                _stars[i] = new Star { image = img, baseAlpha = baseAlpha, phase = Random.Range(0f, Mathf.PI * 2f) };
            }
        }

        void Update()
        {
            float t = Time.unscaledTime * twinkleSpeed;
            for (int i = 0; i < _stars.Length; i++)
            {
                var s = _stars[i];
                float a = s.baseAlpha * (0.5f + 0.5f * Mathf.Sin(t + s.phase));
                var c = s.image.color; c.a = a; s.image.color = c;
            }
        }

        private Sprite MakeCircleSprite()
        {
            int res = 32;
            var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
            float r = res * 0.5f;
            for (int y = 0; y < res; y++)
                for (int x = 0; x < res; x++)
                {
                    float dx = x - r + 0.5f, dy = y - r + 0.5f;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(1f - d / r);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a * a));
                }
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
