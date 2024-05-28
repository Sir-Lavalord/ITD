using ITD.Content.Tiles;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items
{
    public class Blueshroom : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.width = 48;
            Item.height = 50;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = 10000;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = false;
        }
        public override void SetDefaults()
        {
            Item.maxStack = Item.CommonMaxStack;
        }
        public override bool? UseItem(Player player)
        {
            int i = (int)Main.MouseWorld.X;
            int j = (int)Main.MouseWorld.Y;
            WorldGen.PlaceObject(i, j, ModContent.TileType<BlueshroomSapling>());
            WorldGen.GrowTree(i, j);
            return true;
        }
    }
}