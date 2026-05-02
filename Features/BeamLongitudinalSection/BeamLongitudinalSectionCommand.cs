using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class BeamLongitudinalSectionCommand : IExternalCommand
{   
    private BeamLongitudinalSectionViewModel _viewModel => App.Services!.GetRequiredService<BeamLongitudinalSectionViewModel>();
    private BeamLongitudinalSectionView _view => App.Services!.GetRequiredService<BeamLongitudinalSectionView>();

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIDocument uiDoc = commandData.Application.ActiveUIDocument;

        _viewModel.UiDocument = uiDoc;

        _view.Show();

        return Result.Succeeded;
    }
}
