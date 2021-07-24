using UnityEngine;

namespace OCanada
{
    internal class Author
    {
        public string Name;
        public string Description;
        public Sprite Image;

        public static Author Sabooboo = new Author()
        {
            Name = nameof(Sabooboo),
            Description = "He's a pooper and he is cheesed to meet you. Say hello to 😳 Sabooboo. Worked on game logic.",
            Image = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("OCanada.Images.Sabooboo.png")
        };

        public static Author Skalx = new Author()
        {
            Name = nameof(Skalx),
            Description = "Guy who made the UI, member of Team Canada in WC. He's confused.",
            Image = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("OCanada.Images.Skalx.png")
        };

        public static Author PixelBoom = new Author()
        {
            Name = nameof(PixelBoom),
            Description = "Brought the team together, he's Nick Fury but without the eyepatch. Taught everyone here how to mod.",
            Image = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("OCanada.Images.PixelBoom.png")
        };

        public static Author Edison = new Author()
        {
            Name = "Edison | Sh*tmissJesus",
            Description = "Author of BSPlugin1, aka the best Beat Saber mod ever.",
            Image = BeatSaberMarkupLanguage.Utilities.FindSpriteInAssembly("OCanada.Images.Edison.png")
        };
    }
}
