using Common;
using UnityEngine;

namespace OOP
{
    [RequireComponent(typeof(MeshRenderer))]
    public class Perlin : MonoBehaviour
    {
        [Header("Inscribed")] public int textureSize = 1080;

        [Header("Inscribed/Dynamic")] public float scale = 10f;
        public Vector2 offset = Vector2.zero;
        public Vector2 velocity = new(1, 0);
        public Settings settings;

        private Material _material;
        private Texture2D _texture;

        private void Start()
        {
            _material = GetComponent<MeshRenderer>().material;

            _texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            UpdateTexture();
            _material.mainTexture = _texture;
        }

        private void Update()
        {
            offset += velocity * Time.deltaTime;
            UpdateTexture();
        }

        private void UpdateTexture()
        {
            var pixels = new Color[textureSize * textureSize];

            var multiplier = scale / textureSize;
            var index = 0;

            var halfSize = -textureSize * 0.5f;

            for (var v = 0; v < textureSize; v++)
            {
                for (var h = 0; h < textureSize; h++)
                {
                    var x = (halfSize + h) *  multiplier + offset.x;
                    var y = (halfSize + v) *  multiplier + offset.y;

                    var noise = PerlinOctaves(x, y);
                    pixels[index++] = new Color(noise, noise, noise, 1);
                }
            }

            _texture.SetPixels(pixels);
            _texture.Apply(false);
        }

        private float PerlinOctaves(float x, float y)
        {
            float u = 0f, amplitude = 1f, frequency = 1f;

            for (var i = 0; i < settings.octaves; i++)
            {
                if (i > 0)
                {
                    amplitude *= settings.amplitude;
                    frequency *= settings.frequency;
                }
                u += (Mathf.PerlinNoise(x * frequency, y * frequency) - 0.5f) * amplitude;
            }

            return u + 0.5f;
        }
    }
}
