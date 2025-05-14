using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using ITD.Content.Buffs.Debuffs;
using ITD.Content.Projectiles.Friendly.Misc;
using ITD.Content.NPCs;
using ITD.Players;
using ITD.Utilities.Placeholders;
using Terraria.ID;
using ITD.Systems;
using ITD.Content.Items.Placeable;

namespace ITD.Content.Items.Accessories.Combat.All
{
    public class SheeringBane : ModItem
    {
        public override string Texture => Placeholder.PHAxe;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.DefaultToAccessory(28, 38);
            Item.rare = ItemRarityID.LightRed;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<WeaponEnchantmentPlayer>().flamingRazor = true;
            player.GetModPlayer<WeaponEnchantmentPlayer>().poisonFang = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<FlamingRazor>(), 1)
                .AddIngredient(ModContent.ItemType<PoisonFang>(), 1)
                .AddTile(TileID.TinkerersWorkbench)
                .Register();
        }
    }
}
