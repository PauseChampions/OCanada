using System;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using HMUI;
using IPA.Utilities;
using UnityEngine;

namespace OCanada.UI
{
    internal class ClickableFlag
    {
        [UIComponent("clickable-image")]
        private ClickableImage clickableImage;
        internal event Action<int> FlagClickedEvent;

        internal int PointValue;

        internal void SetImage(Sprite flagImage, int pointValue = 0)
        {
            PointValue = pointValue;
            if (flagImage == null)
            {
                clickableImage.sprite = Utilities.ImageResources.BlankSprite;
                return;
            }
            
            clickableImage.sprite = flagImage;
            clickableImage.HighlightColor = PointValue >= 5
                ? Color.yellow
                : Color.red;
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            FieldAccessor<ImageView, float>.Set(clickableImage, "_skew", 0);
        }

        [UIAction("flag-clicked")]
        private void FlagClicked()
        {
            int pointValue = PointValue;
            PointValue = 0;
            clickableImage.sprite = Utilities.ImageResources.BlankSprite;
            FlagClickedEvent?.Invoke(pointValue);
        }
    }
}
