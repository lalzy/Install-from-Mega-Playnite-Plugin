using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

class Program{
    private const string MEGALIBURL = "megalibraryurl";
    private const string PLAYNITEPATH = "localplaynitepath";
    private const string TOOLPATH = "megatoolspath";
    
    
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
            }
        }
        return ret;
    }

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
        return allValid;
    } 

    static void Main(string[] args){
        try{
            var paths = ParseArgs(args);
            if (!VerifyPaths(paths)) throw new Exception("Required parameters not met!");

            Console.WriteLine("=== running sync; please wait =====");
            new Sync(paths[TOOLPATH], paths[MEGALIBURL], paths[PLAYNITEPATH]).RunSync();
            Console.WriteLine("======== finished fetching ========");
        }catch (Exception e){
            Console.WriteLine(e);
        }
    }
}
