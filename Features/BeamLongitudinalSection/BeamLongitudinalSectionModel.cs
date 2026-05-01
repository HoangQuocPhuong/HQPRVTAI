using Autodesk.Revit.DB;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

public sealed class BeamLongitudinalSectionModel
{
    private const double ExtraDepthMm = 100.0;

    public BoundingBoxXYZ? Build(FamilyInstance beam, double extendMm = 1000.0, double minDepthMm = 600.0)
    {
        if (!TryGetBeamLine(beam, out Line beamLine)) return null;

        var axes = BuildAxes(beamLine);
        
        if (axes is null) return null;

        var (right, up, view) = axes.Value;

        var dims   = Measure(beam, beamLine, view, minDepthMm);

        var origin = ComputeOrigin(beam, beamLine);

        return Assemble(origin, right, up, view, dims, extendMm);
    }

    // ── Steps ─────────────────────────────────────────────────────────────

    private static bool TryGetBeamLine(FamilyInstance beam, out Line line)
    {
        line = null!;
        if (beam.Location is LocationCurve lc && lc.Curve is Line l)
        { 
            line = l; return true; 
        }
        return false;        
    }

    private static (XYZ right, XYZ up, XYZ view)? BuildAxes(Line beamLine)
    {
        XYZ axis = beamLine.Direction.Normalize();
        if (Math.Abs(axis.DotProduct(XYZ.BasisZ)) > 0.95) return null;

        XYZ right = new XYZ(axis.X, axis.Y, 0.0).Normalize();
        XYZ up    = XYZ.BasisZ;
        XYZ view  = right.CrossProduct(up).Normalize();
        return (right, up, view);
    }

    private static BeamDimensions Measure(FamilyInstance beam, Line beamLine, XYZ viewDir, double minDepthMm)
    {
        BoundingBoxXYZ? bbox = beam.get_BoundingBox(null);

        if (bbox is null)
            return new BeamDimensions(
                HalfLength  : beamLine.Length / 2.0,
                HalfHeight  : 300/304.8,
                SectionDepth: minDepthMm/304.8,
                CenterZ     : beamLine.Evaluate(0.5, true).Z);

        double halfH   = (bbox.Max.Z - bbox.Min.Z) / 2.0;
        double centerZ = (bbox.Min.Z  + bbox.Max.Z) / 2.0;

        double minP = double.MaxValue, maxP = double.MinValue;
        foreach (XYZ c in GetBBoxCorners(bbox))
        {
            double p = c.DotProduct(viewDir);
            if (p < minP) minP = p;
            if (p > maxP) maxP = p;
        }

        double depth = Math.Max((maxP - minP) / 2.0 + ExtraDepthMm/304.8, minDepthMm/304.8);

        return new BeamDimensions(beamLine.Length / 2.0, halfH, depth, centerZ);
    }

    private static XYZ ComputeOrigin(FamilyInstance beam, Line beamLine)
    {
        XYZ mid = beamLine.Evaluate(0.5, true);
        BoundingBoxXYZ? bbox = beam.get_BoundingBox(null);
        double centerZ = bbox is not null ? (bbox.Min.Z + bbox.Max.Z) / 2.0 : mid.Z;
        return new XYZ(mid.X, mid.Y, centerZ);
    }

    private static BoundingBoxXYZ Assemble(XYZ origin, XYZ right, XYZ up, XYZ view, BeamDimensions d, double extendMm)
    {
        double ext = extendMm/304.8;
        Transform t = Transform.Identity;
        t.Origin = origin;
        t.BasisX = right;
        t.BasisY = up;
        t.BasisZ = view;

        return new BoundingBoxXYZ
        {
            Transform = t,
            Min = new XYZ(-(d.HalfLength + ext), -(d.HalfHeight + ext), 0.0),
            Max = new XYZ( (d.HalfLength + ext),  (d.HalfHeight + ext), d.SectionDepth)
        };
    }

    // ── Utilities ──────────────────────────────────────────────────────────

    private static IEnumerable<XYZ> GetBBoxCorners(BoundingBoxXYZ bb)
    {
        Transform t = bb.Transform; XYZ mn = bb.Min, mx = bb.Max;
        yield return t.OfPoint(mn);
        yield return t.OfPoint(new XYZ(mx.X, mn.Y, mn.Z));
        yield return t.OfPoint(new XYZ(mn.X, mx.Y, mn.Z));
        yield return t.OfPoint(new XYZ(mx.X, mx.Y, mn.Z));
        yield return t.OfPoint(new XYZ(mn.X, mn.Y, mx.Z));
        yield return t.OfPoint(new XYZ(mx.X, mn.Y, mx.Z));
        yield return t.OfPoint(new XYZ(mn.X, mx.Y, mx.Z));
        yield return t.OfPoint(mx);
    }

    private readonly record struct BeamDimensions( double HalfLength, double HalfHeight, double SectionDepth, double CenterZ);
}
