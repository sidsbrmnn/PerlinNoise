using Common;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DOTS
{
    [RequireComponent(typeof(MeshRenderer))]
    public class Perlin : MonoBehaviour
    {
        [Header("Inscribed")] public int textureSize = 1080;
        public Gradient gradient;
        public bool regenerateColors;

        [Header("Inscribed/Dynamic")] public float scale = 10f;
        public Vector2 offset = Vector2.zero;
        public Vector2 velocity = new(1, 0);
        public Settings settings;
        public bool vignette = true;
        public bool colorize = true;

        private Material _material;
        private Texture2D _texture;
        private Color32[] _colors;

        private void Start()
        {
            _material = GetComponent<MeshRenderer>().material;
            _texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            _material.mainTexture = _texture;

            GenerateColors();
        }

        private void Update()
        {
            if (regenerateColors)
                GenerateColors();

            offset += velocity * Time.deltaTime;
            UpdateTexture();
        }

        private void GenerateColors()
        {
            _colors = new Color32[256];
            const float step = 1f / 255f;

            var u = 0f;
            for (var i = 0; i < _colors.Length; i++)
            {
                _colors[i] = gradient.Evaluate(u);
                u += step;
            }

            regenerateColors = false;
        }

        private void UpdateTexture()
        {
            var job = new PerlinImageJob
            {
                Pixels = _texture.GetRawTextureData<Color32>(),
                TextureSize = textureSize,
                Offset = offset,
                Multiplier = scale / textureSize,
                HalfSize = -textureSize * 0.5f,
                Octaves = settings.octaves,
                Frequency = settings.frequency,
                Amplitude = settings.amplitude,
                Vignette = vignette,
                Colorize = colorize,
                Colors = new NativeArray<Color32>(_colors, Allocator.TempJob)
            };

            var handle = job.Schedule(job.Pixels.Length, 64);
            handle.Complete();

            job.Colors.Dispose();

            _texture.Apply(false);
        }
    }
}
