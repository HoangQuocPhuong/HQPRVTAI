using Autodesk.Revit.UI;
using System.Reflection;
using HQPRVTAI.Infrastructure;
using HQPRVTAI.Features.BeamLongitudinalSection;

namespace HQPRVTAI
{
    public class App : IExternalApplication
    {
        private const string TabName = "HQPRVTAI";

        private const string PanelName1 = "Beam Longitudinal Section View";

        public static IServiceProvider? Services { get; private set; }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            try
            {
                Services = DependencyInjection.BuildServiceProvider();

                CreateRibbon(application);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Lỗi khởi tạo Addin", ex.Message);
                return Result.Failed;
            }
        }
        private static void CreateRibbon(UIControlledApplication application)
        {
            application.CreateRibbonTab(TabName);
            RibbonPanel panel = application.CreateRibbonPanel(TabName, PanelName1);
            string dll = Assembly.GetExecutingAssembly().Location;

            panel.AddItem(new PushButtonData(
                name: nameof(BeamLongitudinalSectionCommand),
                text: "Beam\nLongitudinal Section",
                assemblyName: dll,
                className: typeof(BeamLongitudinalSectionCommand).FullName!)
            {
                ToolTip = "Tạo section view dọc theo dầm đã chọn.",
                LongDescription = "Chọn dầm → nhấn nút → điều chỉnh thông số trong dialog → xác nhận."
            });

            // ── Thêm feature mới: panel.AddItem(...) tại đây ──────────────────
        }
    }
}
