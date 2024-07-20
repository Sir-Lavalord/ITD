using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using ITD.Content.Projectiles.Other;

namespace ITD.Content.NPCs.BasicEnemies
{
    public class CicadianTree : ModNPC
    {
        public override void SetDefaults()
        {
            NPC.width = 20;
            NPC.height = 78;
            NPC.damage = 0;
            NPC.defense = 0;
            NPC.lifeMax = 100;
            NPC.HitSound = SoundID.Dig;
            NPC.DeathSound = SoundID.Dig;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = 0;
            NPC.aiStyle = -1;
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = Rectangle.Empty;
        }
        public override void AI()
        {
            int otherNPC = -1;
            Vector2 offsetFromOtherNPC = Vector2.Zero;
            if (NPC.localAI[0] == 0f && Main.netMode != 1)
            {
                NPC.localAI[0] = 1f;
                int newNPC = NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y + 40, ModContent.NPCType<Cicadian>(), NPC.whoAmI, NPC.whoAmI, 0f, 0f, 0f, 255);
                NPC.ai[0] = newNPC;
            }
            int otherNPCCheck = (int)NPC.ai[0];
            if (Main.npc[otherNPCCheck].active)
            {
                otherNPC = otherNPCCheck;
                offsetFromOtherNPC = Vector2.UnitY * -89f;
            }
            if (otherNPC != -1)
            {
                NPC cicadian = Main.npc[otherNPC];
                NPC.velocity = Vector2.Zero;
                NPC.Center = cicadian.Center;
                NPC.Center += offsetFromOtherNPC;
                NPC.gfxOffY = cicadian.gfxOffY;
                NPC.velocity = cicadian.velocity;
            }
            else
            {
                NPC.StrikeInstantKill();
                NPC.netUpdate = true;
            }
        }
        public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life <= 0)
            {
                IEntitySource death = NPC.GetSource_Death();
                Projectile.NewProjectile(death, NPC.Center, new Vector2(hit.HitDirection*2f, -2f), ModContent.ProjectileType<CicadianTreeEnd>(), 0, 0f);
            }
        }
        public override bool? CanBeHitByItem(Player player, Item item) => item.axe > 0;
        public override bool? CanBeHitByProjectile(Projectile projectile) => false;
    }
}
