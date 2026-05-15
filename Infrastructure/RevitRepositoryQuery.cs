using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HQPRVTAI.Infrastructure
{
    public interface IRevitRepositoryQuery
    {
        IReadOnlyList<FamilyInstance> GetAllBeams(Document doc);
        IReadOnlyList<FamilyInstance>? PickBeams(UIDocument uiDoc);
        FamilyInstance? PickBeam(UIDocument uiDoc);
        IReadOnlyList<FamilyInstance> GetAllColumns(Document doc);
        IReadOnlyList<FamilyInstance>? PickColumns(UIDocument uiDoc);
        FamilyInstance? PickColumn(UIDocument uiDoc);
        IReadOnlyList<FamilyInstance> GetIntersectingColumns(Document doc, FamilyInstance beam);
        ViewFamilyType? GetSectionViewFamilyType(Document doc);
    }

    public sealed class RevitRepositoryQuery : IRevitRepositoryQuery
    {
        private const double BoundingBoxTolerance = 1.0 / 304.8; // 1 mm, Revit internal feet.

        public IReadOnlyList<FamilyInstance> GetAllBeams(Document doc) =>
            new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_StructuralFraming)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(fi => fi.StructuralType == StructuralType.Beam)
                .ToList();

        public IReadOnlyList<FamilyInstance>? PickBeams(UIDocument uiDoc)
        {
            try
            {
                var filter = new DelegateSelectionFilter(e => e is FamilyInstance fi &&
                    fi.Category.Id.Value == (int)BuiltInCategory.OST_StructuralFraming &&
                    fi.StructuralType == StructuralType.Beam);

                IList<Reference> refs = uiDoc.Selection.PickObjects(
                    ObjectType.Element, filter,
                    "Chọn các dầm cần tạo section, nhấn Finish khi xong");

                return refs
                    .Select(r => uiDoc.Document.GetElement(r))
                    .Cast<FamilyInstance>()
                    .ToList();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }
        }

        public FamilyInstance? PickBeam(UIDocument uiDoc)
        {
            try
            {
                var filter = new DelegateSelectionFilter(e => e is FamilyInstance fi &&
                    fi.Category.Id.Value == (int)BuiltInCategory.OST_StructuralFraming &&
                    fi.StructuralType == StructuralType.Beam);

                Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, filter,"Select beam");

                var element = uiDoc.Document.GetElement(reference);

                return element as FamilyInstance;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }
        }

        public IReadOnlyList<FamilyInstance> GetAllColumns(Document doc) =>
            new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType()
                .Cast<FamilyInstance>()
                .Where(fi => fi.StructuralType == StructuralType.Column)
                .ToList();
        public IReadOnlyList<FamilyInstance> GetIntersectingColumns(Document doc, FamilyInstance beam)
        {
            if (beam.Location is not LocationCurve beamLoc || beamLoc.Curve is not Line beamLine)
                return Array.Empty<FamilyInstance>();

            BoundingBoxXYZ? beamBox = beam.get_BoundingBox(null);
            if (beamBox == null)
                return Array.Empty<FamilyInstance>();

            XYZ beamStart = beamLine.GetEndPoint(0);
            XYZ beamEnd = beamLine.GetEndPoint(1);
            XYZ beamDir = (beamEnd - beamStart).Normalize();
            double beamLength = beamLine.Length;

            var outline = new Outline(
                new XYZ(
                    beamBox.Min.X - BoundingBoxTolerance,
                    beamBox.Min.Y - BoundingBoxTolerance,
                    beamBox.Min.Z - BoundingBoxTolerance),
                new XYZ(
                    beamBox.Max.X + BoundingBoxTolerance,
                    beamBox.Max.Y + BoundingBoxTolerance,
                    beamBox.Max.Z + BoundingBoxTolerance));

            var candidates = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .WhereElementIsNotElementType()
                .WherePasses(new BoundingBoxIntersectsFilter(outline));

            var intersectingColumns = new List<(FamilyInstance Column, double Projection)>();
            foreach (FamilyInstance column in candidates)
            {
                if (column.StructuralType != StructuralType.Column)
                    continue;

                if (column.Location is LocationPoint colLoc)
                {
                    XYZ colPoint = colLoc.Point;
                    XYZ vecToCol = colPoint - beamStart;
                    double projection = vecToCol.DotProduct(beamDir);

                    if (projection >= 0 && projection <= beamLength)
                    {
                        intersectingColumns.Add((column, projection));
                    }
                }
            }

            intersectingColumns.Sort((left, right) => left.Projection.CompareTo(right.Projection));

            var result = new List<FamilyInstance>(intersectingColumns.Count);
            foreach (var item in intersectingColumns)
            {
                result.Add(item.Column);
            }

            return result;
        }

        public IReadOnlyList<string> GetAllViewNames(Document doc) =>
            new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Select(v => v.Name)
                .ToList();

        public ViewFamilyType? GetViewType(Document doc, ViewFamily view) =>
            new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(v => v.ViewFamily == view);

        public IReadOnlyList<FamilyInstance>? PickColumns(UIDocument uiDoc)
        {
            try
            {
                var filter = new DelegateSelectionFilter(e => e is FamilyInstance fi &&
                    fi.Category.Id.Value == (int)BuiltInCategory.OST_StructuralColumns &&
                    fi.StructuralType == StructuralType.Column);

                IList<Reference> refs = uiDoc.Selection.PickObjects(
                    ObjectType.Element, filter,
                    "Chọn các cột cần tạo section, nhấn Finish khi xong");

                return refs
                    .Select(r => uiDoc.Document.GetElement(r))
                    .Cast<FamilyInstance>()
                    .ToList();
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }
        }

        public FamilyInstance? PickColumn(UIDocument uiDoc)
        {
            try
            {
                var filter = new DelegateSelectionFilter(e => e is FamilyInstance fi &&
                    fi.Category.Id.Value == (int)BuiltInCategory.OST_StructuralColumns &&
                    fi.StructuralType == StructuralType.Column);

                Reference reference = uiDoc.Selection.PickObject(ObjectType.Element, filter);

                var element = uiDoc.Document.GetElement(reference);

                return element as FamilyInstance;
            }

            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return null;
            }
        }

        public ViewFamilyType? GetSectionViewFamilyType(Document doc) =>
            new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(v => v.ViewFamily == ViewFamily.Section);
    }

    public class DelegateSelectionFilter : ISelectionFilter
    {
        private readonly Func<Element, bool> _allowElement;
        public DelegateSelectionFilter(Func<Element, bool> allowElement)
        {
            _allowElement = allowElement ?? throw new ArgumentNullException(nameof(allowElement));
        }

        public bool AllowElement(Element element) => _allowElement(element);

        public bool AllowReference(Reference reference, XYZ position) => true;
    }
}
