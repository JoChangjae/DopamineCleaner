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
        private AudioSource bgmSource;
        private readonly Dictionary<SoundEffect, AudioClip> clipCache = new();

        private AudioClip townBgmClip;
        private AudioClip battleBgmClip;

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

            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
            bgmSource.volume = 0.35f;

            GenerateProceduralClips();
            GenerateProceduralBGM();
        }

        private void Start()
        {
            // 씬 시작 시 기본 BGM 재생
            bool isBattle = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainGame";
            PlayBGM(isBattle);
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

        private void GenerateProceduralBGM()
        {
            townBgmClip = GenerateBGMTrack(8.0f, 105f, false);
            battleBgmClip = GenerateBGMTrack(6.0f, 138f, true);
        }

        public void PlayBGM(bool isBattle)
        {
            if (bgmSource == null) return;

            AudioClip target = isBattle ? battleBgmClip : townBgmClip;
            if (bgmSource.clip == target && bgmSource.isPlaying) return;

            bgmSource.clip = target;
            bgmSource.pitch = 1.0f;
            if (!isMuted && target != null)
            {
                bgmSource.Play();
            }
        }

        public void SetFeverPitch(bool isFever)
        {
            if (bgmSource != null)
            {
                bgmSource.pitch = isFever ? 1.25f : 1.0f;
            }
        }

        [SerializeField] private bool isMuted = false;
        public bool IsMuted => isMuted;

        [SerializeField] private float bgmVolume = 0.35f;
        [SerializeField] private float sfxVolume = 1.0f;

        public float BGMVolume => bgmVolume;
        public float SFXVolume => sfxVolume;

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetBGMVolume(float vol)
        {
            bgmVolume = Mathf.Clamp01(vol);
            if (bgmSource != null) bgmSource.volume = bgmVolume;
        }

        public void SetSFXVolume(float vol)
        {
            sfxVolume = Mathf.Clamp01(vol);
        }

        public bool ToggleMute()
        {
            isMuted = !isMuted;
            if (audioSource != null) audioSource.mute = isMuted;
            if (bgmSource != null)
            {
                bgmSource.mute = isMuted;
                if (!isMuted && !bgmSource.isPlaying && bgmSource.clip != null)
                {
                    bgmSource.Play();
                }
            }
            return isMuted;
        }

        public void Play(SoundEffect sfx, float volume = 0.5f, bool randomizePitch = true)
        {
            if (isMuted) return;

            if (clipCache.TryGetValue(sfx, out var clip) && clip != null)
            {
                if (randomizePitch && audioSource != null)
                {
                    audioSource.pitch = Random.Range(0.93f, 1.07f);
                }
                else if (audioSource != null)
                {
                    audioSource.pitch = 1.0f;
                }
                audioSource.PlayOneShot(clip, volume * sfxVolume);
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

        private AudioClip GenerateBGMTrack(float loopDuration, float bpm, bool isBattle)
        {
            int sampleRate = 44100;
            int totalSamples = Mathf.FloorToInt(sampleRate * loopDuration);
            float[] samples = new float[totalSamples];

            // 멜로디 및 베이스 펜타토닉 음계 (A 마이너 / C 메이저)
            float[] scale = isBattle
                ? new float[] { 220f, 261.63f, 293.66f, 329.63f, 392f, 440f, 523.25f, 659.25f } // A Minor Pentatonic
                : new float[] { 261.63f, 293.66f, 329.63f, 392f, 440f, 523.25f, 587.33f };      // C Major Pentatonic

            float beatInterval = 60f / bpm;
            int totalBeats = Mathf.FloorToInt(loopDuration / beatInterval);

            for (int i = 0; i < totalSamples; i++)
            {
                float timeSec = (float)i / sampleRate;
                float beatIndex = timeSec / beatInterval;
                int currentBeat = Mathf.FloorToInt(beatIndex);

                // 1. 멜로디 (아르페지오)
                int noteIdx = (currentBeat * 3 + Mathf.FloorToInt((beatIndex % 1f) * 4)) % scale.Length;
                float freq = scale[noteIdx];
                float melPhase = 2f * Mathf.PI * freq * timeSec;
                float melVal = Mathf.Sin(melPhase) >= 0f ? 0.15f : -0.15f; // Square wave

                // 2. 베이스라인
                float bassFreq = scale[currentBeat % 4] * 0.5f;
                float bassPhase = 2f * Mathf.PI * bassFreq * timeSec;
                float bassVal = Mathf.Sin(bassPhase) * 0.22f; // Sine wave

                // 3. 퍼커션 (하이햇 노이즈 틱)
                float subBeat = (beatIndex * 2f) % 1f;
                float percVal = (subBeat < 0.12f) ? Random.Range(-0.1f, 0.1f) * (1f - subBeat / 0.12f) : 0f;

                // 믹싱
                samples[i] = (melVal + bassVal + percVal) * 0.35f;
            }

            AudioClip clip = AudioClip.Create(isBattle ? "BGM_Battle" : "BGM_Town", totalSamples, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
