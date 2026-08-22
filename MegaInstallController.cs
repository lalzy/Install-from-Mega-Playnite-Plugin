using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.SDK.Models;
using System.Linq;
using System.IO;

namespace InstallFromMegaPlugin{
    public class MegaInstallController : InstallController{
        private IPlayniteAPI _api;
        private MegaDownload _mega;
        private Game _game;
        private GameStatsManager _statsManager;
        public MegaInstallController(Game game, IPlayniteAPI api, GameStatsManager statsManager) : base(game){
            _api = api;
            _statsManager = statsManager;
            _mega = new MegaDownload(api);
            _game = game;
            Name = "install from Mega";
        }
        public override void Install(InstallActionArgs args){
            var resolvedPath = _api.ExpandGameVariables(_game, _game.InstallDirectory);
            Directory.CreateDirectory(resolvedPath);
            _mega.Download(_api, _game.Links.FirstOrDefault(l => l.Name == "MEGA").Url, resolvedPath, _game.Name);
            _game.IsInstalled = true;
            _api.Database.Games.Update(_game);
            InvokeOnInstalled(new GameInstalledEventArgs(new GameInstallationData{
                InstallDirectory = resolvedPath
            }));
        }
    }
}
