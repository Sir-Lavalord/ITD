﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using ITD.Content.Dusts;
using ITD.Content.NPCs.Bosses;
using Terraria.DataStructures;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;

namespace ITD.Content.Projectiles.Hostile
{
    public class CosmicLightningOrb : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_465";

        Vector2 vToCosJel;
        Vector2 vLockedIn;

        public override void OnSpawn(IEntitySource source)
        {
        }
                
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.CultistBossLightningOrb];
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }
        public bool bLocked;
        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.aiStyle = 0;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.light = 1f;
            Projectile.tileCollide = true;
            Projectile.timeLeft = 1200;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

        }
        public override void AI()
        {

            NPC CosJel = Main.npc[(int)Projectile.ai[0]];
            if (CosJel.active && CosJel.type == ModContent.NPCType<CosmicJellyfish>())
            {
                switch (Projectile.ai[1])
                {
                    case 0:
                        vToCosJel = new Vector2(CosJel.Center.X, CosJel.Center.Y - 150);
                        break;
                    case 1:
                        vToCosJel = new Vector2(CosJel.Center.X - 150, CosJel.Center.Y + 0);
                        break;
                    case 2:
                        vToCosJel = new Vector2(CosJel.Center.X + 150, CosJel.Center.Y + 0);
                        break;
                }
                Player player = Main.player[CosJel.target];

                if (CosJel.ai[3] == 6 && CosJel.HasPlayerTarget)
                {
                    Vector2 normalCenter = vToCosJel + new Vector2(0f, CosJel.velocity.Y);
                    Projectile.Center = Vector2.Lerp(Projectile.Center, normalCenter, 0.3f);

                    if (Projectile.ai[2]++ >= 50)
                    {
                        Projectile.ai[2] = 0;
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings { PositionInWorld = Projectile.Center }, Projectile.owner);
                            Vector2 vel = Projectile.DirectionTo(player.Center) * 12f;
                            int projID = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vel,
                             ProjectileID.MartianTurretBolt, CosJel.damage, 0f, -1, 240, CosJel.whoAmI);
                        }
                    }
                }
                else if (player.dead || !player.active)
                {
                    Projectile.Kill();
                }
                else if (CosJel.ai[3] != 6)
                {
                    if (!bLocked)
                    {
                        bLocked = true;
                        Projectile.velocity = Projectile.DirectionTo(player.Center) * 12f;
                    }
                }
            }
        }
        
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.position = Projectile.oldPos[0];
            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return Color.White;
        }
    }
}
