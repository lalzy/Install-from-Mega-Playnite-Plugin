using Playnite.SDK;
using Playnite.SDK.Events;
using Playnite.SDK.Plugins;
using System.Collections.Generic;
using System;

namespace InstallFromMegaPlugin{
    public class InstallFromMega : GenericPlugin{
        public override Guid Id { get; } = Guid.Parse("320f8637-3660-4e98-87d0-fd12934b145a");
        private GameStatsManager _gameStatsManger;

        public InstallFromMega(IPlayniteAPI api) : base(api){
            Config.Init(GetPluginUserDataPath());
            Config.CreateBlankConfigFile();
            _gameStatsManger = new GameStatsManager(this, api);
        }

        public override void OnApplicationStarted(OnApplicationStartedEventArgs args){
            if(_gameStatsManger.IsEmpty())
                _gameStatsManger.SyncGamesToGameStats();
        }
        
        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args){
            
            yield return new MegaInstallController(args.Game, PlayniteApi);
        }
    }
}
