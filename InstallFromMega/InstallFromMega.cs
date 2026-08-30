using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System.Collections.Generic;
using System;
using System.Net;
using System.IO;
using System.Windows;

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
            DateTime externalSyncDate = DateTime.Parse(client.DownloadString(Config.Read(Config.MEGALASTUPDATEURL)));
            if(externalSyncDate > DateTime.Parse(Config.Read(Config.LASTSYNC)))
                return true;
            else
                return false;
        }

        ///<summary>Syncs if we need to sync</summary>
        private void HandleSync(){
            ErrorHandler.WithTryCatch(()=>{
                if(NeedSyncP()){
                    var result = _api.Dialogs.ShowMessage("Sync available, want to sync(require restart)?", "Title", MessageBoxButton.YesNo);
                    if(result == MessageBoxResult.Yes){
                        string libraryPath = Path.Combine(_api.Paths.ApplicationPath, "library");
                        var client = new WebClient();
                        var megaLib = client.DownloadString(Config.Read(Config.LIBRARYPATH));
                        _megaDownload.DownloadFolder(_api, megaLib, libraryPath // Config.Read(Config.DOWNLOADPATH)
                        );

                        Config.Write(Config.LASTSYNC, DateTime.Now.ToString());
                        _api.Dialogs.ShowMessage($"Downloaded Synced files: {Config.DOWNLOADPATH} to your {libraryPath} folder");
                    }
                }    
            }, _api, "Error in Syncing");
        }
        
        public override void OnApplicationStarted(OnApplicationStartedEventArgs args){
            if(_gameStatsManger.IsEmpty()){
                _api.Dialogs.ShowMessage($"Syncing: {_api.Database.Games.Count} games");
                _gameStatsManger.SyncGamesToGameStats();
                var message = "Done syncing!";
                message += HASDEPENDENCIES ? $"\n{DEPENDENCYMESSAGE}" : "";
                _api.Dialogs.ShowMessage(message);
            }
            HandleSync();
        }
        
        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args){
            yield return new MegaInstallController(args.Game, PlayniteApi, _gameStatsManger);
        }
    }
}
