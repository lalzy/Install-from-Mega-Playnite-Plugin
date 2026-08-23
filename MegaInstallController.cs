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
            _api = api;
            _statsManager = statsManager;
            _stats = _statsManager.Read(Guid.Parse(game.GameId));
            _mega = new MegaDownload(api);
            _game = game;
            Name = "install from Mega";
        }

        ///<summary>Update the GameStats object and DB entry</summary>
        private void UpdateStats(){
            _stats.IsInstalled = true;
            _stats.Version = _game.Version;
            _statsManager.Write(_stats);
        }

        ///<summary>Updatrs the PlayniteGame DBEntry</summary>
        private void UpdateGame(){
            _game.IsInstalled = true;
            _api.Database.Games.Update(_game);
        }

        ///<summary>Wrapper for downloading game, and creating the install directory.</summary>
        private void WithDownload(Action action){
            var resolvedPath = _api.ExpandGameVariables(_game, _game.InstallDirectory);
            Directory.CreateDirectory(resolvedPath);
            _mega.Download(_api, _game.Links.FirstOrDefault(l => l.Name == "MEGA").Url, resolvedPath, _game.Name);
            action();
            InvokeOnInstalled(new GameInstalledEventArgs(new GameInstallationData{
                InstallDirectory = resolvedPath
            }));
        }
        
        // Hook override
        public override void Install(InstallActionArgs args){
            WithDownload(() => {
                UpdateGame();
                UpdateStats();
            });
        }
    }
}
