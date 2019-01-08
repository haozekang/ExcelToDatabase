using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace ExcelToDatabase.Contents
{
    public class VisualHost : FrameworkElement
    {
        Visual child;

        public VisualHost(Visual child)
        {
            if (child == null)
                throw new ArgumentException("child");

            this.child = child;
            AddVisualChild(child);
        }

        protected override Visual GetVisualChild(int index)
        {
            return (index == 0) ? child : null;
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }
    }
}
