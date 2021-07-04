﻿using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Zenject;

namespace OCanada.UI.ViewControllers
{
    internal class OCanadaPauseMenuController : IInitializable, IDisposable
    {
        public event Action ExitClicked;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        private bool parsed;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private ModalView modalView;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        public void Initialize()
        {
            parsed = false;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        [UIAction("resume-pressed")]
        private void ResumeButtonPressed()
        {
            parserParams.EmitEvent("close-modal");
            // resume(); u feel
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