using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace HQPRVTAI.Features.BeamLongitudinalSection;

public partial class BeamLongitudinalSectionView : Window
{        
    public BeamLongitudinalSectionView (BeamLongitudinalSectionViewModel viewmodel)
    {
        DataContext = viewmodel;
        InitializeComponent();
    }
}
