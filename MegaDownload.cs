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

        public void Download(IPlayniteAPI api, string megaURL, string installPath, string gameName){
            try{
                var process = new System.Diagnostics.Process();
                var downloadPath = Path.Combine(Config.Read(Config.DOWNLOADPATH), gameName);
                Directory.CreateDirectory(downloadPath);

                process.StartInfo.FileName = _toolPath;
                process.StartInfo.Arguments = $"dl {megaURL} --path \"{downloadPath}\"";
                process.Start();
                process.WaitForExit();

                var zip = Directory.GetFiles(downloadPath)[0];
                System.IO.Compression.ZipFile.ExtractToDirectory(zip, installPath);
                File.Delete(zip);
                Directory.Delete(downloadPath);
            }catch(Exception e){
                api.Dialogs.ShowMessage(e.ToString());
            }
        }
    }
}
