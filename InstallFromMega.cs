using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System.Collections.Generic;
using System;

namespace InstallFromMegaPlugin{
    public class InstallFromMega : GenericPlugin{
        // set to false if your playnite games does not have dependencies
        // Such as needing DirectX8, or 9 to be installed. INMM Audio Driver, etc.
        private const bool HASDEPENDENCIES = true;
        private const string DEPENDENCYMESSAGE = "Remember to install dependencies under [dependencies] platform!";

    
        public override Guid Id { get; } = Guid.Parse("320f8637-3660-4e98-87d0-fd12934b145a");
        private GameStatsManager _gameStatsManger;
        private IPlayniteAPI _api;

        public InstallFromMega(IPlayniteAPI api) : base(api){
            _api = api;
            Config.Init(GetPluginUserDataPath());
            Config.CreateBlankConfigFile(api);
            _gameStatsManger = new GameStatsManager(this, api);
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args){
            if(_gameStatsManger.IsEmpty()){
                _api.Dialogs.ShowMessage($"Syncing: {_api.Database.Games.Count} games");
                _gameStatsManger.SyncGamesToGameStats();
                var message = "Done syncing!";
                message += HASDEPENDENCIES ? $"\n{DEPENDENCYMESSAGE}" : "";
                _api.Dialogs.ShowMessage(message);
            }
        }
        
        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args){
            yield return new MegaInstallController(args.Game, PlayniteApi, _gameStatsManger);
        }
    }
}
