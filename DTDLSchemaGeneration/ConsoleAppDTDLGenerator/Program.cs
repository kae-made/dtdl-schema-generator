﻿// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Kae.Tools.Generator.Context;
using Kae.Utility.Logging;
using Kae.XTUML.Tools.Generator.DTDL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ConsoleAppDTDLGenerator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var p = new Program(args);
            if (p.ResloveArgs(args))
            {
                p.Work();
            }
        }

        //string ooaOfOOAModelPath;
        //string metaDatatypeDefFilePath;
        //string baseDataTypeDefFilePath;
        //string domainModelFilePath;
        //string genFolderPath;
        //string dtdlNamespace;
        //string dtdlModelVersion;

        DTDLGenerator generator;
        string colorsFileName = null;

        public Program(string[] args)
        {
            generator = new DTDLGenerator(ConsoleLogger.CreateLogger());
        }

        public void Work()
        {
            if (!string.IsNullOrEmpty(colorsFileName))
            {
                generator.Coloring = new Kae.Tools.Generator.Coloring.Generator.ColoringRepository();
                generator.Coloring.Load(colorsFileName);
            }

            generator.ResolveContext();
            generator.LoadMetaModel();
            generator.LoadDomainModels();
            generator.Generate();

        }

        public bool ResloveArgs(string[] args)
        {
            var contextParams = generator.GetContext();
            if (args.Length == 0)
            {
                ShowCommandline();
                return false;
            }

            var options = new List<string>() { "--metamodel", "--base-datatype", "--domainmodel", "--dtdlns", "--dtdlver", "--gen-folder" };
            int index = 0;
            while (index < args.Length)
            {
                if (args[index] == "--metamodel")
                {
                    // var cp = contextParams.Where(c => c.ParamName == DTDLGenerator.CPKeyOOAofOOAModelFilePath).First();
                    options.Remove(args[index]);
                    // ((PathSelectionParam)cp).Path = args[++index];
                    string metaModelPath = args[++index];
                    contextParams.SetOptionValue(DTDLGenerator.CPKeyOOAofOOAModelFilePath, (metaModelPath, !File.Exists(metaModelPath)));
                }
                else if (args[index] == "--meta-datatype")
                {
                    // var cp = contextParams.Where(c => c.ParamName == DTDLGenerator.CPKeyMetaDataTypeDefFilePath).First();
                    // ((PathSelectionParam)cp).Path = args[++index];
                    contextParams.SetOptionValue(DTDLGenerator.CPKeyMetaDataTypeDefFilePath, (args[++index], false));
                }
                else if (args[index] == "--base-datatype")
                {
                    // var cp = contextParams.Where(c => c.ParamName == DTDLGenerator.CPKeyBaseDataTypeDefFilePaht).First();
                    options.Remove(args[index]);
                    // ((PathSelectionParam)cp).Path = args[++index];
                    string baseDatatypePath = args[++index];
                    contextParams.SetOptionValue(DTDLGenerator.CPKeyBaseDataTypeDefFilePaht, (baseDatatypePath, !File.Exists(baseDatatypePath)));
                }
                else if (args[index] == "--domainmodel")
                {
                    // var cp = contextParams.Where(c => c.ParamName == DTDLGenerator.CPKeyDomainModelFilePath).First();
                    options.Remove(args[index]);
                    // ((PathSelectionParam)cp).Path= args[++index];
                    // domainModelFilePath = args[index];
                    string domainModelPath = args[++index];
                    contextParams.SetOptionValue(DTDLGenerator.CPKeyDomainModelFilePath, (domainModelPath, !File.Exists(domainModelPath)));
                }
                else if (args[index] == "--dtdlns")
                {
                    // var cp = contextParams.Where(c => c.ParamName == DTDLGenerator.CPKeyDTDLNameSpace).First();
                    options.Remove(args[index]);
                    // ((StringParam)cp).Value = args[++index];
                    contextParams.SetOptionValue(DTDLGenerator.CPKeyDTDLNameSpace, args[++index]);
                }
                else if (args[index] == "--dtdlver")
                {
                    // var cp = contextParams.Where(c => c.ParamName == DTDLGenerator.CPKeyDTDLModelVersion).First();
                    options.Remove(args[index]);
                    // ((StringParam)cp).Value = args[++index];
                    contextParams.SetOptionValue(DTDLGenerator.CPKeyDTDLModelVersion, args[++index]);
                }
                else if (args[index] == "--gen-folder")
                {
                    // var cp = contextParams.Where(c=>c.ParamName==DTDLGenerator.CPKeyGenFolderPath).First();
                    options.Remove(args[index]);
                    // ((PathSelectionParam)cp).Path = args[++index];
                    contextParams.SetOptionValue(DTDLGenerator.CPKeyGenFolderPath, (args[++index], true));
                }
                else if (args[index] == "--use-keylett")
                {
                    bool useKeyLett = true;
                    if (args.Length > index + 1 && args[index + 1].StartsWith("--") == false)
                    {
                        useKeyLett = bool.Parse(args[++index]);
                    }
                    contextParams.SetOptionValue(DTDLGenerator.CPKeyUseKeyLetterAsFileName, useKeyLett);
                }
                else if (args[index]== "--use-extends")
                {
                    bool useExtends = true;
                    if (args.Length>index+1 && args[index+1].StartsWith("--") == false)
                    {
                        useExtends = bool.Parse(args[++index]);
                    }
                    contextParams.SetOptionValue(DTDLGenerator.CPKeyUseKeyLetterAsFileName, !useExtends);
                }
                else if (args[index] == "--colors")
                {
                    colorsFileName = args[++index];
                }
                else
                {
                    ShowCommandline();
                }
                index++;
            }
            if (options.Count() == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static void ShowCommandline()
        {
            Console.WriteLine("ConsoleAppDTDLGenerator --metamodel metamode_file [--meta-datatype meta_data_type_file] --base-datatype base_data_type_file --domainmodel domainmodel_folder --dtdlns namespace --dtdlver version --gen-folder folder [--colors color_file]");
            Console.WriteLine("Options:");
            Console.WriteLine("  --metamodel        : file path of BridgePoint OOA of OOA sql file path");
            Console.WriteLine("  --meta-datatype    : file path of datatype definition YAML file path when you use specific definition");
            Console.WriteLine("  --base-datatype    : file path of BridgePoint\\plugins\\org.xtuml.bp.pkg_xxx\\globals\\Globals.xtuml");
            Console.WriteLine("  --domainmodel      : folder path of BridgePoint model");
            Console.WriteLine("  --dtdlns           : namespace of DTDL ID. style is 'dtmi:org:domain");
            Console.WriteLine("  --dtdlver          : version of DTDL ID 1|2|3|...");
            Console.WriteLine("  --gen-folder       : folder path for generation");
            Console.WriteLine("  --colors           : optional - file path of coloring when you use coloring feature");
            Console.WriteLine("  --use-keylett      : optional - use key letter of class for dtdl file name without any param or true|false ");
        }
    }
}
