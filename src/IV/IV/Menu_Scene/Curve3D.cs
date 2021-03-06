using System;
using Microsoft.Xna.Framework;

namespace IV.Menu_Scene
{
    class Curve3D
    {

        public Curve curveX = new Curve();
        public Curve curveY = new Curve();
        public Curve curveZ = new Curve();

        public Curve3D(CurveLoopType type)
        {
            curveX.PostLoop = type;
            curveY.PostLoop = type;
            curveZ.PostLoop = type;

            curveX.PreLoop = CurveLoopType.Oscillate;
            curveY.PreLoop = CurveLoopType.Oscillate;
            curveZ.PreLoop = CurveLoopType.Oscillate;
        }
        public void SetTangents()
        {
            CurveKey prev;
            CurveKey current;
            CurveKey next;
            int prevIndex;
            int nextIndex;
            for (int i = 0; i < curveX.Keys.Count; i++)
            {
                prevIndex = i - 1;
                if (prevIndex < 0) prevIndex = i;

                nextIndex = i + 1;
                if (nextIndex == curveX.Keys.Count) nextIndex = i;

                prev = curveX.Keys[prevIndex];
                next = curveX.Keys[nextIndex];
                current = curveX.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next);
                curveX.Keys[i] = current;
                prev = curveY.Keys[prevIndex];
                next = curveY.Keys[nextIndex];
                current = curveY.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next);
                curveY.Keys[i] = current;

                prev = curveZ.Keys[prevIndex];
                next = curveZ.Keys[nextIndex];
                current = curveZ.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next);
                curveZ.Keys[i] = current;
            }
        }
        static void SetCurveKeyTangent(ref CurveKey prev, ref CurveKey cur, 
            ref CurveKey next)
        {
            float dt = next.Position - prev.Position;
            float dv = next.Value - prev.Value;
            if (Math.Abs(dv) < float.Epsilon)
            {
                cur.TangentIn = 0;
                cur.TangentOut = 0;
            }
            else
            {
                // The in and out tangents should be equal to the 
                // slope between the adjacent keys.
                cur.TangentIn = dv * (cur.Position - prev.Position) / dt;
                cur.TangentOut = dv * (next.Position - cur.Position) / dt;
            }
        }

        public void Clear()
        {
            curveX.Keys.Clear();
            curveY.Keys.Clear();
            curveZ.Keys.Clear();
        }

        public void AddPoint(Vector3 point, float time)
        {
            curveX.Keys.Add(new CurveKey(time, point.X));
            curveY.Keys.Add(new CurveKey(time, point.Y));
            curveZ.Keys.Add(new CurveKey(time, point.Z));
        }
        public Vector3 GetPointOnCurve(float time)
        {
            var point = new Vector3 {X = curveX.Evaluate(time), Y = curveY.Evaluate(time), Z = curveZ.Evaluate(time)};
            return point;
        }
    }
}
