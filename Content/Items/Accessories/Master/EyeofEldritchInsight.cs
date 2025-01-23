using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using Terraria.DataStructures;
using System;
using ITD.Systems;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
namespace ITD.Content.Items.Accessories.Master
{
    public class EyeofEldritchInsight : ModItem
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
        public override void PostUpdate()
        {
            if (CorporateInsight)
            {
            }
        }
    }
    public class InsightedProjectiles : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
/*        public override bool PreDraw(Projectile projectile, ref Color lightColor)
        {
            for (int i = 0; i < 1000; i++)
            {
                for (int f = 0; f < Main.maxPlayers; f++)
                {
                    if (Main.player[f].GetModPlayer<InsightedPlayer>().CorporateInsight)
                    {
                        if (projectile.active)
                        {
                            Rectangle hitbox = projectile.getRect();
                            ProjectileLoader.ModifyDamageHitbox(projectile, ref hitbox);
                            hitbox.Offset((int)-Main.screenPosition.X, (int)-Main.screenPosition.Y);
                            hitbox = Main.ReverseGravitySupport(hitbox);
                            if (projectile.hostile)
                            {
                                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, hitbox, Color.DarkRed * 0.8f);
                            }
                            else if (projectile.friendly)
                                Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, hitbox, Color.LimeGreen * 0.8f);
                        }
                    }
                }
            }
            return true;
        }*/
    }
}