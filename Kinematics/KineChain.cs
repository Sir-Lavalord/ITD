using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;

namespace ITD.Kinematics
{
    public class KineChain
    {
        public const float BIAS = 3f;
        public const int ITERATIONS = 32;
        public Vector2 basePoint { get; set; }
        public const int LIMB_LEN = 0;
        public const int LIMB_MINANGLE = 1;
        public const int LIMB_MAXANGLE = 2;
        public float[][] limbs;
        public Vector2[] joints;
        float limbsLength
        {
            get
            {
                float add = 0;
                for (int i = 0; i < limbs.Length; i++)
                {
                    add += limbs[i][LIMB_LEN];
                }
                return add;
            }
        }
        public KineChain(float x, float y, params float[][] limbsP)
        {
            limbs = limbsP;
            joints = new Vector2[limbs.Length + 1];
            joints[0] = basePoint;
            for (int i = 0; i < limbs.Length; i++)
            {
                joints[i + 1] = joints[i] + new Vector2(limbs[i][LIMB_LEN], 0);
            }
        }
        public static float[] CreateKineSegment(float length, float minAngle = -MathF.Tau, float maxAngle = MathF.Tau)
        {
            return [length, minAngle, maxAngle];
        }
        public void GenUpdate(Vector2 target)
        {
            Vector2[] jointsP = [.. joints];
            // DEBUG: Add dust on joints
            /*
            for (int i = 0;i < joints.Length; i++)
            {
                Dust d = Dust.NewDustPerfect(joints[i], DustID.BlueTorch);
                d.noGravity = true;
            }
            */
            var dist = UpdateIK(target);
            if (dist > BIAS * BIAS)
            {
                joints = jointsP;
                UpdateIK(basePoint + (target - basePoint).SafeNormalize(Vector2.Zero) * (basePoint.Distance(joints[limbs.Length])));
            }
        }
        public float UpdateIK(Vector2 target)
        {
            var dist = basePoint.DistanceSQ(target);
            var iterations = 0;

            if (dist > limbsLength * limbsLength)
            {
                BackwardPass(target);
                ForwardPass();
                iterations = 1;
                return 0f;
            }
            var minDist = float.PositiveInfinity;
            Vector2[] minJoints = [];
            while (iterations < ITERATIONS)
            {
                BackwardPass(target);
                ForwardPass();
                iterations++;
                dist = joints[limbs.Length].DistanceSQ(target);
                if (minDist > dist)
                {
                    minDist = dist;
                    minJoints = [.. joints];
                }
                else
                {
                    break;
                }
            }
            if (dist > minDist)
            {
                joints = minJoints;
            }
            return joints[limbs.Length].DistanceSQ(target);
        }
        public void BackwardPass(Vector2 target)
        {
            joints[limbs.Length] = target;
            for (int i = limbs.Length; i > 0; i--)
            {
                var limb = limbs[i - 1];
                var a = joints[i];
                var b = joints[i - 1];
                var c = i > 1 ? joints[i - 2] : basePoint;
                var rootAngle = c.AngleTo(b);
                var rawDiffAngle = b.AngleTo(a) - rootAngle;
                var diffAngle = MathHelper.Clamp(rawDiffAngle, limb[LIMB_MINANGLE], limb[LIMB_MAXANGLE]);
                var angle = rootAngle + diffAngle;
                joints[i - 1] = a + new Vector2(limb[LIMB_LEN], 0).RotatedBy(angle + MathF.PI);
            }
        }
        public void ForwardPass()
        {
            var rootAngle = 0f;
            joints[0] = basePoint;
            for (int i = 0; i < limbs.Length; i++)
            {
                var limb = limbs[i];
                var a = joints[i];
                var b = joints[i + 1];
                var rawDiffAngle = MathHelper.WrapAngle(a.AngleTo(b) - rootAngle);
                var diffAngle = MathHelper.Clamp(rawDiffAngle, limb[LIMB_MINANGLE], limb[LIMB_MAXANGLE]);
                var angle = rootAngle + diffAngle;

                joints[i + 1] = a + new Vector2(limb[LIMB_LEN], 0).RotatedBy(angle);
                rootAngle = angle;
            }
        }
        public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Color color, bool shouldBeMirrored, Texture2D texture, Texture2D startTexture = null, Texture2D endTexture = null)
        {
            for (int i = limbs.Length - 1; i >= 0; i--)
            {
                Texture2D textureToDraw = i == limbs.Length - 1 && startTexture != null ? startTexture : i == 0 && endTexture != null ? endTexture : texture;
                limbs[i].Draw(joints, i, spriteBatch, screenPos, color, textureToDraw, shouldBeMirrored);
            }
        }
    }
    public static class KineExtensions
    {
        /// <summary>
        /// this is kinda cursed
        /// </summary>
        public static void Draw(this float[] limb, Vector2[] joints, int i, SpriteBatch spriteBatch, Vector2 screenPos, Color color, Texture2D tex, bool shouldBeMirrored)
        {
            if (limb.Length != 3)
                return;
            Vector2 a = joints[i];
            Vector2 b = joints[i + 1];
            Point tileCoords = Vector2.Lerp(a, b, 0.5f).ToTileCoordinates();
            spriteBatch.Draw(tex, a - screenPos, null, Lighting.GetColor(tileCoords.X, tileCoords.Y).MultiplyRGB(color), a.AngleTo(b), new Vector2(0f, tex.Height / 2f), 1f, shouldBeMirrored ? SpriteEffects.FlipVertically : SpriteEffects.None, default);
        }
    }
}
