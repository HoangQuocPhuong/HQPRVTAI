using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace HQPRVTAI.Infrastructure
{
    internal interface IRevitRepositoryQuery
    {
        IReadOnlyList<FamilyInstance> GetAllBeams(Document doc);
        IReadOnlyList<FamilyInstance>? PickBeams(UIDocument uiDoc);
        IReadOnlyList<FamilyInstance> GetAllColumns(Document doc);
        IReadOnlyList<FamilyInstance>? PickColumns(UIDocument uiDoc);

        ViewFamilyType? GetSectionViewFamilyType(Document doc);
    }

    internal sealed class RevitRepositoryQuery : IRevitRepositoryQuery
    {
        public IReadOnlyList<FamilyInstance> GetAllBeams(Document doc) =>
            new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_StructuralFraming)
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

        public IReadOnlyList<FamilyInstance> GetAllColumns(Document doc) =>
            new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                .Cast<FamilyInstance>()
                .Where(fi => fi.StructuralType == StructuralType.Column)
                .ToList();

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

        public ViewFamilyType? GetSectionViewFamilyType(Document doc) =>
            new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .FirstOrDefault(v => v.ViewFamily == ViewFamily.Section);
    }

    public class DelegateSelectionFilter(Func<Element, bool> allowElement) : ISelectionFilter
    {
        public bool AllowElement(Element element) => allowElement(element);

        public bool AllowReference(Reference reference, XYZ position) => true;
    }
}
