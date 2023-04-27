using System;
using System.Windows;
using System.Windows.Media;

namespace contract
{
    public interface IShape
    {
        string Name { get; }
        string Icon { get; }
        SolidColorBrush solidColorBrush { get; set; }

        int Thickness { get; set; }
        DoubleCollection Stroke { get; set; }
    }
}
