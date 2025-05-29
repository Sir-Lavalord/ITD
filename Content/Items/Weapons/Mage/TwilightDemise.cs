using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Creative;
using ITD.Content.Projectiles.Friendly.Mage;
using ITD.Systems;
using Microsoft.Xna.Framework.Graphics;
using ITD.Content.Items.Materials;

namespace ITD.Content.Items.Weapons.Mage
{
    public class TwilightDemise : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 1;

            Item.staff[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.DamageType = DamageClass.Magic;
            Item.damage = 50;
            Item.mana = 8;
            Item.width = 56;
            Item.height = 56;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1f;
            Item.value = Item.buyPrice(silver: 10);
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item43;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<TwilightDemiseProj>();
            Item.shootSpeed = 10f;
        }
    }
}
