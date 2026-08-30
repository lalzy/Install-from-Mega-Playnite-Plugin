using System;
using System.Collections.Generic;
using System.Linq;

class Program{
    private const string LIBPATH = "megalibraryurl";
    private const string MEGALIBURL = "localplaynitepath";
    private const string TOOLPATH = "megatoolspath";
    
    
    private static Dictionary<string, string> ParseArgs(string[] args){
        var ret = new Dictionary<string, string>();
        
        for(int i = 0; i < args.Length; i++){
            switch(args[i].ToLower()){
                case "--megalibraryurl":
                    ret.Add(MEGALIBURL, args[++i]);
                    break;
                case "--localplaynitepath":
                    ret.Add(LIBPATH, args[++i]);
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
        if(!paths.ContainsKey(LIBPATH)){
            Console.WriteLine($"--{LIBPATH} - {message}");
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
            var lines = Sync.GetMegaFiles(paths[TOOLPATH], paths[MEGALIBURL]);
            // var files = parseFromMega.ParseLines(lines);
            var files = parseFromMega.ParseLines(lines.Take(50).ToArray());

            // Temporary debugging
            foreach(var file in files){
                Console.WriteLine($"filename: {file.name} | path: {file.path} | is dir: {file.IsDirectory}");
            }
        }catch (Exception e){
            Console.WriteLine(e);
        }
    }
}
