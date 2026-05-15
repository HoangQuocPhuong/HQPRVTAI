using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using HQPRVTAI.Infrastructure;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

public class BeamLongitudinalSectionService : IExternalEventHandler
{
    private readonly ExternalEvent _event;
    private readonly IRevitRepositoryQuery _revitRepositoryQuery;

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
            var beam = _revitRepositoryQuery.PickBeam(uidoc);
            if (beam == null) return;

            ElementId typeId = beam.GetTypeId();

            Element? typeElem = doc.GetElement(typeId);

            double b = GetParamValue(typeElem, "b");

            double h = GetParamValue(typeElem, "h");

            if (beam.Location is not LocationCurve { Curve: Line line })
                return;

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

            double offset = 1000 / 304.8;
            double halfLength = line.Length / 2;

            var box = new BoundingBoxXYZ
            {
                Transform = transform,
                Min = new XYZ(-halfLength - offset, -h - offset, 0),
                Max = new XYZ(halfLength + offset, offset, b / 2)
            };

            var viewType = _revitRepositoryQuery.GetSectionViewFamilyType(doc);
            if (viewType == null) return;

            using var t = new Transaction(doc, "Create Beam Section");
            t.Start();

            ViewSection.CreateSection(doc, viewType.Id, box);

            t.Commit();
        }
        catch
        {
            // User cancelled.
        }
    }

    public string GetName() => "Create Beam Section";

    private static double GetParamValue(Element? elem, string paramName)
    {
        if (elem == null)
            return 0;

        Parameter? param = elem.LookupParameter(paramName);
        if (param != null && param.StorageType == StorageType.Double)
        {
            return param.AsDouble();
        }
        return 0;
    }
}
