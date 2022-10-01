// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Kae.CIM;
using Kae.CIM.MetaModel.CIMofCIM;
using Kae.Tools.Generator;
using Kae.Tools.Generator.Coloring.DomainWeaving;
using Kae.Tools.Generator.Coloring.Generator;
using Kae.Tools.Generator.Context;
using Kae.Tools.Generator.utility;
using Kae.Utility.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Kae.XTUML.Tools.Generator.DTDL
{
    public class DTDLGenerator : Kae.Tools.Generator.IGenerator
    {
        private Logger logger;
        private static string version = "0.0.1";
        private static readonly string CIMOOAofOOADomainName = "OOAofOOA";
        public string Version { get { return version; } set { version = value; } }

        // private List<Kae.Tools.Generator.Context.ContextParam> contextParams = new List<ContextParam>();
        //public IList<ContextParam> ContextParams { get { return contextParams; } }

        public IList<string> DomainModels { get; set; }

        public ColoringRepository Coloring { get; set; }

        protected ColoringManager coloringManager;

        private GenFolder genFolder;
        public GenFolder GenFolder
        {
            get { return genFolder; }
        }

        public string DomainName { get { return CIMOOAofOOADomainName; } }

        public ColoringManager ColoringManagerForDomainWeaving { get { return coloringManager; } }

        public DTDLGenerator(Logger logger, string version=null)
        {
            if (!string.IsNullOrEmpty(version))
            {
                Version = version;
            }
            this.logger = logger;

            CreateContext();
        }

        private string OOAofOOAModelFilePath;
        private string MetaDataTypeDefFilePath;
        private string BaseDataTypeDefFilePath;
        private string DomainModelFilePath;
        private string GenFolderPath;
        private string DTDLNameSpace;
        private string DTDLModelVersion;

        private bool resolvedContext = false;
        private bool loadedMetaModel = false;
        private bool loadedDomainModels = false;

        private CIModelResolver.ConceptualInformationModelResolver modelResolver;

        public void LoadMetaModel()
        {
            if (resolvedContext)
            {
                modelResolver = new CIModelResolver.ConceptualInformationModelResolver(logger);
                modelResolver.LoadOOAofOOA(MetaDataTypeDefFilePath, OOAofOOAModelFilePath);
                logger.LogInfo($"Loaded ${OOAofOOAModelFilePath} as OOA of OOA model schmea");
                loadedMetaModel = true;
            }
            else
            {
                throw new ApplicationException("This method should be called after calling ResolveContext()!");
            }
        }
        public void LoadDomainModels()
        {
            if (loadedMetaModel)
            {
                string[] domainModels = { BaseDataTypeDefFilePath, DomainModelFilePath };
                modelResolver.LoadCIInstances(domainModels);
                logger.LogInfo($"Loaded Domain Models.");
                loadedDomainModels = true;
            }
            else
            {
                throw new ApplicationException("This method should be called after calling LoadMetaModel()!");
            }
        }
        public void GenerateContents()
        {
            if (!loadedDomainModels)
            {
                throw new ApplicationException("This method should be called after calling LoadDomainModels");
            }
            var modelRepository = modelResolver.ModelRepository;
            var attrDefs = modelRepository.GetCIInstances(CIMOOAofOOADomainName, "O_ATTR");
            var objAttrNum = new Dictionary<string,int>();
            foreach (var attrDef in attrDefs)
            {
                var oattrDef = (CIMClassO_ATTR)attrDef;
                var attrName = oattrDef.Attr_Name;
                var objDef = oattrDef.LinkedToR102();
                var objName = objDef.Attr_Name;
                if (objAttrNum.ContainsKey(objName))
                {
                    objAttrNum[objName]++;
                }
                else
                {
                    objAttrNum.Add(objName, 1);
                }
            }

            var sysDef = modelRepository.GetCIInstances(CIMOOAofOOADomainName, "S_SYS").First();
            DTDLNameSpace += $":{((CIMClassS_SYS)sysDef).Attr_Name}";

            var classDefs = modelRepository.GetCIInstances(CIMOOAofOOADomainName,"O_OBJ");
            foreach(var classDef in classDefs)
            {
                var objDef = (CIMClassO_OBJ)classDef;
                // prototype(objDef);
                var dtdlGen = new template.DTDLjson(DTDLNameSpace, DTDLModelVersion, objDef, Version, Coloring);
                var dtdlJson = dtdlGen.TransformText();

                string dtdlFileName = $"{objDef.Attr_Key_Lett}.json";
                genFolder.WriteContentAsync(".", dtdlFileName, dtdlJson, GenFolder.WriteMode.Overwrite).Wait();
                Console.WriteLine($"Generated : {dtdlFileName}");
            }
        }

        public static string GetDTDLID(string elemName, string nameSpace, string modelVersion)
        {
            return $"{nameSpace}:{elemName};{modelVersion}";
        }


        public static string GetAttrDataTypeName(CIMClassO_ATTR attrDef)
        {
            string result = "";

            var subAttrDef = attrDef.SubClassR106();
            if (subAttrDef is CIMClassO_BATTR)
            {
                var battrDef = (CIMClassO_BATTR)subAttrDef;
                var dtDef = attrDef.LinkedToR114();
                result = dtDef.Attr_Name;
            }
            else if (subAttrDef is CIMClassO_RATTR)
            {
                var rattrDef = (CIMClassO_RATTR)subAttrDef;
                var referedAttrDef = rattrDef.LinkedToR113().CIMSuperClassO_ATTR();
                var dtDef = referedAttrDef.LinkedToR114();
                result = dtDef.Attr_Name;
            }
            else
            {
                throw new IndexOutOfRangeException($"O_ATTR[{attrDef.Attr_Attr_ID}]'s subtype is wrong!");
            }

            return result;
        }

        public static string GetDescription(CIClassDef classDef, string descrip)
        {
            string result = "";
            if (classDef is CIMClassO_OBJ)
            {
                var objDef = (CIMClassO_OBJ)classDef;
                result = $"Please see description of class:'{objDef.Attr_Name}'";

            }
            else if (classDef is CIMClassO_ATTR)
            {
                var attrDef = (CIMClassO_ATTR)classDef;
                var objDef = attrDef.LinkedToR102();
                result = $"Please see description of attribute:'{attrDef.Attr_Name}' of class:'{objDef.Attr_Name}'";
            }
            else if (classDef is CIMClassS_DT)
            {
                var dtDef = (CIMClassS_DT)classDef;
                result = $"Please see description of datatype:'{dtDef.Attr_Name}'";
            }
            else if (classDef is CIMClassS_ENUM)
            {
                var enumDef = (CIMClassS_ENUM)classDef;
                var dtDef = enumDef.LinkedToR27().CIMSuperClassS_DT();
                result = $"Please see description of enumerator:'{enumDef.Attr_Name}' of datatype:'{dtDef.Attr_Name}'";
            }
            else if (classDef is CIMClassO_TFR)
            {
                var tfrDef = (CIMClassO_TFR)classDef;
                var objDef = tfrDef.LinkedToR115();
                result = $"Please see description of operation:'{tfrDef.Attr_Name}' of class:'{objDef.Attr_Name}'";
            }
            else if (classDef is CIMClassO_TPARM)
            {
                var tparmDef = (CIMClassO_TPARM)classDef;
                var tfrDef = tparmDef.LinkedToR117();
                var objDef = tfrDef.LinkedToR115();
                result = $"Please see description of parameter:'{tparmDef.Attr_Descrip}' of operation:'{tfrDef.Attr_Name}' of class:'{objDef.Attr_Name}'";
            }
            else if (classDef is CIMClassSM_EVT)
            {
                var smevtDef = (CIMClassSM_EVT)classDef;
                var smDef = smevtDef.LinkedToR502();
                var subSmDef = smDef.SubClassR517();
                CIMClassO_OBJ objDef = null;
                if (subSmDef is CIMClassSM_ISM)
                {
                    objDef = ((CIMClassSM_ISM)subSmDef).LinkedToR518();
                }
                else if (subSmDef is CIMClassSM_ASM)
                {
                    objDef = ((CIMClassSM_ASM)subSmDef).LinkedToR519();
                }
                result = $"Please see description of event:'{objDef.Attr_Key_Lett}{smevtDef.Attr_Numb}:{smevtDef.Attr_Mning}' of class:'{objDef.Attr_Name}'";
            }
            else if (classDef is CIMClassSM_EVTDI)
            {
                var smevtdiDef = (CIMClassSM_EVTDI)classDef;
                var smevtDef = smevtdiDef.LinkedToR532();
                var smDef = smevtDef.LinkedToR502();
                var subSmDef = smDef.SubClassR517();
                CIMClassO_OBJ objDef = null;
                if (subSmDef is CIMClassSM_ISM)
                {
                    objDef = ((CIMClassSM_ISM)subSmDef).LinkedToR518();
                }
                else if (subSmDef is CIMClassSM_ASM)
                {
                    objDef = ((CIMClassSM_ASM)subSmDef).LinkedToR519();
                }
                result = $"Please see description of parameter:'{smevtdiDef.Attr_Name}' of event:'{objDef.Attr_Key_Lett}{smevtDef.Attr_Numb}:{smevtDef.Attr_Mning}' of class:'{objDef.Attr_Name}'";
            }
            else if (classDef is CIMClassR_REL)
            {
                var rrelDef = (CIMClassR_REL)classDef;
                result = $"Please see descripiton of relationship:R{rrelDef.Attr_Numb}";
            }
            else if (classDef is CIMClassS_MBR)
            {
                var smbrDef = (CIMClassS_MBR)classDef;
                var dtDef = smbrDef.LinkedToR44().CIMSuperClassS_DT();
                result = $"Please see description of member:'{smbrDef.Attr_Name}' of complex datatype:'{dtDef.Attr_Name}'";
            }
            else
            {
                result = $"Unsupported for {classDef.ClassName}";
            }

            return result;
        }

        private void prototype(CIMClassO_OBJ objDef)
        {
            var definedRels = new List<string>();
            var oattrDefs = objDef.LinkedFromR102();
            foreach(var oattrDef in oattrDefs)
            {
                var subOattrDef = oattrDef.SubClassR106();
                CIMClassS_DT dtDef = null;
                if (subOattrDef is CIMClassO_BATTR)
                {
                    var subBattrDef = ((CIMClassO_BATTR)subOattrDef).SubClassR107();
                    if (subBattrDef is CIMClassO_DBATTR)
                    {
                        var dbattrDef = (CIMClassO_DBATTR)subBattrDef;
                        
                    }
                    else if (subBattrDef is CIMClassO_NBATTR)
                    {

                    }
                }
                else if (subOattrDef is CIMClassO_RATTR)
                {

                }
                if (subOattrDef is CIMClassO_RATTR)
                {
                    var rattrDef = (CIMClassO_RATTR)subOattrDef;
                    var orefDefs = rattrDef.LinkedFromR108();
                    foreach (var orefDef in orefDefs)
                    {
                        var rrgoDef = orefDef.LinkedOneSideR111();
                        var subRrgoDef = rrgoDef.SubClassR205();
                        CIMClassR_REL rrelDef = null;
                        if (subRrgoDef is CIMClassR_FORM)
                        {
                            var rformDef = (CIMClassR_FORM)subRrgoDef;
                            var txtPhrs = rformDef.Attr_Txt_Phrs;

                            rrelDef = rformDef.LinkedToR208().CIMSuperClassR_REL();


                        }
                        else if(subRrgoDef is CIMClassR_SUB)
                        {
                            var rsubDef = (CIMClassR_SUB)subRrgoDef;
                            rrelDef= rsubDef.LinkedToR213().CIMSuperClassR_REL();
                        }
                        else if (subRrgoDef is CIMClassR_ASSR)
                        {
                            var rassrDef = (CIMClassR_ASSR)subRrgoDef;
                            var mult = rassrDef.Attr_Mult;
                            rrelDef = rassrDef.LinkedToR211().CIMSuperClassR_REL();
                        }
                        var relId = "";
                        if (rrelDef != null)
                        {
                            relId = $"R{rrelDef.Attr_Numb}";
                        }

                        var rtoDef = orefDef.LinkedOtherSideR111().LinkedOneSideR110();
                        var targetObjDef = rtoDef.LinkedToR109().LinkedToR104();
                        var subRtoDef = rtoDef.SubClassR204();
                        string minMult = "";
                        string maxMult = "";
                        if (subRtoDef is CIMClassR_SUPER)
                        {
                            var rsuperDef = (CIMClassR_SUPER)subRtoDef;
                            maxMult = "1";
                            minMult = "1";
                        }
                        else if (subRtoDef is CIMClassR_PART)
                        {
                            var rpartDef = (CIMClassR_PART)subRtoDef;
                            if (!string.IsNullOrEmpty(rpartDef.Attr_Txt_Phrs))
                            {
                                relId += $"_{rpartDef.Attr_Txt_Phrs}";
                            }
                            relId += $"_{targetObjDef.Attr_Key_Lett}";
                            if (rpartDef.Attr_Mult == 0)
                            {
                                maxMult = "1";
                            }
                            if (rpartDef.Attr_Cond == 0)
                            {
                                minMult = "1";
                            }
                        }
                        else if (subRtoDef is CIMClassR_AONE)
                        {
                            var raone = (CIMClassR_AONE)subRtoDef;
                            if (!string.IsNullOrEmpty(raone.Attr_Txt_Phrs))
                            {
                                relId += $"_{raone.Attr_Txt_Phrs}";
                            }
                            relId += $"_{targetObjDef.Attr_Key_Lett}";
                            if (raone.Attr_Mult == 0)
                            {
                                maxMult = "1";
                            }
                            if (raone.Attr_Cond == 0)
                            {
                                minMult= "1";
                            }
                        }
                        else if (subRtoDef is CIMClassR_AOTH)
                        {
                            var raoth = (CIMClassR_AOTH)subRtoDef;
                            if (!string.IsNullOrEmpty(raoth.Attr_Txt_Phrs))
                            {
                                relId += $"_{raoth.Attr_Txt_Phrs}";
                            }
                            relId += $"_{targetObjDef.Attr_Key_Lett}";
                            if (raoth.Attr_Mult == 0)
                            {
                                maxMult = "1";
                            }
                            if (raoth.Attr_Cond == 0)
                            {
                                minMult = "1";
                            }
                        }
                        if (!definedRels.Contains(relId))
                        {
                            // TODO: Work
                            definedRels.Add(relId);
                        }
                    }
                }
            }
        }

        
        public static readonly string CPKeyOOAofOOAModelFilePath = "metamodel-path";
        public static readonly string CPKeyMetaDataTypeDefFilePath = "meta-datatype-path";
        public static readonly string CPKeyDomainModelFilePath = "domainmodel-path";
        public static readonly string CPKeyGenFolderPath = "genpath";
        public static readonly string CPKeyDTDLNameSpace = "dtdl-ns";
        public static readonly string CPKeyDTDLModelVersion = "dtdl-mv";
        public static readonly string CPKeyBaseDataTypeDefFilePaht = "base-datatype-path";

        private void CreateContext()
        {
            var ooaOfOOAModelFilePath = new PathSelectionParam(CPKeyOOAofOOAModelFilePath) { IsFolder = false };
            var domainModelFilePath = new PathSelectionParam(CPKeyDomainModelFilePath) { IsFolder= true };
            var metaDataTypeDefFilePath = new PathSelectionParam(CPKeyMetaDataTypeDefFilePath) { IsFolder = false };
            var baseDataTypeDefFilePath = new PathSelectionParam(CPKeyBaseDataTypeDefFilePaht) { IsFolder = false };
            var genFolderPath = new PathSelectionParam(CPKeyGenFolderPath) { IsFolder = true };
            var dtdlNamespace = new StringParam(CPKeyDTDLNameSpace);
            var dtdlModelVersion = new StringParam(CPKeyDTDLModelVersion);
            // contextParams.Add(ooaOfOOAModelFilePath);
            // contextParams.Add(domainModelFilePath);
            // contextParams.Add(metaDataTypeDefFilePath);
            // contextParams.Add(baseDataTypeDefFilePath);
            // contextParams.Add(genFolderPath);
            // contextParams.Add(dtdlNamespace);
            // contextParams.Add(dtdlModelVersion);
            generatorContext.AddOption(ooaOfOOAModelFilePath);
            generatorContext.AddOption(domainModelFilePath);
            generatorContext.AddOption(metaDataTypeDefFilePath);
            generatorContext.AddOption(baseDataTypeDefFilePath);
            generatorContext.AddOption(genFolderPath);
            generatorContext.AddOption(dtdlNamespace);
            generatorContext.AddOption(dtdlModelVersion);
        }

        public void ResolveContext()
        {
            var index = 0;
            foreach (var c in generatorContext.Options)
            {
                if (c.ParamName == CPKeyOOAofOOAModelFilePath)
                {
                    OOAofOOAModelFilePath = ((PathSelectionParam)c).Path;
                    index++;
                }
                else if (c.ParamName == CPKeyMetaDataTypeDefFilePath)
                {
                    MetaDataTypeDefFilePath = ((PathSelectionParam)c).Path;
                    index++;
                }
                else if (c.ParamName == CPKeyDomainModelFilePath)
                {
                    DomainModelFilePath = ((PathSelectionParam)c).Path;
                    index++;
                }
                else if (c.ParamName == CPKeyBaseDataTypeDefFilePaht)
                {
                    BaseDataTypeDefFilePath = ((PathSelectionParam)c).Path;
                    index++;
                }
                else if (c.ParamName == CPKeyGenFolderPath)
                {
                    GenFolderPath = ((PathSelectionParam)c).Path;
                    genFolder = new GenFolder() { BaseFolder = GenFolderPath };
                    index++;
                }
                else if (c.ParamName == CPKeyDTDLNameSpace)
                {
                    DTDLNameSpace = ((StringParam)c).Value;
                    index++;
                }
                else if (c.ParamName == CPKeyDTDLModelVersion)
                {
                    DTDLModelVersion = ((StringParam)c).Value;
                    index++;
                }
            }
            if (index != generatorContext.Options.Count)
            {
                throw new ArgumentOutOfRangeException("some context parameters are missing!");
            }
            resolvedContext = true;
        }

        protected GeneratorContext generatorContext = new GeneratorContext();
        public GeneratorContext GetContext()
        {
            return generatorContext;
        }

        public bool GenerateEnvironment()
        {
            return true;
        }

        public void Generate()
        {
            GenerateContents();
        }
    }
}
