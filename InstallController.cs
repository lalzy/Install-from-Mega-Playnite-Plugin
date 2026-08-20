using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.SDK.Models;

namespace InstallFromMegaPlugin{
    public class MegaInstallController : InstallController{
        private IPlayniteAPI api;
        private MegaDownload mega;
        public MegaInstallController(Game game, IPlayniteAPI api) : base(game){
            this.api = api;
            this.mega = new MegaDownload(api);
            Name = "install from Mega";
        }
        public override void Install(InstallActionArgs args){
            
        }
    }
}
