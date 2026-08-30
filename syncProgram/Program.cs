using System;

class Program{
    public static string megaToolsPath;
    public static string libraryPath;
    public static string megaLibraryURL;
    
    private static void ParseArgs(string[] args){
        for(int i = 0; i < args.Length; i++){
            switch(args[i].ToLower()){
                case "--megalibraryurl":
                    megaLibraryURL = args[++i];
                    break;
                case "--localplaynitepath":
                    libraryPath = args[++i];
                    break;
                case "--megatoolspath":
                    megaToolsPath = args[++i];
                    break;
            }
        }
    }
    
    static void Main(string[] args){
        ParseArgs(args);
        if(megaToolsPath == null || libraryPath == null || megaLibraryURL == null)
            throw new Exception("Error, --megalibraryurl, --localplaynitepath, and --megatoolspath is required parameters");
        
    }
}
