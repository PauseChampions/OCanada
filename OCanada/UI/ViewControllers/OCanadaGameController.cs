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
    internal class OCanadaGameController : MonoBehaviour, IInitializable, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool parsed;
        private Mode selectedMode;
        private OCanadaPauseMenuController oCanadaPauseMenuController;
        private int prevUpdate;
        private float currentTime;
        private readonly float SPAWN_TIME = 5000;
        public event Action GameExit;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIValue("clickable-images")]
        private List<object> clickableImages = Enumerable.Range(1, 24).Select(i =>
        {
            return new ClickableFlag() as object;
        }).ToList();

        [Inject]
        public void Construct(OCanadaPauseMenuController oCanadaPauseMenuController)
        {
            this.oCanadaPauseMenuController = oCanadaPauseMenuController;
        }

        public void Initialize()
        {
            parsed = false;
            gameObject.SetActive(false);
            selectedMode = Mode.None;
            prevUpdate = 1;
            currentTime = SPAWN_TIME;
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
                gameObject.SetActive(true);
            }
            rootTransform.SetParent(siblingTranform.parent);
            transform.SetParent(rootTransform);
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

        public void Update()
        {
            if (Time.time * 1000 >= prevUpdate)
            {
                var currentUpdate = Mathf.FloorToInt(Time.time * 1000);
                var difference = currentUpdate - prevUpdate; //Calculate difference in time
                SpawnFlags(difference); //Pass difference to flash image
                prevUpdate = currentUpdate;
            }
        }

        private void SpawnFlags(int difference)
        {
            currentTime -= difference;
            if (currentTime <= 0)
            {
                currentTime = SPAWN_TIME;
            }
        }

        private void ExitGame()
        {
            // TODO: exit game pog
        }

        private void FlagImage_SpriteLoaded(object sender, System.EventArgs e)
        {
            if (sender is FlagImage flagImage)
            {
                foreach (var clickableFlag in clickableImages.OfType<ClickableFlag>())
                {
                    clickableFlag.clickableImage.sprite = flagImage.Sprite;
                }
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
