using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.GameplaySetup;
using UnityEngine;
using Zenject;

namespace OCanada.UI
{
    internal class OCanadaGameController : IInitializable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool parsed;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIValue("clickable-images")]
        private List<object> clickableImages = Enumerable.Range(1, 24).Select(i =>
        {
            return new ClickableImage() as object;
        }).ToList();
        public void Initialize()
        {
            parsed = false;
        }

        private void Parse(RectTransform siblingTranform)
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "OCanada.UI.Views.OCanadaGame.bsml"), siblingTranform.parent.gameObject, this);
                parsed = true;
            }
            rootTransform.SetParent(siblingTranform.parent);
        }

        internal void StartGame(RectTransform siblingTransform)
        {
            Parse(siblingTransform);
        }
    }
}
