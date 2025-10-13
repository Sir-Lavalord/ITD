using ITD.Content.Buffs.Debuffs;
using ITD.Utilities;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.DataStructures;

namespace ITD.Content.Projectiles.Friendly.Ranger;

public class WheelerTrashProj : ModProjectile
{
    public override void SetStaticDefaults()
    {
        Main.projFrames[Projectile.type] = 15;

    }

    public override void SetDefaults()
    {
        Projectile.width = 20;
        Projectile.height = 40;
        Projectile.aiStyle = 1;
        Projectile.friendly = true;
        Projectile.hostile = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 600;
        Projectile.alpha = 0;
        Projectile.light = 0.5f;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = true;
        Projectile.extraUpdates = 1;
        Projectile.localNPCHitCooldown = 20;
        Projectile.usesLocalNPCImmunity = true;
        AIType = ProjectileID.Bullet;
    }
    private NPC HomingTarget
    {
        get => Projectile.ai[1] == 0 ? null : Main.npc[(int)Projectile.ai[1] - 1];
        set
        {
            Projectile.ai[1] = value == null ? 0 : value.whoAmI + 1;
        }
    }
    Vector2 spawnvel;

    public override void OnSpawn(IEntitySource source)
    {
        Player player = Main.player[Projectile.owner];
        string name = player.name ?? "";
        if (player.active)
        {
            if (name == "plague")
            {
                Projectile.frame = 2;//black mold black mold black mold black mold

            }
            else
            {
                Projectile.frame = (int)Projectile.ai[0];
            }
        }
        switch (Projectile.ai[0])
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:
                Projectile.penetrate = 4;
                break;
            case 4:
                break;
            case 5:
                break;
            case 6:
                Projectile.damage = (int)(Projectile.damage * 0.5f);
                break;
            case 7:
                Projectile.damage = (int)(Projectile.damage * 0.75f);
                break;
            case 9:
                Projectile.damage = (int)(Projectile.damage * 2f);
                break;
            case 14:
                Projectile.hide = true;
                Projectile.damage = (int)(Projectile.damage * 4f);
                Projectile.spriteDirection = (Vector2.Dot(Projectile.velocity, Vector2.UnitX) >= 0f).ToDirectionInt();
                spawnrotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection;
                spawnvel = Projectile.velocity;
                break;
        }
    }
    int dustcolor;
    bool bBounced;
    float spawnrotation;

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Collision.HitTiles(Projectile.position, new Vector2(-spawnvel.X / 2, -spawnvel.Y / 2), Projectile.width, Projectile.height);

        if (Projectile.ai[0] == 12)
        {
            HomingTarget ??= Projectile.FindClosestNPC(6000);

            if (HomingTarget != null)
            {
                if (!bBounced)
                {
                    bBounced = true;
                    if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                    {
                        Projectile.velocity.X = Projectile.DirectionTo(HomingTarget.Center).X * 11f;
                    }

                    // If the projectile hits the top or bottom side of the tile, reverse the Y velocity
                    if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                    {
                        Projectile.velocity.Y = Projectile.DirectionTo(HomingTarget.Center).Y * 11f;
                    }
                }
            }
            return false;
        }
        else if (Projectile.ai[0] == 14)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.position);
            grounded = true;
            Projectile.velocity *= 0f;
            Projectile.rotation = spawnrotation;
            Collision.HitTiles(Projectile.position, new Vector2(-spawnvel.X / 2, -spawnvel.Y / 2), Projectile.width, Projectile.height);

            for (int i = 0; i < 8; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.Bone, -spawnvel.X / 2, -spawnvel.Y / 2, 60, default, Main.rand.NextFloat(1f, 1.7f));
                dust.noGravity = true;
                dust.velocity *= 4f;
                Dust.NewDustDirect(Projectile.position, Projectile.width / 4, Projectile.height / 4, DustID.Blood, -spawnvel.X / 2, -spawnvel.Y / 2, 60, default, Main.rand.NextFloat(1f, 1.7f));
            }
            return false;
        }
        else return true;


    }
    public bool IsStickingToTarget
    {
        get => Projectile.localAI[0] == 1f;
        set => Projectile.localAI[0] = value ? 1f : 0f;
    }

    // Index of the current target
    public int TargetWhoAmI
    {
        get => (int)Projectile.localAI[1];
        set => Projectile.localAI[1] = value;
    }

    public int GravityDelayTimer
    {
        get => (int)Projectile.localAI[2];
        set => Projectile.localAI[2] = value;
    }

    public float StickTimer
    {
        get => Projectile.ai[2];
        set => Projectile.ai[2] = value;
    }
    public override void AI()
    {
        Projectile.rotation += 8f;
        if (Main.rand.NextBool(2))
        {
            Dust dust = Main.dust[Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, dustcolor, Projectile.velocity.X / 2, Projectile.velocity.Y / 2)];
            dust.noGravity = true;
            dust.velocity *= 0.9f;
        }
        switch (Projectile.ai[0])
        {
            case 0:
                dustcolor = DustID.t_Crystal;
                break;
            case 1:
                dustcolor = DustID.Pixie;
                Projectile.velocity.Y += 0.15f;
                break;
            case 2:
                Projectile.velocity.Y += 0.15f;
                break;
            case 3:
                Projectile.rotation = Projectile.velocity.ToRotation() - (float)Math.PI * 1.25f;
                dustcolor = DustID.PinkFairy;
                break;
            case 4:
                Projectile.velocity.Y += 0.15f;

                dustcolor = DustID.CursedTorch;
                break;
            case 5:
                Projectile.velocity.Y += 0.15f;

                dustcolor = DustID.Ichor;
                break;
            case 6:
                Projectile.velocity.Y += 0.15f;

                dustcolor = DustID.Poop;
                break;
            case 7:
                Projectile.velocity.Y += 0.15f;
                dustcolor = DustID.Ash;
                break;
            case 8:
                dustcolor = DustID.Palladium;
                break;
            case 9:
                dustcolor = DustID.Titanium;
                break;
            case 10:
                Projectile.velocity.Y += 0.2f;
                dustcolor = DustID.Pearlsand;
                break;
            case 11:
                dustcolor = DustID.Water;
                break;
            case 12:
                if (!bBounced)
                    Projectile.velocity.Y += 0.25f;
                dustcolor = DustID.Water_Hallowed;
                break;
            case 13:
                dustcolor = DustID.Blood;
                break;
            case 14:
                UpdateAlpha();
                if (IsStickingToTarget)
                {
                    StickyAI();
                }
                else
                {
                    NormalAI();
                }
                Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI * 1.25f;
                break;
        }
    }
    bool grounded;
    private void NormalAI()
    {
        if (!grounded)
        {
            for (int i = 0; i < 1; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Bone, 0f, 0f, 100, default, 1f);
                dust.noGravity = false;
                dust.velocity *= 0.4f;
            }
            Projectile.spriteDirection = (Vector2.Dot(Projectile.velocity, Vector2.UnitX) >= 0f).ToDirectionInt();

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2 - MathHelper.PiOver4 * Projectile.spriteDirection;
        }
    }
    private void StickyAI()
    {
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;

        int npcTarget = TargetWhoAmI;
        if (Main.npc[npcTarget].active && !Main.npc[npcTarget].dontTakeDamage)
        {
            Projectile.Center = Main.npc[npcTarget].Center - Projectile.velocity * 2f;
            Projectile.gfxOffY = Main.npc[npcTarget].gfxOffY;
        }
        else
        { // Otherwise, kill the projectile
            Projectile.Kill();
        }
    }
    public override bool? CanDamage()
    {
        return !IsStickingToTarget;
    }
    private const int MaxStickingJavelin = 4;
    private readonly Point[] stickingJavelins = new Point[MaxStickingJavelin];
    int javelinstuck;
    public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough, ref Vector2 hitboxCenterFrac)
    {
        width = height = 10;
        return true;
    }

    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
    {

        if (targetHitbox.Width > 6 && targetHitbox.Height > 6)
        {
            targetHitbox.Inflate(-targetHitbox.Width / 6, -targetHitbox.Height / 6);
        }
        return projHitbox.Intersects(targetHitbox);
    }

    public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
    {
        if (IsStickingToTarget)
        {
            int npcIndex = TargetWhoAmI;
            if (npcIndex >= 0 && npcIndex < 200 && Main.npc[npcIndex].active)
            {
                if (Main.npc[npcIndex].behindTiles)
                {
                    behindNPCsAndTiles.Add(index);
                }
                else
                {
                    behindNPCsAndTiles.Add(index);
                }

                return;
            }
        }
        behindNPCsAndTiles.Add(index);
    }

    private const int AlphaFadeInSpeed = 25;

    private void UpdateAlpha()
    {
        if (Projectile.alpha > 0)
        {
            Projectile.alpha -= AlphaFadeInSpeed;
        }
        if (Projectile.alpha < 0)
        {
            Projectile.alpha = 0;
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        Player player = Main.player[Projectile.owner];
        string name = player.name ?? "";
        if (player.active)
        {
            if (name == "plague")
            {
                target.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 300);

            }
        }
        switch (Projectile.ai[0])
        {
            case 0:
                break;
            case 1:
                break;
            case 2:
                target.AddBuff(ModContent.BuffType<MelomycosisBuff>(), 300);
                break;
            case 3:
                break;
            case 4:
                target.AddBuff(BuffID.CursedInferno, 300);
                break;
            case 5:
                target.AddBuff(BuffID.Ichor, 300);
                break;
            case 6:
                Projectile.damage = (int)(Projectile.damage * 0.5f);
                target.AddBuff(BuffID.Stinky, 600);
                break;
            case 7:
                target.AddBuff(BuffID.Oiled, 800);
                break;
            case 8:
                player.AddBuff(BuffID.RapidHealing, 300);
                break;
            case 13:
                for (int b = 0; b <= 3; b++)
                {
                    FTWDebuff();
                    target.AddBuff(i_randomDebuff, 1200);
                }
                break;
            case 14:
                javelinstuck++;
                IsStickingToTarget = true;
                TargetWhoAmI = target.whoAmI;
                Projectile.velocity = (target.Center - Projectile.Center) *
                    0.75f;
                Projectile.netUpdate = true;
                break;
        }
    }
    public static int i_randomDebuff = 0;
    public static float fChance;
    public static void FTWDebuff()
    {
        var list = new List<int>
        {
        BuffID.Poisoned,
        BuffID.Darkness,
        BuffID.Cursed,
        BuffID.OnFire,
        BuffID.Bleeding,
        BuffID.Suffocation,
        BuffID.Slow,
        BuffID.Silenced,
        BuffID.Weak,
        BuffID.BrokenArmor,
        BuffID.Confused,
        };
        i_randomDebuff = list[Main.rand.Next(list.Count)];
    }

    public override void OnKill(int timeLeft)
    {

        switch (Projectile.ai[0])
        {
            case 0:
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int amount = 3 + Main.rand.Next(1, 4);
                    for (int i = 0; i < amount; i++)
                    {
                        double rad = Math.PI / (amount / 2) * i;
                        int damage = Projectile.damage / 2;
                        int knockBack = 1;
                        Vector2 vector = Vector2.One.RotatedBy(rad) * Projectile.velocity;
                        int projID = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, vector, ProjectileID.CrystalShard, damage, knockBack, Main.myPlayer, 0, 1);
                        Main.projectile[projID].tileCollide = false;
                    }
                }
                break;
            case 1:
                break;
            case 2:
                break;
            case 3:

                break;
            case 4:

                break;
            case 5:

                break;
            case 6:
                break;
            case 10:
                break;
        }
        if (Projectile.owner == Main.myPlayer)
        {
            for (int i = 0; i < 12; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustcolor, 0f, 0f, 100, default, 0.6f);
                dust.noGravity = true;
                dust.velocity *= 1f;
                dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustcolor, 0f, 0f, 100, default, 0.6f);
                dust.velocity *= 0.5f;
                dust.noGravity = true;

            }
        }
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
    }
}