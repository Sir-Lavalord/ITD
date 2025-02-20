using System.IO;
using ITD.Utilities;
using ITD.Content.Items.Weapons.Ranger;

namespace ITD.Content.Projectiles.Friendly.Ranger
{
    public class PotshotReticle : ModProjectile
    {
        public override void SetStaticDefaults()
        {
        }
        private NPC HomingTarget
        {
            get => Projectile.ai[0] == 0 ? null : Main.npc[(int)Projectile.ai[0] - 1];
            set
            {
                Projectile.ai[0] = value == null ? 0 : value.whoAmI + 1;
            }
        }
        public override void SetDefaults()
        {
            Projectile.width = 56;
            Projectile.height = 56;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.aiStyle = 0;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 1000;
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            float maxDetectRadius = 30;
            HomingTarget ??= Projectile.FindClosestNPC(maxDetectRadius);

            if (HomingTarget == null)
            {
                Projectile.Kill();
                return;
            }
            if (!HomingTarget.active || HomingTarget.life <= 0 || HomingTarget.Distance(player.Center) >= 400 || player.HeldItem.ModItem is not Potshot)
            {
                Projectile.Kill();
            }
            Projectile.velocity = Vector2.Zero;

            if (++Projectile.ai[1] > 45)
            {
                Projectile.Center = HomingTarget.Center;
                Projectile.rotation = 0;
                Projectile.alpha = 0;
                Projectile.scale = 1;
            }
            else
            {
                Projectile.Center = HomingTarget.Center;
                HomingTarget.GetGlobalNPC<PotshotTarget>().isTargeted = true;//despite the lock anim, the actual lock happens immediatly to avoid free damage
                float spindown = 1f - Projectile.ai[1] / 45f;
                Projectile.alpha = (int)(255 * spindown);
                Projectile.scale = 1 + 2 * spindown;
            }
        }
        public override void OnKill(int timeLeft)
        {
            if (HomingTarget == null)
            {
                return;
            }
            HomingTarget.GetGlobalNPC<PotshotTarget>().isTargeted = false;
        }
        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 255, 255, 128) * (1f - Projectile.alpha / 255f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture2D13 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            int num156 = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value.Height / Main.projFrames[Projectile.type]; //ypos of lower right corner of sprite to draw
            int y3 = num156 * Projectile.frame;
            Rectangle rectangle = new(0, y3, texture2D13.Width, num156);
            Vector2 origin2 = rectangle.Size() / 2f;
            Main.EntitySpriteDraw(texture2D13, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(rectangle), Projectile.GetAlpha(lightColor), Projectile.rotation, origin2, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
    }
    public class PotshotTarget : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        public bool isTargeted;
        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            if (projectile.GetGlobalProjectile<ITDInstancedGlobalProjectile>().isFromPotshot)
            {
                if (Main.player[projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<PotshotReticle>()] >= 1)
                {
                    if (!isTargeted)
                    {
                        modifiers.FinalDamage.Flat += 4;
                    }
                }
            }
        }
    }
}