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
    partial class EnumDef
    {
        string indent;
        string indentDelta;
        CIMClassS_EDT edtDef;

        public EnumDef(string indent, string indentDelta, CIMClassS_EDT edtDef)
        {
            this.indent = indent;
            this.indentDelta = indentDelta;
            this.edtDef = edtDef;
        }

        private void prototype()
        {
            var dtDef = edtDef.CIMSuperClassS_DT();
            var displayName = dtDef.Attr_Name;
            var descrip = dtDef.Attr_Descrip;

            var firstEnumDef = edtDef.LinkedFromR27().First();
            while (true)
            {
                var prevEnumDef = firstEnumDef.LinkedToR56Precedes();
                if (prevEnumDef == null)
                {
                    break;
                }
                firstEnumDef = prevEnumDef;
            }

            var currentEnumDef = firstEnumDef;
            while (true)
            {
                var enumName = currentEnumDef.Attr_Name;
                var enumDescrip = currentEnumDef.Attr_Descrip;
                var nextEnumDef = currentEnumDef.LinkedFromR56Succeeds();
                if (nextEnumDef == null)
                {
                    break;
                }
                currentEnumDef = nextEnumDef;
            }
        }
    }
}
