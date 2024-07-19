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
        public bool CicadianKilled
        {
            get => NPC.ai[0] == 1f;
            set => NPC.ai[0] = value ? 1f : 0f;
        }
        public Vector2 BasePosition
        {
            get => new Vector2(NPC.ai[1], NPC.ai[2]);
            set { NPC.ai[1] = value.X; NPC.ai[2] = value.Y; }
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
        }
        public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
        {
            boundingBox = Rectangle.Empty;
        }
        public override void OnSpawn(IEntitySource source)
        {
            NPC.Center += new Vector2(0, 40);
            NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y, ModContent.NPCType<Cicadian>(), NPC.whoAmI, NPC.whoAmI);
            BasePosition = NPC.Center;
        }
        public override void AI()
        {
            NPC.Center = BasePosition - new Vector2(0f, NPC.height/2f);
            if (CicadianKilled)
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
