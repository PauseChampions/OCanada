using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using System;
using System.ComponentModel;
using System.Reflection;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace OCanada.UI
{
    internal class OCanadaAuthorModalController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        private readonly GameplaySetupViewController gameplaySetupViewController;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool parsed;
        private Author selectedAuthor;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private ModalView modalView;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        [UIComponent("author-image")]
        private ImageView authorImage;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        public OCanadaAuthorModalController(GameplaySetupViewController gameplaySetupViewController)
        {
            this.gameplaySetupViewController = gameplaySetupViewController;
        }

        public void Initialize()
        {
            gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewController_didDeactivateEvent;
            parsed = false;
        }

        public void Dispose()
        {
            gameplaySetupViewController.didDeactivateEvent -= GameplaySetupViewController_didDeactivateEvent;
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
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "OCanada.UI.Views.OCanadaAuthorModal.bsml"), parentTransform.gameObject, this);
                modalPosition = modalTransform.localPosition;
                parsed = true;
            }
            modalTransform.localPosition = modalPosition;
            FieldAccessor<ModalView, bool>.Set(ref modalView, "_animateParentCanvas", true);
        }

        internal void ShowModal(Transform parentTransform, Author selectedAuthor)
        {
            Parse(parentTransform);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
            this.selectedAuthor = selectedAuthor;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AuthorName)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AuthorAbout)));
            authorImage.sprite = selectedAuthor.Image;
        }

        [UIValue("author-name")]
        private string AuthorName => selectedAuthor == null ? "" : selectedAuthor.Name;

        [UIValue("about")]
        private string AuthorAbout => selectedAuthor == null ? "" : selectedAuthor.Description;
    }
}
