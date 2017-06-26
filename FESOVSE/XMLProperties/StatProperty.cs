using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace FESOVSE.XMLProperties
{
    class StatProperty
    {
        public static readonly DependencyProperty MyStatProperty = DependencyProperty.RegisterAttached(
            "Stat",
             typeof(Enum.Stat),
             typeof(StatProperty),
             new PropertyMetadata(default(Enum.Stat)));

        public static void SetStat(UIElement element, Enum.Stat value)
        {
            element.SetValue(MyStatProperty, value);
        }

        public static Enum.Stat GetStat(UIElement element)
        {
            return (Enum.Stat)element.GetValue(MyStatProperty);
        }
    }
}
