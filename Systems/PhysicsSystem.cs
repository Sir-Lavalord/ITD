
using ITD.Physics;
using System.Collections.Generic;

namespace ITD.Systems;

public class PhysicsSystem : ModSystem
{
    public const int ConstraintIterations = 16;
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
