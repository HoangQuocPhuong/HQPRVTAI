using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using HQPRVTAI.Infrastructure;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

public class BeamLongitudinalSectionService : IExternalEventHandler
{
    private ExternalEvent _event;
    private IRevitRepositoryQuery _revitRepositoryQuery;

    public BeamLongitudinalSectionService(IRevitRepositoryQuery revitRepositoryQuery)
    {
        _revitRepositoryQuery = revitRepositoryQuery;
        _event = ExternalEvent.Create(this);
    }

    public void Raise()
    {
        _event.Raise();
    }

    public void Execute(UIApplication app)
    {
        var uidoc = app.ActiveUIDocument;
        var doc = uidoc.Document;

        try
        {
            // 1. Pick beam
            var beam = _revitRepositoryQuery.PickBeam(uidoc);   

            ElementId typeId = beam.GetTypeId();

            Element typeElem = doc.GetElement(typeId);

            double b = GetParamValue(typeElem, "b");

            double h = GetParamValue(typeElem, "h");

            if (beam == null) return;

            var curve = (beam.Location as LocationCurve)?.Curve;
            if (curve == null) return;

            var line = curve as Line;
            if (line == null) return;

            var direction = (line.GetEndPoint(1) - line.GetEndPoint(0)).Normalize();
            var midpoint = line.Evaluate(0.5, true);

            // Vector vuông góc để tạo section
            var up = XYZ.BasisZ;
            var viewDir = direction.CrossProduct(up).Normalize();

            var transform = Transform.Identity;
            transform.Origin = midpoint;
            transform.BasisX = direction;
            transform.BasisY = up;
            transform.BasisZ = viewDir;

            // Bounding box cho section
            var box = new BoundingBoxXYZ
            {
                Transform = transform,
                Min = new XYZ(- line.Length / 2  - 1000 / 304.8, - h - 1000 / 304.8, 0),
                Max = new XYZ(line.Length / 2 + 1000 / 304.8, 1000 / 304.8, b / 2)
            };

            using var t = new Transaction(doc, "Create Beam Section");
            t.Start();

            var viewType = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .First(x => x.ViewFamily == ViewFamily.Section);

            ViewSection.CreateSection(doc, viewType.Id, box);

            t.Commit();
        }
        catch
        {
            // user cancel → ignore
        }
    }

    public string GetName() => "Create Beam Section";

    private double GetParamValue(Element elem, string paramName)
    {
        Parameter param = elem.LookupParameter(paramName);
        if (param != null && param.StorageType == StorageType.Double)
        {
            return param.AsDouble();
        }
        return 0;
    }
}