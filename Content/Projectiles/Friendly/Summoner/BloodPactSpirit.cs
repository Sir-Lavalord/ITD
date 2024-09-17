using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ITD.Utilities;
using System.Collections.Generic;
using Terraria.DataStructures;
using FullSerializer.Internal;
using Mono.Cecil;
using ITD.Content.Buffs.SummonBuffs;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class BloodPactSpirit : ModProjectile
    {
        public NPC Target;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 15;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 56;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = -1;
            Projectile.timeLeft = 150;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;

            AIType = ProjectileID.ZephyrFish;
        }

        public override bool PreAI()
        {
            Player player = Main.player[Projectile.owner];

            if (Projectile.localAI[0] == 0f)
            {
                Projectile.ai[0] = Main.rand.Next(3);
                Projectile.netUpdate = true;
                Projectile.localAI[0] = 1f;
            }

            attackEnemies(Projectile.Center, Projectile.velocity, ModContent.ProjectileType<Content.Projectiles.Friendly.Misc.BloodPactCut>(), 50, 0f, player);
            player.zephyrfish = false;

            return true;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];

            if (!player.dead && player.HasBuff(ModContent.BuffType<Content.Buffs.SummonBuffs.BloodPactBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            for (int i = 0; i < 20; i++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Smoke, 0, 0, 0, default, 1f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int j = 0; j < 20; ++j)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 26, 0, 0, 0, default, 1f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Rectangle frame = new Rectangle((int)(Projectile.ai[0]) * Projectile.width, Projectile.frame * Projectile.height, Projectile.width, Projectile.height);
            SpriteEffects spriteEffects = (Projectile.spriteDirection == 1) ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Main.EntitySpriteDraw(Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, Projectile.Size / 2f, 1f, spriteEffects, 0);
            return false;
        }

        public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
        {
            if (Target != null)
                fallThrough = Projectile.Bottom.Y < Target.Top.Y;
            else
                fallThrough = false;
            return base.TileCollideStyle(ref width, ref height, ref fallThrough, ref hitboxCenterFrac);
        }

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        private void attackEnemies(Vector2 position, Vector2 velocity, int type, int damage, float knockback, Player player)
        {
            // Define the radius
            float radius = 100f; // Adjust the radius as needed
                                 // List to store NPC positions
            List<Vector2> enemyPositions = new List<Vector2>();

            // Iterate through NPCs
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy() && !npc.friendly) // Check if NPC is active and not friendly
                {
                    // Calculate distance from projectile to NPC
                    float distance = Vector2.Distance(npc.position, position);
                    if (distance <= radius)
                    {
                        // Store the NPC's position if within the radius
                        enemyPositions.Add(npc.position);
                    }
                }
            }

            // Use EntitySource_Parent for the player as the source
            IEntitySource source = player.GetSource_FromThis();

            foreach (var enemyPosition in enemyPositions)
            {
                // Optionally, provide a valid velocity for each projectile (e.g., towards the enemy or some preset direction)
                Vector2 newVelocity = velocity != Vector2.Zero ? velocity : Vector2.UnitY; // Change as needed

                Projectile.NewProjectile(source, enemyPosition, newVelocity, ModContent.ProjectileType<Content.Projectiles.Friendly.Misc.BloodPactCut>(), damage, knockback, player.whoAmI);
            }
        }
    }
}