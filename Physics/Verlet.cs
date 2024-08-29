using ITD.Physics;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace ITD.Physics
{
    public class VerletPoint // Defines VerletPoint
    {
        public int ID { get; set; }
        public Vector2 pos { get; set; }
        public Vector2 oldPos { get; set; }
        public bool isVerletPinned { get; set; }
        public void Update()
        {
            if (!isVerletPinned)
            {
                Vector2 velocity = pos - oldPos;
                oldPos = pos;
                pos += velocity;
                pos += new Vector2(0, PhysicsMethods.gravity);
                /*
                Main.NewText("point");
                Dust d = Dust.NewDustPerfect(pos, 1);
                d.velocity = Vector2.Zero;
                d.noGravity = true;
                */
            }
        }
    }

    public class VerletStick // Defines VerletStick
    {
        public int ID { get; set; }
        public VerletPoint pointA { get; set; }
        public VerletPoint pointB { get; set; }
        public float length { get; set; }

        public void Update()
        {
            if (pointB != null && pointA != null)
            {
                float dX = pointB.pos.X - pointA.pos.X;
                float dY = pointB.pos.Y - pointA.pos.Y;
                float distance = Vector2.Distance(pointA.pos, pointB.pos);
                float difference = length - distance;
                if (distance > 0)
                {
                    float percent = difference / distance / 2f;
                    float offsetX = dX * percent;
                    float offsetY = dY * percent;
                    if (!pointA.isVerletPinned)
                    {
                        pointA.pos -= new Vector2(offsetX, offsetY);
                    }
                    if (!pointB.isVerletPinned)
                    {
                        pointB.pos += new Vector2(offsetX, offsetY);
                    }
                }
            }
        }
    }

    public class VerletChain // Defines VerletChain. Contains the first and last sticks, then a list of all of the inbetween sticks.
    {
        public VerletStick startStick { get; set; }
        public VerletStick endStick { get; set; }
        public List<VerletStick> allSticks { get; set; }

        public void Update(Vector2 startPos, Vector2 endPos)
        {
            if (this != null)
            {
                startStick.pointA.pos = startPos;
                endStick.pointB.pos = endPos;

                // Ensure the constraints are satisfied multiple times per frame
                for (int i = 0; i < PhysicsMethods.ConstraintIterations; i++)
                {
                    foreach (var stick in allSticks)
                    {
                        stick.Update();
                    }
                }
            }
        }

        public void UpdateStart(Vector2 startPos)
        {
            if (this != null)
            {
                startStick.pointA.pos = startPos;
            }
        }

        public void Kill()
        {
            foreach (VerletStick stick in allSticks)
            {
                stick.pointA = null;
                stick.pointB = null;
            }
            allSticks.Clear();
            allSticks = null;
            endStick = null;
            startStick = null;
            PhysicsMethods.GetChains().Remove(this);
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Texture2D texture, Color drawColor, bool useLighting, Texture2D texture2 = null, Texture2D startTexture = null, Texture2D endTexture = null)
        {
            bool hasAlternatingTexture = texture2 != null;
            bool hasStartTexture = startTexture != null;
            bool hasEndTexture = endTexture != null;
            for (int i = 0; i < allSticks.Count; i++)
            {
                bool isStart = hasStartTexture && i == 0;
                bool isEnd = hasEndTexture && i == allSticks.Count - 1;
                if (hasAlternatingTexture && i % 2 == 0 && !isStart && !isEnd)
                    continue;
                VerletStick stick = allSticks[i];
                Vector2 chainCenter = Vector2.Lerp(stick.pointA.pos, stick.pointB.pos, 0.5f);
                float angle = (stick.pointB.pos - stick.pointA.pos).ToRotation();
                Texture2D textureToDraw = texture;
                Color lighting = Lighting.GetColor(chainCenter.ToTileCoordinates());
                Color color = useLighting ? lighting : drawColor;
                if (isStart)
                    textureToDraw = startTexture;
                if (isEnd)
                    textureToDraw = endTexture;
                spriteBatch.Draw(textureToDraw, chainCenter - screenPos, null, color, angle, textureToDraw.Size() / 2f, 1f, SpriteEffects.None, 0f);
            }
            if (hasAlternatingTexture)
            {
                for (int i = 0; i < allSticks.Count; i++)
                {
                    if (i % 2 == 0)
                    {
                        VerletStick stick = allSticks[i];
                        Vector2 chainCenter = Vector2.Lerp(stick.pointA.pos, stick.pointB.pos, 0.5f);
                        float angle = (stick.pointB.pos - stick.pointA.pos).ToRotation();
                        spriteBatch.Draw(texture2, chainCenter - screenPos, null, drawColor, angle, texture2.Size() / 2f, 1f, SpriteEffects.None, 0f);
                    }
                }
            }
        }
    }

    public static class PhysicsMethods
    {
        public static readonly float gravity = 0.5f;
        public static readonly int ConstraintIterations = 10;
        private static List<VerletPoint> points = new List<VerletPoint>();

        public static VerletPoint CreateVerletPoint(int ID, Vector2 pos, bool isVerletPinned)
        {
            var point = new VerletPoint { ID = ID, pos = pos, oldPos = pos, isVerletPinned = isVerletPinned };
            points.Add(point);
            return point;
        }

        public static List<VerletPoint> GetPoints()
        {
            return points;
        }

        private static List<VerletStick> sticks = new List<VerletStick>();

        public static VerletStick CreateVerletStick(int ID, VerletPoint pointA, VerletPoint pointB, float length)
        {
            var stick = new VerletStick { ID = ID, pointA = pointA, pointB = pointB, length = length };
            sticks.Add(stick);
            return stick;
        }

        public static List<VerletStick> GetSticks()
        {
            return sticks;
        }

        private static List<VerletChain> chains = new List<VerletChain>();

        public static VerletChain CreateVerletChain(int segmentsNum, float segmentLength, Vector2 posStart, Vector2 posEnd, bool pinStart = true, bool pinEnd = false, float startLength = 0f, float endLength = 0f)
        {
            startLength = startLength == 0f ? segmentLength : startLength;
            endLength = endLength == 0f ? segmentLength : endLength;
            if (!(chains.Count > 0))
            {
                ClearAll();
            }
            Vector2 chainLength = posEnd - posStart;
            Vector2 segmentVector = chainLength / segmentsNum;
            List<VerletPoint> chainPoints = [];
            List<VerletStick> chainSticks = [];
            VerletStick startStick = null;
            VerletStick endStick = null;
            for (int i = 0; i < segmentsNum + 1; i++)
            {
                Vector2 pointPos = posStart + (segmentVector * i);
                bool shouldBePinned = (i == 0 && pinStart) || (i == segmentsNum && pinEnd);
                var point = new VerletPoint { ID = 1, pos = pointPos, oldPos = pointPos, isVerletPinned = shouldBePinned };
                points.Add(point);
                chainPoints.Add(point);
            }
            for (int i = 0; i < segmentsNum; i++)
            {
                var stick = new VerletStick { ID = (i % 2 == 0 ? 1 : 2), length = segmentLength, pointA = chainPoints[i], pointB = chainPoints[i + 1] };
                if (stick.pointA == chainPoints[0])
                {
                    stick.length = startLength;
                    startStick = stick;
                }
                else if (stick.pointA == chainPoints[segmentsNum - 1])
                {
                    stick.length = endLength;
                    endStick = stick;
                }
                sticks.Add(stick);
                chainSticks.Add(stick);
            }
            var chain = new VerletChain { endStick = endStick, startStick = startStick, allSticks = chainSticks };
            chains.Add(chain);
            return chain;
        }

        public static List<VerletChain> GetChains()
        {
            return chains;
        }

        public static void ClearAll()
        {
            sticks.Clear();
            points.Clear();
            chains.Clear();
        }
    }
}