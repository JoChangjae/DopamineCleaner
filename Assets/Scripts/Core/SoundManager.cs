using System.Collections.Generic;
using UnityEngine;

namespace DopamineCleaner.Core
{
    public enum SoundEffect
    {
        Shoot,
        Hit,
        Explosion,
        GemCollect,
        LevelUp,
        Fever,
        Dash,
        Missile,
        Whirlwind,
        Mist,
        Slash
    }

    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource audioSource;
        private readonly Dictionary<SoundEffect, AudioClip> clipCache = new();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;

            GenerateProceduralClips();
        }

        private void GenerateProceduralClips()
        {
            clipCache[SoundEffect.Shoot] = GenerateTone(0.08f, 600f, 300f, WaveType.Square);
            clipCache[SoundEffect.Hit] = GenerateTone(0.06f, 220f, 90f, WaveType.Noise);
            clipCache[SoundEffect.Explosion] = GenerateTone(0.35f, 150f, 40f, WaveType.Noise);
            clipCache[SoundEffect.GemCollect] = GenerateTone(0.09f, 880f, 1760f, WaveType.Sine);
            clipCache[SoundEffect.Dash] = GenerateTone(0.12f, 300f, 600f, WaveType.Noise);
            clipCache[SoundEffect.LevelUp] = GenerateFanfare(0.4f);
            clipCache[SoundEffect.Fever] = GenerateFanfare(0.55f);
            clipCache[SoundEffect.Missile] = GenerateTone(0.2f, 250f, 750f, WaveType.Square);
            clipCache[SoundEffect.Whirlwind] = GenerateTone(0.28f, 180f, 90f, WaveType.Noise);
            clipCache[SoundEffect.Mist] = GenerateTone(0.18f, 900f, 400f, WaveType.Noise);
            clipCache[SoundEffect.Slash] = GenerateTone(0.14f, 850f, 150f, WaveType.Sine);
        }

        [SerializeField] private bool isMuted = false;
        public bool IsMuted => isMuted;

        public bool ToggleMute()
        {
            isMuted = !isMuted;
            if (audioSource != null)
            {
                audioSource.mute = isMuted;
            }
            return isMuted;
        }

        public void Play(SoundEffect sfx, float volume = 0.5f)
        {
            if (isMuted) return;

            if (clipCache.TryGetValue(sfx, out var clip) && clip != null)
            {
                audioSource.PlayOneShot(clip, volume);
            }
        }

        private enum WaveType { Sine, Square, Noise }

        private AudioClip GenerateTone(float duration, float startFreq, float endFreq, WaveType type)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                float currentFreq = Mathf.Lerp(startFreq, endFreq, t);
                float phase = 2f * Mathf.PI * currentFreq * ((float)i / sampleRate);

                float val = 0f;
                switch (type)
                {
                    case WaveType.Sine:
                        val = Mathf.Sin(phase);
                        break;
                    case WaveType.Square:
                        val = Mathf.Sin(phase) >= 0f ? 0.6f : -0.6f;
                        break;
                    case WaveType.Noise:
                        val = Random.Range(-0.8f, 0.8f) * (1f - t * 0.7f);
                        break;
                }

                // 페이드아웃 엔벨로프
                float env = 1f - t;
                samples[i] = val * env * 0.45f;
            }

            AudioClip clip = AudioClip.Create($"SFX_{type}", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private AudioClip GenerateFanfare(float duration)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.FloorToInt(sampleRate * duration);
            float[] samples = new float[totalSamples];

            float[] notes = { 523.25f, 659.25f, 783.99f, 1046.50f }; // C, E, G, High C

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / totalSamples;
                int noteIndex = Mathf.Clamp(Mathf.FloorToInt(t * notes.Length), 0, notes.Length - 1);
                float freq = notes[noteIndex];

                float phase = 2f * Mathf.PI * freq * ((float)i / sampleRate);
                float val = Mathf.Sin(phase);

                float noteProgress = (t * notes.Length) % 1f;
                float env = 1f - noteProgress * 0.5f;

                samples[i] = val * env * 0.4f;
            }

            AudioClip clip = AudioClip.Create("SFX_Fanfare", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
