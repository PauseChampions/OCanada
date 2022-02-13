using System.Collections.Generic;
using UnityEngine;

namespace OCanada.Flag
{
    public class FlagLoader
    {
        private static readonly Dictionary<string, Sprite> cachedSprites = new Dictionary<string, Sprite>();

        public static Sprite GetSprite(string name)
        {
            if (cachedSprites.TryGetValue(name, out var cachedSprite))
            {
                return cachedSprite;
            }
            
            var sprite = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly($"OCanada.Images.{name}.png");
            cachedSprites.Add(name, sprite);
            return sprite;
        }
    }
}
