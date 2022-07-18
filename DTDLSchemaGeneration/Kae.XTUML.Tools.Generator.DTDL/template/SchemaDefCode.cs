// Copyright (c) Knowledge & Experience. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
using Kae.CIM.MetaModel.CIMofCIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kae.XTUML.Tools.Generator.DTDL.template
{
    partial class SchemaDef
    {
        string indent;
        string indentDelta;
        CIMClassS_DT sdtDef;
        private static IDictionary<string, string> dtToDTDLDataType = new Dictionary<string, string>() {
            { "boolean", "boolean" },
            { "unique_id", "string" },
            { "integer", "integer" },
            { "real", "double" },
            { "timestamp", "dateTime" },
            { "string", "string" },
            { "state<State_Model>", "string" },
            { "inst_ref<Timer>", "string" }
        };

        public SchemaDef(string indent, string indentDelta, CIMClassS_DT sdtDef)
        {
            this.indent = indent;
            this.indentDelta = indentDelta;
            this.sdtDef = sdtDef;
        }

        private void prototype()
        {
            var subSdtDef = sdtDef.SubClassR17();
            if (subSdtDef is CIMClassS_CDT)
            {
                var cdtDef = (CIMClassS_CDT)subSdtDef;

                if (dtToDTDLDataType.ContainsKey(sdtDef.Attr_Name))
                {
                    string typeName = dtToDTDLDataType[sdtDef.Attr_Name];
                    string descrip = sdtDef.Attr_Descrip;
                }
                else
                {
                    // TODO: ???
                }
            }
            else if (subSdtDef is CIMClassS_UDT)
            {
                var udtDef = (CIMClassS_UDT)subSdtDef;
                var baseSdtDef = udtDef.LinkedToR18();
                var schemaGen = new SchemaDef(indent, indentDelta, baseSdtDef);
                var schemaContent = schemaGen.TransformText();
            }
            else if (subSdtDef is CIMClassS_EDT)
            {
                var edtDef = (CIMClassS_EDT)subSdtDef;
                var schemaGen = new EnumDef(indent, indentDelta, edtDef);
                var schemaContent = schemaGen.TransformText();
            }
            else if (subSdtDef is CIMClassS_SDT)
            {
                var sdtDef = (CIMClassS_SDT)subSdtDef;
                var schemaGen = new ObjectDef(indent, indentDelta, sdtDef);
                var schemaContent = schemaGen.TransformText();
            }
            else if (subSdtDef is CIMClassS_IRDT)
            {
                var rdtDef = (CIMClassS_IRDT)subSdtDef;
                // TODO: ???
            }
        }
    }
}
