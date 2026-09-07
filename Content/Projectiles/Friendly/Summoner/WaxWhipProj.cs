using ITD.Content.Buffs.Debuffs;
using ITD.Particles;
using ITD.Particles.Projectiles;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Tile_Entities;
using Terraria.ID;
using Terraria.ModLoader;

namespace ITD.Content.Projectiles.Friendly.Summoner
{
    public class WaxWhipProj : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.IsAWhip[Type] = true;
        }
        public ParticleEmitter emitter;
        public override void SetDefaults()
        {
            Projectile.DefaultToWhip();
            Projectile.WhipSettings.RangeMultiplier = 0.25f;
            Projectile.WhipSettings.Segments = 25;
            Projectile.rotation += Projectile.ai[0];
            emitter = ParticleSystem.NewEmitter<WispFlame>(ParticleEmitterDrawCanvas.WorldUnderProjectiles);
            emitter.tag = Projectile;
        }
        public override void AI()
        {
            if (emitter != null) emitter.keptAlive = true;

            List<Vector2> points = Projectile.WhipPointsForCollision;
            Projectile.FillWhipControlPoints(Projectile, points);
            emitter?.Emit(points[points.Count - 1],
                (-Vector2.UnitY * Main.rand.NextFloat(2, 4)).RotatedByRandom(MathHelper.ToRadians(30)), 0f, 2);
        }
        private void DrawLine(List<Vector2> list)
        {
            Texture2D texture = TextureAssets.FishingLine.Value;
            Rectangle frame = texture.Frame();
            Vector2 origin = new(frame.Width / 2, 2);

            Vector2 pos = list[0];
            for (int i = 0; i < list.Count - 1; i++)
            {
                Vector2 element = list[i];
                Vector2 diff = list[i + 1] - element;

                float rotation = diff.ToRotation() - MathHelper.PiOver2;

                Color color = Lighting.GetColor(element.ToTileCoordinates(), Color.DarkTurquoise);
                Vector2 scale = new Vector2(1, (diff.Length() + 2) / frame.Height);

                Main.EntitySpriteDraw(texture, pos - Main.screenPosition, frame, color, rotation, origin, scale, SpriteEffects.None, 0);

                pos += diff;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            int debuffType = ModContent.BuffType<WaxWhipTagDebuff>();
            bool isNewTarget = !target.HasBuff(debuffType);
            target.AddBuff(debuffType, 60);
            if (isNewTarget)
            {
                foreach (var npc in Main.ActiveNPCs)
                {
                    if (npc.whoAmI != target.whoAmI)
                    {
                        int buffIndex = npc.FindBuffIndex(debuffType);
                        if (buffIndex != -1)
                        {
                            npc.DelBuff(buffIndex);
                        }
                    }
                }
            }
            base.OnHitNPC(target, hit, damageDone);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            List<Vector2> list = new List<Vector2>();
            Projectile.FillWhipControlPoints(Projectile, list);

            DrawLine(list);
            Main.DrawWhip_WhipBland(Projectile, list);

            SpriteEffects flip = Projectile.spriteDirection < 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            
            return false;
        }
    }
}