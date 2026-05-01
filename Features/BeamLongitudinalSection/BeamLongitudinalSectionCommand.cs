using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class BeamLongitudinalSectionCommand : IExternalCommand
{
    private readonly BeamLongitudinalSectionViewModel _viewModel;
    private readonly BeamLongitudinalSectionView _view;

    public BeamLongitudinalSectionCommand(BeamLongitudinalSectionViewModel viewModel, BeamLongitudinalSectionView view)
    {
        _viewModel = viewModel;
        _view = view;
    }

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIDocument uiDoc = commandData.Application.ActiveUIDocument;

        _viewModel.UiDocument = uiDoc;

        _view.DataContext = _viewModel;

        _view.Show();

        return Result.Succeeded;
    }
}
