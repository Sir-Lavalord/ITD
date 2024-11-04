using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ITD.Content.Projectiles.Friendly.Misc;
using Terraria.DataStructures;

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
        public override void ResetEffects()
        {
            Active = false;
        }
        public override void PostUpdateEquips()
        {
            if (Active)
            {
                if (Player.ownedProjectileCounts[ModContent.ProjectileType<GalacticJellyBeanHand>()] <= 0)
                {
                    int projID = Projectile.NewProjectile(Player.GetSource_FromThis(),Player.Center,Vector2.Zero,
                        ModContent.ProjectileType<GalacticJellyBeanHand>(),(int)(Player.GetDamage(DamageClass.Generic).ApplyTo(50)),10f,Player.whoAmI);//knock yo clock off
                    Main.projectile[projID].scale = 1f;
                }
            }
        }
    }
}
