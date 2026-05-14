using HQPRVTAI.Features.BeamLongitudinalSection;
using HQPRVTAI.Features.AddDimension;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace HQPRVTAI.Infrastructure;

internal static class DependencyInjection
{
    public static IServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        RegisterInfrastructure(services);
        RegisterFeatureBeamLongitudinalSection(services);
        RegisterFeatureAddDimension(services);
        // RegisterFeatureXxx(services); ← thêm feature mới tại đây

        return services.BuildServiceProvider();
    }

    // ── Infrastructure ────────────────────────────────────────────────────
    private static void RegisterInfrastructure(IServiceCollection services)
    {        
        services.AddSingleton<IRevitRepositoryQuery, RevitRepositoryQuery>();
        services.AddSingleton<IRevitRepositoryCommand, RevitRepositoryCommand>();
    }

    // ── Feature: BeamLongitudinalSection ────────────────────────────────────────
    private static void RegisterFeatureBeamLongitudinalSection(IServiceCollection services)
    {       
        services.AddSingleton<BeamLongitudinalSectionService>();
        services.AddTransient<BeamLongitudinalSectionModel>();
        services.AddTransient<BeamLongitudinalSectionViewModel>();
        services.AddTransient<BeamLongitudinalSectionView>();
    }

    // ── Feature: AddDimension ────────────────────────────────────────
    private static void RegisterFeatureAddDimension(IServiceCollection services)
    {
        services.AddSingleton<AddDimensionService>();
        services.AddTransient<AddDimensionViewModel>();
        services.AddTransient<AddDimensionView>();
    }
}