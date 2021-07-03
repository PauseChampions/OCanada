using System;
using System.ComponentModel;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
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
        }

        [UIValue("game-mode")]
        private string GameMode => selectedMode.ToString();

        [UIValue("play-active")]
        private bool PlayActive => selectedMode != Mode.About;

        [UIValue("amongus")]
        private string TextPage => "venogay";
    }

    public enum Mode
    {
        Standard,
        Endless,
        About,
        None
    }
}