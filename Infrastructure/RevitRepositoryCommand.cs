using Autodesk.Revit.DB;
namespace HQPRVTAI.Infrastructure
{
    internal interface IRevitRepositoryCommand
    {
        ViewSection CreateSectionView(Document doc, ElementId elementId, BoundingBoxXYZ boundingBoxXYZ);
    }

    internal sealed class RevitRepositoryCommand : IRevitRepositoryCommand
    {
        public ViewSection CreateSectionView(Document document, ElementId viewFamilyTypeId, BoundingBoxXYZ boundingBoxXYZ)
        => ViewSection.CreateSection(document, viewFamilyTypeId, boundingBoxXYZ);
    }
}
