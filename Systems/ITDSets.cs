using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using System;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace ITD
{
    public class IDSet<T>(T defaultValue)
    {
        private readonly Dictionary<int, T> _storage = [];
        private readonly T _defaultValue = defaultValue;
        public T this[int id]
        {
            get
            {
                return _storage.TryGetValue(id, out T value) ? value : _defaultValue;
            }
            set
            {
                _storage[id] = value;
            }
        }
    }
    public class ITDSets
    {
        
        public static readonly IDSet<int> ToScrapeableMoss = CreateSet(-1);
        public static readonly IDSet<int> LeafGrowFX = CreateSet(GoreID.TreeLeaf_Normal);
        public static readonly IDSet<bool> SnowpoffDiggable = CreateBoolSet(TileID.SnowBlock);
        
        //public static int[] ToScrapeableMoss = TileID.Sets.Factory.CreateIntSet(-1);
        //public static int[] LeafGrowFX = TileID.Sets.Factory.CreateIntSet(GoreID.TreeLeaf_Normal);
        //public static bool[] SnowpoffDiggable = TileID.Sets.Factory.CreateBoolSet(TileID.SnowBlock);
        public static IDSet<T> CreateSet<T>(T defaultValue, int[] entries = default, T[] values = default)
        {
            IDSet<T> set = new(defaultValue);
            if (entries is null || values is null || entries.Length == 0)
                return set;
            if (entries.Length != values.Length)
                throw new Exception("Can't map the entries to their values as they are not equal in length");
            for (int i = 0; i < entries.Length; i++)
            {
                set[entries[i]] = values[i];
            }
            return set;
        }
        public static IDSet<bool> CreateBoolSet(bool defaultValue = false, params int[] entries)
        {
            IDSet<bool> set = new(defaultValue);
            if (entries.Length == 0)
                return set;
            for (int i = 0; i < entries.Length; i++)
            {
                set[entries[i]] = !defaultValue;
            }
            return set;
        }
        public static IDSet<bool> CreateBoolSet(params int[] entries) => CreateBoolSet(false, entries);
    }
}