using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using OCanada.Configuration;
using System;
using System.ComponentModel;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace OCanada.UI
{
    class OCanadaResultsScreenController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        private bool parsed;
        private Mode selectedMode;

        GameplaySetupViewController gameplaySetupViewController;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private ModalView modalView;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        private int _currentScore;
        private int CurrentScore
        {
            get => _currentScore;
            set
            {
                _currentScore = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentScoreFormatted)));
            }
        }

        [UIValue("current-score")]
        private string CurrentScoreFormatted => $"Score: {CurrentScore}";

        [UIValue("high-score")]
        private string HighScoreFormatted
        {
            get
            {
                int highScore;
                if (selectedMode == Mode.Standard)
                {
                    highScore = PluginConfig.Instance.HighScoreStandard;
                }
                else if (selectedMode == Mode.Endless)
                {
                    highScore = PluginConfig.Instance.HighScoreEndless;
                }
                else
                {
                    return ""; // ah yes.
                }

                if (highScore == CurrentScore)
                {
                    return $"<color=green>High Score: {highScore}</color>";
                }
                return $"High Score: {highScore}";
            }
        }

        public OCanadaResultsScreenController(GameplaySetupViewController gameplaySetupViewController)
        {
            this.gameplaySetupViewController = gameplaySetupViewController;
        }
        public void Initialize()
        {
            gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewController_didDeactivateEvent;
            parsed = false;
            _currentScore = 0;
        }

        private void GameplaySetupViewController_didDeactivateEvent(bool firstActivation, bool addedToHierarchy)
        {
            if (parsed && rootTransform != null && modalTransform != null)
            {
                modalTransform.SetParent(rootTransform);
                modalTransform.gameObject.SetActive(false);
            }
        }

        public void Dispose()
        {
            gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewController_didDeactivateEvent;
        }

        private void Parse(Transform parentTransform)
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "OCanada.UI.Views.OCanadaResultsScreen.bsml"), parentTransform.gameObject, this);
                modalPosition = modalTransform.localPosition;
                parsed = true;
            }
            modalTransform.localPosition = modalPosition;
            FieldAccessor<ModalView, bool>.Set(ref modalView, "_animateParentCanvas", true);
        }

        internal void ShowModal(Transform parentTransform, Mode selectedMode, int score)
        {
            Parse(parentTransform);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
            this.selectedMode = selectedMode;
            CurrentScore = score;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighScoreFormatted)));
        }
    }
}
