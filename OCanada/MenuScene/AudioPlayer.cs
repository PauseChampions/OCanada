using System;
using System.IO;
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
        private static readonly DirectoryInfo TestDir = new DirectoryInfo(Path.Combine(RootDir.ToString(), "test"));
        private static readonly DirectoryInfo NotesDir = new DirectoryInfo(Path.Combine(RootDir.ToString(), "notes"));
        
        private readonly FileInfo NoteAudioFile;

        public event Action ClipFinishedEvent;

        // public static bool ShouldInitialize() => RootDir.Exists && NotesDir.Exists && NotesDir.GetFiles().Length > 0;
        
        public AudioPlayer(AudioClipAsyncLoader audioClipAsyncLoader, SongPreviewPlayer songPreviewPlayer)
        {
            this.audioClipAsyncLoader = audioClipAsyncLoader;
            this.songPreviewPlayer = songPreviewPlayer;
            // TODO: add audio file if not exists in dir
            NoteAudioFile = NotesDir.GetFiles()[0];
        }

        public void PlayNote(string note) => PlayClip(NoteAudioFile, Notes.NotePitches[note]/440f);

        private async void PlayClip(FileInfo audioFile, float pitch = 5f, bool notifyFinished = false)
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
            audioSource.PlayOneShot(audioClip, 1f);

            if (!notifyFinished) return;
            await Task.Delay(audioClip.length.TotalSeconds() * 1000 + audioClip.length.Milliseconds() + 125);
            ClipFinishedEvent?.Invoke();
        }
    }
}

