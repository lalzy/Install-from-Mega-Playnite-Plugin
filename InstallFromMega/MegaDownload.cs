using System.IO;
using System.Linq;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using System;

namespace InstallFromMegaPlugin{
    public class MegaDownload {
        private string _toolPath;
        public MegaDownload(IPlayniteAPI api){
            _toolPath = Config.Read(Config.MEGATOOLS);
        }

        
        ///<summary>Wrapper that create a temporary download directory for the game's zip file, and delete the download and zip file on completion</summary>
        private void WithDownloadedZip(Action action, IPlayniteAPI api, string downloadPath, string installPath, string gameName){
            ErrorHandler.WithTryCatch(() => {
                Directory.CreateDirectory(downloadPath);
                action();
                var zip = Directory.GetFiles(downloadPath)[0];
                System.IO.Compression.ZipFile.ExtractToDirectory(zip, installPath);
            }, api, finallyBlock: () => {
                if(Directory.Exists(downloadPath)) Directory.Delete(downloadPath, true);
            });
        }

        private void GetMegaFolders(string megaURL){
            var process = new System.Diagnostics.Process();
            process.StartInfo.FileName = _toolPath;
            process.StartInfo.Arguments = $"dl --choose-files {megaURL}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            
        }

        private void CompareLocalToMega(string megaURL, string localFolder){
            GetMegaFolders(megaURL);
        }
        
        ///<summary>Use megatools to download from mega</summary>
        public void DownloadProcess(string megaURL, string downloadPath){
            var process = new System.Diagnostics.Process();

            process.StartInfo.FileName = _toolPath;
            process.StartInfo.Arguments =  $"dl {megaURL} --path \"{downloadPath}\"";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            if(error != null){
                throw new Exception(error);
            }
            else if(process.ExitCode != 0){
                throw new Exception($"DOwnload failed with exit code: {process.ExitCode}");
            }
        }
        
        ///<summary>Download a zip file from Mega</summary>
        ///<param name="api">Playnite API instance</param>
        ///<param name="megaURL">The Mega decrypt Link to the game's zip file</param>
        ///<param name="installPath">Where to extract the game to (must match game.DB's installpath)</param>
        ///<param name="gameName">The name of the game (temporary download folder)</param>
        public void Download(IPlayniteAPI api, string megaURL, string installPath, string gameName){
            var downloadPath = Path.Combine(Config.Read(Config.DOWNLOADPATH), gameName);
            WithDownloadedZip(() => {
                DownloadProcess(megaURL, downloadPath);
            }, api, downloadPath, installPath, gameName);
        }

        public void DownloadFolder(IPlayniteAPI api, string megaURL, string downloadPath){
            
            DownloadProcess(megaURL, downloadPath);

        }
    }
}
