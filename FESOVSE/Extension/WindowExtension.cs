using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace FESOVSE.Extension
{
    public static class WindowExtension
    {

        /* Code snippet from https://stackoverflow.com/questions/974598/find-all-controls-in-wpf-window-by-type */
        /* Used to get all controls specified from the xaml */
        public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
    }
}
