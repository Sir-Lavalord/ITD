using Microsoft.Xna.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent.ItemDropRules;

namespace ITD.Physics
{
    public class VerletPoint // Defines VerletPoint
    {
        public int ID { get; set; }
        public Vector2 pos { get; set; }
        public Vector2 oldPos { get; set; }
        public bool isVerletPinned {  get; set; }
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
                    float percent = difference / distance / 2;
                    float offsetX = dX * percent;
                    float offsetY = dY * percent;
                    if (!pointA.isVerletPinned) // If a point is pinned it shouldn't be moved with the stick.
                    {
                        pointA.pos = pointA.pos - new Vector2(offsetX, offsetY);
                    }
                    if (!pointB.isVerletPinned)
                    {
                        pointB.pos = pointB.pos + new Vector2(offsetX, offsetY);
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
            }
        }

        public void Kill()
        {
            allSticks.Clear();
            PhysicsMethods.GetChains().Remove(this);
        }
    }

    public static class PhysicsMethods
    {
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
            var stick = new VerletStick { ID = ID, pointA = pointA, pointB = pointB, length = length};
            sticks.Add(stick);
            return stick;
        }

        public static List<VerletStick> GetSticks()
        {
            return sticks;
        }

        private static List<VerletChain> chains = new List<VerletChain>();
        public static VerletChain CreateVerletChain(int segmentsNum, float segmentLength, Vector2 posStart, Vector2 posEnd)
        {
            if (!(chains.Count > 0))
            {
                ClearAll();
            }
            Vector2 chainLength = posEnd - posStart;
            Vector2 segmentVector = chainLength / segmentsNum;
            List<VerletPoint> chainPoints = new List<VerletPoint>();
            List<VerletStick> chainSticks = new List<VerletStick>();
            VerletStick startStick = null;
            VerletStick endStick = null;
            for (int i = 0; i < segmentsNum+1; i++)
            {
                Vector2 pointPos = posStart + (segmentVector * i);
                bool shouldBePinned = (i == 0 || i == segmentsNum);
                var point = new VerletPoint { ID = 1, pos = pointPos, oldPos = pointPos, isVerletPinned = shouldBePinned};
                points.Add(point);
                chainPoints.Add(point);
            }
            for (int i = 0; i < segmentsNum; i++)
            {
                var stick = new VerletStick { ID = (i % 2 == 0 ? 1 : 2), length = segmentLength, pointA = chainPoints[i], pointB = chainPoints[i+1] };
                if (stick.pointA == chainPoints[0]){
                    startStick = stick;
                }
                else if (stick.pointA == chainPoints[segmentsNum-1])
                {
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
