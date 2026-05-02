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

    private FamilyInstance? _selectedBeam;
    public FamilyInstance? SelectedBeam
    {
        get => _selectedBeam;
        set
        {
            _selectedBeam = value;
            OnPropertyChanged(nameof(SelectedBeam));
        }
    }   

    public UIDocument UiDocument { get; set; }

    public ICommand CreateSectionCommand { get; set; }

    public BeamLongitudinalSectionViewModel(IMediator mediator, IRevitRepositoryQuery revitRepositoryQuery)
    {
        _mediator = mediator;
        _revitRepositoryQuery = revitRepositoryQuery;

        CreateSectionCommand = new RelayCommand<object>(
           execute: _ =>
           {
               try
               {
                   TaskDialog.Show("Select Beam", "Please select a beam in the Revit model.");

                   SelectedBeam = _revitRepositoryQuery.PickBeam(UiDocument);

                   if (SelectedBeam != null)
                   {
                       TaskDialog.Show("Selected Beam", $"You selected: {SelectedBeam.Name}");

                       _mediator.Send(new BeamLongitudinalSectionRequest(UiDocument, SelectedBeam)).GetAwaiter().GetResult();

                       TaskDialog.Show("Success", "Section created successfully!");
                   }
                   else
                   {
                       TaskDialog.Show("No Selection", "No beam was selected.");
                   }
               }
               catch (Exception ex)
               {
                   TaskDialog.Show("Error", $"An error occurred: {ex.Message}");
               }
           },
           canExecute: _ => UiDocument != null
       );
    } 
}