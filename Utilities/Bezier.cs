namespace ITD.Utilities
{
    public static class Bezier
    {
        public static Vector2[] Quadratic(Vector2 start, Vector2 mid, Vector2 end, int resolution)
        {
            Vector2[] result = new Vector2[resolution];
            for (int i = 0; i < resolution; i++)
            {
                float t = i / (float)resolution;
                Vector2 point = (1 - t) * (1 - t) * start + 2 * (1 - t) * t * mid + t * t * end;
                result[i] = point;
            }
            return result;
        }
        public static float[] Rotations(Vector2[] positions)
        {
            int res = positions.Length;
            float[] rotations = new float[res];
            for (int i = 0; i < res; i++)
            {
                Vector2 cur = positions[i];
                Vector2 next = i < res - 1 ? positions[i + 1] : positions[i];
                Vector2 diff = next - cur;

                if (diff == Vector2.Zero)
                    rotations[i] = i > 0 ? rotations[i - 1] : 0;
                else
                    rotations[i] = diff.ToRotation();
            }
            return rotations;
        }
    }
}
