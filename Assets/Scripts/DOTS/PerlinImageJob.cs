using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace DOTS
{
    [BurstCompile]
    public struct PerlinImageJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Color32> Pixels;
        [ReadOnly] public int TextureSize;
        [ReadOnly] public float2 Offset;
        [ReadOnly] public float Multiplier;
        [ReadOnly] public float HalfSize;
        [ReadOnly] public float Octaves;
        [ReadOnly] public float Frequency;
        [ReadOnly] public float Amplitude;
        [ReadOnly] public bool Vignette;
        [ReadOnly] public bool Colorize;
        [ReadOnly] public NativeArray<Color32> Colors;

        public void Execute(int index)
        {
            var h = index % TextureSize;
            var v = index / TextureSize;

            var location = new float2
            {
                x = (HalfSize + h) * Multiplier + Offset.x,
                y = (HalfSize + v) * Multiplier + Offset.y
            };

            float u = 0f, amplitude = 1f, frequency = 1f;
            for (var i = 0; i < Octaves; i++)
            {
                if (i > 0)
                {
                    amplitude *= Amplitude;
                    frequency *= Frequency;
                }

                u += noise.cnoise(location * frequency) * amplitude;
            }

            u = (u + 1) * 0.5f;

            if (Vignette)
            {
                location.x = h / (float)TextureSize;
                location.y = v / (float)TextureSize;
                location.x = (location.x - 0.5f) * 3f;
                location.y = (location.y - 0.5f) * 3f;
                location.x = 1 - (location.x * location.x);
                location.y = 1 - (location.y * location.y);

                u *= (location.x + location.y) * 0.5f;
            }

            if (u < 0) u = 0;
            if (u > 1) u = 1;

            var value = (byte)(u * 255);
            Pixels[index] = Colorize ? Colors[value] : new Color32(value, value, value, 255);
        }
    }
}
