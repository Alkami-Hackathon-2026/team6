using System;
using System.Runtime.Serialization;

namespace Alkami.MicroServices.Settings.ProviderBased.Contracts
{
    /// <summary>
    /// A class that contains information about the settings.
    /// </summary>
    [DataContract(IsReference=true)]
    public class SettingDescriptor
    {
        /// <summary>
        /// The only way to create a descriptor
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <param name="description">the description of the setting</param>
        /// <param name="type">the type of the setting</param>
        /// <param name="isRequired">Is the setting</param>
        /// <param name="displayName">An optional Parameter specified</param>
        /// <param name="isEnvironmentSpecific"></param>
        public SettingDescriptor(string name, string description, string type, bool isRequired,
            string displayName = null, bool isEnvironmentSpecific = false)
        {
            Name = name;
            Description = description;
            Type = type;
            IsRequired = isRequired;
            DisplayName = string.IsNullOrEmpty(displayName) ? name : displayName;
            IsEnvironmentSpecific = isEnvironmentSpecific;
        }

        /// <summary>
        /// The only way to create a descriptor
        /// </summary>
        /// <param name="name">The name of the setting</param>
        /// <param name="description">the description of the setting</param>
        /// <param name="type">the type of the setting</param>
        /// <param name="isRequired">Is the setting</param>
        /// <param name="displayName">An optional Parameter specified</param>
        /// <param name="isEnvironmentSpecific"></param>
        public SettingDescriptor(string name, string description, Type type, bool isRequired,
            string displayName = null, bool isEnvironmentSpecific = false)
        {
            Name = name;
            Description = description;
            Type = type.FullName;
            IsRequired = isRequired;
            DisplayName = string.IsNullOrEmpty(displayName) ? name : displayName;
            IsEnvironmentSpecific = isEnvironmentSpecific;
        }

        public SettingDescriptor() { }

        /// <summary>
        /// The Setting Name. This correlates to the Name in the DB!
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        
        /// <summary>
        /// The setting description
        /// </summary>
        [DataMember]
        public string Description { get; set; }
        
        /// <summary>
        /// The type of the setting
        /// </summary>
        [DataMember]
        public string Type { get; set; }
        
        /// <summary>
        /// Is the Setting Required
        /// </summary>
        [DataMember]
        public bool IsRequired { get; set; }

        /// <summary>
        /// The friendly name this name is displayed to the end user if set instead of the Name
        /// </summary>
        [DataMember]
        public string DisplayName { get; set; }

        /// <summary>
        /// Is this setting environment specific? If set to true this means that this setting will frequently differe between environments (Eg: Dev, Staging Production)
        /// </summary>
        [DataMember]
        public bool IsEnvironmentSpecific { get; set; }
    }
}