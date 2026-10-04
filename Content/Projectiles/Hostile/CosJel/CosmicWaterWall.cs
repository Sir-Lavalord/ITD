using ITD.Content.Dusts;
using ITD.Content.NPCs.Bosses;
using ITD.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI;

namespace ITD.Content.Projectiles.Hostile.CosJel
{
    public class CosmicWaterWall : ITDProjectile
    {
        private static BasicEffect basicEffect;

        public override string Texture => ITD.BlankTexture;

        public int OwnerIndex => (int)Projectile.ai[0];
        public int ParentIndex => (int)Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 3600;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.hide = true;
            Projectile.netImportant = true;
            Projectile.hostile = true;
            Projectile.friendly = false;
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
        {
            behindProjectiles.Add(index);
            overPlayers.Add(index);
        }

        public override void OnSpawn(IEntitySource source)
        {
            NPC CosJel = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
            if (CosJel == null)
            {
                Projectile.timeLeft = 0;
                Projectile.active = false;
            }
        }

        public override void AI()
        {
            NPC owner = MiscHelpers.NPCExists(OwnerIndex, ModContent.NPCType<CosmicJellyfish>());
            if (owner == null)
            {
                Projectile.Kill();
                return;
            }

            if (ParentIndex < 0 || ParentIndex >= Main.maxProjectiles)
            {
                Projectile.Kill();
                return;
            }

            Projectile hand = Main.projectile[ParentIndex];
            if (hand == null || !hand.active || hand.timeLeft <= 0)
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = hand.Center;
            float dir = Projectile.rotation / MathHelper.PiOver2;
            int layers = 3;

            if (hand.ai[2] != 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i <= layers; i++)
                {
                    SpawnProjectileWall(i, layers, dir, i % 2 == 0 ? 0 : 1);
                }

                hand.ai[2] = 0;
                hand.netUpdate = true;
                Projectile.netUpdate = true;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float num1 = 0f;
            float wallHeight = 2400f;
            Vector2 surfaceDir = Projectile.rotation.ToRotationVector2();
            Vector2 lineStart = Projectile.Center - (surfaceDir * wallHeight);
            Vector2 lineEnd = Projectile.Center + (surfaceDir * wallHeight);

            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), lineStart, lineEnd, 50f, ref num1))
            {
                return true;
            }
            return base.Colliding(projHitbox, targetHitbox);
        }

        private void SpawnProjectileWall(int layer, int maxLayer, float dir, float deviation)
        {
            float wallHeight = 2400f;
            float spacing = 30f;
            int damage = 20;
            int wallSize = 5;
            float totalProjectiles = wallHeight / spacing;
            float startY = Projectile.Center.Y - (wallHeight / 2) + (spacing * wallSize * deviation);
            float spawnX = Projectile.Center.X - (25 * layer + 200) * dir;

            for (int i = 0; i < totalProjectiles; i++)
            {
                if (i % 10 < wallSize)
                {
                    Vector2 spawnPos = new Vector2(spawnX, startY + (i * spacing));
                    Vector2 velocity = new Vector2(2 * dir, 0);
                    Projectile.NewProjectile(
                        Projectile.GetSource_FromAI(),
                        spawnPos,
                        velocity,
                        ModContent.ProjectileType<CosmicWaterBlob>(),
                        damage,
                        1f, Main.myPlayer, (maxLayer + 1) - layer
                    );
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            GraphicsDevice gd = Main.graphics.GraphicsDevice;

            sb.End();

            List<VertexPositionColorTexture> bodyVertices = new List<VertexPositionColorTexture>();
            List<VertexPositionColorTexture> rimVertices = new List<VertexPositionColorTexture>();

            float time = Main.GlobalTimeWrappedHourly;
            float step = 15f;
            Vector2 surfaceDir = Projectile.rotation.ToRotationVector2();
            Vector2 depthDir = (Projectile.rotation + MathHelper.PiOver2).ToRotationVector2();
            float drawLength = 3000f;
            float depthLength = 2000f;

            for (float i = -drawLength; i <= drawLength; i += step)
            {
                Vector2 basePos = Projectile.Center + (surfaceDir * i);

                float wave1 = (float)Math.Sin(i * 0.012f + time * 1.5f) * 10f;
                float wave2 = (float)Math.Sin(i * 0.04f + time * 2.2f) * 3f;
                float totalWaveOffset = wave1 + wave2;

                Vector2 surfacePos = basePos + (depthDir * totalWaveOffset);
                Vector2 bottomPos = basePos + (depthDir * depthLength);

                Vector2 screenSurface = surfacePos - Main.screenPosition;
                Vector2 screenBottom = bottomPos - Main.screenPosition;

                float u = i / 1000f;
                float vTop = totalWaveOffset / 1000f;
                float vBottom = depthLength / 1000f;

                Color topColor = new Color(160, 32, 240);
                Color bottomColor = Color.Black;

                bodyVertices.Add(new VertexPositionColorTexture(new Vector3(screenSurface, 0), topColor, new Vector2(u, vTop)));
                bodyVertices.Add(new VertexPositionColorTexture(new Vector3(screenBottom, 0), bottomColor, new Vector2(u, vBottom)));

                float rimWidth = 6f;
                Vector2 rimOffset = depthDir * (rimWidth / 2f);

                rimVertices.Add(new VertexPositionColorTexture(new Vector3(screenSurface - rimOffset, 0), new Color(253, 241, 186), new Vector2(u, 0)));
                rimVertices.Add(new VertexPositionColorTexture(new Vector3(screenSurface + rimOffset, 0), new Color(253, 241, 186), new Vector2(u, 1)));
            }

            int shaderID = GameShaders.Armor.GetShaderIdFromItemId(ItemID.TwilightDye);
            if (shaderID != 0)
            {
                ArmorShaderData shaderData = GameShaders.Armor.GetSecondaryShader(shaderID, Main.LocalPlayer);
                sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
                gd.Textures[0] = TextureAssets.MagicPixel.Value;
                shaderData.Apply(Projectile);

                if (bodyVertices.Count >= 3)
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, bodyVertices.ToArray(), 0, bodyVertices.Count - 2);

                sb.End();
            }

            if (basicEffect == null)
            {
                basicEffect = new BasicEffect(gd)
                {
                    VertexColorEnabled = true
                };
            }

            basicEffect.Projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);
            basicEffect.View = Main.GameViewMatrix.TransformationMatrix;

            sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                if (rimVertices.Count >= 3)
                    gd.DrawUserPrimitives(PrimitiveType.TriangleStrip, rimVertices.ToArray(), 0, rimVertices.Count - 2);
            }

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}