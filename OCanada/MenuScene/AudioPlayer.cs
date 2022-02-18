using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IPA.Utilities;
using JetBrains.Annotations;
using UnityEngine;

namespace OCanada.GameplaySetupScene
{
    internal class AudioPlayer
    {
        private readonly AudioClipAsyncLoader audioClipAsyncLoader;
        private readonly SongPreviewPlayer songPreviewPlayer;
        private AudioSource audioSource;
        private readonly FileInfo noteAudioFile;
        
        private static readonly DirectoryInfo RootDir = new DirectoryInfo(UnityGame.UserDataPath);

        private static readonly string FileName = "OCanadaHitSound.ogg";
        public event Action ClipFinishedEvent;

        public AudioPlayer(AudioClipAsyncLoader audioClipAsyncLoader, SongPreviewPlayer songPreviewPlayer)
        {
            this.audioClipAsyncLoader = audioClipAsyncLoader;
            this.songPreviewPlayer = songPreviewPlayer;
            
            Initialize();
            noteAudioFile = RootDir.GetFiles(FileName)[0];
        }

        public void PlayNote(string note) => PlayClip(noteAudioFile, Notes.NotePitches[note]/440f);

        private async void PlayClip(FileInfo audioFile, float pitch = 1f, bool notifyFinished = false)
        {
            var audioClip = await audioClipAsyncLoader.Load(audioFile.FullName);
            if (audioClip == null) return;
            
            if (audioSource == null)
            {
                audioSource = new GameObject("Test AudioSource").AddComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = songPreviewPlayer
                    .GetField<AudioSource, SongPreviewPlayer>("_audioSourcePrefab").outputAudioMixerGroup;
            }

            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip, 5f);

            if (!notifyFinished) return;
            await Task.Delay(audioClip.length.TotalSeconds() * 1000 + audioClip.length.Milliseconds() + 125);
            ClipFinishedEvent?.Invoke();
        }

        private void Initialize()
        {
            if (RootDir.GetFiles(FileName).Length > 0) return;
            
            var manifestStream = Assembly.
                GetExecutingAssembly()
                .GetManifestResourceStream("OCanada.CustomSounds.a4.ogg");
            if (manifestStream != null)
            {
                var fs = File.Create(Path.Combine(RootDir.ToString(), FileName));
                manifestStream.CopyTo(fs);
            }
            RootDir.Refresh();
        }
    }
}

