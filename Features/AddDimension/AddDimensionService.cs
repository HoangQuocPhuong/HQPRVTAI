using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

namespace HQPRVTAI.Features.AddDimension;

public class AddDimensionService : IExternalEventHandler
{
    private ExternalEvent _event;

    public AddDimensionService()
    {
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
        var view = uidoc.ActiveView;

        try
        {
            // Check if active view is valid for dimensioning
            if (view == null || view.IsTemplate)
            {
                TaskDialog.Show("Error", "Active view is not valid for adding dimensions.");
                return;
            }

            // Pick first reference (face or edge)
            Reference ref1 = uidoc.Selection.PickObject(
                ObjectType.Face,
                "Pick first face for dimension");

            if (ref1 == null) return;

            // Pick second reference (face or edge)
            Reference ref2 = uidoc.Selection.PickObject(
                ObjectType.Face,
                "Pick second face for dimension");

            if (ref2 == null) return;

            // Get geometry for dimension placement
            var geom1 = GetReferenceGeometry(doc, ref1);
            var geom2 = GetReferenceGeometry(doc, ref2);

            if (geom1 == null || geom2 == null)
            {
                TaskDialog.Show("Error", "Could not get geometry from selected references.");
                return;
            }

            // Create dimension line between the two points
            var dimensionLine = Line.CreateBound(geom1, geom2);

            using var t = new Transaction(doc, "Add Aligned Dimension");
            t.Start();

            try
            {
                // Create reference array for dimension
                ReferenceArray refArray = new ReferenceArray();
                refArray.Append(ref1);
                refArray.Append(ref2);

                // The Revit API in 2026 may have different method names
                // We'll attempt the most common pattern
                // If this fails, check the Revit API documentation for your version
                var creator = doc.Create;
                Dimension dimension = null;

                // Try the standard aligned dimension method
                var method = creator.GetType().GetMethod("NewAlignedDimension", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (method != null)
                {
                    dimension = (Dimension)method.Invoke(creator, new object[] { view, dimensionLine, refArray });
                }
                else
                {
                    throw new InvalidOperationException(
                        "NewAlignedDimension method not found in Revit API. " +
                        "Check the Revit 2026 documentation for the correct method name.");
                }

                if (dimension == null)
                {
                    throw new Exception("Dimension creation returned null");
                }

                t.Commit();
                TaskDialog.Show("Success", "Dimension added successfully.");
            }
            catch (Exception ex)
            {
                t.RollBack();
                TaskDialog.Show("Error", $"Failed to create dimension: {ex.Message}");
            }
        }
        catch (Autodesk.Revit.Exceptions.OperationCanceledException)
        {
            // User cancelled - ignore
        }
        catch (Exception ex)
        {
            TaskDialog.Show("Error", $"Unexpected error: {ex.Message}");
        }
    }

    public string GetName() => "Add Aligned Dimension";

    /// <summary>
    /// Extract a reference point from the selected reference (face or edge).
    /// </summary>
    private XYZ? GetReferenceGeometry(Document doc, Reference reference)
    {
        try
        {
            Element elem = doc.GetElement(reference);
            GeometryObject geoObj = elem.GetGeometryObjectFromReference(reference);

            if (geoObj is Face face)
            {
                // Get center point of face
                BoundingBoxUV bbox = face.GetBoundingBox();
                UV center = new UV((bbox.Min.U + bbox.Max.U) / 2, (bbox.Min.V + bbox.Max.V) / 2);
                return face.Evaluate(center);
            }
            else if (geoObj is Edge edge)
            {
                // Get midpoint of edge
                Curve curve = edge.AsCurve();
                return curve.Evaluate(0.5, true);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}




