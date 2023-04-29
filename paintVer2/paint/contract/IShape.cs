﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Contract
{
    public interface IShape
    {
        string Name { get; }
       
        string Icon { get; }
        SolidColorBrush SolidColorBrush { get; set; }

        int Thickness { get; set; }

        DoubleCollection Stroke { get; set; }

        void HandleStart(double x, double y);
        void HandleEnd(double x, double y);

        /*public void HandleEnd(Point point)
        {
            HandleEnd(point.X, point.Y);
        }
        public void HandleStart(Point point)
        {
            HandleStart(point.X, point.Y);
        }*/


        IShape Clone();
        UIElement Draw(SolidColorBrush brush, double thickness, DoubleCollection line);
        UIElement Draw();

        byte[] Serialize();
        IShape Deserialize(byte[] data);



    }
}
