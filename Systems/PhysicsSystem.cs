
using ITD.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace ITD.Systems
{
    public class PhysicsSystem : ModSystem
    {
        public static readonly int ConstraintIterations = 16;
        public override void PostUpdateProjectiles()
        {
            List<VerletPoint> pointsList = PhysicsMethods.GetPoints();

            foreach (VerletPoint point in pointsList)
            {
                point.Update();
            }

            List<VerletStick> sticksList = PhysicsMethods.GetSticks();

            for (int i = 0; i < ConstraintIterations; i++)
            {
                foreach (VerletStick stick in sticksList)
                {
                    stick.Update();
                }
            }
        }
    }
}
