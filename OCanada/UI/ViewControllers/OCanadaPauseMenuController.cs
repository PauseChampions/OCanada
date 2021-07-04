using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Parser;
using HMUI;
using IPA.Utilities;
using System;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace OCanada.UI
{
    internal class OCanadaPauseMenuController : IInitializable, IDisposable
    {
        public event Action ResumeClicked;
        public event Action ExitClicked;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        private bool parsed;

        GameplaySetupViewController gameplaySetupViewController;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private ModalView modalView;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        public OCanadaPauseMenuController(GameplaySetupViewController gameplaySetupViewController)
        {
            this.gameplaySetupViewController = gameplaySetupViewController;
        }
        public void Initialize()
        {
            gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewController_didDeactivateEvent;
            parsed = false;
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

        [UIAction("resume-pressed")]
        private void ResumeButtonPressed()
        {
            parserParams.EmitEvent("close-modal");
            ResumeClicked?.Invoke();
        }

        [UIAction("exit-pressed")]
        private void ExitButtonPressed()
        {
            parserParams.EmitEvent("close-modal");
            ExitClicked?.Invoke();
        }


        private void Parse(Transform parentTransform)
        {
            if (!parsed)
            {
                BSMLParser.instance.Parse(Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "OCanada.UI.Views.OCanadaPauseMenu.bsml"), parentTransform.gameObject, this);
                modalPosition = modalTransform.position;
                parsed = true;
            }
            modalTransform.position = modalPosition;
            FieldAccessor<ModalView, bool>.Set(ref modalView, "_animateParentCanvas", true);
        }

        internal void ShowModal(Transform parentTransform)
        {
            Parse(parentTransform);
            parserParams.EmitEvent("close-modal");
            parserParams.EmitEvent("open-modal");
        }
    }
}
