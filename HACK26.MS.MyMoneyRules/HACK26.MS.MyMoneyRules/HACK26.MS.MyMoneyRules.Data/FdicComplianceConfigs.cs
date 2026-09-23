using System.Runtime.Serialization;

namespace HACK26.MS.MyMoneyRules.Data
{
    [DataContract(IsReference = true)]
    public class FdicComplianceConfigs
    {
        [DataMember]
        public Desktop Desktop { get; set; }
        [DataMember]
        public Mobile Mobile { get; set; }
        [DataMember]
        public Pages Pages { get; set; }
    }

    [DataContract(IsReference = true)]
    public class Desktop
    {
        [DataMember]
        public Header Header { get; set; }
        [DataMember]
        public Footer Footer { get; set; }
        [DataMember]
        public Login Login { get; set; }
    }

    [DataContract(IsReference = true)]
    public class Mobile
    {
        [DataMember]
        public Footer Footer { get; set; }
        [DataMember]
        public Login Login { get; set; }
    }

    [DataContract(IsReference = true)]
    public class Pages
    {
        [DataMember]
        public bool IsEnabled { get; set; }
    }

    [DataContract(IsReference = true)]
    public class Header
    {
        [DataMember]
        public bool IsEnabled { get; set; }
    }

    [DataContract(IsReference = true)]
    public class Footer
    {
        [DataMember]
        public bool IsEnabled { get; set; }
    }

    [DataContract(IsReference = true)]
    public class Login
    {
        /// <summary>
        /// 
        /// </summary>
        [DataMember]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 
        /// </summary
        [DataMember]
        public string SignColor { get; set; }
    }
}

