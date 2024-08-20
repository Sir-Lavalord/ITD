using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using ITD.Utils;
using ITD.Content.Tiles.BlueshroomGroves;
using ITD.Content.Dusts;

namespace ITD.Content.NPCs.BlueshroomGroves
{
    public class ShroomishSlime : ModNPC
    {
		private readonly Asset<Texture2D> glow = ModContent.Request<Texture2D>("ITD/Content/NPCs/BlueshroomGroves/ShroomishSlime_Glow");
		public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 2;
        }
        public override void SetDefaults()
        {
            NPC.damage = 26;
            NPC.aiStyle = NPCAIStyleID.Slime;
			AnimationType = NPCID.BlueSlime;
            NPC.width = 28;
            NPC.height = 36;
            NPC.defense = 8;
            NPC.lifeMax = 80;
            NPC.value = Item.buyPrice(silver: 4);
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
        }
		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {			
			Rectangle sourceRectangle = NPC.frame;
			Vector2 origin = sourceRectangle.Size() / 2f;

            spriteBatch.Draw(glow.Value, NPC.Center - screenPos + new Vector2(0f, NPC.gfxOffY + 2f), sourceRectangle, Color.White * BlueshroomTree.opac, 0f, origin, 1f, SpriteEffects.None, 0f);
        }
			
		public override void HitEffect(NPC.HitInfo hit)
        {
            if (NPC.life > 0)
			{
				int i = 0;
				while ((double)i < hit.Damage / (double)NPC.lifeMax * 50.0)
				{
					Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<BlueshroomSporesDust>(), hit.HitDirection, -1f, 0, default, 1f);
					i++;
				}
				return;
			}
			for (int j = 0; j < 20; ++j)
			{
				Dust.NewDust(NPC.position, NPC.width, NPC.height, ModContent.DustType<BlueshroomSporesDust>(), 0f, 0f, 0, default, 1f);
			}
        }
			
        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.GetITDPlayer().ZoneBlueshroomsUnderground)
            {
                return 0.25f;
            }
            return 0f;
        }
	}
}
