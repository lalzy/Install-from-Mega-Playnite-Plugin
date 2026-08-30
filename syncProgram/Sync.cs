using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

public static class Sync{

    ///<summary>Read the lines from process output into a string</summary>
    private static string ReadOutput(Process  process){
        var output = new System.Text.StringBuilder();
        
        string line;
        while((line = process.StandardOutput.ReadLine()) != null){
            // The first non-file line of the interactive prompt is where we stop reading
            if(line.Contains("Enter numbers of files or folders to download separated by spaces"))
                break;
            output.AppendLine(line);
        }

        return output.ToString();
    }

    ///<summary>Wrapper over process that will force close the process after func runs</summary>
    private static void WithProcess(Action<Process> func, string megaToolsPath, string megaLibraryURL){
        var process = new Process();
        process.StartInfo.FileName = megaToolsPath;
        process.StartInfo.Arguments = $"dl --choose-files {megaLibraryURL}";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardInput = true;
        process.Start();
        
        func(process);;

        process.Kill();
        process.WaitForExit();
    }

    ///<summary>Read all files from a MegaURL, meant to get playnite's library/ to check what needs to be downloaded</summary>
    ///<param name="megaToolsPath">Local path to the MegaTools binary</param>
    ///<param name="megaLibraryURL">The Mega decrypted URL that holds the playnite library</param>
    ///<returns>string of files and folders</returns>
    public static string GetMegaFiles(string megaToolsPath, string megaLibraryURL){
        Console.WriteLine("Fetching library data from mega");
        Console.WriteLine("======== Please wait ========");

        string output = "";
        WithProcess((process) =>
        {
            output = ReadOutput(process);
        }, megaToolsPath, megaLibraryURL);

        Console.WriteLine("======== finished fetching ========");
        return output;
    }
}
