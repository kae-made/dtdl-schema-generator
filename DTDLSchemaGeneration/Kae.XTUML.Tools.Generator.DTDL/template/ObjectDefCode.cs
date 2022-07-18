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
    partial class ObjectDef
    {
        string indent;
        string indentDelta;
        CIMClassS_SDT sdtDef;

        public ObjectDef(string indent, string indentDelta, CIMClassS_SDT sdtDef)
        {
            this.indent = indent;
            this.indentDelta = indentDelta;
            this.sdtDef = sdtDef;
        }

        public void prototype()
        {
            var dtDef = sdtDef.CIMSuperClassS_DT();
            var name = dtDef.Attr_Name;
            var descrip = dtDef.Attr_Descrip;

            var memberDefs = sdtDef.LinkedFromR44();
            foreach (var memberDef in memberDefs)
            {
                var memberName = memberDef.Attr_Name;
                var memberDescrip = memberDef.Attr_Descrip;
                var memberDtDef = memberDef.LinkedToR45();
                var schemaDefGen = new SchemaDef(indent + indentDelta, indentDelta, memberDtDef);
                var content = schemaDefGen.TransformText();
            }
        }
    }
}
