using Autodesk.Revit.DB;
using System;
using System.Windows.Controls;

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
                transaction.Start();

                var viewSection = ViewSection.CreateSection(document, viewFamilyTypeId, boundingBoxXYZ) ?? 
                    throw new InvalidOperationException("Failed to create section view");

                transaction.Commit();
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                System.Diagnostics.Debug.WriteLine($"Error creating section view: {ex.Message}");
                throw;
            }
        }            
    }
}
