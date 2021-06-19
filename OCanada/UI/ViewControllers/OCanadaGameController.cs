using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.ViewControllers;
using Zenject;

namespace OCanada.UI.ViewControllers
{
    internal class OCanadaGameController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [UIValue("clickable-images")]
        private List<object> clickableImages = Enumerable.Range(1, 24).Select(i =>
        {
            return new ClickableImage() as object;
        }).ToList();
        public void Initialize()
        {
            GameplaySetup.instance.AddTab("O Canada", "OCanada.UI.Views.OCanadaGame.bsml", this);
        }

        public void Dispose()
        {
            GameplaySetup.instance?.RemoveTab("O Canada");
        }

    }
}
