using ITD.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using ITD.Particles;
using ITD.Particles.Testing;

namespace ITD.Systems.Recruitment.Behaviors
{
    public class Merchant : RecruitBehavior
    {
        public override int NPCType => NPCID.Merchant;
        public override void Run(NPC npc, Player player)
        {
            Vector2 targetPoint = player.Center - new Vector2(player.direction * 48f, 0f);
            if (npc.DistanceSQ(targetPoint) < 10f)
                npc.velocity.X = 0;
            else
                npc.velocity.X = (targetPoint - npc.Center).SafeNormalize(Vector2.Zero).X * 2f;
            npc.spriteDirection = npc.direction = (targetPoint - npc.Center).X > 0 ? 1 : -1;
            NPCHelpers.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height);
            if (npc.IsOnStandableGround() && targetPoint.Y - npc.Center.Y < -6f)
            {
                npc.velocity.Y -= 6f;
            }
            NPC min = Main.npc[0];
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.DistanceSQ(npc.Center) < min.DistanceSQ(npc.Center) && n.CanBeChasedBy()) min = n;
            }
            if (min.active && min.whoAmI != npc.whoAmI)
            {
                if (AITimer++ > 64)
                {
                    AITimer = 0;
                    Attack(npc, min);
                }
            }
        }
        public override void Attack(NPC npc, NPC other)
        {
            attacking = true;
            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center + npc.velocity, npc.DirectionTo(other.Center) * 8f, ProjectileID.ThrowingKnife, 10, 0.1f);
        }
    }
}
