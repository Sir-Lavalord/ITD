﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using System;
using ITD.Systems;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
namespace ITD.Content.Items.Other
{
    public class EoCInsight : ModItem
    {
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemNoGravity[Type] = true;
            Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
        {
            Item.Size = new Vector2(30);
            Item.master = true;
        }
        public override void PostUpdate()
        {
            Item.Center += Main.rand.NextVector2Circular(1, 1);
            Lighting.AddLight(Item.Center, Color.Turquoise.ToVector3() * 0.5f);
        }
        public override void UpdateInventory(Player player)
        {
            if (Item.favorited)
            {
                player.GetModPlayer<InsightedPlayer>().CorporateInsight = true;
            }
            else
            {
                player.GetModPlayer<InsightedPlayer>().CorporateInsight = false;
            }
            }
        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
    public class InsightedPlayer : ModPlayer
    {
        public bool CorporateInsight;
    }
}