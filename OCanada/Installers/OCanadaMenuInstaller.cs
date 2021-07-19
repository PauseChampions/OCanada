using OCanada.UI;
using SiraUtil;
using Zenject;

namespace OCanada.Installers
{
    class OCanadaMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<OCanadaMenuController>().AsSingle();
            Container.BindInterfacesAndSelfTo<OCanadaDetailsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<OCanadaGameController>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesAndSelfTo<OCanadaPauseMenuController>().AsSingle();
            Container.BindInterfacesAndSelfTo<OCanadaResultsScreenController>().AsSingle();
            Container.BindInterfacesAndSelfTo<OCanadaAuthorModalController>().AsSingle();
        }
    }
}
