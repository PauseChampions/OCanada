using System;
using System.Collections.Generic;
using UnityEngine;

namespace OCanada
{
    public class FlagImage
    {
        public string Path { get; private set; }
        private Sprite _sprite;
        private bool SpriteLoadQueued;
        public int PointValue { get; private set; }

        public bool SpriteWasLoaded { get; private set; }
        public bool Blacklist { get; private set; }
        public event EventHandler SpriteLoaded;

        private static readonly object _loaderLock = new object();
        private static bool CoroutineRunning = false;
        private static readonly Queue<Action> SpriteQueue = new Queue<Action>();

        public FlagImage(string path, int pointValue)
        {
            Path = path;
            SpriteWasLoaded = false;
            Blacklist = false;
            SpriteLoadQueued = false;
            PointValue = pointValue;
        }

        public Sprite Sprite
        {
            get
            {
                if (_sprite == null)
                {
                    if (!SpriteLoadQueued)
                    {
                        SpriteLoadQueued = true;
                        QueueLoadSprite(this);
                    }
                    return BeatSaberMarkupLanguage.Utilities.ImageResources.WhitePixel;
                }
                return _sprite;
            }
        }

        public static YieldInstruction LoadWait = new WaitForEndOfFrame();

        private static void QueueLoadSprite(FlagImage flagImage)
        {
            SpriteQueue.Enqueue(() =>
            {
                try
                {
                    flagImage._sprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly(flagImage.Path);
                    if (flagImage._sprite != null)
                    {
                        flagImage.SpriteWasLoaded = true;
                    }
                    else
                    {
                        Plugin.Log.Critical("Could not load " + flagImage.Path);
                        flagImage.SpriteWasLoaded = false;
                        flagImage.Blacklist = true;
                    }
                    flagImage.SpriteLoaded?.Invoke(flagImage, null);
                }
                catch (Exception e)
                {
                    Plugin.Log.Critical("Could not load " + flagImage.Path + "\nException message: " + e.Message);
                    flagImage.SpriteWasLoaded = false;
                    flagImage.Blacklist = true;
                    flagImage.SpriteLoaded?.Invoke(flagImage, null);
                }
            });

            if (!CoroutineRunning)
                SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
        }

        private static IEnumerator<YieldInstruction> SpriteLoadCoroutine()
        {
            lock (_loaderLock)
            {
                if (CoroutineRunning)
                    yield break;
                CoroutineRunning = true;
            }
            while (SpriteQueue.Count > 0)
            {
                yield return LoadWait;
                var loader = SpriteQueue.Dequeue();
                loader?.Invoke();
            }
            CoroutineRunning = false;
            if (SpriteQueue.Count > 0)
                SharedCoroutineStarter.instance.StartCoroutine(SpriteLoadCoroutine());
        }
    }
}
