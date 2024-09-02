using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ITD.Content.Projectiles.Friendly.Misc;

namespace ITD.Content.Items.Accessories.Expert
{
    public class GalacticJellyBean : ModItem
    {
        public override void SetDefaults()
        {
            Item.DefaultToAccessory(20);
            Item.expert = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.GetModPlayer<CosmicHandMinionPlayer>().Active = true;
        }
    }
    public class CosmicHandMinionPlayer : ModPlayer
    {
        public bool Active;
        private int proj = -1;
        public override void ResetEffects()
        {
            Active = false;
        }
        public override void PostUpdateEquips()
        {
            if (Active)
            {
                if (proj == -1)
                {
                    proj = Projectile.NewProjectile(
                        Player.GetSource_FromThis(),
                        Player.Center,
                        Vector2.Zero,
                        ModContent.ProjectileType<GalacticJellyBeanHand>(),
                        20,
                        0f,
                        Player.whoAmI);
                }
                else
                {
                    Projectile projectile = Main.projectile[proj];
                    if (projectile.active && projectile.type == ModContent.ProjectileType<GalacticJellyBeanHand>())
                    {
                        //Vector2 offset = new Vector2(-16f, -16f);
                        //projectile.Center = Player.Center + Player.velocity + offset;
                        projectile.timeLeft = 2;
                    }
                    else
                    {
                        proj = -1;
                    }
                }
            }
        }
    }
}
