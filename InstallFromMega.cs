using Playnite.SDK;
using Playnite.SDK.Plugins;
using System.Collections.Generic;
using System;

namespace InstallFromMegaPlugin{
    public class InstallFromMega : GenericPlugin{
        public override Guid Id { get; } = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public InstallFromMega(IPlayniteAPI api) : base(api){}

        public override IEnumerable<InstallController> GetInstallActions(GetInstallActionsArgs args){
            yield return new MegaInstallController(args.Game, PlayniteApi);
        }
    }
}
