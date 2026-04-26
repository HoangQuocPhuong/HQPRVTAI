using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using HQPRVTAI.Infrastructure;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Interop;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class BeamLongitudinalSectionCommand : IExternalCommand
{
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        UIDocument uiDoc = commandData.Application.ActiveUIDocument;

        Document doc = uiDoc.Document;

        var mediator = App.Services.GetRequiredService<IMediator>();

        var revitRepositoryQuery = App.Services.GetRequiredService<IRevitRepositoryQuery>();

        var revitRepositoryCommand = App.Services.GetRequiredService<IRevitRepositoryCommand>();

        var beamLongitudinalSectionViewModel = App.Services.GetRequiredService<BeamLongitudinalSectionViewModel>();

        var dialog = new BeamLongitudinalSectionView(beamLongitudinalSectionViewModel);

        new WindowInteropHelper(dialog).Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;

        if (dialog.ShowDialog() is not true) return Result.Cancelled;

        return Result.Succeeded;
    }
}
