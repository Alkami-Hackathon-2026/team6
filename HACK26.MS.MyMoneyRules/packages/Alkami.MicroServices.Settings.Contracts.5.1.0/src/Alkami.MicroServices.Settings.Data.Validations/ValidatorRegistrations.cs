using Alkami.Data.Validations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Alkami.MicroServices.Settings.Data.Validations
{
    /// <summary>
    /// 
    /// </summary>
    public static class ValidatorRegistrations
    {
        /// <summary>
        /// Finds and registers all <see cref="EntityValidator"/>
        /// </summary>
        public static void RegisterAvailableValidators()
        {
            var registerMethod = typeof(EntityValidator).GetMethod("AddValidator", BindingFlags.Static | BindingFlags.Public);

            foreach (var type in GetValidatorTypes())
            {
                var entityValidatorInstance = Activator.CreateInstance(type);
                var specificMethod = registerMethod.MakeGenericMethod(type.BaseType.GetGenericArguments().First());
                specificMethod.Invoke(null, new[] { entityValidatorInstance });
            }
        }

        private static IEnumerable<Type> GetValidatorTypes()
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => type.BaseType != null)
                .Where(type => type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(EntityValidatorImpl<>));
        }
    }
}
