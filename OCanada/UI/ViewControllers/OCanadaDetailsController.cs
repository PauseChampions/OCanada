using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.Parser;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using IPA.Utilities;
using UnityEngine;
using Zenject;

namespace OCanada.UI.ViewControllers
{
    class OCanadaDetailsController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        private GameplaySetupViewController gameplaySetupViewController;
        public event PropertyChangedEventHandler PropertyChanged;
        private bool parsed;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("modal")]
        private ModalView modalView;

        [UIComponent("modal")]
        private readonly RectTransform modalTransform;

        private Vector3 modalPosition;

        [UIParams]
        private readonly BSMLParserParams parserParams;

        [UIValue("amongus")]
        public static string amongus = "Justin Lachance-Guillemette, toujours champion canadien du jeu vidéo Beat Saber. Un jeune Félicinois, Justin Lachance-Guillemette, alias Electrostats, a défendu avec succès, dimanche dernier, son titre de champion canadien du jeu vidéo Beat Saber. Le jeune homme de 22 ans, natif de Saint-Félicien, a ainsi battu au jeu Beat Saber son adversaire, Orinix, un joueur provenant de la Nouvelle-Écosse, en finale du tournoi canadien, division AAA. Pour le néophyte, Beat Saber est en fait un jeu vidéo en réalité virtuelle, où le joueur doit détruire des blocs à l’aide de sabres en suivant le rythme de différentes chansons, le tout en évitant des obstacles pour obtenir le meilleur taux d'efficacité. Bien humblement, il avoue faire partie de l’élite mondiale pour ce jeu vidéo. Dans un classement qui réunit les 300 000 meilleurs joueurs au monde, Justin Lachance-Guillemette, ou plutôt Electrostats, occupe présentement le 6e rang. D'ailleurs, il est le seul Canadien dans le top-10 mondial, classement largement dominé par les Américains. (Justin Lachance-Guillemette, alias Electrostats, a défendu avec succès son titre de champion canadien du jeu vidéo Beat Saber)";

        public OCanadaDetailsController(GameplaySetupViewController gameplaySetupViewController)
        {
            this.gameplaySetupViewController = gameplaySetupViewController;
            parsed = false;
        }

        public void Initialize()
        {
            gameplaySetupViewController.didDeactivateEvent += GameplaySetupViewController_didDeactivateEvent;
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
                BSMLParser.instance.Parse(BeatSaberMarkupLanguage.Utilities.GetResourceContent(Assembly.GetExecutingAssembly(), "OCanada.UI.Views.OCanadaDetails.bsml"), parentTransform.gameObject, this);
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