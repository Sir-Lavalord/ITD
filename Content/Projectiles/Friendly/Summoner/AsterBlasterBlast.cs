using ITD.Content.Projectiles.Friendly.Misc;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using ITD.Content.Buffs.Debuffs;
using ITD.Players;
using ITD.Utilities;
using Microsoft.Build.Evaluation;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Microsoft.CodeAnalysis;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class AsterBlasterBlast : BigBlankExplosion
    {   
        public override int Lifetime => 30;
        public override Color GetCurrentExplosionColor(float pulseCompletionRatio) => Color.Lerp(Color.WhiteSmoke * 1.6f, Color.White, MathHelper.Clamp(pulseCompletionRatio * 2.2f, 0f, 1f));

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.DrawScreenCheckFluff[Projectile.type] = 1000;
        }
        public override string Texture => "ITD/Content/Projectiles/BlankTexture";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 2;
            Projectile.ignoreWater = true;
            Projectile.hostile = false;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = Lifetime;
        }
        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                SoundEngine.PlaySound(new SoundStyle("ITD/Content/Sounds/UltraLoading"), Projectile.Center);
            }
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];

                if (i != Projectile.whoAmI && other.aiStyle != -999 &&
                other.hostile &&
                
                other.active
                    && Math.Abs(Projectile.Center.X - other.position.X)
                    + Math.Abs(Projectile.Center.Y - other.position.Y) < CurrentRadius * 3)
                {
                    if (!Main.dedServ)
                    {
                        other.GetGlobalProjectile<FishbackerReflectedProj>().IsReflected = true;
                        other.owner = Main.myPlayer;
                        other.velocity.X *= -1f;
                        other.velocity.Y *= -1f;

                        ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.WallOfFleshGoatMountFlames, new ParticleOrchestraSettings
                        {
                            PositionInWorld = other.Center,
                        }, other.whoAmI);

                        other.friendly = true;
                        other.hostile = false;
                        other.damage *= 100;
                        other.netUpdate = true;
                    }
                }
            }
            base.AI();
        }
        public override void PostAI() => Lighting.AddLight(Projectile.Center, 0.2f, 0.1f, 0f);
    }
}