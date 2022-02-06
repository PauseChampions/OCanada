using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using IPA.Utilities;
using UnityEngine;

namespace OCanada.GameplaySetupScene
{
    internal class AudioPlayer
    {
        private readonly AudioClipAsyncLoader audioClipAsyncLoader;
        private readonly SongPreviewPlayer songPreviewPlayer;
        private AudioSource audioSource;
        
        private static readonly DirectoryInfo RootDir = new DirectoryInfo(Path.Combine(UnityGame.UserDataPath, nameof(OCanada)));
        
        private static readonly DirectoryInfo NoteDir = new DirectoryInfo(Path.Combine(RootDir.ToString(), "notes"));
        
        private readonly FileInfo NoteAudioFile;

        private bool ShouldInitialize() => !RootDir.Exists || !NoteDir.Exists || NoteDir.GetFiles().Length <= 0;
        public event Action ClipFinishedEvent;

        public AudioPlayer(AudioClipAsyncLoader audioClipAsyncLoader, SongPreviewPlayer songPreviewPlayer)
        {
            this.audioClipAsyncLoader = audioClipAsyncLoader;
            this.songPreviewPlayer = songPreviewPlayer;
            
            if (ShouldInitialize()) Initialize();
            NoteAudioFile = NoteDir.GetFiles()[0];
        }

        public void PlayNote(string note) => PlayClip(NoteAudioFile, Notes.NotePitches[note]/440f);

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
            if (!RootDir.Exists || !NoteDir.Exists)
            {
                Directory.CreateDirectory(Path.Combine(UnityGame.UserDataPath, nameof(OCanada)));
                RootDir.Refresh();
            }

            if (!NoteDir.Exists)
            {
                Directory.CreateDirectory(Path.Combine(RootDir.ToString(), "notes"));
                NoteDir.Refresh();
            }

            if (NoteDir.GetFiles().Length <= 0)
            {
                var manifestStream = Assembly.
                    GetExecutingAssembly()
                    .GetManifestResourceStream("OCanada.CustomSounds.a4.ogg");
                if (manifestStream != null)
                {
                    var fs = File.Create(Path.Combine(NoteDir.ToString(), "a4.ogg"));
                    manifestStream.CopyTo(fs);
                }
                else
                {
                    Plugin.Log.Debug(Assembly.GetExecutingAssembly().ToString());
                }
            }
            NoteDir.Refresh();
        }
    }
}

