using System.Linq;
using System.IO;
using System;

namespace InstallFromMegaPlugin{
    public static class Config{
        private const string CONFIGPATH = "config.ini";
        private const string LASTSYNC = "lastSync";
        private const string MEGATOOLS = "megaToolspath";

        ///<summary>Read an config entry</summary>
        public static string Read(string field){
            var config = File.ReadAllLines(CONFIGPATH).Select(l => l.Split('=')).ToDictionary(a => a[0], a => a[1]);
            return config[field];
        }

        ///<summary>Write new value to the config file</summary>
        public static void Write(string field, string value){
            var lines = File.ReadAllLines(CONFIGPATH).ToList();
            var index = lines.FindIndex(l => l.StartsWith(field + '='));
            if(index >= 0)
                lines[index] = $"{field}={value}";
            else
                lines.Add($"{field}={value}");
            File.WriteAllLines(CONFIGPATH, lines);
        }

        ///<summary>Helper to get the megatoolspath</summary>
        public static string GetMegaToolsPath(){
            return Read(MEGATOOLS);
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
