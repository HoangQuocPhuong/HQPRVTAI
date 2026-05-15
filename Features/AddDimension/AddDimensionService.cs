using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HQPRVTAI.Infrastructure;

namespace HQPRVTAI.Features.AddDimension;

public class AddDimensionService : IExternalEventHandler
{
    private const double ParallelFaceTolerance = 0.95;

    private readonly ExternalEvent _event;
    private readonly IRevitRepositoryQuery _revitRepositoryQuery;

    public AddDimensionService(IRevitRepositoryQuery revitRepositoryQuery)
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

            if (beam.Location is not LocationCurve beamLoc || beamLoc.Curve is not Line beamLine)
            {
                TaskDialog.Show("Error", "Beam location is not a line");
                return;
            }

            XYZ beamStart = beamLine.GetEndPoint(0);
            XYZ beamEnd = beamLine.GetEndPoint(1);
            XYZ beamDir = (beamEnd - beamStart).Normalize();

            var intersectingColumns = _revitRepositoryQuery.GetIntersectingColumns(doc, beam);
            if (intersectingColumns.Count < 2)
            {
                TaskDialog.Show("Info", "At least two columns must intersect with the beam");
                return;
            }

            var sectionView = GetActiveSectionView(doc);
            if (sectionView == null)
            {
                TaskDialog.Show("Error", "Active view must be a non-template section view");
                return;
            }

            var columnReferences = GetColumnDimensionReferences(intersectingColumns, beamDir, beamStart);

            using var transaction = new Transaction(doc, "Add Beam Dimension");
            transaction.Start();

            try
            {
                int dimensionCount = CreateDimensionsBetweenColumns(doc, sectionView, columnReferences);

                if (dimensionCount == 0)
                {
                    transaction.RollBack();
                    TaskDialog.Show("Info", "No valid column faces found for dimensions");
                    return;
                }

                transaction.Commit();
                TaskDialog.Show("Success", $"Added {dimensionCount} dimensions");
            }
            catch (Exception ex)
            {
                transaction.RollBack();
                TaskDialog.Show("Error", $"Failed to create dimensions: {ex.Message}");
            }
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            // User cancelled.
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Error", $"Unexpected error: {ex.Message}");
        }
    }

    public string GetName() => "Add Aligned Dimension";

    private static ViewSection? GetActiveSectionView(Document doc)
    {
        return doc.ActiveView is ViewSection sectionView && !sectionView.IsTemplate
            ? sectionView
            : null;
    }

    private static ColumnDimensionReferences?[] GetColumnDimensionReferences(
        IReadOnlyList<FamilyInstance> columns,
        XYZ beamDir,
        XYZ beamStart)
    {
        var options = new Options
        {
            ComputeReferences = true,
            IncludeNonVisibleObjects = false,
            DetailLevel = ViewDetailLevel.Medium
        };

        var result = new ColumnDimensionReferences?[columns.Count];
        for (int i = 0; i < columns.Count; i++)
        {
            result[i] = TryGetColumnDimensionReferences(columns[i], options, beamDir, beamStart);
        }

        return result;
    }

    private static int CreateDimensionsBetweenColumns(
        Document doc,
        ViewSection view,
        IReadOnlyList<ColumnDimensionReferences?> columns)
    {
        int dimensionCount = 0;
        double shortCurveTolerance = doc.Application.ShortCurveTolerance;

        for (int i = 0; i < columns.Count - 1; i++)
        {
            var left = columns[i];
            var right = columns[i + 1];

            if (left == null || right == null)
                continue;

            if (left.EndPoint.DistanceTo(right.StartPoint) <= shortCurveTolerance)
                continue;

            var dimensionLine = Line.CreateBound(left.EndPoint, right.StartPoint);

            var refArray = new ReferenceArray();
            refArray.Append(left.EndReference);
            refArray.Append(right.StartReference);

            try
            {
                if (doc.Create.NewDimension(view, dimensionLine, refArray) != null)
                    dimensionCount++;
            }
            catch
            {
                // Keep processing remaining column pairs when one pair cannot be dimensioned.
            }
        }

        return dimensionCount;
    }

    private static ColumnDimensionReferences? TryGetColumnDimensionReferences(
        FamilyInstance column,
        Options options,
        XYZ beamDir,
        XYZ beamStart)
    {
        ColumnFaceReference? start = null;
        ColumnFaceReference? end = null;

        foreach (var solid in GetSolids(column.get_Geometry(options)))
        {
            foreach (Face face in solid.Faces)
            {
                CaptureDimensionFace(face, beamDir, beamStart, ref start, ref end);
            }
        }

        return start == null || end == null
            ? null
            : new ColumnDimensionReferences(start.Reference, start.Point, end.Reference, end.Point);
    }

    private static void CaptureDimensionFace(
        Face face,
        XYZ beamDir,
        XYZ beamStart,
        ref ColumnFaceReference? start,
        ref ColumnFaceReference? end)
    {
        Reference? reference = face.Reference;
        if (reference == null)
            return;

        XYZ normal = face.ComputeNormal(new UV(0.5, 0.5));
        if (Math.Abs(normal.DotProduct(beamDir)) < ParallelFaceTolerance)
            return;

        XYZ point = GetFaceCenter(face);
        double projection = (point - beamStart).DotProduct(beamDir);
        var candidate = new ColumnFaceReference(reference, point, projection);

        if (start == null || projection < start.Projection)
            start = candidate;

        if (end == null || projection > end.Projection)
            end = candidate;
    }

    private static XYZ GetFaceCenter(Face face)
    {
        BoundingBoxUV bbox = face.GetBoundingBox();
        var center = new UV(
            (bbox.Min.U + bbox.Max.U) / 2,
            (bbox.Min.V + bbox.Max.V) / 2);

        return face.Evaluate(center);
    }

    private static IEnumerable<Solid> GetSolids(GeometryElement? geometry)
    {
        if (geometry == null)
            yield break;

        foreach (GeometryObject geometryObject in geometry)
        {
            if (geometryObject is Solid solid && solid.Faces.Size > 0)
            {
                yield return solid;
                continue;
            }

            if (geometryObject is GeometryInstance geometryInstance)
            {
                foreach (var nestedSolid in GetSolids(geometryInstance.GetInstanceGeometry()))
                {
                    yield return nestedSolid;
                }
            }
        }
    }

    private sealed record ColumnFaceReference(Reference Reference, XYZ Point, double Projection);

    private sealed record ColumnDimensionReferences(
        Reference StartReference,
        XYZ StartPoint,
        Reference EndReference,
        XYZ EndPoint);
}
