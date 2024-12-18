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
    public class StarlightStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            HeldItemLayer.RegisterData(Item.type, new DrawLayerData()
            {
                Texture = ModContent.Request<Texture2D>(Texture + "_Glow"),
                Color = () => Color.White * 0.7f
            });
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;

            Item.staff[Type] = true;
        }
        //No Glowmask Yet
        public override void SetDefaults()
        {
            //Change later
            Item.DamageType = DamageClass.Magic;
            Item.damage = 16;
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
            Item.shoot = ModContent.ProjectileType<StarlightStaffProj>();
            Item.shootSpeed = 8f;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D texture = (Texture2D)Mod.Assets.Request<Texture2D>("Content/Items/Weapons/Mage/StarlightStaff_Glow");

            spriteBatch.Draw(texture, new Vector2(Item.position.X - Main.screenPosition.X + Item.width * 0.5f, Item.position.Y - Main.screenPosition.Y + Item.height - texture.Height * 0.5f),
                new Rectangle(0, 0, texture.Width, texture.Height), Color.White * 0.7f, rotation, texture.Size() * 0.5f, scale, SpriteEffects.None, 0f);
        }
        public override void AddRecipes()
        {
            CreateRecipe(1)
                .AddIngredient(ItemID.FallenStar, 5)
                .AddIngredient(ItemID.MeteoriteBar, 3)
                .AddIngredient(ModContent.ItemType<StarlitBar>(), 6)
                .AddTile(TileID.Anvils)
                .Register();
        }
    }
}
