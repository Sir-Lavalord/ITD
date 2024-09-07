using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using ITD.Content.Buffs.Debuffs;
using ITD.Players;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class FishbackerReflectProj : ModProjectile
    {
        public override string Texture => "ITD/Content/Projectiles/BlankTexture";

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 1;

        }
        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2000;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.scale = 0.2f;
            DrawOffsetX = -35;
            DrawOriginOffsetY = -35;
            Projectile.ArmorPenetration = 0;

        }
        public override bool? CanDamage()
        {
            return false;
        }
        int dustoffset;
        public override void AI()
        {
            if (Projectile.scale <= 1.2f)
            {
                dustoffset += 4;

                Projectile.scale += 0.06f;
            }
            else
            {
                Projectile.alpha += 10;


                Projectile.velocity.X *= 0.9f;
                Projectile.velocity.Y *= 0.9f;
            }
            if (Projectile.alpha > 150)
            {
                Projectile.Kill();
            }
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];

                if (i != Projectile.whoAmI && other.hostile &&
                    !other.friendly && other.active && 
                    //TODO: Change this
                    other.aiStyle != -100

                    && Math.Abs(Projectile.position.X - other.position.X)
                    + Math.Abs(Projectile.position.Y - other.position.Y) < dustoffset)
                {
                    if (!Main.dedServ)
                    {
                        CombatText.NewText(Projectile.Hitbox, Color.LimeGreen, "PARRY", true);
                        Main.player[Projectile.owner].GetModPlayer<ITDPlayer>().Screenshake = 20;
                        other.GetGlobalProjectile<FishbackerReflectedProj>().IsReflected = true;
                        other.owner = Main.myPlayer;
                        other.velocity.X *= -2f;
                        other.velocity.Y *= -1f;

                        ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings
                        {
                            PositionInWorld = other.Center,
                        }, other.whoAmI);

                        other.friendly = true;
                        other.hostile = false;
                        other.damage *= 2;
                        other.netUpdate = true;
                    }
                }
            }
        }
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            hitbox.Width = dustoffset;
            hitbox.Height = dustoffset;
            hitbox.X -= dustoffset / 2 - (Projectile.width / 2);
            hitbox.Y -= dustoffset / 2 - (Projectile.height / 2);
            base.ModifyDamageHitbox(ref hitbox);
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity *= 0;
            Projectile.Kill();
            return true;
        }
    }
    public class FishbackerReflectedProj : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool IsReflected;
        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (IsReflected)
            {
                target.AddBuff(ModContent.BuffType<FishbackerTagDebuff>(),300);
            }
        }
    }
}