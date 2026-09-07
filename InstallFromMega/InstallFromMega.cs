using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using Playnite.SDK.Models;
using System.Linq;
using System.Threading;


namespace InstallFromMegaPlugin{
    public class InstallFromMega : GenericPlugin{
        // set to false if your playnite games does not have dependencies
        // Such as needing DirectX8, or 9 to be installed. INMM Audio Driver, etc.
        private const bool HASDEPENDENCIES = true;
        private const string DEPENDENCYMESSAGE = "Remember to install dependencies under [dependencies] platform!";

        public override Guid Id { get; } = Guid.Parse("320f8637-3660-4e98-87d0-fd12934b145a");
        private GameStatsManager _gameStatsManager;
        private IPlayniteAPI _api;
        public MegaDownload _megaDownload;
        private Timer _timer;
        private bool _askedForSync = false;

        public InstallFromMega(IPlayniteAPI api) : base(api){
            _api = api;
            Config.Init(GetPluginUserDataPath());
            Config.CreateBlankConfigFile(api);
            _megaDownload = new MegaDownload(api);
            _gameStatsManager = new GameStatsManager(this, api);
        }

        ///<summary>Check if we need to sync</summary>
        private bool NeedSyncP(){
            var client = new WebClient();
            // read from the sync-file that owns SoT of library update time
            DateTime externalSyncDate = DateTime.Parse(client.DownloadString(Config.Read(Config.MEGALASTUPDATEURL)));
            if(externalSyncDate > DateTime.Parse(Config.Read(Config.LASTSYNC)))
                return true;
            else
                return false;
        }

        ///<summary>Helper to fetch the library URL from server holding the link</summary>
        private string GetMegaLibraryPath(){
            var client = new WebClient();
            return client.DownloadString(Config.Read(Config.LIBRARYPATH));
        }

        ///<summary>Helper to shut down playnite</summary>
        private void ShutdownPlaynite(){
            var shutdown = new Process();
            shutdown.StartInfo.FileName = Path.Combine(PlayniteApi.Paths.ApplicationPath, "Playnite.DesktopApp.exe");
            shutdown.StartInfo.Arguments = "--shutdown";
            shutdown.Start();
        }
        
        private void RunSyncProgram(string playnitePath, string configFilePath){
            string arguments = $"--megalibraryurl {GetMegaLibraryPath()} --localplaynitepath {PlayniteApi.Paths.ApplicationPath} --megatoolspath {Config.Read(Config.MEGATOOLS)} --configfilepath {configFilePath} --syncfieldname {Config.LASTSYNC} --needmigratename {Config.NEEDMIGRATE}";
            var process = new Process();
            process.StartInfo.FileName = Path.Combine(playnitePath, "syncProgram.exe");
            process.StartInfo.Arguments = arguments;
            process.Start();

            // need to shutdown as playnite holds access-rights to database files
            ShutdownPlaynite();
        }

        ///<summary>Wrapper that handle the game.db->plugin.db portion of our migration to handle playtime etc</summary>
        private void RunSyncSteps(string playnitePath, string configFilePath){
            foreach(var game in _api.Database.Games){
                var stats = _gameStatsManager.Read(game.Id);
                _gameStatsManager.UpdateGameObject(stats, game);
            }
            RunSyncProgram(playnitePath, configFilePath);
        }
        
        ///<summary>Syncs if we need to sync</summary>
        private void HandleSync(string pluginPath, string configFilePath){
            ErrorHandler.WithTryCatch(()=>{
                var syncType = Config.GetSyncType();
                if(NeedSyncP() && syncType == Config.SyncType.OnStart || syncType == Config.SyncType.Periodic){
                    var result = _api.Dialogs.ShowMessage("Sync available, want to sync(Playnite will be shutdown while it syncs the library)?", "Title", MessageBoxButton.YesNo);
                    if(result == MessageBoxResult.Yes){
                        RunSyncSteps(pluginPath, configFilePath);
                        _askedForSync = false;
                    }else{
                        _askedForSync = true;
                    }
                }
            }, _api, "Error in Syncing");
        }
        
        private void SyncChecking(Object state){
            Thread thread = new Thread(() => {
                if(!_askedForSync && NeedSyncP()){
                    _askedForSync = true;
                    HandleSync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Config.GetFullPath());
                }
            });

            thread.IsBackground = true;
            thread.Start();
        }

        public override void OnApplicationStopped(OnApplicationStoppedEventArgs args){
            _timer?.Dispose();
        }
        
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args){
            
            if(_gameStatsManager.IsEmpty()){
                _api.Dialogs.ShowMessage($"First time game-setup. Setting up for {_api.Database.Games.Count} games");
                _gameStatsManager.SyncGamesToGameStats();
                var message = "Done!";
                message += HASDEPENDENCIES ? $"\n{DEPENDENCYMESSAGE}" : "";
                _api.Dialogs.ShowMessage(message);
            }else{
                HandleSync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Config.GetFullPath());

                bool.TryParse(Config.Read(Config.NEEDMIGRATE), out bool needMigrate);
                if(needMigrate){
                    _gameStatsManager.SyncGamesToGameStats();
                    Config.Write(Config.NEEDMIGRATE, "false");
                }
            }

            if(Config.GetSyncType() == Config.SyncType.Periodic){
                int TimeToCheck = (int)TimeSpan.FromHours(168).TotalMilliseconds;
                _timer = new Timer(SyncChecking, null, TimeToCheck, TimeToCheck); // First check at Time to check, instead of immediate
            }
            
            // To track if a game gets uninstalled
            PlayniteApi.Database.Games.ItemUpdated += Games_ItemUpdated;
    
        }

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args){
            yield return new MegaInstallController(args.Game, PlayniteApi, _gameStatsManager);
        }

        // To set the GameStats to uninstalled if we unflag installed
        private void Games_ItemUpdated(object sender, ItemUpdatedEventArgs<Game> args)
        {
            foreach (var change in args.UpdatedItems)
            {
                if (change.OldData.IsInstalled && !change.NewData.IsInstalled)
                {
                    var stats = _gameStatsManager.Read(change.NewData.Id);
                    stats.IsInstalled = false;
                    _gameStatsManager.Write(stats);
                    MegaInstallController.SetSharedUninstall(_api, change.NewData, _gameStatsManager);
                }
            }
        }

        
        ///<summary>Chekc if the game has update text markign it for needing to be updated</sumamry>
        ///<param name=game>Playnite Game object</param>
        ///<returns>True if game has updated text</returns>
        private static bool NeedsUpdate(Game game){
            return game.Name.StartsWith(GameStatsManager.UPDATETEXT);
        }
        

        private void RemoveUpdateString(Game game){
            string name = game.Name;
            game.Name = name.Substring((name.IndexOf(GameStatsManager.UPDATETEXT) + 1) + (GameStatsManager.UPDATETEXT.Length -1));
            _api.Database.Games.Update(game);
        }
        
        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args){
            yield return new MainMenuItem{
                Description = "Sync Library from Mega",
                MenuSection = "@",
                Action = (a) => RunSyncSteps(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Config.GetFullPath())
            };
        }

        ///<summary>Create an Update context-menu entry if game needs an update</summary>
        private GameMenuItem AddUpdateEntry(Game game, GameStats stats){
            if(NeedsUpdate(game)){
                var megaDownload = new MegaDownload(_api);

                return new GameMenuItem{
                    Description = "Update/Redownload",
                    Action = (actionArgs) =>
                    {
                        megaDownload.Download(_api, MegaInstallController.GetMegaLink(game), game.InstallDirectory, game.Name);
                        
                        // Update local tracked version
                        stats.Version = game.Version;
                        _gameStatsManager.Write(stats);
                        
                        RemoveUpdateString(game);
                    }
                };
            }
            return null;
        }

        ///<summary>Create an uninstall entry if the game is installed</summary>
        private GameMenuItem AddUninstallEntry(Game game){
            if(game.IsInstalled){
                return new GameMenuItem{
                    Description = "uninstall",
                    Action = (actionArgs) => {
                        var result = _api.Dialogs.ShowMessage($"Do you want to uninstall {game.Name}?", $"Uninstalling {game.Name}", MessageBoxButton.YesNo);

                        if(result == MessageBoxResult.Yes){
                            if(Directory.Exists(game.InstallDirectory))
                                Directory.Delete(game.InstallDirectory, recursive: true);

                            game.IsInstalled = false;
                            _api.Database.Games.Update(game);
                        }
                    }
                };
            }
            return null;
        }
        
        public override IEnumerable<GameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args){
            Game game = args.Games[0];
            GameStats stats = _gameStatsManager.Read(game.Id);
            var updateEntry = AddUpdateEntry(game, stats);
            var uninstallEntry = AddUninstallEntry(game);
            if(updateEntry != null) yield return updateEntry;
            if(uninstallEntry != null) yield return AddUninstallEntry(game);
        }
    }
}
