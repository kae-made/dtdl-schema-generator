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
    partial class TelemetryDef
    {
        private string indent;
        private string indentDelta;
        private CIMClassSM_EVT smEvtDef;
        private string name;
        private string displayName;
        private CIMClassO_OBJ objDef;

        public TelemetryDef(string indent, string indentDelta, CIMClassO_OBJ objDef, CIMClassSM_EVT smEvtDef)
        {
            this.indent = indent;
            this.indentDelta = indentDelta;
            this.smEvtDef = smEvtDef;
            this.objDef = objDef;

            this.displayName = $"{objDef.Attr_Key_Lett}{smEvtDef.Attr_Numb}:{smEvtDef.Attr_Mning}";
            var frags = smEvtDef.Attr_Mning.Split(new char[] { ' '});
            foreach (var frag in frags)
            {
                this.name += frag.Substring(0,1).ToUpper()+frag.Substring(1);
            }
        }

        public void prototype()
        {
            var evtdiDefs = smEvtDef.LinkedFromR532();
            if (evtdiDefs.Count() == 0)
            {

            }
            else
            {
                foreach(var evtdiDef in evtdiDefs)
                {
                    var evtdiName = evtdiDef.Attr_Name;
                    var evtdiDescrip = evtdiDef.Attr_Descrip;
                    var sdtDef = evtdiDef.LinkedToR524();

                    var schemaGen = new SchemaDef(indent, indentDelta, sdtDef);
                    var schema = schemaGen.TransformText();
                }
            }
        }
    }
}
