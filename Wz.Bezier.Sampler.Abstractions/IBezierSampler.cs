using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wz.Bezier.Sampler.Abstractions
{
    public struct Vec2 : IVec2
    {
        public Vec2(double x, double y)
        {
            X = x;
            Y = y;
        }

        public double X { get; }
        public double Y { get; }
    }

    public interface IBezierSampler
    {
        IEnumerable<IVec2> Sample(int samplesPerSegment);
    }
}
