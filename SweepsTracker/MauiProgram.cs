using Microsoft.Extensions.Logging;
using SweepsTracker.Core;

namespace SweepsTracker;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		Constants.DatabasePath = FileSystem.AppDataDirectory+"/"+Constants.DatabaseFilename;
		builder.Services.AddSingleton<SweepsDatabase>();
		builder.Services.AddSingleton<GameSessionData>();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
