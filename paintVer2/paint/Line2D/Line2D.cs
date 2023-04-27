using contract;

namespace Line2D
{
    public class Line2D:IShape
    {

        public SolidColorBrush Brush { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection Stroke { get; set; }

        public string Name => "line";
        public string Icon => "Images/line.png";


    }
}