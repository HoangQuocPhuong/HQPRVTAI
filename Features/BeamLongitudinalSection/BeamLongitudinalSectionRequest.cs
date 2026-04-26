using Autodesk.Revit.DB;
using MediatR;

namespace HQPRVTAI.Features.BeamLongitudinalSection
{
    internal sealed record BeamLongitudinalSectionRequest(Document Document, IReadOnlyCollection<FamilyInstance> Beams) : IRequest<BeamLongitudinalSectionResponse>;    
}
