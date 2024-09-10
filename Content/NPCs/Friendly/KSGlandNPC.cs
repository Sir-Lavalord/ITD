using ITD.Content.Items.Accessories.Expert;
using ITD.Content.Items.Accessories.Master;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ITD.Content.NPCs.Friendly
{
    public class KSGlandNPC : ModNPC
    {
        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 3;
            NPCID.Sets.DontDoHardmodeScaling[Type] = true;
            NPCID.Sets.ImmuneToAllBuffs[Type] = true;
            NPCID.Sets.NeedsExpertScaling[Type] = true;
            //
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
        }
        public override void SetDefaults()
        {
            NPC.width = 40;
            NPC.height = 56;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 50;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.knockBackResist = 0.3f;
            NPC.netAlways = true;
            NPC.friendly = true;
            NPC.scale = 1.25f;
            NPC.aiStyle = -1;
            NPC.gfxOffY = -10;
            NPC.alpha = 30;
        }
        public override bool CanHitPlayer(Player target, ref int cooldownSlot)
        {
            cooldownSlot = ImmunityCooldownID.Bosses; // use the boss immunity cooldown counter, to prevent ignoring boss attacks by taking damage from other sources
            return false;
        }
        int iRetardedIframe;
        public override bool CanBeHitByNPC(NPC attacker)
        {
                return iRetardedIframe <= 0;
        }
        public override bool? CanBeHitByProjectile(Projectile projectile)
        {
            if (projectile.hostile && !projectile.friendly)
                return iRetardedIframe <= 0;
            else return false;
        }
        public override void AI()
        {
            //Closest target is not good enough
            Player player = Main.player[(int)NPC.ai[0]];
            if (!CheckActive(player))
            {
                return;
            }
            if (!player.GetModPlayer<KSGlandPlayer>().ksMasterAcc)
            {
                player.GetModPlayer<KSGlandPlayer>().RegrowCD = 900;
                NPC.checkDead();
                NPC.life = 0;
            }
            NPC.spriteDirection =NPC.direction= -player.direction;
            iRetardedIframe = player.immuneTime;
            NPC.Center = player.Center;
            player.AddBuff(BuffID.Slimed, 5);
            NPC.netUpdate = true;
            
        }
        int iHitDamage;
        public override void HitEffect(NPC.HitInfo hit)
        {
            Player player = Main.player[(int)NPC.ai[0]];
            player.Hurt(PlayerDeathReason.ByCustomReason(player.name +
                " was crushed by the aftershock"), (int)(hit.Damage),0);
            player.immune = true;
            player.immuneTime = 60;
        }
        public override void ModifyHitByProjectile(Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            projectile.damage = (int)(projectile.damage/2);
        }
        public override void OnSpawn(IEntitySource source)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, DustID.t_Slime, 0, 0f, 40, Color.LightSkyBlue, Main.rand.NextFloat(1f, 2f));
                dust.velocity *= 2f;
                Dust dust2 = Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, DustID.t_Slime, 0, 0f, 40, Color.LightSkyBlue, Main.rand.NextFloat(1f, 2f));
                dust2.velocity *= 1f;
                dust2.noGravity = true;
            }
        }
        public override void OnKill()
        {
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, DustID.t_Slime, 0, 0f, 40, Color.LightSkyBlue, Main.rand.NextFloat(1f, 2f));
                dust.velocity *= 2f;
                Dust dust2 = Dust.NewDustDirect(NPC.Center, NPC.width, NPC.height, DustID.t_Slime, 0, 0f, 40, Color.LightSkyBlue, Main.rand.NextFloat(1f, 2f));
                dust2.velocity *= 1f;
                dust2.noGravity = true;
            }
            Player player = Main.player[(int)NPC.ai[0]];
            player.GetModPlayer<KSGlandPlayer>().RegrowCD = 900;
        }
        public override void DrawBehind(int index)
        {
            Main.instance.DrawCacheNPCsOverPlayers.Add(index);
        }
        public override void OnHitByProjectile(Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            NPC.immune[projectile.owner] = 120;
        }
        public override void FindFrame(int frameHeight)
        {
            Player player = Main.player[(int)NPC.ai[0]];

            if (player.velocity.Y < 0)
            {
                NPC.frame.Y = 1 * frameHeight;

            }
            else if (player.velocity.Y > 0)
            {
                NPC.frame.Y = 2 * frameHeight;

            }
            else
            {
                NPC.frame.Y = 0 * frameHeight;
            }
        }
        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.GetModPlayer<KSGlandPlayer>().RegrowCD = 900;
                owner.GetModPlayer<KSGlandPlayer>().ksMasterAcc = false;
                NPC.life = 0;
                NPC.checkDead();
                return false;
            }

            return true;
        }
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {

                Texture2D texture = TextureAssets.Npc[Type].Value;
                Vector2 drawOrigin = texture.Size() / 2f;
                for (int k = 0; k < NPC.oldPos.Length; k++)
                {
                    Vector2 drawPos = NPC.oldPos[k] - screenPos + new Vector2(NPC.width * 0.5f, NPC.height * 0.5f) + new Vector2(0f, NPC.gfxOffY + 4f);
                    Color color = drawColor * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
                    SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                    spriteBatch.Draw(texture, drawPos, null, color, 0f, drawOrigin, NPC.scale, effects, 0);
                }
            
            return true;
        }
    }
    public class KSGlandGlobalNPC : GlobalNPC
    {
        public override void ModifyHitNPC(NPC npc, NPC target, ref NPC.HitModifiers modifiers)
        {
            if (!npc.friendly)
            {
                if (target.type == ModContent.NPCType<KSGlandNPC>())
                {
                    npc.damage = (int)(npc.damage / 2);
                }
            }
           
        }
    }
}