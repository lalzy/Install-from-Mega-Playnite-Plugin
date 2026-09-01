using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;

public class Sync{
    private string _megaToolPath;
    private string _megaLibraryURL;
    private string _localPath;

    public Sync(string megaToolPath, string megaLibraryURL, string localPath){
        _megaToolPath = megaToolPath;
        _megaLibraryURL = megaLibraryURL;
        _localPath = localPath;
    }
    
    private bool EndOfFileRead(string line){
        return line.Contains("Enter numbers of files or folders to download separated by spaces");
    }
    
    private void WithCMDProcess(string argument, Action<Process> func){
        var process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = argument;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardInput = true;

        process.Start();
        func(process);
    }

    private void FilterOutput(Process process){
        string line;
        while((line = process.StandardOutput.ReadLine()) != null)
            if(EndOfFileRead(line)) break;
        while((line = process.StandardOutput.ReadLine()) != null)
            Console.WriteLine(line);
    }
    

    private void DownloadFiles(){
        WithCMDProcess( $"/C {_megaToolPath} dl --path {_localPath} --choose-files {_megaLibraryURL}", (process)=>{
            process.StandardInput.WriteLine("1");
            process.StandardInput.Flush();
            process.StandardInput.Close();
            FilterOutput(process);
        });
    }

    public void RunSync(){
        string root = _localPath + "/library/";

        // Delete the databases as MegaTools can't overwrite existing files
        foreach (string file in Directory.GetFiles(root, "*.db").Concat(Directory.GetFiles(root, "database.json"))){
            File.Delete(file);
        }

        DownloadFiles();

    }
}
