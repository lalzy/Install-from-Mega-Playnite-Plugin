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

    ///<summary>Start of input designator</summary>
    private bool EndOfFileRead(string line){
        return line.Contains("Enter numbers of files or folders to download separated by spaces");
    }

    ///<summary>Runs process with CMD as middleman</summary>
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

    ///<summary>Filter the console output so we only get download information</summary>
    private void FilterOutput(Process process){
        string line;
        while((line = process.StandardOutput.ReadLine()) != null)
            if(EndOfFileRead(line)) break;
        while((line = process.StandardOutput.ReadLine()) != null){
            if(line.Contains("File already exists at"))
                Console.WriteLine("file exist, skipping");
            else
                Console.WriteLine(line);
        }
    }
    

    ///<summary>Downloads from Mega using a process</summary>
    private void DownloadFiles(){
        // 2>&1 merges error and out streams into one
        WithCMDProcess( $"/C {_megaToolPath} dl --path {_localPath} --choose-files {_megaLibraryURL}  2>&1", (process)=>{
            process.StandardInput.WriteLine("1");
            process.StandardInput.Flush();
            process.StandardInput.Close();
            FilterOutput(process);
        });
    }

    ///<summary>Runs the sync logic. Delete's the .db files, and re-download them, and any missing files/ files/folders</summary>
    public void RunSync(){
        string root = _localPath + "/library/";

        // Delete the databases as MegaTools can't overwrite existing files
        Console.WriteLine("Deleting Database files");
        foreach (string file in Directory.GetFiles(root, "*.db").Concat(Directory.GetFiles(root, "database.json"))){
            File.Delete(file);
        }

        DownloadFiles();

    }
}
