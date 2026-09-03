using System.Linq;
using System.IO;
using System;
using Playnite.SDK;

namespace InstallFromMegaPlugin{
    public static class Config{
        public static string _configPath = "config.ini";
        public const string LASTSYNC = "lastSync"; // When the extension was last synced for the mega.nz library. If out of sync, a reminder will be thrown.
        public const string MEGATOOLS = "megaToolspath"; // Where the local MegaTools is setup
        public const string MEGALASTUPDATEURL = "lastupdatedurl"; // Link to where the last-sync file is. Need to contain a dateTime timestamp
        public const string LIBRARYPATH = "libraryurl"; // Link to where the playnite/library is on mega.nz
        public const string DOWNLOADPATH = "downloadPath"; // Temporary location to download files to
        public const string NEEDMIGRATE = "needmigration";
        public const string SHAREDPLATFORMS = "sharedgameplatforms"; // platform where all games are 'shared' paths. Such as virtualmachines holding the games

        ///<summary>Initialize the static config path to be from pluginDataPath</summary>
        ///<param name="pluginDataPath">The Playnite Plugin Data Path</param>
        public static void Init(string pluginDataPath){
            _configPath = Path.Combine(pluginDataPath, "config.ini");
        }

        public static string GetFullPath(){
            return _configPath;
        }
        
        ///<summary>Create a blank config file if it doesn't exist.</summary>
        public static void CreateBlankConfigFile(IPlayniteAPI api){
            if(!File.Exists(_configPath)){
                var configContent = new System.Text.StringBuilder();
                string[] keys = { MEGATOOLS, LASTSYNC, DOWNLOADPATH, MEGALASTUPDATEURL, MEGALASTUPDATEURL, LIBRARYPATH, SHAREDPLATFORMS, NEEDMIGRATE};

                foreach(var key in keys){
                    string value = "";
                    if (key == LASTSYNC){
                        value = DateTime.Now.ToString();
                    }else if (key == SHAREDPLATFORMS)
                        value = "";
                    else if (key == NEEDMIGRATE)
                        value = "false";
                    else{
                        value = api.Dialogs.SelectString($"Enter: {key} value", "Input", "").SelectedString;
                    }
                    configContent.Append(key).Append('=').Append(value).Append("\n");
                }
                File.WriteAllText(_configPath, configContent.ToString());
            };
        }
        
        ///<summary>Read an config entry</summary>
        public static string Read(string field){
            var config = File.ReadAllLines(_configPath).Select(l => l.Split('=')).ToDictionary(a => a[0], a => a[1]);
            return config[field];
        }

        ///<summary>Write new value to the config file</summary>
        public static void Write(string field, string value){
            var lines = File.ReadAllLines(_configPath).ToList();
            var index = lines.FindIndex(l => l.StartsWith(field + '='));
            if(index >= 0)
                lines[index] = $"{field}={value}";
            else
                lines.Add($"{field}={value}");
            File.WriteAllLines(_configPath, lines);
        }

        ///<summary>Helper to get last synced time</summary>
        public static DateTime GetLastSynced(){
            return new DateTime(long.Parse(Read(LASTSYNC)));
        }

        ///<summary>Helper to update last synced time</summary>
        public static void UpdateLastSync(){
            Write(LASTSYNC, DateTime.Now.ToString());
        }
    }

}
