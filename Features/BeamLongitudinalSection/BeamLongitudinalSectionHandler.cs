using Autodesk.Revit.DB;
using HQPRVTAI.Infrastructure;
using MediatR;

namespace HQPRVTAI.Features.BeamLongitudinalSection
{
    internal class BeamLongitudinalSectionHandler : IRequestHandler<BeamLongitudinalSectionRequest, BeamLongitudinalSectionResponse>
    {        
        private readonly RevitRepositoryQuery _revitRepositoryQuery;
        private readonly RevitRepositoryCommand _revitRepositoryCommand;
        private readonly Document _document;
        private readonly BeamLongitudinalSectionModel _beamLongitudinalSectionModel;

        internal BeamLongitudinalSectionHandler(RevitRepositoryQuery revitRepositoryQuery, RevitRepositoryCommand revitRepositoryCommand, Document document, BeamLongitudinalSectionModel beamLongitudinalSectionModel)
        {
            _revitRepositoryQuery = revitRepositoryQuery;
            _revitRepositoryCommand = revitRepositoryCommand;
            _document = document;
            _beamLongitudinalSectionModel = beamLongitudinalSectionModel;
        }

        public Task<BeamLongitudinalSectionResponse> Handle(BeamLongitudinalSectionRequest request, CancellationToken cancellationToken)
        {
            Document document = request.Document;

            ViewFamilyType viewFamilyType = _revitRepositoryQuery.GetViewType(document, ViewFamily.Section);

            var results = new List<BeamLongitudinalSectionResult>(request.Beams.Count);

            using var transaction = new Transaction(document, "Create Beam Longitudinal Sections");

            transaction.Start();

            foreach (FamilyInstance beam in request.Beams)
            {
                try
                {
                    var sectionView = _revitRepositoryCommand.CreateSectionView(document, viewFamilyType.Id, _beamLongitudinalSectionModel.Build(beam));
                    if (sectionView != null)
                    {
                        results.Add(new BeamLongitudinalSectionResult(true));
                    }
                    else
                    {
                        results.Add(new BeamLongitudinalSectionResult(false));
                    }
                }
                catch
                {
                    results.Add(new BeamLongitudinalSectionResult(false));
                }
            }
            transaction.Commit();

            return Task.FromResult(new BeamLongitudinalSectionResponse(results));
        }
    }
}
