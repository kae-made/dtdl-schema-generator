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
    partial class PropertyDef
    {
        private string indent;
        private string indentDelta;
        private CIMClassO_ATTR oattrDef;
        private string comment;
        private string fieldType = "Property";
        bool isExtendingClass;
        bool writable = true;

        IoTPnPColoringForAttribute iotPnPPropColor;

        public PropertyDef(string indent, string indentDelta, CIMClassO_ATTR oattrDef, string comment, IoTPnPColoringForAttribute colors,  bool isExtendingClass=false)
        {
            this.indent = indent;
            this.indentDelta = indentDelta;
            this.oattrDef = oattrDef;
            this.comment = comment;
            this.iotPnPPropColor = colors;
            if (colors!=null && colors.IsCurrentIoTPnP)
            {
                if (iotPnPPropColor.IsTelemetry)
                {
                    fieldType = "Telemetry";
                }
                if (iotPnPPropColor.IsReadOnly)
                {
                    writable = false;
                }
            }
            this.isExtendingClass = isExtendingClass;
        }

        private void prototype()
        {
            var objDef = oattrDef.LinkedToR102();
            var kl = objDef.Attr_Key_Lett;
            if (isExtendingClass) ;
            var name = oattrDef.Attr_Name;
            var descip = oattrDef.Attr_Descrip;
            // var sdtDef = oattrDef.LinkedToR114();
            var sdtDef = GetBaseDT();

            var schemaGen = new SchemaDef(indent, indentDelta, sdtDef);
            var content = schemaGen.TransformText();
        }

        private CIMClassS_DT GetBaseDT()
        {
            var subAttrDef = oattrDef.SubClassR106();
            if (subAttrDef is CIMClassO_BATTR)
            {
                var battrDef = (CIMClassO_BATTR)subAttrDef;
                var dtDef = oattrDef.LinkedToR114();
                return dtDef;
            }
            else if (subAttrDef is CIMClassO_RATTR)
            {
                var rattrDef = (CIMClassO_RATTR)subAttrDef;
                var referedAttrDef = rattrDef.LinkedToR113().CIMSuperClassO_ATTR();
                var dtDef = referedAttrDef.LinkedToR114();
                return dtDef;
            }
            else
            {
                throw new IndexOutOfRangeException($"O_ATTR[{oattrDef.Attr_Attr_ID}]'s subtype is wrong!");
            }


        }
    }
}
