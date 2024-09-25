using ITD.Content.Buffs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Ranger.Ammo
{
    public class FrostichorArrow : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.FiresFewerFromDaedalusStormbow[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8; 
            Projectile.height = 8; 
            Projectile.friendly = true; 
            Projectile.hostile = false; 
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 1200;
            Projectile.alpha = 255;
            Projectile.light = 0.5f; 
            Projectile.ignoreWater = true; 
            Projectile.tileCollide = true; 
            Projectile.extraUpdates = 0; 
            
            AIType = ProjectileID.WoodenArrowFriendly;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            for (int i = 0; i < 5; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Ice);
                dust.noGravity = true;
                dust.velocity *= 1.5f;
                dust.scale *= 0.9f;
            }

            Vector2 position = Projectile.Center;
            Vector2 velocity = Vector2.Zero;
            int type = ModContent.ProjectileType<Content.Projectiles.Friendly.Misc.CyaniteIceShard>();
            int damage = 6;
            float ai0 = 0f;

            Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), position, velocity, type, damage, ai0);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Ichor, 10);
        }
    }
}