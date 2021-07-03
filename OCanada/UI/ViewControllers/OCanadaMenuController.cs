using System;
using System.ComponentModel;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.GameplaySetup;
using HMUI;
using UnityEngine;
using Zenject;


namespace OCanada.UI
{
    class OCanadaMenuController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        private OCanadaDetailsController oCanadaDetailsController;
        private readonly OCanadaGameController oCanadaGameController;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        public OCanadaMenuController(OCanadaDetailsController oCanadaDetailsController, OCanadaGameController oCanadaGameController)
        {
            this.oCanadaDetailsController = oCanadaDetailsController;
            this.oCanadaGameController = oCanadaGameController;
        }

        public void Initialize()
        {
            GameplaySetup.instance.AddTab("O Canada", "OCanada.UI.Views.OCanadaMenu.bsml", this);
            oCanadaDetailsController.PlayClicked += OCanadaDetailsController_PlayClicked;
        }

        public void Dispose()
        {
            GameplaySetup.instance?.RemoveTab("O Canada");
            oCanadaDetailsController.PlayClicked -= OCanadaDetailsController_PlayClicked;
        }

        private void OCanadaDetailsController_PlayClicked()
        {
            oCanadaGameController.StartGame(rootTransform);
            rootTransform.gameObject.SetActive(false);
        }

        [UIAction("#post-parse")]
        private void PostParse()
        {
            customListTableData.data.Clear();
            customListTableData.data.Add(new CustomListTableData.CustomCellInfo("⏱ Standard"));
            customListTableData.data.Add(new CustomListTableData.CustomCellInfo("♾ Endless"));
            customListTableData.data.Add(new CustomListTableData.CustomCellInfo("📕 About"));
            customListTableData.tableView.ReloadData();
        }

        [UIAction("funny-selected")]
        private void FunnySelected(TableView tableView, int index)
        {
            customListTableData.tableView.ClearSelection();
            oCanadaDetailsController.ShowModal(rootTransform);
        }
    }
}
