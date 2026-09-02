using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

class Program{
    private const string MEGALIBURL = "megalibraryurl";
    private const string PLAYNITEPATH = "localplaynitepath";
    private const string TOOLPATH = "megatoolspath";
    private const string CONFIGFILEPATH = "configfilepath";
    private const string LASTSYNCFIELD = "syncfieldname";

    ///<summary>parse the arguments to get the values</summary>
    private static Dictionary<string, string> ParseArgs(string[] args){
        var ret = new Dictionary<string, string>();
        
        for(int i = 0; i < args.Length; i++){
            switch(args[i].ToLower()){
                case "--megalibraryurl":
                    ret.Add(MEGALIBURL, args[++i]);
                    break;
                case "--localplaynitepath":
                    ret.Add(PLAYNITEPATH, args[++i]);
                    break;
                case "--megatoolspath":
                    ret.Add(TOOLPATH, args[++i]);
                    break;
                case "--configfilepath":
                    ret.Add(CONFIGFILEPATH, args[++i]);
                    break;
                case "--syncfieldname":
                    ret.Add(LASTSYNCFIELD, args[++i]);
                    break;
            }
        }
        return ret;
    }

    ///<summary>Verify that we have injected the 3 required parameters</summary>
    private static bool VerifyPaths(Dictionary<string, string> paths){
        string message = "Is required parameter!";
        bool allValid = true;
        if(!paths.ContainsKey(PLAYNITEPATH)){
            Console.WriteLine($"--{PLAYNITEPATH} - {message}");
            allValid = false;
        }
        if(!paths.ContainsKey(TOOLPATH)){
            Console.WriteLine($"--{TOOLPATH} - {message}");
            allValid = false;
        }
        if(!paths.ContainsKey(MEGALIBURL)){
            Console.WriteLine($"--{MEGALIBURL} - {message}");
            allValid = false;
        }
        if(!paths.ContainsKey(CONFIGFILEPATH)){
            Console.WriteLine($"--{CONFIGFILEPATH} - {message}");
            allValid = false;
        }
        return allValid;
    } 

    ///<summary>Update the lastSync field in the config file of the playnite app to now</summary>
    private static void UpdateSyncStampInPluginConfig(string configFile, string fieldName){
        var lines = File.ReadAllLines(configFile).ToList();
        var index = lines.FindIndex(l => l.StartsWith(fieldName));
        string value = $"{fieldName}={DateTime.Now}";
        if(index >= 0)
            lines[index] = value;
        else
            lines.Add(value);
        File.WriteAllLines(configFile, lines);
    }

    ///<summary>Starts Playnite</summary>
    static private void StartPlaynite(string playnitePath){
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = playnitePath;
        process.Start();
    }
    
    static void Main(string[] args){
        try{
            var paths = ParseArgs(args);
            if (!VerifyPaths(paths)) throw new Exception("Required parameters not met!");

            
            Console.WriteLine("Delay to let playnite shutdown");
            
            for (int seconds = 3; seconds > 0; seconds--){
                string text = $"waiting for {seconds} ";
                text += seconds == 1 ? "second" : "seconds";
                Console.WriteLine(text);
                System.Threading.Thread.Sleep(1000);
            }

                Console.WriteLine("=== running sync; please wait =====");
            new Sync(paths[TOOLPATH], paths[MEGALIBURL], paths[PLAYNITEPATH]).RunSync();
            Console.WriteLine("======== finished fetching ========");
            UpdateSyncStampInPluginConfig(paths[CONFIGFILEPATH], paths[LASTSYNCFIELD] != null ?  paths[LASTSYNCFIELD] : "lastSync");

            Console.WriteLine("Starting up Playnite");
            StartPlaynite(Path.Combine(paths[PLAYNITEPATH], "Playnite.DesktopApp.exe"));
        }catch (Exception e){
            Console.WriteLine(e);
        }
    }
}
