using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using BeatSaberMarkupLanguage;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.GameplaySetup;
using BeatSaberMarkupLanguage.ViewControllers;
using HMUI;
using UnityEngine;
using Zenject;


namespace OCanada.UI.ViewControllers
{
    class OCanadaMenuController : IInitializable, IDisposable, INotifyPropertyChanged
    {
        private OCanadaDetailsController oCanadaDetailsController;
        public event PropertyChangedEventHandler PropertyChanged;

        [UIComponent("root")]
        private readonly RectTransform rootTransform;

        [UIComponent("list")]
        public CustomListTableData customListTableData;

        public OCanadaMenuController(OCanadaDetailsController oCanadaDetailsController)
        {
            this.oCanadaDetailsController = oCanadaDetailsController;
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

        public void Initialize()
        {
            GameplaySetup.instance.AddTab("O Canada", "OCanada.UI.Views.OCanadaMenu.bsml", this);           
        }

        public void Dispose()
        {
            GameplaySetup.instance?.RemoveTab("O Canada");
        }

        [UIAction("funny-selected")]
        private void FunnySelected(TableView tableView, int index)
        {
            customListTableData.tableView.ClearSelection();
            oCanadaDetailsController.ShowModal(rootTransform);
        }
    }
}
