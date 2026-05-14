using System.Windows;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

public partial class BeamLongitudinalSectionView : Window
{
    private BeamLongitudinalSectionViewModel _viewModel;
    public BeamLongitudinalSectionView(BeamLongitudinalSectionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }
}