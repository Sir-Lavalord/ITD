using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using ITD.Content.Tiles;
using ITD.Content.Buffs.GeneralBuffs;
using ITD.Content.Tiles.Misc;
using ITD.Content.Items.Placeable;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using ITD.Content.Buffs.Debuffs;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class PocketSyringeProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.manualDirectionChange = true;
            Projectile.scale = 0.66f;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            Projectile.velocity.Y = Projectile.velocity.Y + 0.25f; 
            if (Projectile.velocity.Y > 16f) 
            {
                Projectile.velocity.Y = 16f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);

            for (int i = 0; i < 10; i++)
            {
                Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Glass);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            if (player.active)
            {
                for (int i = 0; i < 20; i++)
                {
                    if (player.buffType[i] > 0)
                    {
                        int buffType = player.buffType[i];
                        int buffTime = player.buffTime[i];

                        if (buffType == BuffID.Bleeding)
                        {
                            buffType = ModContent.BuffType<BleedingII>();
                        }
                        target.AddBuff(buffType, buffTime);
                    }
                }
            }
        }

        private bool CanDebuffEnemies(int buffType)
        {
            switch (buffType)
            {
                case BuffID.Venom:
                case BuffID.Bleeding:
                case BuffID.Confused:
                case BuffID.CursedInferno:
                case BuffID.Frostburn:
                case BuffID.Frostburn2:
                case BuffID.OnFire:
                case BuffID.OnFire3:
                case BuffID.Ichor:
                case BuffID.Poisoned:
                case BuffID.ShadowFlame:
                case BuffID.Slimed:
                case BuffID.Stinky:
                case BuffID.Wet:
                    return true;
                default:
                    return false;
            }
        }
    }
}