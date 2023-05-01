using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Contract;

public class Point : IShape
{
    public double X { get; set; }
    public double Y { get; set; }

    public string Icon => "";

    public SolidColorBrush BrushColor { get; set; }
    public DoubleCollection BrushStyle { get; set; }

    public int BrushThickness { get; set; }
    public string Name => "Point";

    public bool isHovering(double x, double y)
    {
        return false;
    }


    public void HandleStart(double x, double y)
    {
        X = x;
        Y = y;
    }

    public void HandleEnd(double x, double y)
    {
        X = x;
        Y = y;
    }


    public UIElement Draw(SolidColorBrush brush, double thickness, DoubleCollection stroke)
    {
        this.BrushColor = brush;
        this.BrushStyle = stroke;
        this.BrushThickness = (int)thickness;
        return Draw();
    }

    public UIElement Draw()
    {
        Line line = new Line()
        {
            X1 = X,
            Y1 = Y,
            X2 = X,
            Y2 = Y,
            StrokeThickness = BrushThickness,
            Stroke = BrushColor,
            StrokeDashArray = BrushStyle
        };
        throw new System.NotImplementedException();
    }


    public IShape Clone()
    {
        return new Point();
    }

    public Point deepCopy()
    {
        Point temp = new Point();
        temp.Y = this.Y;
        temp.X = this.X;
        return temp;
    }

    public byte[] Serialize()
    {
        using (MemoryStream data = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(data))
            {
                writer.Write(X);
                writer.Write(Y);

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
        Point result = new Point();
        using (MemoryStream dataStream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(dataStream))
            {
                double x = reader.ReadDouble();
                double y = reader.ReadDouble();
                result.HandleStart(x, y);

                return result;
            }
        }
    }

    public IShape DeepClone()
    {
        throw new NotImplementedException();
    }
}