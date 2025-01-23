using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.DataStructures;
using ITD.Content.Tiles.Misc;
using ITD.Content.Dusts;
using Terraria.GameContent.Drawing;
using Terraria.Audio;
using Mono.Cecil;

namespace ITD.Content.Projectiles.Friendly.Mage
{
    public class WeepWandWisp : ModProjectile
    {

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.scale = 1.25f;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            Projectile.alpha = 118;
            Projectile.light = 0.9f;
            int DustID1 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.GreenFairy, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 120, default(Color), 0.75f);
            int DustID2 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.Clentaminator_Green, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 120, default(Color), 0.75f);
            int DustID3 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.GreenTorch, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 120, default(Color), 0.75f);
            int DustID4 = Dust.NewDust(new Vector2(Projectile.position.X, Projectile.position.Y), Projectile.width, Projectile.height, DustID.WhiteTorch, Projectile.velocity.X * 0.2f, Projectile.velocity.Y * 0.2f, 120, default(Color), 0.75f);
            Main.dust[DustID1].noGravity = true;
            Main.dust[DustID2].noGravity = true;
            Main.dust[DustID3].noGravity = true;
            Main.dust[DustID4].noGravity = true;
            Projectile.rotation += (float)Projectile.direction * 0.8f;

            //This whole thing is for projectile control
            if (Main.myPlayer == Projectile.owner && Projectile.ai[0] == 0f)
            {
                Projectile.rotation += (float)Projectile.direction * 0.8f;
                if (Main.player[Projectile.owner].channel)
                {
                    Projectile.rotation += (float)Projectile.direction * 0.8f;
                    float num146 = 12f;
                    Vector2 vector10 = new Vector2(Projectile.position.X + (float)Projectile.width * 0.5f, Projectile.position.Y + (float)Projectile.height * 0.5f);
                    Projectile.rotation += (float)Projectile.direction * 0.8f;
                    float num147 = (float)Main.mouseX + Main.screenPosition.X - vector10.X;
                    Projectile.rotation += (float)Projectile.direction * 0.8f;
                    float num148 = (float)Main.mouseY + Main.screenPosition.Y - vector10.Y;
                    Projectile.rotation += (float)Projectile.direction * 0.8f;
                    if (Main.player[Projectile.owner].gravDir == -1f)
                    {
                        num148 = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY - vector10.Y;
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                    }
                    float num149 = (float)Math.Sqrt((double)(num147 * num147 + num148 * num148));
                    num149 = (float)Math.Sqrt((double)(num147 * num147 + num148 * num148));
                    if (num149 > num146)
                    {
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        num149 = num146 / num149;
                        num147 *= num149;
                        num148 *= num149;
                        int num150 = (int)(num147 * 1000f);
                        int num151 = (int)(Projectile.velocity.X * 1000f);
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        int num152 = (int)(num148 * 1000f);
                        int num153 = (int)(Projectile.velocity.Y * 1000f);
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        if (num150 != num151 || num152 != num153)
                        {
                            Projectile.rotation += (float)Projectile.direction * 0.8f;
                            Projectile.netUpdate = true;
                            Projectile.rotation += (float)Projectile.direction * 0.8f;
                        }
                        Projectile.velocity.X = num147;
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        Projectile.velocity.Y = num148;
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                    }
                    else
                    {
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        int num154 = (int)(num147 * 1000f);
                        int num155 = (int)(Projectile.velocity.X * 1000f);
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        int num156 = (int)(num148 * 1000f);
                        int num157 = (int)(Projectile.velocity.Y * 1000f);
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        if (num154 != num155 || num156 != num157)
                        {
                            Projectile.rotation += (float)Projectile.direction * 0.8f;
                            Projectile.netUpdate = true;
                            Projectile.rotation += (float)Projectile.direction * 0.8f;
                        }
                        Projectile.velocity.X = num147;
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        Projectile.velocity.Y = num148;
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                    }
                    Projectile.rotation += (float)Projectile.direction * 0.8f;
                }
                else
                {
                    Projectile.rotation += (float)Projectile.direction * 0.8f;
                    if (Projectile.ai[0] == 0f)
                    {
                        Projectile.ai[0] = 1f;
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        Projectile.netUpdate = true;
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        float num158 = 12f;
                        Vector2 vector11 = new Vector2(Projectile.position.X + (float)Projectile.width * 0.5f, Projectile.position.Y + (float)Projectile.height * 0.5f);
                        float num159 = (float)Main.mouseX + Main.screenPosition.X - vector11.X;
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        float num160 = (float)Main.mouseY + Main.screenPosition.Y - vector11.Y;
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        if (Main.player[Projectile.owner].gravDir == -1f)
                        {
                            num160 = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY - vector11.Y;
                            Projectile.rotation += (float)Projectile.direction * 0.8f;
                        }
                        float num161 = (float)Math.Sqrt((double)(num159 * num159 + num160 * num160));
                        if (num161 == 0f)
                        {
                            vector11 = new Vector2(Main.player[Projectile.owner].position.X + (float)(Main.player[Projectile.owner].width / 2), Main.player[Projectile.owner].position.Y + (float)(Main.player[Projectile.owner].height / 2));
                            Projectile.rotation += (float)Projectile.direction * 0.8f;
                            num159 = Projectile.position.X + (float)Projectile.width * 0.5f - vector11.X;
                            Projectile.rotation += (float)Projectile.direction * 0.8f;
                            num160 = Projectile.position.Y + (float)Projectile.height * 0.5f - vector11.Y;
                            Projectile.rotation += (float)Projectile.direction * 0.8f;
                            num161 = (float)Math.Sqrt((double)(num159 * num159 + num160 * num160));
                        }
                        num161 = num158 / num161;
                        num159 *= num161;
                        num160 *= num161;
                        Projectile.velocity.X = num159;
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        Projectile.velocity.Y = num160;
                        Projectile.rotation += (float)Projectile.direction * 0.8f;
                        if (Projectile.velocity.X == 0f && Projectile.velocity.Y == 0f)
                        {
                            Projectile.Kill();
                        }
                    }
                    Projectile.rotation += (float)Projectile.direction * 0.8f;
                }
                Projectile.rotation += (float)Projectile.direction * 0.8f;
            }
            Projectile.rotation += (float)Projectile.direction * 0.8f;
            if (Projectile.velocity.X != 0f || Projectile.velocity.Y != 0f)
            {
                Projectile.rotation = (float)Math.Atan2((double)Projectile.velocity.Y, (double)Projectile.velocity.X) - 2.355f;
                Projectile.rotation += (float)Projectile.direction * 0.8f;
            }
            Projectile.rotation += (float)Projectile.direction * 0.8f;
            if (Projectile.velocity.Y > 16f)
            {
                Projectile.velocity.Y = 16f;
                Projectile.rotation += (float)Projectile.direction * 0.8f;
            }

            if (++Projectile.frameCounter >= 3)
            {
                Projectile.frameCounter = 0;
                if (++Projectile.frame >= 6)
                {
                    Projectile.frame = 0;
                }
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (target.life < 1)
            {

            }
        }
    }
}