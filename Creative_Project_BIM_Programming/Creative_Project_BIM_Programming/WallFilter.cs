using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Creative_Project_BIM_Programming
{
    public class WallFilter : ISelectionFilter
    {
        public bool AllowElement(Element e)
        {
            return (e.Category.Id.IntegerValue.Equals(
                (int)BuiltInCategory.OST_Walls));
        }
        public bool AllowReference(Reference r, XYZ p)
        {
            return false;
        }
    }
}
