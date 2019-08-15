using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Automation;
using System.Windows.Input;

namespace TumblerTimePicker
{
    public static class ExtensionMethods
    {
        public static T FindChild<T>(this DependencyObject parent, string childName, int findIndex)
            where T : DependencyObject
        {
            int foundCount = 0;
            return FindChild<T>(parent, childName, findIndex, ref foundCount);
        }
        /// <summary>
        /// Finds a Child of a given item in the visual tree. 
        /// </summary>
        /// <param name="parent">A direct parent of the queried item.</param>
        /// <typeparam name="T">The type of the queried item.</typeparam>
        /// <param name="childName">x:Name or Name of child. </param>
        /// <param name="findIndex">the index of the item to be found.  0 to find the first name/type match, 1 to find the second match, etc </param>
        /// <param name="foundCount">recursion counter to keep track of the number of name/type matches found so far. </param>
        /// <returns>The first parent item that matches the submitted type parameter. 
        /// If not matching item can be found, a null parent is being returned.</returns>
        private static T FindChild<T>(DependencyObject parent, string childName, int findIndex, ref int foundCount)
           where T : DependencyObject
        {
            // Confirm parent and childName are valid. 
            if (parent == null) return null;

            T foundChild = null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                // If the child is not of the request child type child
                T childType = child as T;
                if (childType == null)
                {
                    // recursively drill down the tree
                    foundChild = FindChild<T>(child, childName, findIndex, ref foundCount);

                    // If the child is found, break so we do not overwrite the found child. 
                    if (foundChild != null) break;
                }
                else if (!string.IsNullOrEmpty(childName))
                {
                    var frameworkElement = child as FrameworkElement;
                    // If the child's name is set for search
                    if (frameworkElement != null && (frameworkElement.Name == childName || AutomationProperties.GetAutomationId(frameworkElement) == childName))
                    {
                        // if the child's name is of the request name
                        if (foundCount == findIndex)
                        {
                            foundChild = (T)child;
                            break;
                        }
                        else
                            foundCount++;
                    }
                    else
                    {
                        // recursively drill down the tree
                        foundChild = FindChild<T>(child, childName, findIndex, ref foundCount);

                        // If the child is found, break so we do not overwrite the found child. 
                        if (foundChild != null) break;
                    }
                }
                else
                {
                    // child element found.
                    foundChild = (T)child;
                    break;
                }
            }

            return foundChild;
        }

        /// <summary>
        /// Helper method to determine if the given framework element has the mouse over it or not
        /// </summary>
        /// <param name="grid">The FrameworkElement to test for mouse containment</param>
        /// <returns>true if the mouse is over the FrameworkElement, false otherwise</returns>
        public static bool ContainsMouse(this FrameworkElement grid)
        {
            Point pt = Mouse.GetPosition(grid);
            return pt.X >= 0 && pt.X <= grid.ActualWidth && pt.Y >= 0 && pt.Y <= grid.ActualHeight;
        }
    }
}
