using System.Windows;

namespace HQPRVTAI.Features.AddDimension;

public partial class AddDimensionView : Window
{
    private AddDimensionViewModel _viewModel;
    public AddDimensionView(AddDimensionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;
    }
}
