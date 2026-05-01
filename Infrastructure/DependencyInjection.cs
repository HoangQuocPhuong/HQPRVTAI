using HQPRVTAI.Features.BeamLongitudinalSection;
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
        // RegisterFeatureXxx(services); ← thêm feature mới tại đây

        return services.BuildServiceProvider();
    }

    // ── Infrastructure ────────────────────────────────────────────────────
    private static void RegisterInfrastructure(IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        services.AddSingleton<IRevitRepositoryQuery, RevitRepositoryQuery>();
        services.AddSingleton<IRevitRepositoryCommand, RevitRepositoryCommand>();
    }

    // ── Feature: BeamLongitudinalSection ────────────────────────────────────────
    private static void RegisterFeatureBeamLongitudinalSection(IServiceCollection services)
    {        
        services.AddTransient<BeamLongitudinalSectionModel>();
        services.AddTransient<BeamLongitudinalSectionViewModel>();
        services.AddTransient<BeamLongitudinalSectionView>();
    }
}