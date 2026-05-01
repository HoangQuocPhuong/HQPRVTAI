using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HQPRVTAI.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.DirectoryServices.ActiveDirectory;
using System.Windows.Interop;

namespace HQPRVTAI.Features.BeamLongitudinalSection
{
    public record BeamLongitudinalSectionRequest(UIDocument UIDocument, FamilyInstance beam) : IRequest<string>;  
    
    public class BeamLongitudinalSectionHandler(RevitRepositoryQuery revitRepositoryQuery, 
        RevitRepositoryCommand revitRepositoryCommand, 
        BeamLongitudinalSectionModel beamLongitudinalSectionModel) 
        : IRequestHandler<BeamLongitudinalSectionRequest, string>
    {
        private RevitRepositoryQuery _revitRepositoryQuery;
        private RevitRepositoryCommand _revitRepositoryCommand;
        private BeamLongitudinalSectionModel _beamLongitudinalSectionModel;        
        
        Task<string> IRequestHandler<BeamLongitudinalSectionRequest, string>.Handle(
            BeamLongitudinalSectionRequest request, 
            CancellationToken cancellationToken)
        {
            try
            {
                Document doc = request.UIDocument.Document;

                _revitRepositoryQuery = revitRepositoryQuery;
                _revitRepositoryCommand = revitRepositoryCommand;
                _beamLongitudinalSectionModel = beamLongitudinalSectionModel;

                // ❌ Check 1: GetViewType() có thể return null
                var sectionViewType = _revitRepositoryQuery.GetViewType(doc, ViewFamily.Section);
                
                if (sectionViewType == null)
                    return Task.FromResult("Error: No Section view type found in the project.");

                // ❌ Check 2: Build() có thể return null (beam vertical hoặc geometry invalid)
                var boundingBoxXYZ = _beamLongitudinalSectionModel.Build(request.beam, 1000, 1000);
                
                if (boundingBoxXYZ == null)
                    return Task.FromResult("Error: Cannot build section box for this beam (possibly vertical or invalid geometry).");

                // ❌ Check 3: Validate bounding box
                if (boundingBoxXYZ.Min.Z >= boundingBoxXYZ.Max.Z)
                    return Task.FromResult("Error: Invalid bounding box dimensions.");

                // ❌ Check 4: CreateSectionView() với exception handling
                _revitRepositoryCommand.CreateSectionView(doc, sectionViewType.Id, boundingBoxXYZ);

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
