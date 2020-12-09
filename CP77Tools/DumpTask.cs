﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using CP77Tools.Model;
using WolvenKit.CR2W;

namespace CP77Tools
{
    public class ArchiveDumpObject
    {
        public Dictionary<ulong, Cr2wDumpObject> FileDictionary { get; set; }
        public string Filename { get; set; }
    }

    public class Cr2wDumpObject
    {
        public Dictionary<uint, string> stringdict { get; set; }
        public string Filename { get; set; }
    }


    public static partial class ConsoleFunctions
    {

        public static async Task<int> DumpTask(DumpOptions options)
        {
            // initial checks
            var inputFileInfo = new FileInfo(options.path);
            if (!inputFileInfo.Exists)
                return 0;

            var ar = new Archive(inputFileInfo.FullName);




            if (options.dumpstrings)
            {

                Dictionary<ulong, Cr2wDumpObject> fileDictionary = new Dictionary<ulong, Cr2wDumpObject>();
                foreach (var key in ar.HashDictionary.Keys)
                {
                    var f = ar.ExtractOne(key);

                    try
                    {
                        var cr2w = new CR2WFile();

                        using var ms = new MemoryStream(f);
                        using var br = new BinaryReader(ms);
                        var (imports, _, buffers) = cr2w.ReadImportsAndBuffers(br);

                        var obj = new Cr2wDumpObject()
                        {
                            stringdict = cr2w.StringDictionary,
                            Filename = key.ToString()
                        };

                        fileDictionary.Add(key, obj);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        //throw;
                        continue;
                    }
                }

                
                var arobj = new ArchiveDumpObject()
                {
                    Filename = ar._filepath,
                    FileDictionary = fileDictionary
                };

                
                var joptions = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                var jsonstring = JsonSerializer.Serialize(arobj, joptions);

                File.WriteAllText($"{ar._filepath}.dump.json", jsonstring);
            }

            return 1;
        }
    }
}