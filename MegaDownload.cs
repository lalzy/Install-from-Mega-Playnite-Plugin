using System.IO;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Plugins;

namespace InstallFromMegaPlugin{

    public class MegaDownload {
        private string toolPath;
        public MegaDownload(IPlayniteAPI api){
            var config = File.ReadAllLines("config.ini").Select(l => l.Split('=')).ToDictionary(a => a[0], a => a[1]);
            toolPath =  api.Paths.ApplicationPath + config["megaToolspath"];
        }
    }
}
