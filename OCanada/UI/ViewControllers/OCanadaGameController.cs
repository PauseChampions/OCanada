using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using OCanada.UI.ViewControllers;
using UnityEngine;
using Zenject;

namespace OCanada.UI
{
    internal class OCanadaGameController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool parsed;
        private Mode selectedMode;
        private OCanadaPauseMenuController oCanadaPauseMenuController;
        public event Action GameExit;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIValue("clickable-images")]
        private List<object> clickableImages = Enumerable.Range(1, 24).Select(i =>
        {
            return new ClickableFlag() as object;
        }).ToList();

        public OCanadaGameController(OCanadaPauseMenuController oCanadaPauseMenuController)
        {
            this.oCanadaPauseMenuController = oCanadaPauseMenuController;
        }
        public void Initialize()
        {
            parsed = false;
            selectedMode = Mode.None;
            oCanadaPauseMenuController.ExitClicked += OCanadaPauseMenuController_ExitClicked;
        }

        public void Dispose()
        {
            oCanadaPauseMenuController.ExitClicked -= OCanadaPauseMenuController_ExitClicked;
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

        internal void StartGame(RectTransform siblingTransform, Mode selectedMode)
        {
            Parse(siblingTransform);
            rootTransform.gameObject.SetActive(true);
            this.selectedMode = selectedMode;
            FlagImage flagImage = new FlagImage("OCanada.Images.Canada.png");
            flagImage.SpriteLoaded += FlagImage_SpriteLoaded;
            _ = flagImage.Sprite;
        }

        internal void ExitGame()
        {
            // TODO: exit game pog
        }

        private void FlagImage_SpriteLoaded(object sender, System.EventArgs e)
        {
            if (sender is FlagImage flagImage)
            {
                (clickableImages[0] as ClickableFlag).clickableImage.sprite = flagImage.Sprite;
                flagImage.SpriteLoaded -= FlagImage_SpriteLoaded;
            }
        }

        [UIAction("pause-clicked")]
        private void PauseButtonClicked()
        {
            oCanadaPauseMenuController.ShowModal(rootTransform);
            // pause(); u feel
        }

        private void OCanadaPauseMenuController_ExitClicked()
        {
            rootTransform.gameObject.SetActive(false);
            GameExit?.Invoke();
        }
    }

    internal class ClickableFlag
    {
        [UIComponent("clickable-image")]
        internal ClickableImage clickableImage;
    }
}
