using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json.Linq;

namespace Cognex.InSight.Web
{
    public class CvsCameraInfo
    {
        private JToken _cameraInfo;

        /// <summary>
        /// Constructs a new CvsCameraInfo object to provide specific field from the CameraInfo.
        /// </summary>
        /// <param name="cameraInfo">The camera info for the connection</param>
        internal CvsCameraInfo(JToken cameraInfo)
        {
            _cameraInfo = cameraInfo;
        }

        public string HostName
        {
            get { return (_cameraInfo.SelectToken("name") ?? "").ToString(); }
        }

        public string IPAddress
        {
            get { return (_cameraInfo.SelectToken("ipAddress") ?? "").ToString(); }
        }

        public string FirmwareVersion
        {
            get { return (_cameraInfo.SelectToken("firmwareVersion") ?? "").ToString(); }
        }

        public string ApiVersion
        {
            get { return (_cameraInfo.SelectToken("hmiProtocolVersion") ?? "").ToString(); }
        }

        public string ModelNumber
        {
            get { return (_cameraInfo.SelectToken("model") ?? "").ToString(); }
        }

        public string MacAddress
        {
            get { return (_cameraInfo.SelectToken("macID") ?? "").ToString(); }
        }

        public string SerialNumber
        {
            get { return (_cameraInfo.SelectToken("serial") ?? "").ToString(); }
        }

        public string[] Capabilities
        {
            get
            {
                JArray arr = _cameraInfo.SelectToken("capabilities") as JArray;
                if (arr != null)
                {
                    return arr.ToObject<string[]>();
                }
                else
                {
                    return new string[0];
                }
            }
        }

        public bool IsColor
        {
            get
            {
                JToken token = _cameraInfo.SelectToken("acq.isColor");
                if (token != null)
                    return Convert.ToBoolean(token);
                else
                    return false;
            }
        }
    }
}
