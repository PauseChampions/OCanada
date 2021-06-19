using OCanada.UI.ViewControllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zenject;

namespace OCanada.Installers
{
    class OCanadaMenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<OCanadaGameController>().AsSingle();
        }
    }
}
