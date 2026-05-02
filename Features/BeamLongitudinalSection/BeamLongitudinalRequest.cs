using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HQPRVTAI.Infrastructure;
using MediatR;

namespace HQPRVTAI.Features.BeamLongitudinalSection
{
    public record BeamLongitudinalSectionRequest(UIDocument UIDocument, FamilyInstance Beam) : IRequest<string>;  
    
    public class BeamLongitudinalSectionHandler(IRevitRepositoryQuery revitRepositoryQuery, 
        IRevitRepositoryCommand revitRepositoryCommand, 
        BeamLongitudinalSectionModel beamLongitudinalSectionModel) 
        : IRequestHandler<BeamLongitudinalSectionRequest, string>
    {        
        Task<string> IRequestHandler<BeamLongitudinalSectionRequest, string>.Handle(
            BeamLongitudinalSectionRequest request, CancellationToken cancellationToken)
        {
            try
            {
                Document doc = request.UIDocument.Document;

                // ❌ Check 1: GetViewType() có thể return null
                var sectionViewType = revitRepositoryQuery.GetSectionViewFamilyType(doc);
                
                if (sectionViewType == null)
                    return Task.FromResult("Error: No Section view type found in the project.");

                // ❌ Check 2: Build() có thể return null (beam vertical hoặc geometry invalid)
                var boundingBoxXYZ = beamLongitudinalSectionModel.Build(request.Beam, 1000, 1000);

                if (boundingBoxXYZ == null)
                    return Task.FromResult("Error: Cannot build section box for this beam (possibly vertical or invalid geometry).");                

                // ❌ Check 3: Validate bounding box
                if (boundingBoxXYZ.Min.Z >= boundingBoxXYZ.Max.Z)
                    return Task.FromResult("Error: Invalid bounding box dimensions.");

                // ❌ Check 4: CreateSectionView() với exception handling
                revitRepositoryCommand.CreateSectionView(doc, sectionViewType.Id, boundingBoxXYZ);

                return Task.FromResult("Section was created successfully.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating section: {ex.Message}\n{ex.StackTrace}");
                return Task.FromResult($"Error: {ex.Message}");
            }
        }
    }
}
