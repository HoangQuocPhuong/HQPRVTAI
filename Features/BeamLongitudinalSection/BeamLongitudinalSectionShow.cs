using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class BeamLongitudinalSectionShow : IExternalCommand
{       
    private BeamLongitudinalSectionView view => App.Services!.GetRequiredService<BeamLongitudinalSectionView>();
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        view.Show();

        return Result.Succeeded;
    }
}