using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Terraria.DataStructures;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;

using ITD.Content.Items.Placeable;
using ITD.Utilities;
using ITD.Content.Projectiles.Friendly.Mage;
using ITD.Content.Dusts;
using ITD.Content.Projectiles.Friendly.Melee.Snaptraps.Extra;
using ITD.Content.Projectiles.Friendly;
using System;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;


namespace ITD.Content.Items.Weapons.Mage
{
    public class WeepingWand : ModItem
    {
        public override void SetStaticDefaults()
        {
Item.ResearchUnlockCount = 1;
        }
        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.DamageType = DamageClass.Magic;
            Item.width = 38;
            Item.height = 38;
            Item.scale = 1.1f;
            Item.maxStack = 1;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item71;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.useTurn = true;
            Item.useStyle = 5;
            Item.value = Item.buyPrice(silver: 25);
            Item.rare = 5;
            Item.shoot = ModContent.ProjectileType<WeepWandWisp>();
            Item.shootSpeed = 5f;
            Item.mana = 11;
        }
    }
}