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

        ///<summary>Download a zip file from Mega</summary>
        ///<param name="api">Playnite API instance</param>
        ///<param name="megaURL">The Mega decrypt Link to the game's zip file</param>
        ///<param name="installPath">Where to extract the game to (must match game.DB's installpath)</param>
        ///<param name="gameName">The name of the game (temporary download folder)</param>
        public void Download(IPlayniteAPI api, string megaURL, string installPath, string gameName){
            var downloadPath = Path.Combine(Config.Read(Config.DOWNLOADPATH), gameName);
            WithDownloadedZip(() => {
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = _toolPath;
                process.StartInfo.Arguments = $"dl {megaURL} --path \"{downloadPath}\"";
                process.Start();
                process.WaitForExit();
                if(process.ExitCode != 0) throw new Exception($"Download failed with exit code: {process.ExitCode}");
            }, api, downloadPath, installPath, gameName);
        }
    }
}
