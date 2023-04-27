using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace contract
{
    public class Point2D : IShape
    {
        public string Name => throw new NotImplementedException();

        public string Icon => throw new NotImplementedException();

        public SolidColorBrush solidColorBrush => throw new NotImplementedException();

        public int Thickness { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public DoubleCollection Stroke { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
