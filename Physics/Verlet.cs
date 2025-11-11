using System;

namespace ITD.Physics;

public sealed class VerletChain
{
    public ref Vector2 Start => ref Points[0];
    public ref Vector2 End => ref Points[^1];
    public Vector2[] Points;
    public Vector2[] OldPoints;
    public bool[] Pins;
    public float[] Lengths;
    public VerletChain(int segmentsNum, float segmentLength, Vector2 posStart, Vector2 posEnd, bool pinStart = true, bool pinEnd = false, float startLength = 0f, float endLength = 0f)
    {
        startLength = startLength == 0f ? segmentLength : startLength;
        endLength = endLength == 0f ? segmentLength : endLength;
        Vector2 chainLength = posEnd - posStart;
        Vector2 segmentVector = chainLength / segmentsNum;

        Points = new Vector2[segmentsNum + 1];
        OldPoints = new Vector2[segmentsNum + 1];
        Pins = new bool[segmentsNum + 1];

        Lengths = new float[segmentsNum];
        Array.Fill(Lengths, segmentLength);

        for (int i = 0; i < segmentsNum + 1; i++)
        {
            Vector2 pointPos = posStart + (segmentVector * i);
            bool shouldBePinned = (i == 0 && pinStart) || (i == segmentsNum && pinEnd);
            OldPoints[i] = Points[i] = pointPos;
            Pins[i] = shouldBePinned;
        }
    }
    public void UpdatePhysics()
    {
        for (int i = 0; i < Points.Length; i++)
            UpdatePoint(i);

        for (int i = 0; i < PhysicsMethods.ConstraintIterations; i++)
            for (int j = 0; j < Lengths.Length; j++)
                UpdateStick(j);
    }
    public void Update(Vector2 startPos, Vector2 endPos)
    {
        Start = startPos;
        End = endPos;

        // Ensure the constraints are satisfied multiple times per frame
        for (int i = 0; i < PhysicsMethods.ConstraintIterations; i++)
            for (int j = 0; j < Lengths.Length; j++)
                UpdateStick(j);
    }
    public void UpdatePoint(int i)
    {
        if (Pins[i])
            return;

        ref Vector2 me = ref Points[i];
        ref Vector2 oldMe = ref OldPoints[i];

        Vector2 velo = me - oldMe;
        oldMe = me;
        me += velo;
        me.Y += PhysicsMethods.Gravity;
    }
    public void UpdateStick(int i)
    {
        ref float myLength = ref Lengths[i];
        ref Vector2 myA = ref Points[i];
        ref Vector2 myB = ref Points[i + 1];

        Vector2 d = myB - myA;
        float distance = d.Length();
        float difference = myLength - distance;
        if (distance > 0)
        {
            float percent = difference / distance * 0.5f;
            Vector2 offset = d * percent;
            if (!Pins[i])
                myA -= offset;
            if (!Pins[i + 1])
                myB += offset;
        }
    }

    public void UpdateStart(Vector2 startPos)
    {
        Start = startPos;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 screenPos, Texture2D texture, Color drawColor, bool useLighting, Texture2D texture2 = null, Texture2D startTexture = null, Texture2D endTexture = null)
    {
        bool hasAlternatingTexture = texture2 != null;
        bool hasStartTexture = startTexture != null;
        bool hasEndTexture = endTexture != null;
        for (int i = 0; i < Lengths.Length; i++)
        {
            bool isStart = hasStartTexture && i == 0;
            bool isEnd = hasEndTexture && i == Lengths.Length - 1;
            if (hasAlternatingTexture && i % 2 == 0 && !isStart && !isEnd)
                continue;
            Vector2 myA = Points[i];
            Vector2 myB = Points[i + 1];
            Vector2 chainCenter = Vector2.Lerp(myA, myB, 0.5f);
            float angle = (myB - myA).ToRotation();
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
            for (int i = 0; i < Lengths.Length; i++)
            {
                if (i % 2 == 0)
                {
                    Vector2 myA = Points[i];
                    Vector2 myB = Points[i + 1];
                    Vector2 chainCenter = Vector2.Lerp(myA, myB, 0.5f);
                    float angle = (myB - myA).ToRotation();
                    spriteBatch.Draw(texture2, chainCenter - screenPos, null, drawColor, angle, texture2.Size() / 2f, 1f, SpriteEffects.None, 0f);
                }
            }
        }
    }
}

public static class PhysicsMethods
{
    public const float Gravity = 0.5f;
    public const int ConstraintIterations = 10;
}