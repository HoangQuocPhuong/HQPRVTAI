using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Microsoft.Extensions.DependencyInjection;

namespace HQPRVTAI.Features.AddDimension;

[Transaction(TransactionMode.Manual)]
[Regeneration(RegenerationOption.Manual)]
public sealed class AddDimensionShow : IExternalCommand
{
    private AddDimensionView view => App.Services!.GetRequiredService<AddDimensionView>();

    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
        view.Show();
        return Result.Succeeded;
    }
}
