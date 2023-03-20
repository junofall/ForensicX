using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace ForensicX.Views
{
    public class VariableRowHeightWrapPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            double maxHeightInRow = 0;
            double rowWidth = 0;
            double totalHeight = 0;
            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);

                if (rowWidth + child.DesiredSize.Width > availableSize.Width)
                {
                    totalHeight += maxHeightInRow;
                    maxHeightInRow = 0;
                    rowWidth = 0;
                }

                rowWidth += child.DesiredSize.Width;
                maxHeightInRow = Math.Max(maxHeightInRow, child.DesiredSize.Height);
            }

            totalHeight += maxHeightInRow;
            return new Size(availableSize.Width, totalHeight);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double maxHeightInRow = 0;
            double rowWidth = 0;
            double totalHeight = 0;

            foreach (UIElement child in Children)
            {
                if (rowWidth + child.DesiredSize.Width > finalSize.Width)
                {
                    totalHeight += maxHeightInRow;
                    maxHeightInRow = 0;
                    rowWidth = 0;
                }

                child.Arrange(new Rect(rowWidth, totalHeight, child.DesiredSize.Width, child.DesiredSize.Height));
                rowWidth += child.DesiredSize.Width;
                maxHeightInRow = Math.Max(maxHeightInRow, child.DesiredSize.Height);
            }

            return finalSize;
        }
    }
}
