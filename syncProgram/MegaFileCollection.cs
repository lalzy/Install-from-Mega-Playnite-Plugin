using System.Collections.Generic;

public class MegaFileCollection{
    public Dictionary<string, MegaFile> Files = new Dictionary<string, MegaFile>();

    public bool Contains(string path){
        return Files.ContainsKey(path);
    }
}
