using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HQPRVTAI.Infrastructure;
using MediatR;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

public sealed class BeamLongitudinalSectionViewModel : BaseViewModel
{
    private readonly IMediator _mediator;
    private readonly IRevitRepositoryQuery _revitRepositoryQuery;

    public FamilyInstance? selectedBeam;

    public UIDocument UiDocument { get; set; }

    public ICommand CreateSectionCommand { get; set; }

    public BeamLongitudinalSectionViewModel(IMediator mediator, IRevitRepositoryQuery revitRepositoryQuery)
    {
        _mediator = mediator;
        _revitRepositoryQuery = revitRepositoryQuery;

        CreateSectionCommand = new AsyncRelayCommand<object>(
           execute: async _ =>
           {
               selectedBeam = _revitRepositoryQuery.PickBeam(UiDocument);
               if (selectedBeam != null)
               {
                   await _mediator.Send(new BeamLongitudinalSectionRequest(UiDocument, selectedBeam));
               }
           },
           canExecute: _ => UiDocument != null
       );
    } 
}