using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using NLog;



namespace Drive_Icon_Changer {
  /// <summary>
  ///   Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();



    protected override void OnStartup(StartupEventArgs e) {
      base.OnStartup(e);

      SetupExceptionHandling();
    }



    private void SetupExceptionHandling() {
      AppDomain.CurrentDomain.UnhandledException += (s, e) =>
                                                      LogUnhandledException(
                                                        (Exception)e.ExceptionObject,
                                                        "AppDomain.CurrentDomain.UnhandledException"
                                                      );

      DispatcherUnhandledException += (s, e) => {
                                        LogUnhandledException(
                                          e.Exception,
                                          "Application.Current.DispatcherUnhandledException"
                                        );
                                        e.Handled = true;
                                      };

      TaskScheduler.UnobservedTaskException += (s, e) => {
                                                 LogUnhandledException(
                                                   e.Exception,
                                                   "TaskScheduler.UnobservedTaskException"
                                                 );
                                                 e.SetObserved();
                                               };
    }



    private void LogUnhandledException(Exception exception, string source) {
      var message = $"Unhandled exception ({source})";
      try {
        var assemblyName = Assembly.GetExecutingAssembly().GetName();
        message = $"Unhandled exception in {assemblyName.Name} v{assemblyName.Version}";
      } catch (Exception ex) {
        Logger.Error(ex, "Exception in LogUnhandledException");
      } finally {
        Logger.Error(exception, message);
      }
    }
  }
}
