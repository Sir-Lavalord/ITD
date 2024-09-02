using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class FishbackerProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            // This makes the projectile use whip collision detection and allows flasks to be applied to it.
            ProjectileID.Sets.IsAWhip[Type] = true;
        }


        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.width = 18;
            Projectile.WhipSettings.RangeMultiplier = 1f;
            Projectile.WhipSettings.Segments = 12;
        }

        private float Timer
        {
            get => Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        float fTest;
        public float fDistance = 10 ;
        public override void AI()
        {
            fTest+= 0.15f;
            Player owner = Main.player[Projectile.owner];
            fDistance = (10 * fTest);
/*            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile other = Main.projectile[i];

                if (i != Projectile.whoAmI && other.hostile && !other.friendly && other.active && other.aiStyle != -100 && Vector2.Distance(other.Center, Projectile.Center) <= fDistance)
                {
                    if (!Main.dedServ)
                    {
                        other.owner = Main.myPlayer;
                        other.velocity.X *= -2f;
                        other.velocity.Y *= -1f;

                        ParticleOrchestrator.RequestParticleSpawn(clientOnly: true, ParticleOrchestraType.Excalibur, new ParticleOrchestraSettings
                        {
                            PositionInWorld = other.Center,
                        }, other.whoAmI);

                        other.friendly = true;
                        other.hostile = false;
                        other.damage *= 10;
                        other.netUpdate = true;
                    }
                }
            }*/
        }
        Vector2 linepos;
        private void DrawLine(List<Vector2> list)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new Vector2(frame.Width / 2);//offset tip

            linepos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;
                Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.Brown);
                Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

                Main.EntitySpriteDraw(texture, linepos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                linepos += diff;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);
            DrawLine(list);
            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.instance.LoadProjectile(Type);
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 pos = list[0];

            for (int i = 0; i < list.Count - 1; i++)
            {
                //NOTE: CHANGE ORGIN FOR SEGMENTS
                Rectangle frame = new Rectangle(0, 0, 18, 26);
                Vector2 origin = new Vector2(5, 11);
                float scale = 1;
                if (i == list.Count - 2)
                {
                    frame.Y = 74;
                    frame.Height = 18;
                    origin.Y = -2;
                    Projectile.GetWhipSettings(Projectile, out float timeToFlyOut, out int _, out float _);
                    float t = Timer / timeToFlyOut;
                    scale = MathHelper.Lerp(0.5f, 1.5f, Utils.GetLerpValue(0.1f, 0.7f, t, true) * Utils.GetLerpValue(0.9f, 0.7f, t, true));
                }
                else if (i > 10)
                {
                    frame.Y = 58;
                    frame.Height = 16;
                    origin.Y = -2;
                }
                else if (i > 5)
                {
                    frame.Y = 42;
                    frame.Height = 16;
                    origin.Y = -2;

                }
                else if (i > 0)
                {
                    frame.Y = 26;
                    frame.Height = 16;
                    origin.Y = -2;
                }

                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2; // This projectile's sprite faces down, so PiOver2 is used to correct rotation.
                Color color = Lighting.GetColor(element.ToTileCoordinates());

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, flip, 0);

                pos += diff;
            }
            return false;
        }
    }
}