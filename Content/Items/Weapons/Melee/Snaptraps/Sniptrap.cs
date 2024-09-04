﻿using ITD.Content.Projectiles.Friendly.Melee.Snaptraps;
using ITD.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Weapons.Melee.Snaptraps
{
    public class Sniptrap : ModItem
    {
        public override void SetDefaults()
        {
            Item.autoReuse = false;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = Item.useTime = 22;
            Item.knockBack = 0f;
            Item.width = 30;
            Item.height = 10;
            Item.damage = 10;
            Item.shoot = ModContent.ProjectileType<SniptrapProjectile>();
            Item.shootSpeed = 12f;
            Item.UseSound = SoundID.Item1;
            Item.rare = ItemRarityID.White;
            Item.value = Item.sellPrice(0, 0, 25);
            Item.DamageType = DamageClass.Melee;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
        }
        public override bool CanUseItem(Player player)
        {
            return MiscHelpers.SnaptrapUseCondition(player.whoAmI);
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.IronBar, 4);
            recipe.AddIngredient(ItemID.Chain, 8);
            recipe.AddIngredient(ItemID.Hook, 4);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
}