using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.SDK.Models;
using System.Linq;
using System.IO;
using System;

namespace InstallFromMegaPlugin{
    public class MegaInstallController : InstallController{
        private IPlayniteAPI _api;
        private MegaDownload _mega;
        private Game _game;
        private GameStatsManager _statsManager;
        private GameStats _stats;

        
        public MegaInstallController(Game game, IPlayniteAPI api, GameStatsManager statsManager) : base(game){
            _statsManager = statsManager;
            _stats = _statsManager.Read(game.Id);
            _mega = new MegaDownload(api);
            _api = api;
            _game = game;
            Name = "install from Mega";
        }

        ///<summary>Update the GameStats object and DB entry</summary>
        private void UpdateStats(string installPath){
            _stats.IsInstalled = true;
            _stats.Version = _game.Version;
            _stats.InstallDirectory = installPath;
            _statsManager.Write(_stats);
        }
        
        // Hook override
        public override void Install(InstallActionArgs args){
            string installPath = _api.Dialogs.SelectFolder(_api.ExpandGameVariables(_game, _game.InstallDirectory));
            Directory.CreateDirectory(installPath);
            _mega.Download(_api, _game.Links.FirstOrDefault(l => l.Name == "MEGA").Url, installPath, _game.Name);
            UpdateStats(installPath);
            _api.Database.Games.Update(_game);
            InvokeOnInstalled(new GameInstalledEventArgs(new GameInstallationData{
                InstallDirectory = installPath
            }));
        }
    }
}
