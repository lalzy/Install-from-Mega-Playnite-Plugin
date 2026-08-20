using System;

namespace InstallFromMegaPlugin {
    public class GameStats{
        public Guid GameID { get; set; }
        public bool IsInstalled { get; set; }
        public ulong Playtime { get; set; }
    }
}
