using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HQPRVTAI.Infrastructure
{
    public interface IRevitRepositoryCommand
    {
        public void CreateSectionView(Document doc, ElementId elementId, BoundingBoxXYZ boundingBoxXYZ);
    }

    public sealed class RevitRepositoryCommand : IRevitRepositoryCommand
    {        
        public void CreateSectionView(Document document, ElementId viewFamilyTypeId, BoundingBoxXYZ boundingBoxXYZ)
        {
            TaskDialog.Show("test", "Created section view has invalid element ID");
            try
            {
                // Validate inputs
                ArgumentNullException.ThrowIfNull(document);

                if (viewFamilyTypeId == null || viewFamilyTypeId == ElementId.InvalidElementId)
                    throw new ArgumentException("Invalid view family type ID", nameof(viewFamilyTypeId));

                ArgumentNullException.ThrowIfNull(boundingBoxXYZ);

                // Check if bounding box is valid (has proper height)
                if (boundingBoxXYZ.Min.Z >= boundingBoxXYZ.Max.Z)
                    throw new ArgumentException("Bounding box must have valid height (Min.Z < Max.Z)", nameof(boundingBoxXYZ));

                using var transaction = new Transaction(document, "Create Section View");
                var status = transaction.Start();
                
                if (status != TransactionStatus.Started)
                    TaskDialog.Show("test", $"Failed to start transaction. Status: {status}");

                var viewSection = ViewSection.CreateSection(document, viewFamilyTypeId, boundingBoxXYZ);

                if (viewSection == null)
                    throw new InvalidOperationException("Failed to create section view - ViewSection.CreateSection returned null");

                // Verify the view was created and has a valid ID
                if (viewSection.Id == ElementId.InvalidElementId)
                    TaskDialog.Show("test","Created section view has invalid element ID");

                var commitStatus = transaction.Commit();

                if (commitStatus != TransactionStatus.Committed)
                    TaskDialog.Show("test", $"Failed to commit transaction. Status: {commitStatus}");

                System.Diagnostics.Debug.WriteLine($"Section view created successfully with ID: {viewSection.Id}");
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                System.Diagnostics.Debug.WriteLine($"Error creating section view: {ex.Message}\nStackTrace: {ex.StackTrace}");
                throw;
            }
        }            
    }
}
