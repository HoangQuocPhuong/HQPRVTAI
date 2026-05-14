using HQPRVTAI.Infrastructure;
using System.Windows.Input;

namespace HQPRVTAI.Features.AddDimension;

public class AddDimensionViewModel : BaseViewModel
{
    private readonly AddDimensionService _service;

    public ICommand AddDimensionCommand { get; }

    public AddDimensionViewModel(AddDimensionService service)
    {
        _service = service;
        AddDimensionCommand = new RelayCommand(() => { _service.Raise(); });
    }
}
