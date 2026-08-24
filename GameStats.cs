using System;

namespace InstallFromMegaPlugin {
    public class GameStats{
        public Guid GameID { get; set; }
        public bool IsInstalled { get; set; }
        public ulong Playtime { get; set; }
        public string InstallDirectory {get; set;}
        
        // Version is what version the game was installed with, if it missmatch the game on later sync
        // We warn the user about it.
        public string Version {get; set;}=null;
    }
}
