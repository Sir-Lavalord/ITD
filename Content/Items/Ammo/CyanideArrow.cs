using ITD.Content.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Items.Ammo
{
    public class CyanideArrow : ModItem
    {
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }

        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 36;

            Item.damage = 18;
            Item.DamageType = DamageClass.Ranged;

            Item.maxStack = Item.CommonMaxStack;
            Item.consumable = true;
            Item.knockBack = 3.75f;
            Item.value = Item.sellPrice(copper: 24);
            Item.shoot = ModContent.ProjectileType<Content.Projectiles.Friendly.Ranger.Ammo.CyanideArrow>();
            Item.shootSpeed = 5.25f;
            Item.ammo = AmmoID.Arrow;
        }
    }
}