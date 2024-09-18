using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace ITD.Content.Projectiles.Friendly.Misc
{
    public class BloodPactSpirit : ModProjectile
    {
        public ref float AITimer => ref Projectile.ai[1];
        public override void SetStaticDefaults()
        {
            //Main.projFrames[Projectile.type] = 15;
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 56;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 150;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Generic;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.timeLeft = (int)Projectile.ai[0];
        }
        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            float speed = 16f;
            float range = 500f;
            Vector2 toPlayer = (player.Center + new Vector2(0f, -100f) - Projectile.Center).SafeNormalize(Vector2.Zero);
            Projectile.velocity = Vector2.SmoothStep(Projectile.velocity, toPlayer * speed, 0.1f);
            // this gets the valid NPCs that the summon can target
            IEnumerable<NPC> targets = Main.npc.Where(npc => npc.active && !npc.friendly && Projectile.DistanceSQ(npc.Center) < range * range);
            if (targets.Any())
            {
                AITimer++;
                if (AITimer >= 20f)
                {
                    AITimer = 0;
                    foreach (NPC target in targets)
                    {
                        Vector2 towards = (target.Center - Projectile.Center);
                        Vector2 towardsNormalized = towards.SafeNormalize(Vector2.Zero);
                        int slashDamage = (int)(Projectile.ai[0] / 2f);
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, towardsNormalized * (towards.Length() / 16f), ModContent.ProjectileType<BloodPactCut>(), slashDamage, 0f, player.whoAmI);
                    }
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int j = 0; j < 20; ++j)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Bone, 0, 0, 0, default, 1f);
            }
        }
    }
}