using ITD.Content.Projectiles.Other;

namespace ITD.Content.NPCs.Minibiomes.BlackMold;

public abstract class MoldNpc : ITDNPC
{
    public void MoldExplode()
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            Projectile.NewProjectile(NPC.GetSource_FromThis(), NPC.Center, new Vector2(), ModContent.ProjectileType<MoldExplosion>(), 25, 0, -1);
        }
    }

    public override void ResetEffects()
    {
        bool fireFlag = false;
        for (int i = 0; i < NPC.maxBuffs; i++)
        {
            if (NPC.buffType[i] == BuffID.OnFire || NPC.buffType[i] == BuffID.OnFire3)
            {
                NPC.DelBuff(i);
                fireFlag = true;
                i--;
            }
        }
        if (fireFlag)
            MoldExplode();
    }

    public override bool CheckDead()
    {
        if (NPC.HasBuff(BuffID.OnFire) || NPC.HasBuff(BuffID.OnFire3))
            MoldExplode();
        return base.CheckDead();
    }
}
