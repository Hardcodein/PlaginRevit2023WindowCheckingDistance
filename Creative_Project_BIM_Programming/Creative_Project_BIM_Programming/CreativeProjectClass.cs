#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Creative_Project_BIM_Programming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.RightsManagement;
using System.Text;
using System.Windows.Controls;
using System.Xml.Linq;

#endregion

[TransactionAttribute(TransactionMode.Manual)]
[RegenerationAttribute(RegenerationOption.Manual)]
public class CreativeProjectClass : IExternalCommand
{
    
    
    public Result Execute(
      ExternalCommandData commandData,
      ref string message,
      ElementSet elements)
    {
        
        UIApplication uiapp = commandData.Application;
        UIDocument uidoc = uiapp.ActiveUIDocument;
        Application app = uiapp.Application;
        Document doc = uidoc.Document;
        //Фильтр всех элементов
        FilteredElementCollector Collector = new FilteredElementCollector(doc);

        //Фильтр для окон, которые собирает только экземпляры
        FilteredElementCollector myWindows
           = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .OfCategory(BuiltInCategory.OST_Windows);

        ICollection<Element> levels = Collector.OfClass(typeof(Level)).ToElements();


        //Выбор
        Reference refElement = null; // Ссылка на элемент
        Selection sel = uidoc.Selection;
        WallFilter selFilter = new WallFilter();
        refElement = sel.PickObject(ObjectType.Element, selFilter, "Пожалуйста,выберите стену");
        if (refElement == null)
        {
            return Result.Failed;
        }
        Set_Distance_Window set_Distance_Window = new Set_Distance_Window(doc);
        set_Distance_Window.ShowDialog();
        if(!set_Distance_Window.IsCorrectDistance)
        {
            return Result.Failed;
        }
        TaskDialog.Show("Введённые координаты",set_Distance_Window.Return_Distance().ToString());
        double  Distance_Window_User = set_Distance_Window.Return_Distance();

        Element elem = doc.GetElement(refElement);
        HostObject hostObj = elem as HostObject;
        IList<ElementId> elId = hostObj.FindInserts(false, false, false, false);
        if (elId.Count == 0)
        {
            TaskDialog.Show("Нет размещенных объетов", "Выберите хост-стену");
            return Result.Failed;

        }


        List<ElementId> filterIds = new List<ElementId>();
        foreach (ElementId id in elId)
        {

            foreach (Element w in myWindows)
            {
                if (w.Id.Compare(id) == 0)
                {
                    filterIds.Add(w.Id);
                }
            }
        }
        List<ElementId> WrongFiltersId = new List<ElementId>();
        Options options = new Options();
        options.ComputeReferences = true;
        Dictionary<XYZ, int> map = new Dictionary<XYZ, int>();
        ElementId FirstWindow = filterIds.First();
        ElementId LastWindow = filterIds.Last();
        for (int i = 0; i < filterIds.Count - 1; i++)
        {  
            if (i ==filterIds.Count -1 )
            {
                break;
            }
            FamilyInstance fi = doc.GetElement(filterIds[i]) as FamilyInstance;
            FamilySymbol fs = fi.Symbol;
            double width = fs.get_Parameter(BuiltInParameter.WINDOW_WIDTH).AsDouble();
          
            double DistanceBetweenWindowRevit = Math.Abs((UnitUtils.ConvertFromInternalUnits(getEndPointOfElement(doc.GetElement(filterIds[i])).Y, UnitTypeId.Millimeters)) - (UnitUtils.ConvertFromInternalUnits(getEndPointOfElement(doc.GetElement(filterIds[i + 1])).Y, UnitTypeId.Millimeters))) - UnitUtils.ConvertFromInternalUnits(width, UnitTypeId.Millimeters);

            if((DistanceBetweenWindowRevit < Distance_Window_User) && (filterIds[i+1] !=filterIds.Last()))
            {
                WrongFiltersId.Add(filterIds[i+1]);
            }
            if ((DistanceBetweenWindowRevit < Distance_Window_User) && (filterIds[i + 1] == filterIds.Last()))
            {
                WrongFiltersId.Add(filterIds[i]);
            }

        }

        //Подсветка окон
        uidoc.Selection.SetElementIds(WrongFiltersId);
        //Обновление отображения активного представления в активном документе.
        uidoc.RefreshActiveView();

        // Изменить документ в транзакции

        using (Transaction tx = new Transaction(doc))
        {
            tx.Start("Transaction Name");
            tx.Commit();
        }


        return Result.Succeeded;


    }
    public static XYZ getEndPointOfElement(Element ele)
    {
        IList<Solid> solids = GetTargetSolids(ele);
        Curve curve = solids.ElementAt(0).Edges.get_Item(0).AsCurve();
        XYZ endPoint = curve.GetEndPoint(0);
        return endPoint;
    }
    public static IList<Solid> GetTargetSolids(Element element)
    {
        List<Solid> solids = new List<Solid>();


        Options options = new Options();
        options.DetailLevel = ViewDetailLevel.Fine;
        GeometryElement geomElem = element.get_Geometry(options);
        foreach (GeometryObject geomObj in geomElem)
        {
            if (geomObj is Solid)
            {
                Solid solid = (Solid)geomObj;

                if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                {
                    solids.Add(solid);
                }
                // Single-level recursive check of instances. If viable solids are more than
                // one level deep, this example ignores them.
            }
            else if (geomObj is GeometryInstance)
            {
                GeometryInstance geomInst = (GeometryInstance)geomObj;
                GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                foreach (GeometryObject instGeomObj in instGeomElem)
                {
                    if (instGeomObj is Solid)
                    {
                        Solid solid = (Solid)instGeomObj;
                        if (solid.Faces.Size > 0 && solid.Volume > 0.0)
                        {
                            solids.Add(solid);
                        }
                    }
                }
            }
        }
        return solids;
    }

}