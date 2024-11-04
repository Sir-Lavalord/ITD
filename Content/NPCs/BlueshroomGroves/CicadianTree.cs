using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using ITD.Content.Projectiles.Other;
using ReLogic.Content;
using ITD.Content.Tiles.BlueshroomGroves;
using ITD.Content.Dusts;
using ITD.Utilities;

namespace ITD.Content.NPCs.BlueshroomGroves
{
    public class CicadianTree : ModNPC
    {
        private readonly Asset<Texture2D> glow = ModContent.Request<Texture2D>("ITD/Content/NPCs/BlueshroomGroves/CicadianTree_Glow");
        public override void SetStaticDefaults()
        {
            NPCID.Sets.NPCBestiaryDrawModifiers bestiaryData = new NPCID.Sets.NPCBestiaryDrawModifiers()
            {
                Hide = true
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, bestiaryData);
        }
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
            NPC.chaseable = false;
        }
        public override Color? GetAlpha(Color drawColor)
        {
            NPC cicadian = Main.npc[(int)NPC.ai[0]];
            if (cicadian.type == ModContent.NPCType<Cicadian>() && cicadian.active)
            {
                return Lighting.GetColor(cicadian.Center.ToTileCoordinates());
            }
            return drawColor;
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = Rectangle.Empty;
        }
        public override void AI()
        {
            int otherNPC = -1;
            Vector2 offsetFromOtherNPC = Vector2.Zero;
            if (NPC.localAI[0] == 0f && Main.netMode != NetmodeID.MultiplayerClient)
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
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Vector2 offset = new Vector2(0f, NPC.gfxOffY - 22f);
            spriteBatch.Draw(glow.Value, NPC.Center - screenPos + offset, null, Color.White * BlueshroomTree.opac, 0f, glow.Size()/2f, 1f, SpriteEffects.None, 0f);
            if (Main.rand.NextBool(24))
            {
                int offset1 = 20;
                Dust.NewDust(NPC.Center + offset - new Vector2(offset1, offset1 + 42), 16 + offset1 * 2, 16 + offset1 * 2, ModContent.DustType<BlueshroomSporesDust>());
            }
        }
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.GetITDPlayer().ZoneBlueshroomsUnderground)
                return 0.1f;
            return 0f;
        }
    }
}
