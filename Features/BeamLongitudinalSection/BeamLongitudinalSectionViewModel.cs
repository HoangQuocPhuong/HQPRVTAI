using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HQPRVTAI.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Input;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

public sealed class BeamLongitudinalSectionViewModel : BaseViewModel
{
    public ICommand SelectBeamsCommand { get; set; }
    public ICommand CreateBeamsLongitudinalSectionCommand { get; set; }

    public IReadOnlyCollection<FamilyInstance> SelectedBeams { get; set; }
   
    private IServiceProvider _service => App.Services;


    internal BeamLongitudinalSectionViewModel(UIDocument uiDocument)
    {
        SelectBeamsCommand = new AsyncRelayCommand<IReadOnlyCollection<FamilyInstance>>(
            execute: async _ => SelectedBeams = _service.GetService<IRevitRepositoryQuery>().PickBeams(uiDocument),
            canExecute: _ => true
        );
    }
   

}