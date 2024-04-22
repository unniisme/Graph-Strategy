using System.Collections.Generic;
using Godot;

namespace Utils
{
    public static class Bezier
    {
        public static Vector2 QuadraticBezier(Vector2 p0, Vector2 p1, Vector2 p2, float t)
        {
            Vector2 q0 = p0.Lerp(p1, t);
            Vector2 q1 = p1.Lerp(p2, t);
            Vector2 r = q0.Lerp(q1, t);
            return r;
        }

        public static List<Vector2> QuadraticBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, int n)
        {
            List<Vector2> curve = new();
            float t = 1 / ((float)n);
            for (int i = 0; i < n;i++)
            {
                curve.Add(QuadraticBezier(p0,p1,p2,t*i));
            }
            return curve;
        }

        public static Vector2 CubicBezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            Vector2 q0 = p0.Lerp(p1, t);
            Vector2 q1 = p1.Lerp(p2, t);
            Vector2 q2 = p2.Lerp(p3, t);

            Vector2 r0 = q0.Lerp(q1, t);
            Vector2 r1 = q1.Lerp(q2, t);

            Vector2 s = r0.Lerp(r1, t);
            return s;
        }

        public static Vector2 CubicPointWiseBezier(Vector2 start, Vector2 end, Vector2 point, float t)
        {
            Vector2 diff = end - start;
            Vector2 diffDir = diff.Normalized();
            Vector2 pointDiff = point - start;
            Vector2 pp = pointDiff - diffDir.Dot(pointDiff) * diffDir;
            Vector2 p1 = start + pp;
            Vector2 p2 = end + pp;
            return CubicBezier(start, p1, p2, end, t);
        }

        public static List<Vector2> CubicBezierCurve(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, int n)
        {
            List<Vector2> curve = new();
            float t = 1 / ((float)n);
            for (int i = 0; i < n;i++)
            {
                curve.Add(CubicBezier(p0,p1,p2,p3,t*i));
            }
            return curve;
        }

        public static List<Vector2> CubicPointWiseBezierCurve(Vector2 start, Vector2 end, Vector2 point, int n)
        {
            Vector2 diff = end - start;
            Vector2 diffDir = diff.Normalized();
            Vector2 pointDiff = point - start;
            Vector2 pp = pointDiff - diffDir.Dot(pointDiff) * diffDir;
            Vector2 p1 = start + pp;
            Vector2 p2 = end + pp;

            List<Vector2> curve = new();
            float t = 1 / ((float)n);
            for (int i = 0; i < n;i++)
            {
                curve.Add(CubicBezier(start,p1,p2,end,t*i));
            }
            return curve;
        }

    }
}