using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using Utilities;

namespace SwordsAndSorcery_2020.Models
{
    public class PayPalConfig : ConfigurationSection
    {
        [ConfigurationProperty("paypal/settings")]
        public string mode
        {
            get
            {
                return this["mode"].StringSafe();
            }
            set
            {
                value = this["mode"].StringSafe();
            }
        }
        public string clientId
        {
            get
            {
                return this["clientId"].StringSafe();
            }
            set
            {
                value = this["clientId"].StringSafe();
            }
        }
        public string clientSecret
        {
            get
            {
                return this["clientSecret"].StringSafe();
            }
            set
            {
                value = this["clientSecret"].StringSafe();
            }
        }
    }
}