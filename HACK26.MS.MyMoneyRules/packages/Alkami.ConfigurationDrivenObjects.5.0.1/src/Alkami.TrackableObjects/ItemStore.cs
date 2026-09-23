using Alkami.Broker.App;
using Alkami.MicroServices.Settings.Data;
using Common.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Alkami.Utilities.Extensions;
using System.Runtime.Caching;

namespace Alkami.TrackableObjects
{
    internal static class ItemStore
    {


        private const string KeyFormat = "{0}|{1}|{2}";
        private const string KeyPrefix = "ItemStore";
        private static readonly MemoryCache InnerStore = new MemoryCache("InnerStoreCache");
        /// <summary>
        /// This is an instance guid
        /// </summary>
        private static readonly Guid InstanceGuid;

        private static readonly ILog Logger = LogManager.GetLogger("Alkami.Settings.ItemStore");

        static ItemStore()
        {
            InstanceGuid = Guid.NewGuid();
            Subscription.Add(Events.TrackableEntityChangeCommited, objects =>
            {
                HandleTrackableEntityChangeCommitedEvent(objects);
            });
        }

        internal static void HandleTrackableEntityChangeCommitedEvent(Dictionary<string, string> objects)
        {

            Logger.DebugFormat("TrackableEnvitytChangeCommited event received...");
            string bankIdentifier;
            Guid bankIdentifierGuid;

            if (objects.TryGetValue(Broker.Base.ReservedKeyNames.BankIdentifier, out bankIdentifier) && Guid.TryParse(bankIdentifier, out bankIdentifierGuid))
            {
                Logger.DebugFormat($"External cache removed event received for {bankIdentifier}...");

                InternalRemove(objects, bankIdentifierGuid);
            }

        }

        public static void AddToCache(ItemStoreCacheKey itemStoreCacheKey, Item item)
        {
            var cacheItem = new CacheItem(itemStoreCacheKey.ToString(), item);
            InnerStore.Set(cacheItem, GetPolicy());
        }

        public static Item FromCache(ItemStoreCacheKey itemStoreCacheKey)
        {

            var value = (Item)InnerStore.Get(itemStoreCacheKey.ToString());
            return value;
        }

        private static void InternalRemove(Dictionary<string, string> args, Guid bankIdentifier)
        {
            if (args["ItemType"] == "BankSettings")
            {
                var keys = InnerStore.Where(x => ((Item)(x.Value))?.ItemType == "BankSettings");
                keys.ForEach(key => InnerStore.Remove(key.Key));
                return;
            }

            var cacheToRemove = InnerStore.FirstOrDefault(x => x.Key.Contains(bankIdentifier.ToString()) && ((Item)(x.Value))?.Id.ToString() == args["ItemId"]);
            if (cacheToRemove.Value != null)
            {
                InnerStore.Remove(cacheToRemove.Key);
            }

            cacheToRemove = InnerStore.FirstOrDefault(x => x.Key.Contains(bankIdentifier.ToString()) && ((Item)(x.Value))?.ParentId.ToString() == args["ParentId"]);
            if (cacheToRemove.Value != null)
            {
                InnerStore.Remove(cacheToRemove.Key);
            }
        }



        public static void ClearAll()
        {
            InnerStore.ForEach(x => InnerStore.Remove(x.Key));
        }

        private static CacheItemPolicy GetPolicy()
        {
            var policy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTime.Now.AddMinutes(10),  //set your refresh interval
            };

            return policy;
        }


    }
}
