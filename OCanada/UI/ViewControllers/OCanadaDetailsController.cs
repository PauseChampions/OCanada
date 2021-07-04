using System;
using System.ComponentModel;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using OCanada.Configuration;
using UnityEngine;
using Zenject;

namespace OCanada.UI
{
    public class OCanadaDetailsController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        private readonly GameplaySetupViewController gameplaySetupViewController;
        private bool parsed;
        private Mode selectedMode;

        public event PropertyChangedEventHandler PropertyChanged;
        internal event Action<Mode> PlayClicked;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private ModalView modalView;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public OCanadaDetailsController(GameplaySetupViewController gameplaySetupViewController)
        {
            this.gameplaySetupViewController = gameplaySetupViewController;
        }

        public void Initialize()
        {
            gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewController_didDeactivateEvent;
            parsed = false;
            selectedMode = Mode.None;
        }

        public void Dispose()
        {
            gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewController_didDeactivateEvent;
        }

        [UIAction("play-button-clicked")]
        private void PlayButtonClicked()
        {
            parserParams.EmitEvent("close-modal");
            PlayClicked?.Invoke(selectedMode);
        }

        private void GameplaySetupViewController_didDeactivateEvent(bool firstActivation, bool addedToHierarchy)
        {
            if (parsed && rootTransform != null && modalTransform != null)
            {
                modalTransform.SetParent(rootTransform);
                modalTransform.gameObject.SetActive(false);
            }
        }
        private void Parse(Transform parentTransform)
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "OCanada.UI.Views.OCanadaDetails.bsml"), parentTransform.gameObject, this);
                modalPosition = modalTransform.position;
                parsed = true;
            }
            modalTransform.position = modalPosition;
            FieldAccessor<ModalView, bool>.Set(ref modalView, "_animateParentCanvas", true);
        }

        internal void ShowModal(Transform parentTransform, int index)
        {
            Parse(parentTransform);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
            selectedMode = (Mode)index;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GameMode)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayActive)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextPage)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(HighScoreFormatted)));
        }

        [UIValue("game-mode")]
        private string GameMode => selectedMode.ToString();

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
                    return ""; // ah yes.
                }
            }
        }

        [UIValue("play-active")]
        private bool PlayActive => selectedMode != Mode.About;

        [UIValue("amongus")]
        private string TextPage
        {
            get
            {
                switch(selectedMode)
                {
                    case Mode.Standard:
                        return "Get as many points as you can in 30 seconds!" +
                            "\n\nWatch out for Canadian flags! Hitting those will subtract points!" +
                            "\n\nThe BSWC and BSWC Staff logos give bonus points." +
                            "\n\nGood luck 🙂";
                    case Mode.Endless:
                        return "How long can you go? 😳" +
                            "\n\nStart off with 10 seconds. Every flag you click will add time. " +
                            "Don't hit Canadian flags though! Hitting those will subtract time." +
                            "\n\nGood luck 🙂";
                    case Mode.About:
                        return "O Canada is essentially a Beat Saber port of Whack-a-Mole for Epic" +
                            " Canadians competing in the World cup! Click on competitors' flags to " +
                            "assert dominance and compete against your team mates (IN A FRIENDLY WAY THOUGH)." +
                            "\n\nGood luck Canadian gamers!" +
                            "\n - PauseChampions™ Team";
                    default:
                        return null;
                }
            }
        }
    }

    public enum Mode
    {
        Standard,
        Endless,
        About,
        None
    }
}