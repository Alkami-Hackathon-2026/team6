#if NET6_0_OR_GREATER
using CoreWCF;
using CoreWCF.Description;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;

namespace Alkami.Services.Subscriptions.ParticipatingService
{
    /// <summary>
    /// 
    /// </summary>
    public class AlkamiDataContractSerializerOperationBehavior : DataContractSerializerOperationBehavior
    {
        /// <inheritdoc/>
        public AlkamiDataContractSerializerOperationBehavior(OperationDescription operation) : base(operation)
        {
        }

        /// <inheritdoc/>
        public AlkamiDataContractSerializerOperationBehavior(OperationDescription operation, DataContractFormatAttribute dataContractFormatAttribute) : base(operation, dataContractFormatAttribute)
        {
        }

        /// <inheritdoc/>
        public AlkamiDataContractSerializerOperationBehavior(OperationDescription operation, DataContractFormatAttribute dataContractFormatAttribute, bool builtInOperationBehavior) : base(operation, dataContractFormatAttribute, builtInOperationBehavior)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public ISerializationSurrogateProvider SerializationSurrogateProvider { get; set; }

        /// <inheritdoc/>
        public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
        {
            XmlDictionary dictionary = new XmlDictionary(2);
            DataContractSerializerSettings settings = new()
            {
                RootName = dictionary.Add(name),
                RootNamespace = dictionary.Add(ns),
                KnownTypes = knownTypes,
                MaxItemsInObjectGraph = MaxItemsInObjectGraph,
                DataContractResolver = DataContractResolver
            };
            DataContractSerializer dcs = new(type, settings);
            dcs.SetSerializationSurrogateProvider(SerializationSurrogateProvider);
            return dcs;
        }

        /// <inheritdoc/>
        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
        {
            DataContractSerializerSettings settings = new()
            {
                RootName = name,
                RootNamespace = ns,
                KnownTypes = knownTypes,
                MaxItemsInObjectGraph = MaxItemsInObjectGraph,
                DataContractResolver = DataContractResolver
            };
            DataContractSerializer dcs = new(type, settings);
            dcs.SetSerializationSurrogateProvider(SerializationSurrogateProvider);
            return dcs;
        }
    }
}
#endif