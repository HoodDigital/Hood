using Hood.Core;
using Hood.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hood.Models.Payments
{
    public class SagePayCredentials
    {
        public string Endpoint { get; set; }
        public string Key { get; set; }
        public string Password { get; set; }
        public string VendorName { get; set; }

        private const string sagePayEndpointSandbox = "https://pi-test.sagepay.com/api/v1";
        private const string vendorNameSandbox = "sandbox";
        private const string integrationKeySandbox = "hJYxsw7HLbj40cB8udES8CDRFLhuJ8G54O6rDpUXvE6hYDrria";
        private const string integrationPasswordSandbox = "o2iHSrFybYMZpmWOQMuhsXP52V4fBtpuSDshrKDSWsBY1OiN6hwd9Kb12z4j5Us5u";

        public static SagePayCredentials GetCredentials(string forceMode = null)
        {
            var billingSettings = Engine.Settings.Billing;
            if (forceMode == "Sandbox")
                return new SagePayCredentials()
                {
                    Endpoint = sagePayEndpointSandbox,
                    VendorName = vendorNameSandbox,
                    Key = integrationKeySandbox,
                    Password = integrationPasswordSandbox
                };
            else if (forceMode == "Testing" || billingSettings.SagePayMode == "Testing")
                return new SagePayCredentials()
                {
                    Endpoint = billingSettings.SagePayTestingEndpoint,
                    VendorName = billingSettings.SagePayTestingVendorName,
                    Key = billingSettings.SagePayTestingKey,
                    Password = billingSettings.SagePayTestingPassword
                };
            else if (billingSettings.SagePayMode == "Sandbox")
                return new SagePayCredentials()
                {
                    Endpoint = sagePayEndpointSandbox,
                    VendorName = vendorNameSandbox,
                    Key = integrationKeySandbox,
                    Password = integrationPasswordSandbox
                };
            else
                return new SagePayCredentials()
                {
                    Endpoint = billingSettings.SagePayEndpoint,
                    VendorName = billingSettings.SagePayVendorName,
                    Key = billingSettings.SagePayKey,
                    Password = billingSettings.SagePayPassword
                };
        }
    }
}
