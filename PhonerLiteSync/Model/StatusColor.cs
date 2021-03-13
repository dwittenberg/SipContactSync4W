using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace PhonerLiteSync.Model
{
    public enum StatusColor
    {
        Error = 0,
        Problematic,
        Ok,
    }

    public static class Colors
    {
        public static SolidColorBrush[] StatusBrushesBorder =
        {
            Brushes.Crimson,
            Brushes.Gold,
            Brushes.Gray,
        };

        public static SolidColorBrush[] StatusBrushesBg =
        {
            Brushes.LightCoral,
            Brushes.Gold,
            (SolidColorBrush)new BrushConverter().ConvertFrom("#DDDDDDDD")
        };
    }
}
