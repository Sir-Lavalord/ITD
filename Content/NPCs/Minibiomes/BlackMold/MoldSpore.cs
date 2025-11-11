using ITD.Content.Buffs.Debuffs;
using Terraria.Audio;

namespace ITD.Content.NPCs.Minibiomes.BlackMold;

public class MoldSpore : ModNPC
{
    public ref float ImmuneTimer => ref NPC.ai[0];
    public override void SetStaticDefaults()
    {
        NPCID.Sets.SpecificDebuffImmunity[Type][ModContent.BuffType<MelomycosisBuff>()] = true;
    }
    public override void SetDefaults()
    {
        NPC.damage = 26;
        NPC.aiStyle = NPCAIStyleID.Spore;
        //AnimationType = NPCID.BlueSlime;
        NPC.width = 12;
        NPC.height = 12;
        NPC.lifeMax = 1;
        NPC.HitSound = SoundID.NPCHit1;
        NPC.DeathSound = SoundID.NPCDeath1;
        NPC.noGravity = true;
        NPC.noTileCollide = true;
        NPC.npcSlots = 0f;
    }

    public override bool PreAI()
    {
        if (ImmuneTimer > 0)
            ImmuneTimer--;
        //if (Main.rand.NextBool())
        //{
        Dust dust = Main.dust[Dust.NewDust(NPC.Top, 0, 0, DustID.Ambient_DarkBrown, 0f, 0f, 0, default, 1f)];
        dust.velocity = new Vector2();
        dust.noGravity = true;
        //}
        return true;
    }

    public override bool? CanBeHitByItem(Player player, Item item) // invulnerability so it doesn't die instantly when spawned in by mold slime
    {
        if (ImmuneTimer > 0)
            return false;
        return null;
    }

    public override bool? CanBeHitByProjectile(Projectile projectile)
    {
        if (ImmuneTimer > 0)
            return false;
        return null;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        target.AddBuff<MelomycosisBuff>(60 * 8);
    }

    public override void HitEffect(NPC.HitInfo hit)
    {
        for (int j = 0; j < 10; ++j)
        {
            Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Ambient_DarkBrown, 0f, 0f, 0, default, 1f);
        }
        if (NPC.HasBuff(BuffID.OnFire) || NPC.HasBuff(BuffID.OnFire3))
        {
            SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            for (int i = 0; i < 5; i++)
            {
                int num898 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.Smoke, 0f, 0f, 100, default, 1.5f);
                Dust dust2 = Main.dust[num898];
                dust2.velocity *= 1.4f;
            }
            for (int i = 0; i < 3; i++)
            {
                int num900 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, default, 3.5f);
                Main.dust[num900].noGravity = true;
                Dust dust2 = Main.dust[num900];
                dust2.velocity *= 7f;
                num900 = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, default, 1.5f);
                dust2 = Main.dust[num900];
                dust2.velocity *= 3f;
            }
            Gore gore = Main.gore[Gore.NewGore(NPC.GetSource_Death(), new Vector2(NPC.position.X, NPC.position.Y), default, Main.rand.Next(61, 64), 1f)];
            gore.velocity *= 0.4f;
            gore.velocity.X++;
            gore.velocity.Y++;
        }
    }

    //public override float SpawnChance(NPCSpawnInfo spawnInfo)
    //{
    //    if (spawnInfo.Player.ITD().ZoneBlueshroomsUnderground)
    //    {
    //        return 0.45f;
    //    }
    //    return 0f;
    //}
}
