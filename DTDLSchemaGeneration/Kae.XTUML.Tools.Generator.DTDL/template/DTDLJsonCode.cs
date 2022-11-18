// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Kae.CIM;
using Kae.CIM.MetaModel.CIMofCIM;
using Kae.Tools.Generator;
using Kae.Tools.Generator.Coloring.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kae.XTUML.Tools.Generator.DTDL.template
{
    partial class DTDLjson
    {
        public enum R_SUPERSUB_Mode
        {
            Extends,
            Relationship
        }
        private string version;

        private CIMClassO_OBJ objDef;
        string nameSpace;
        string modelVersion;

        string indent;
        string indentDelta = "  ";

        bool isIoTPnP = false;
        bool isIoTPnPMarked = false;

        ColoringRepository coloringRepository;

        private R_SUPERSUB_Mode rSuperSubMode;
        // Coloring:
        //   R_SUBSUP:
        //     subsupgen: relationship|extends

        public DTDLjson(string nameSpace, string modelVersion, CIMClassO_OBJ objDef, bool isIoTPnP, string version, ColoringRepository coloringRepository)
        {
            this.version = version;
            this.objDef = objDef;
            this.nameSpace = nameSpace;
            this.modelVersion = modelVersion;
            this.coloringRepository = coloringRepository;
            this.isIoTPnP = isIoTPnP;

            if (objDef.Attr_Descrip.StartsWith("@iotpnp"))
            {
                this.isIoTPnPMarked = true;
            }

            this.rSuperSubMode = R_SUPERSUB_Mode.Relationship;

            if (coloringRepository != null && coloringRepository.Colors.ContainsKey("R_SUBSUP"))
            {
                var coloring = coloringRepository.Colors["R_SUBSUP"];
                var subsupMode = coloring.Where(p => p.Key == "subsupgen").FirstOrDefault();
                if (subsupMode != null)
                {
                    var modeValue = subsupMode.Params["mode"];
                    if (modeValue == "relationship")
                    {
                        rSuperSubMode = R_SUPERSUB_Mode.Relationship;
                    }
                    else if (modeValue == "extends")
                    {
                        rSuperSubMode = R_SUPERSUB_Mode.Extends;
                    }
                }
            }
        }

        private bool IsIoTPnPCommand(CIMClassO_TFR tfrDef)
        {
            bool result = false;

            if (!string.IsNullOrEmpty(tfrDef.Attr_Descrip) && tfrDef.Attr_Descrip.StartsWith("@iotpnp"))
            {
                result = true;
            }

            return result;
        }
        private string GetFieldType(CIMClassO_ATTR attrDef)
        {
            string result = "Property";

            if (isIoTPnP)
            {
                string descrip = attrDef.Attr_Descrip;
                if (descrip.IndexOf("@telemetry") > 0)
                {
                    result = "Telemetry";
                }
            }

            return result;
        }

        public void prototype2()
        {
            if (rSuperSubMode == R_SUPERSUB_Mode.Extends)
            {
                string objComment = "";
                var extendsDef = GetBaseTarget(ref objComment);
            }
            if (rSuperSubMode== R_SUPERSUB_Mode.Relationship)
            {

            }
        }

        public void prototype()
        {
            string objId = DTDLGenerator.GetDTDLID(objDef.Attr_Key_Lett,nameSpace, modelVersion);
            string content = "";

            // for properties
            var attrSet = objDef.LinkedFromR102();
            var relAttrs = new List<CIMClassO_RATTR>();
            var sbProp = new StringBuilder();
            foreach (var attr in attrSet)
            {
                var attrDef = (CIMClassO_ATTR)attr;
                var dtDef = attrDef.LinkedToR114();
                var oidaDefs = attrDef.LinkedOneSideR105();
                string propertyName = attrDef.Attr_Name;

                string descrip = DTDLGenerator.GetDescription(attrDef, attrDef.Attr_Descrip);
                string comment = "";
                var dtName = DTDLGenerator.GetAttrDataTypeName(attrDef);
                if (oidaDefs.Count() > 0)
                {
                    comment = GetIdentityAttrComment(oidaDefs, comment);
                }
                var rattr = attrDef.SubClassR106();
                if (rattr is CIMClassO_RATTR)
                {
                    var rattrDef = (CIMClassO_RATTR)rattr;
                    comment=GetRefAttrComment(rattrDef,comment);
                    relAttrs.Add(rattrDef);
                }
                else if (rattr is CIMClassO_BATTR)
                {
                    var subBAttrDef = ((CIMClassO_BATTR)(rattr)).SubClassR107();
                    if (subBAttrDef is CIMClassO_DBATTR)
                    {

                    }
                    else if (subBAttrDef is CIMClassO_NBATTR)
                    {

                    }
                }
                if (attrDef.Attr_Descrip.IndexOf("@readonly") >= 0)
                {

                }

                var propertyGen = new PropertyDef(indent, indentDelta, attr, comment, null);
                var propertyDef = propertyGen.TransformText();
                sbProp.Append(propertyDef);
            }
            string propertyDefs = sbProp.ToString();
            content = propertyDefs.Substring(0, propertyDefs.LastIndexOf(","));

            // for telemetries
            var sbTlem = new StringBuilder();
            var ismDef = objDef.LinkedFromR518();
            CIMClassSM_SM smDef = null;
            if (ismDef != null)
            {
                smDef = ismDef.CIMSuperClassSM_SM();
            }
            var asmDef = objDef.LinkedFromR519();
            if (asmDef != null)
            {
                smDef = asmDef.CIMSuperClassSM_SM();
            }
            if (smDef != null)
            {
                var evtDefs = smDef.LinkedFromR502();
                foreach (var evtDef in evtDefs)
                {
                    var telemetryGen = new TelemetryDef(indent, indentDelta, objDef, evtDef);
                    var telemetryDef = telemetryGen.TransformText();
                    sbTlem.Append(telemetryDef);
                }
            }
            string telemetryDefs = sbTlem.ToString();
            if (!string.IsNullOrEmpty(telemetryDefs)){
                telemetryDefs = telemetryDefs.Substring(0, telemetryDefs.LastIndexOf(","));
                content += "," + Environment.NewLine;
                content += telemetryDefs;
            }

            // for command
            var sbCmd = new StringBuilder();
            var tfrDes = objDef.LinkedFromR115();
            foreach (var tfrDef in tfrDes)
            {
                var commandGen = new CommandDef(indent, indentDelta, tfrDef);
                var commandDef = commandGen.TransformText();
                sbCmd.Append(commandDef);
            }
            string commandDefs = sbCmd.ToString();
            if (!string.IsNullOrEmpty(commandDefs))
            {
                commandDefs = commandDefs.Substring(0, commandDefs.LastIndexOf(","));
                content += "," + Environment.NewLine;
                content += commandDefs;
            }

            // for relationship
            var relationshipGen = new RelationshipDef(indent, indentDelta, nameSpace, modelVersion, objDef);
            var relationshipDef = relationshipGen.TransformText();
            if (!string.IsNullOrEmpty(relationshipDef))
            {
                relationshipDef = relationshipDef.Substring(0, relationshipDef.LastIndexOf(","));
                content += "," + Environment.NewLine;
                content += relationshipDef;
            }

            // TODO: Write

            var definedRels = new List<string>();
            // for relationship
            foreach (var rattrDef in relAttrs)
            {
                var orefDefs = rattrDef.LinkedFromR108();
                foreach (var orefDef in orefDefs)
                {
                    var rgoDef = orefDef.LinkedOneSideR111();
                    CIMClassR_REL relDef = GetR_REL(rgoDef);
                    if (relDef != null)
                    {
                        string relName = $"R{relDef.Attr_Numb}";
                        if (!definedRels.Contains(relName))
                        {
                            var objDef = orefDef.LinkedOtherSideR111().LinkedOtherSideR110().LinkedOneSideR105().LinkedToR104();
                            var relId = DTDLGenerator.GetDTDLID(relName, nameSpace, modelVersion);
                            string maxMultiplicity = "";
                            var subRgoDef = rgoDef.SubClassR205();
                            if (subRgoDef is CIMClassR_SUB)
                            {
                                var rsubDef = (CIMClassR_SUB)subRgoDef;
                                maxMultiplicity = "1";
                            }
                            else if (subRgoDef is CIMClassR_FORM)
                            {
                                var rformDef = (CIMClassR_FORM)subRgoDef;
                                relName += $"_{GetProgramString(rformDef.Attr_Txt_Phrs)}";
                                if (rformDef.Attr_Mult == 0)
                                {
                                    maxMultiplicity = "1";
                                }
                            }
                            else if (subRgoDef is CIMClassR_ASSOC)
                            {
                                var rassocDef = (CIMClassR_ASSOC)subRgoDef;
                                var raoneDef = rassocDef.LinkedFromR209();
                                relName += $"_{GetProgramString(raoneDef.Attr_Txt_Phrs)}";
                                if (raoneDef.Attr_Mult == 0)
                                {
                                    maxMultiplicity = "1";
                                }
                            }
                            else
                            {
                                // TODO: Error
                            }
                        }
                    }
                    else
                    {
                        // TODO: Error
                    }
                }

                // Is OK?
                var oidaDefs = rattrDef.CIMSuperClassO_ATTR().LinkedOneSideR105();
                foreach (var oidaDef in oidaDefs)
                {
                    var rtidaDefs = oidaDef.LinkedOneSideR110();
                    foreach (var rtidaDef in rtidaDefs)
                    {
                        var rtoDef = rtidaDef.LinkedOneSideR110();
                        var subRtoDef = rtoDef.SubClassR204();
                        string relId = "";
                        string relName = "";
                        string maxMultiplicity = "";
                        CIMClassR_REL relDef = null;
                        if (subRtoDef is CIMClassR_SUPER)
                        {
                            var rSuperDef = (CIMClassR_SUPER)subRtoDef;
                            relDef = rSuperDef.LinkedToR212().CIMSuperClassR_REL();
                            maxMultiplicity = "1";
                            relName = $"R{relDef.Attr_Numb}";
                            relId = relName;
                        }
                        else if (subRtoDef is CIMClassR_PART)
                        {
                            var rPartDef = (CIMClassR_PART)subRtoDef;
                            relDef =  rPartDef.LinkedToR207().CIMSuperClassR_REL();
                            if (rPartDef.Attr_Mult == 0)
                            {
                                maxMultiplicity = "1";
                            }
                            relName = $"R{relDef.Attr_Numb}";
                            relId = $"{relName}_{GetProgramString(rPartDef.Attr_Txt_Phrs)}";
                        }
                        else if (subRtoDef is CIMClassR_AONE)
                        {
                            var rAoneDef = (CIMClassR_AONE)subRtoDef;
                            relDef = rAoneDef.LinkedToR209().CIMSuperClassR_REL();
                            if (rAoneDef.Attr_Mult == 0)
                            {
                                maxMultiplicity = "1";
                            }
                            relName = $"R{relDef.Attr_Numb}";
                            relId = $"{relName}_{GetProgramString(rAoneDef.Attr_Txt_Phrs)}";
                        }
                        else if (subRtoDef is CIMClassR_AOTH)
                        {
                            var rAothDef = (CIMClassR_AOTH)subRtoDef;
                            relDef = rAothDef.LinkedToR210().CIMSuperClassR_REL();
                            if (rAothDef.Attr_Mult == 0)
                            {
                                maxMultiplicity="1";
                            }
                            relName = $"R{relDef.Attr_Numb}";
                            relId = $"{relName}_{GetProgramString(rAothDef.Attr_Txt_Phrs)}";
                        }
                        if (!definedRels.Contains(relName))
                        {
                            // define DTDL relationship

                            definedRels.Add(relName);
                        }
                    }

                }
            }
        }

        private string GetProgramString(string original)
        {
            string result = "";
            var frags = original.Split(new char[] { ' ' });
            foreach(var frag in frags)
            {
                result += frag.Substring(0, 1).ToUpper() + frag.Substring(1);
            }
            return result;
        }

        private string GetIdentityAttrComment(IEnumerable<CIMClassO_OIDA> oidaDefs, string comment)
        {
            foreach (var oidaDef in oidaDefs)
            {
                var oidDef = oidaDef.LinkedOneSideR105();
                if (string.IsNullOrEmpty(comment))
                {
                    comment = "@";
                }
                else
                {
                    comment += ",";
                }
                comment += $"I{oidDef.Attr_Oid_ID}";

                var partingRels = new List<string>();
                var rtidaDefs = oidaDef.LinkedOneSideR110();
                foreach(var rtidaDef in rtidaDefs)
                {
                    var orefDefs = rtidaDef.LinkedOneSideR111();
                    foreach(var orefDef in orefDefs)
                    {
                        var rgoDef = orefDef.LinkedOneSideR111();
                        var relDef = rgoDef.CIMSuperClassR_OIR().LinkedOneSideR201();
                        string relName = "";
                        string minMultiplicity = "";
                        string maxMultiplicity = "";
                        string descrip = "";
                        CIMClassR_REL rrelDef = null;
                        CIMClassO_OBJ targetObjDef = RelationshipDef.GetRelSpec(rSuperSubMode, orefDef, ref rrelDef, ref relName, ref minMultiplicity, ref maxMultiplicity, ref descrip);

                        partingRels.Add(relName);
                    }
                }
                foreach(var relId in partingRels)
                {
                    if (string.IsNullOrEmpty(comment))
                    {
                        comment = "@";
                    }
                    else
                    {
                        comment += ",";
                    }
                    comment += $"P{relId}";
                }
            }
            return comment;
        }
        private string GetRefAttrComment(CIMClassO_RATTR rattrDef, string comment)
        {
            var orefDefs = rattrDef.LinkedFromR108();
            foreach (var orefDef in orefDefs)
            {
                var subRgoDef = orefDef.LinkedOneSideR111().SubClassR205();
                CIMClassR_REL relDef = null;
                if (subRgoDef is CIMClassR_FORM)
                {
                    var rformDef = (CIMClassR_FORM)subRgoDef; 
                    relDef=rformDef.LinkedToR208().CIMSuperClassR_REL();
                }
                else if (subRgoDef is CIMClassR_SUB)
                {
                    var rsubDef = (CIMClassR_SUB)subRgoDef;
                    relDef=rsubDef.LinkedToR213().CIMSuperClassR_REL();
                }
                else if (subRgoDef is CIMClassR_ASSR)
                {
                    var rassrDef = (CIMClassR_ASSR)subRgoDef;
                    relDef=rassrDef.LinkedToR211().CIMSuperClassR_REL();
                }
                if (relDef != null)
                {
                    if (string.IsNullOrEmpty(comment))
                    {
                        comment = "@";
                    }
                    else
                    {
                        comment += ",";
                    }
                    comment += $"R{relDef.Attr_Numb}";
                }
            }
            return comment;
        }

        private CIMClassR_REL GetR_REL(CIMClassR_RGO rgoDef)
        {
            var subRgo = rgoDef.SubClassR205();
            CIMClassR_REL relDef = null;
            if (subRgo is CIMClassR_SUB)
            {
                var rsubDef = (CIMClassR_SUB)subRgo;
                relDef = rsubDef.LinkedToR213().CIMSuperClassR_REL();
            }
            else if (subRgo is CIMClassR_FORM)
            {
                var rformDef = (CIMClassR_FORM)subRgo;
                relDef = rformDef.LinkedToR208().CIMSuperClassR_REL();
            }
            else if (subRgo is CIMClassR_ASSOC)
            {
                relDef = ((CIMClassR_ASSOC)subRgo).CIMSuperClassR_REL();
            }

            return relDef;
        }

        private string GetBaseTarget(ref string comment)
        {
            comment = "";
            string result = "";
            var attrDefs = objDef.LinkedFromR102();
            foreach (var attrDef in attrDefs)
            {
                var subAttrDef = attrDef.SubClassR106();
                if (subAttrDef is CIMClassO_RATTR)
                {
                    var orefDefs = ((CIMClassO_RATTR)subAttrDef).LinkedFromR108();
                    foreach(var orefDef in orefDefs)
                    {
                        var subRgoDef = orefDef.LinkedOneSideR111().SubClassR205();
                        if (subRgoDef is CIMClassR_SUB)
                        {
                            var rsubSupDef = ((CIMClassR_SUB)subRgoDef).LinkedToR213();
                            var rrelDef = rsubSupDef.CIMSuperClassR_REL();
                            if (string.IsNullOrEmpty(comment))
                            {
                                comment = "@extends:";
                            }
                            else
                            {
                                comment += ",";
                            }
                            comment += $"R{rrelDef.Attr_Numb}";
                            var rrtoDef = rsubSupDef.LinkedFromR212().CIMSuperClassR_RTO();
                            var targetObjDef = rrtoDef.LinkedToR109().LinkedToR104();
                            var targetObjId = DTDLGenerator.GetDTDLID(targetObjDef.Attr_Key_Lett, nameSpace, modelVersion);
                            if (!string.IsNullOrEmpty(result))
                            {
                                result += ",";
                            }
                            result += $"\"{targetObjId}\"";
                        }
                    }
                }
            }
            return result;
        }

        private bool IsNotReferenceAttribute(CIMClassO_ATTR attrDef)
        {
            if (rSuperSubMode == R_SUPERSUB_Mode.Relationship)
            {
                return true;
            }
            bool result = true;
            var subAttrDef = attrDef.SubClassR106();
            if (subAttrDef is CIMClassO_RATTR)
            {
                var rattrDef = (CIMClassO_RATTR)subAttrDef;
                var orefDefs = ((CIMClassO_RATTR)subAttrDef).LinkedFromR108();
                foreach (var orefDef in orefDefs)
                {
                    var subRgoDef = orefDef.LinkedOneSideR111().SubClassR205();
                    if (subRgoDef is CIMClassR_SUB)
                    {
                        result = false;
                        break;
                    }
                }
            }
            return result;
        }
    }
}
