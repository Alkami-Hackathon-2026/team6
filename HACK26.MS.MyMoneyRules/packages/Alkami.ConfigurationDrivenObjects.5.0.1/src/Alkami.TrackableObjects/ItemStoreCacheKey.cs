using System;

namespace Alkami.TrackableObjects
{
    internal class ItemStoreCacheKey
    {
        public Guid BankIdentifier { get; set; }

        public string ItemType { get; set; }

        public string ItemName { get; set; }

        public override int GetHashCode()
        {
            var hash = 13;
            hash = (hash * 7) + BankIdentifier.GetHashCode();
            hash = (hash * 7) + ItemType.GetHashCode();
            hash = (hash * 7) + ItemName.GetHashCode();

            return hash;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ItemStoreCacheKey;
            return other != null && other.ItemType == ItemType && other.BankIdentifier == BankIdentifier && other.ItemName == ItemName;
        }

        public override string ToString()
        {
            return BankIdentifier + ":" + ItemType + ":" + ItemName;
        }

        public static ItemStoreCacheKey FromString(string itemStoreKey)
        {
            var split = itemStoreKey.Split();

            var bankIdentifer = Guid.Parse(split[0]);

            return new ItemStoreCacheKey
            {
                BankIdentifier = bankIdentifer,
                ItemType = split[1],
                ItemName = split[2]
            };
        }
    }
}
