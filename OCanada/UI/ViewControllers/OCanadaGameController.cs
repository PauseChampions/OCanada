﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using OCanada.Configuration;
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
        private string _userName;
        private IPlatformUserModel platformUserModel;

        private int _score;
        private int Score
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ScoreFormatted)));
            }
        }

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIValue("clickable-images")]
        private List<object> clickableImages = Enumerable.Range(1, 24).Select(i =>
        {
            return new ClickableFlag() as object;
        }).ToList();



        [UIValue("username")]
        private string Username
        {
            get => _userName;
            set
            {
                _userName = value.Substring(0, 3).ToUpper();
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Username)));
            }
        }

        [UIValue("timer")]
        private string FormattedTimer => "0.001s";

        [UIValue("score")]
        private string ScoreFormatted
        {
            get => $"Score: {Score}";
        }

        [UIValue("high-score")]
        private string HighScoreFormatted
        {
            get
            {
                if (selectedMode == Mode.Standard)
                {
                    return $"High Score: {PluginConfig.Instance.HighScoreStandard}";
                }
                else if (selectedMode == Mode.Endless)
                {
                    return $"High Score: {PluginConfig.Instance.HighScoreEndless}";
                }
                else
                {
                    return "uh oh";
                }
            }
        }
        public OCanadaGameController(OCanadaPauseMenuController oCanadaPauseMenuController, IPlatformUserModel platformUserModel)
        {
            this.oCanadaPauseMenuController = oCanadaPauseMenuController;
            this.platformUserModel = platformUserModel;
        }
        public void Initialize()
        {
            parsed = false;
            selectedMode = Mode.None;
            GetUsername();
            oCanadaPauseMenuController.ExitClicked += OCanadaPauseMenuController_ExitClicked;
        }

        public void Dispose()
        {
            oCanadaPauseMenuController.ExitClicked -= OCanadaPauseMenuController_ExitClicked;
        }

        private async void GetUsername()
        {
            UserInfo user = await platformUserModel.GetUserInfo();
            Username = user.userName;
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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighScoreFormatted)));
            Score = 0;
            FlagImage flagImage = new FlagImage("OCanada.Images.Canada.png", 1);
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
