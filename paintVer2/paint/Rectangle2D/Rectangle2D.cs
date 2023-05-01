using Contract;
using System;
using System.IO;
using System.Security.Policy;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = Contract.Point;

namespace Rectangle2D
{
    public class Rectangle2D : IShape
    {
        public string Name => "Rectangle";
        public string Icon => "images/rectangle.png";
        public SolidColorBrush SolidColorBrush { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection Stroke { get; set; }

        private Point start = new Point();
        private Point end = new Point();


        public IShape Clone (){ 
            return new Rectangle2D();
        }
        public UIElement Draw()
        {
            var left = Math.Min(start.X, end.X);
            var top = Math.Min(start.Y, end.Y);

            var right = Math.Max(start.X, end.X);
            var bottom = Math.Max(start.Y, end.Y);
            
            var width = right - left;
            var height = bottom - top;

            var rec = new Rectangle()
            {
                Height = height,
                Width = width,

                StrokeThickness = Thickness,
                Stroke = SolidColorBrush,
                StrokeDashArray = Stroke

            };
            Canvas.SetLeft(rec, left);
            Canvas.SetTop(rec, top);
            return rec;
        }


        public UIElement Draw(SolidColorBrush brush , double thickness, DoubleCollection stroke )
        {
            this.SolidColorBrush= brush;
            this.Stroke = stroke;
            this.Thickness = (int)thickness;
            return Draw();
        }

        public void HandleEnd(double x, double y)
        {
            end = new Point() { X = x, Y = y };
        }

        public void HandleStart(double x, double y)
        {
            start = new Point() { X = x, Y = y };
        }

        public byte[] Serialize()
        {
            using (MemoryStream data = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(data))
                {
                    writer.Write(start.Serialize());
                    writer.Write(end.Serialize());
                    writer.Write(SolidColorBrush.ToString());
                    writer.Write(Thickness);
                    writer.Write(Stroke.ToString());

                    using (MemoryStream content = new MemoryStream())
                    {
                        using (BinaryWriter writer1 = new BinaryWriter(content))
                        {
                            writer1.Write(Name);
                            writer1.Write(data.Length);
                            writer1.Write(data.ToArray());

                            return content.ToArray();
                        }
                    }
                }
            }
        }



        public IShape Deserialize(byte[] data)
        {
            Rectangle2D result = new Rectangle2D();
            using (MemoryStream dataStream = new MemoryStream(data))
            {
                using (BinaryReader reader = new BinaryReader(dataStream))
                {
                    reader.ReadString(); // Read name point
                    long sizeStart = reader.ReadInt64();
                    result.start = result.start.Deserialize(reader.ReadBytes((int)sizeStart)) as Point;

                    reader.ReadString(); // Read name point
                    long sizeEnd = reader.ReadInt64();
                    result.end = result.end.Deserialize(reader.ReadBytes((int)sizeEnd)) as Point;

                    BrushConverter brushConverter = new BrushConverter();
                    result.SolidColorBrush = brushConverter.ConvertFromString(reader.ReadString()) as SolidColorBrush;

                    result.Thickness = reader.ReadInt32();

                    DoubleCollectionConverter converter = new DoubleCollectionConverter();
                    result.Stroke = converter.ConvertFromString(reader.ReadString()) as DoubleCollection;

                    return result;
                }
            }
        }
    }
}
