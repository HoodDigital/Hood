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

        private static BillingSettings BillingSettings
        {
            get
            {
                var siteSettings = EngineContext.Current.Resolve<ISettingsRepository>();
                BillingSettings contentSettings = siteSettings.GetBillingSettings();
                return contentSettings;
            }
        }

        public static SagePayCredentials GetCredentials(string forceMode = null)
        {
            if (forceMode == "Sandbox")
                return new SagePayCredentials()
                {
                    Endpoint = sagePayEndpointSandbox,
                    VendorName = vendorNameSandbox,
                    Key = integrationKeySandbox,
                    Password = integrationPasswordSandbox
                };
            else if (forceMode == "Testing" || BillingSettings.SagePayMode == "Testing")
                return new SagePayCredentials()
                {
                    Endpoint = BillingSettings.SagePayTestingEndpoint,
                    VendorName = BillingSettings.SagePayTestingVendorName,
                    Key = BillingSettings.SagePayTestingKey,
                    Password = BillingSettings.SagePayTestingPassword
                };
            else if (BillingSettings.SagePayMode == "Sandbox")
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
                    Endpoint = BillingSettings.SagePayEndpoint,
                    VendorName = BillingSettings.SagePayVendorName,
                    Key = BillingSettings.SagePayKey,
                    Password = BillingSettings.SagePayPassword
                };
        }
    }
}
