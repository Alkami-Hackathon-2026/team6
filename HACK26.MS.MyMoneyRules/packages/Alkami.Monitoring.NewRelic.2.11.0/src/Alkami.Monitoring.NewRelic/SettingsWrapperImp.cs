using System;
using Alkami.Utilities.Configuration;

namespace Alkami.Monitoring
{
    /// <summary>
    /// Interface for GetSetting function that will return settings in config files.
    /// </summary> 
    public interface ISettingsWrapper
    {
        /// <summary>
        /// Finding the value for "name" setting from config files
        /// </summary>
        /// <param name="name">name of setting key</param>
        /// <returns>value of key</returns>
        string GetSetting(string name);

        /// <summary>
        /// Finding the value for "name" setting from Environment Variables
        /// </summary>
        /// <param name="name">name of setting key</param>
        /// <returns>value of key</returns>
        string? GetEnvironmentVariable(string name);
    }
    /// <summary>
    /// Class that implements ISettingsWrapper
    /// </summary>
    public class SettingsWrapperImp : ISettingsWrapper
    {
        /// <inheritdoc />
        public string? GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        /// <summary>
        /// Finding the value for "name" setting from config files
        /// </summary>
        /// <param name="name">name of setting key</param>
        /// <returns>value of key</returns>
        public string GetSetting(string name)
        {
            return Manager.GetSetting(name);
        }
    }
}
