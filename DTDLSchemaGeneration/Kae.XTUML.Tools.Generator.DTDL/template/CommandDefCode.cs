using Kae.CIM.MetaModel.CIMofCIM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kae.XTUML.Tools.Generator.DTDL.template
{
    partial class CommandDef
    {
        private string indent;
        private string indentDelta;
        private CIMClassO_TFR otfrDef;

        public CommandDef(string indent, string indentDelta, CIMClassO_TFR otfrDef)
        {
            this.indent = indent;
            this.indentDelta = indentDelta;
            this.otfrDef = otfrDef;
        }

        private void prototype()
        {
            var descrip = otfrDef.Attr_Descrip;
            var name = otfrDef.Attr_Name;

            var paramDefs = otfrDef.LinkedFromR117();
            if (paramDefs.Count() > 0)
            {
                foreach(var paramDef in paramDefs)
                {
                    var paramName = paramDef.Attr_Name;
                    var paramDescrip = paramDef.Attr_Descrip;
                    var sdtDef = paramDef.LinkedToR118();

                    var schemaGen = new SchemaDef(indent, indentDelta, sdtDef);
                    var schema = schemaGen.TransformText();
                }
            }

            var returnDef = otfrDef.LinkedToR116();
            if (returnDef != null)
            {
                var schemaGen = new SchemaDef(indent, indentDelta, returnDef);
                var schema = schemaGen.TransformText();
            }
        }
    }
}
