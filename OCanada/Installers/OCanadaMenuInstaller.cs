using OCanada.UI;
using Zenject;

namespace OCanada.Installers
{
    class OCanadaMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<OCanadaMenuController>().AsSingle();
            Container.BindInterfacesAndSelfTo<OCanadaDetailsController>().AsSingle();
            Container.BindInterfacesAndSelfTo<OCanadaGameController>().AsSingle();
        }
    }
}
