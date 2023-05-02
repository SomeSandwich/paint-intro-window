using Contract;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Rectangle2D;

public class Rectangle2D : IShape
{
    public string Name => "Rectangle";
    public string Icon => "images/rectangle.png";

    public System.Windows.Point Start { get; set; }
    public System.Windows.Point End { get; set; }

    public SolidColorBrush BrushColor { get; set; }
    public int BrushThickness { get; set; }
    public DoubleCollection BrushStyle { get; set; }


    public UIElement Draw(SolidColorBrush brush, double thickness, DoubleCollection stroke)
    {
        BrushColor = brush;
        BrushStyle = stroke;
        BrushThickness = (int)thickness;

        return Draw();
    }

    public UIElement Draw()
    {
        var left = Math.Min(Start.X, End.X);
        var top = Math.Min(Start.Y, End.Y);

        var right = Math.Max(Start.X, End.X);
        var bottom = Math.Max(Start.Y, End.Y);

        var width = right - left;
        var height = bottom - top;

        var shape = new Ellipse()
        {
            Width = width,
            Height = height,
            Stroke = BrushColor,
            StrokeThickness = BrushThickness,
            StrokeDashArray = BrushStyle
        };

        Canvas.SetLeft(shape, left);
        Canvas.SetTop(shape, top);

        return shape;
    }

    public void HandleStart(double x, double y)
    {
        Start = new System.Windows.Point(x, y);
    }

    public void HandleEnd(double x, double y)
    {
        End = new System.Windows.Point(x, y);
    }

    public byte[] Serialize()
    {
        try
        {
            using var detailData = new MemoryStream();
            using var writerDetail = new BinaryWriter(detailData);

            writerDetail.Write(Start.Serialize());
            writerDetail.Write(End.Serialize());
            writerDetail.Write(BrushColor.ToString());
            writerDetail.Write(BrushThickness);
            writerDetail.Write(BrushStyle.ToString());

            using var shapeData = new MemoryStream();
            using var writerShape = new BinaryWriter(shapeData);

            writerShape.Write(Name);
            writerShape.Write(detailData.Length);
            writerShape.Write(detailData.ToArray());

            return shapeData.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        return null;
    }

    public IShape Deserialize(byte[] data)
    {
        var result = new Rectangle2D();
        using var dataStream = new MemoryStream(data);
        using var reader = new BinaryReader(dataStream);

        // Start Point
        reader.ReadString();
        var szDetailStart = reader.ReadInt64();
        result.Start = result.Start.Deserialize(reader.ReadBytes((int)szDetailStart));

        // End Point
        reader.ReadString();
        var szDetailEnd = reader.ReadInt64();
        result.End = result.End.Deserialize(reader.ReadBytes((int)szDetailEnd));

        // Brush Color
        var brushConverter = new BrushConverter();
        result.BrushColor = brushConverter.ConvertFromString(reader.ReadString()) as SolidColorBrush;

        // Brush Thickness
        result.BrushThickness = reader.ReadInt32();

        // Brush Style
        var converter = new DoubleCollectionConverter();
        result.BrushStyle = converter.ConvertFromString(reader.ReadString()) as DoubleCollection;

        return result;
    }

    public IShape DeepClone()
    {
        return new Rectangle2D();
    }
}