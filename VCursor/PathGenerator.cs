using System.Collections.Generic;
using SharpDX;

namespace VCursor
{
    internal static class PathGenerator
    {
        private static readonly NormalDistribution TargetDistribution = new NormalDistribution();
        private static readonly NormalDistribution MidpointDistribution = new NormalDistribution();

        public static Queue<MouseManager.MousePoint> GeneratePath(Vector2 start, Vector2 end)
        {
            var midPoint = (start - end) / 2;
            var bezierMidPoint = midPoint +
                                 new Vector2(
                                     (float) (midPoint.X / 4 * MidpointDistribution.NextGaussian()),
                                     (float) (midPoint.Y / 4 * MidpointDistribution.NextGaussian()));


            double[] input = { start.X, start.Y, bezierMidPoint.X, bezierMidPoint.Y, end.X, end.Y };

            const int numberOfDataPoints = 1000;
            var output = new double[numberOfDataPoints];

            var bc = new BezierCurve();
            bc.Bezier2D(input, numberOfDataPoints / 2, output);

            var pause = 0;
            var path = new Queue<MouseManager.MousePoint>();
            for (var count = 1; count != numberOfDataPoints - 1; count += 2)
            {
                if (count % 50 == 0)
                {
                    pause = 10 + (count ^ 2) / (count * 10);
                }

                var point = new Vector2((float) output[count + 1], (float) output[count]);
                path.Enqueue(new MouseManager.MousePoint(point, pause));
            }

            return path;
        }
    }
}