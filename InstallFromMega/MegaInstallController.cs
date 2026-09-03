using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.SDK.Models;
using System.Linq;
using System.IO;
using System.Collections.Generic;
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
        private void UpdateStats(string installPath, GameStats stats){
            stats.IsInstalled = true;
            stats.Version = _game.Version;
            stats.InstallDirectory = installPath;
            _statsManager.Write(stats);
        }

        private static Platform CheckSharedPlatform(Game game){
            if(game.Platforms == null) return null;
            var platforms = new HashSet<string>(Config.Read(Config.SHAREDPLATFORMS).ToString().Split(','));
             return game.Platforms.FirstOrDefault(p => platforms.Contains(p.Name));
        }

        private static List<Game> GetGamesFromPlatform(IPlayniteAPI api, Platform platform){
            return api.Database.Games.Where(g => g.Platforms != null && g.Platforms.Any(p => p.Name == platform.Name)).ToList();
        }
        
        public static void SetSharedUninstall(IPlayniteAPI api, Game currentGame, GameStatsManager statsManager){
            var platform = CheckSharedPlatform(currentGame);
            if(platform == null) return;

            var games = GetGamesFromPlatform(api, platform);
            foreach(var game in games){
                if (game != currentGame){
                    var stats = statsManager.Read(game.Id);
                    stats.IsInstalled = false;
                    game.IsInstalled = false;
                    statsManager.Write(stats);
                    api.Database.Games.Update(game);
                }
            }
        }

        private void SetSharedInstall(Game currentGame, string installPath){
            var platform = CheckSharedPlatform(currentGame);
            if(platform == null) return;

            var games = GetGamesFromPlatform(_api, platform);
            foreach(var game in games){
                if(game != currentGame){
                    UpdateStats(installPath, _statsManager.Read(game.Id));
                    game.IsInstalled = true;
                    _api.Database.Games.Update(game);
                }
            }
        }

        // Hook overrides
        public override void Install(InstallActionArgs args){
            string installPath = _api.Dialogs.SelectFolder(_api.ExpandGameVariables(_game, _game.InstallDirectory));
            Directory.CreateDirectory(installPath);
            _mega.Download(_api, _game.Links.FirstOrDefault(l => l.Name == "MEGA").Url, installPath, _game.Name);
            UpdateStats(installPath, _stats);
            _api.Database.Games.Update(_game);
            SetSharedInstall(_game, installPath);
            InvokeOnInstalled(new GameInstalledEventArgs(new GameInstallationData{
                InstallDirectory = installPath
            }));
        }
    }
}
