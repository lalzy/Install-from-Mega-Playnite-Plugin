using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Globalization;
using System.Linq;

public static class parseFromMega{
    static int GetDepth(string line){
        int depth = 0;
        foreach(char c in line){
            if(c == ' ') depth++;
            else break;
        }
        return depth;
    }
    
    static string ExtractName(string line){
        var trimmedLine = line.Substring(line.IndexOf('.') + 2);
        var endIndex = trimmedLine.LastIndexOf('(');
        if(endIndex == -1) return trimmedLine.TrimEnd('/');
        else return trimmedLine.Substring(0, --endIndex);
    }

    ///<summary>Return the line number</summary>
    static long ExtractNumber (string line){
        // Strip the first set of digits before the dot designator
        return int.Parse(new string(line.Substring(0, line.IndexOf('.')).Where(char.IsDigit).ToArray()));
    }
    
    static long ExtractSize(string line){
        const int K = 1024;
        var start = line.LastIndexOf('(');
        // if holds no ( it's a folder
        if(start == -1) return -1;
        
        var end = line.LastIndexOf(')');

        // Splits the size string into value and size-designator
        var parts = line.Substring(start + 1, end - start - 1).Split(new char[]{'\u00A0', ' '}, System.StringSplitOptions.RemoveEmptyEntries);

        // parse the size value from the string
        if(!double.TryParse(Regex.Replace(parts[0], @"[^0-9.]", ""), NumberStyles.Any, CultureInfo.InvariantCulture, out double value)) return -1;

        // return size multiplied by K for correct sizing
        switch(parts[parts.Length - 1].ToLower().Trim()){
            case "bytes":
                return (long)value;
            case "kib":
                return (long)(value * K);
            case "mib":
                return (long)(value * K * K);
        }
        return -1;
    }
    
    ///<summary>Recursively walk the paths and produce MegaFiles</summary>
    private static List<MegaFile> ParseLines(string[] lines, int index, Dictionary<int, string> depthMap, List<MegaFile> result){
        if(index >= lines.Length)  return result;

        var line = lines[index];
        var name = ExtractName(line);
        var size = ExtractSize(line);
        var depth = GetDepth(line);
        var path = (depthMap.ContainsKey(depth - 3) ? depthMap[depth - 3] : "") + "/" + name;
        depthMap[depth] = path;
        
        result.Add(new MegaFile{
            name = name,
            size = size,
            IsDirectory = (size == -1),
            path = path,
            LineNumber = ExtractNumber(line)
        });
        
        if(++index >= lines.Length) return result;
        return ParseLines(lines, index, depthMap, result);
    }

    ///<summary>Parse the lines from MegaTool's file lists</summary>
    ///<returns>A list of MegaFiles</returns>
    public static List<MegaFile> ParseLines(string[] lines){
        return ParseLines(lines, 1, new Dictionary<int, string>(), new List<MegaFile>());
    }

    ///<summary>Convert a list of megafiles into a directory key'ed by it's path</summary>
    ///<returns>The dictionary</returns>
    public static Dictionary<string, MegaFile> ConvertToLookup(List<MegaFile> files){
        Dictionary<string, MegaFile> output = new Dictionary<string, MegaFile>();
        foreach(MegaFile file in files){
            if(!file.IsDirectory){
                output[file.path.TrimStart('/')] = file;
            }
        }
        return output;
    }
}
