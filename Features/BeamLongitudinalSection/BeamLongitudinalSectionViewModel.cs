using HQPRVTAI.Infrastructure;
using System.Windows.Input;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

public class BeamLongitudinalSectionViewModel : BaseViewModel
{
    private readonly BeamLongitudinalSectionService _service;

    public ICommand RunCommand { get; }

    public BeamLongitudinalSectionViewModel(BeamLongitudinalSectionService service)
    {
        _service = service;
        RunCommand = new RelayCommand(() => { _service.Raise(); });
    }
}