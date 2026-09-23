using Alkami.MicroServices.Settings.Data;
using Common.Logging;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Alkami.TrackableObjects
{
    using static Utilities.Extensions.SettingReaderUtility;

    public class Settings
    {
        private static readonly ILog Logger = LogManager.GetLogger<Settings>();
        private readonly ReadOnlyDictionary<string, string> _settingsDictionary;
        public IDictionary<string, string> ReadonlySettings => _settingsDictionary;

        protected internal Settings(ConfigurationDrivenObject parent, Item instance)
        {
             var settingsDictionary = parent.SettingDescriptors()
                 .Select(x => x.Name)
                 .ToDictionary(x => x, y => string.Empty);

            foreach (var defaultSetting in parent.DefaultSettings())
            {
                settingsDictionary[defaultSetting.Key] = defaultSetting.Value;
                Logger.Debug(x => x($"Default setting = {defaultSetting.Key}:{defaultSetting.Value}"));
            }

            if (instance?.ItemSettings == null)
            {
                return;
            }

            foreach (var itemSetting in instance.ItemSettings)
            {
                settingsDictionary[itemSetting.Name] = itemSetting.Value;
            }

            _settingsDictionary = new ReadOnlyDictionary<string, string>(settingsDictionary);
        }

        ///  <summary>
        /// GetSettingOrDefault
        ///  </summary>
        ///  <typeparam name="T"></typeparam>
        ///  <param name="name"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public T GetSettingOrDefault<T>(string name, T defaultValue = default(T))
            => _settingsDictionary.GetSettingOrDefault(name, () => defaultValue);

        
    }
}
