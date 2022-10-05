using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kae.XTUML.Tools.Generator.DTDL.template
{
    public class IoTPnPColoringForAttribute
    {
        bool isDeviceId = false;
        bool isReadOnly = false;
        bool isExclude = false;
        bool isTelemetry = false;
        bool isCurrentIoTPnP = false;
        public IoTPnPColoringForAttribute(string descrip)
        {
            string colorKey = "iotpnp";
            int startPos = descrip.IndexOf($"@{colorKey}");
            if (startPos >= 0)
            {
                string part = descrip.Substring(startPos);
                int lpPos = part.IndexOf("(");
                int rpPos = part.IndexOf(")");
                if (lpPos >= 0 && rpPos >= 0 && (rpPos - lpPos) > 2)
                {
                    part = part.Substring(lpPos + 1, rpPos - lpPos - 1);
                    var colors = part.Split(new char[] { ',' });
                    foreach (var c in colors)
                    {
                        switch (c)
                        {
                            case "deviceid":
                                isDeviceId = true;
                                break;
                            case "readonly":
                                isReadOnly = true;
                                break;
                            case "exclude":
                                isExclude = true;
                                break;
                            case "telemetry":
                                isTelemetry = true;
                                break;
                            default:
                                throw new ArgumentOutOfRangeException("iotpnp coloring should be '@iotpnp(item,item,...)'. item should be 'deviceid'|'readonly'|'exclude'|'telemetry' ");
                        }
                    }
                }
            }
        }
        public void ChangeMode(bool isIoTPnP)
        {
            this.isCurrentIoTPnP = isIoTPnP;
        }
        public bool IsDeviceId { get { return isDeviceId; } }
        public bool IsReadOnly { get { return isReadOnly; } }
        public bool IsExclude { get { return isExclude; } }
        public bool IsTelemetry { get { return isTelemetry; } }

        public bool IsCurrentIoTPnP { get { return isCurrentIoTPnP; } }
    }
}
