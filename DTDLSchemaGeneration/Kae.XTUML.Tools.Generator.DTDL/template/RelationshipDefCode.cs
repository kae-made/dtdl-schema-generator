using Kae.CIM.MetaModel.CIMofCIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kae.XTUML.Tools.Generator.DTDL.template
{
    partial class RelationshipDef
    {
        private string indent;
        private string indentDelta;
        private string relId;
        private string relName;
        private string maxMultiplicity;
        private string minMultiplicity;
        private string target;
        private string descrip;
        private string nameSpace;
        private string modelVersion;
        CIMClassO_OBJ objDef;

        DTDLjson.R_SUPERSUB_Mode rSuperSubMode;

        public RelationshipDef(string indent, string indentDelta, string nameSpace, string modelVersion, CIMClassO_OBJ objDef, DTDLjson.R_SUPERSUB_Mode rSuperSubMode= DTDLjson.R_SUPERSUB_Mode.Relationship)
        {
            this.indent = indent;
            this.indentDelta = indentDelta;
            this.objDef = objDef;
            this.nameSpace = nameSpace;
            this.modelVersion = modelVersion;
            this.rSuperSubMode = rSuperSubMode;
        }

        private void prototype(CIMClassO_OBJ objDef)
        {
            var definedRels = new List<string>();
            var oattrDefs = objDef.LinkedFromR102();
            foreach (var oattrDef in oattrDefs)
            {
                var subOattrDef = oattrDef.SubClassR106();
                if (subOattrDef is CIMClassO_RATTR)
                {
                    var rattrDef = (CIMClassO_RATTR)subOattrDef;
                    var orefDefs = rattrDef.LinkedFromR108();
                    foreach (var orefDef in orefDefs)
                    {
                        relName = "";
                        minMultiplicity = "";
                        maxMultiplicity = "";
                        descrip = "";
                        CIMClassR_REL rrelDef = null;
                        CIMClassO_OBJ targetObjDef = GetRelSpec(orefDef,ref rrelDef, ref relName, ref minMultiplicity, ref maxMultiplicity, ref descrip);
                        string target = DTDLGenerator.GetDTDLID(targetObjDef.Attr_Key_Lett, nameSpace, modelVersion); 
                        if (!definedRels.Contains(relName))
                        {
                            relId = GetDTDLID(relName);
                            // TODO: Work
                            definedRels.Add(relName);
                        }
                    }
                }
            }
        }

        string GetDTDLID(string elemName)
        {
            return $"{nameSpace}:{elemName};{modelVersion}";
        }

        private CIMClassO_OBJ GetRelSpec(CIMClassO_REF orefDef,ref CIMClassR_REL rrelDef, ref string relName, ref string minMult, ref string maxMult, ref string desc)
        {
            CIMClassO_OBJ targetObjDef = null;
            var rrgoDef = orefDef.LinkedOneSideR111();
            var subRrgoDef = rrgoDef.SubClassR205();
            relName = "";
            if (subRrgoDef is CIMClassR_FORM)
            {
                var rformDef = (CIMClassR_FORM)subRrgoDef;
                var txtPhrs = rformDef.Attr_Txt_Phrs;

                rrelDef = rformDef.LinkedToR208().CIMSuperClassR_REL();

                relName = $"R{rrelDef.Attr_Numb}";
            }
            else if (subRrgoDef is CIMClassR_SUB)
            {
                if (rSuperSubMode == DTDLjson.R_SUPERSUB_Mode.Relationship)
                {
                    var rsubDef = (CIMClassR_SUB)subRrgoDef;
                    rrelDef = rsubDef.LinkedToR213().CIMSuperClassR_REL();
                    var sourceObjDef = orefDef.LinkedToR108().CIMSuperClassO_ATTR().LinkedToR102();
                    relName = $"R{rrelDef.Attr_Numb}From{sourceObjDef.Attr_Key_Lett}";
                }
            }
            else if (subRrgoDef is CIMClassR_ASSR)
            {
                var rassrDef = (CIMClassR_ASSR)subRrgoDef;
                var mult = rassrDef.Attr_Mult;
                rrelDef = rassrDef.LinkedToR211().CIMSuperClassR_REL();
                relName = $"R{rrelDef.Attr_Numb}";
            }
            if (rrelDef != null)
            {
                desc = rrelDef.Attr_Descrip;
            }

            var rtoDef = orefDef.LinkedOtherSideR111().LinkedOneSideR110();
            targetObjDef = rtoDef.LinkedToR109().LinkedToR104();
            var subRtoDef = rtoDef.SubClassR204();
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
                    relName += $"_{TranslateTxtPhrs(rpartDef.Attr_Txt_Phrs)}";
                }
                relName += $"_{targetObjDef.Attr_Key_Lett}";
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
                    relName += $"_{TranslateTxtPhrs(raone.Attr_Txt_Phrs)}";
                }
                relName += $"_{targetObjDef.Attr_Key_Lett}";
                if (raone.Attr_Mult == 0)
                {
                    maxMult = "1";
                }
                if (raone.Attr_Cond == 0)
                {
                    minMult = "1";
                }
            }
            else if (subRtoDef is CIMClassR_AOTH)
            {
                var raoth = (CIMClassR_AOTH)subRtoDef;
                if (!string.IsNullOrEmpty(raoth.Attr_Txt_Phrs))
                {
                    relName += $"_{TranslateTxtPhrs(raoth.Attr_Txt_Phrs)}";
                }
                relName += $"_{targetObjDef.Attr_Key_Lett}";
                if (raoth.Attr_Mult == 0)
                {
                    maxMult = "1";
                }
                if (raoth.Attr_Cond == 0)
                {
                    minMult = "1";
                }
            }

            return targetObjDef;
        }

        private string TranslateTxtPhrs(string txtPhrs)
        {
            var frags = txtPhrs.Split(new char[] { ' ' });
            string result = "";
            foreach(var frag in frags)
            {
                result += frag.Substring(0, 1).ToUpper() + frag.Substring(1);
            }
            return result;
        }

    }
}
