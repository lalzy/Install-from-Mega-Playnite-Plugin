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

namespace InstallFromMegaPlugin{
    public class InstallFromMega : GenericPlugin{
        // set to false if your playnite games does not have dependencies
        // Such as needing DirectX8, or 9 to be installed. INMM Audio Driver, etc.
        private const bool HASDEPENDENCIES = true;
        private const string DEPENDENCYMESSAGE = "Remember to install dependencies under [dependencies] platform!";

    
        public override Guid Id { get; } = Guid.Parse("320f8637-3660-4e98-87d0-fd12934b145a");
        private GameStatsManager _gameStatsManger;
        private IPlayniteAPI _api;
        public MegaDownload _megaDownload;

        public InstallFromMega(IPlayniteAPI api) : base(api){
            _api = api;
            Config.Init(GetPluginUserDataPath());
            Config.CreateBlankConfigFile(api);
            _megaDownload = new MegaDownload(api);
            _gameStatsManger = new GameStatsManager(this, api);
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
            string arguments = $"--megalibraryurl {GetMegaLibraryPath()} --localplaynitepath {PlayniteApi.Paths.ApplicationPath} --megatoolspath {Config.Read(Config.MEGATOOLS)} --configfilepath {configFilePath} --syncfieldname {Config.LASTSYNC}";
            var process = new Process();
            process.StartInfo.FileName = Path.Combine(playnitePath, "syncProgram.exe");
            process.StartInfo.Arguments = arguments;
            process.Start();

            // need to shutdown as playnite holds access-rights to database files
            ShutdownPlaynite();
        }
        
        ///<summary>Syncs if we need to sync</summary>
        private void HandleSync(string pluginPath, string configFilePath){
            ErrorHandler.WithTryCatch(()=>{
                if(NeedSyncP()){
                    var result = _api.Dialogs.ShowMessage("Sync available, want to sync(Playnite will be shutdown while it syncs the library)?", "Title", MessageBoxButton.YesNo);
                    if(result == MessageBoxResult.Yes){
                        RunSyncProgram(pluginPath, configFilePath);
                        
                    }
                }
            }, _api, "Error in Syncing");
        }
        
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args){
            if(_gameStatsManger.IsEmpty()){
                _api.Dialogs.ShowMessage($"First time game-setup. Setting up for {_api.Database.Games.Count} games");
                _gameStatsManger.SyncGamesToGameStats();
                var message = "Done!";
                message += HASDEPENDENCIES ? $"\n{DEPENDENCYMESSAGE}" : "";
                _api.Dialogs.ShowMessage(message);
            }
            HandleSync(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Config.GetFullPath());
        }
        
        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args){
            yield return new MegaInstallController(args.Game, PlayniteApi, _gameStatsManger);
        }

        public override IEnumerable<MainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args){
            yield return new MainMenuItem{
                Description = "Sync Library from Mega",
                MenuSection = "@",
                Action = (a) => RunSyncProgram(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), Config.GetFullPath())
            };
        }
    }
}
