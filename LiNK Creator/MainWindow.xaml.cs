using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CB.Win32;
using IWshRuntimeLibrary;



namespace LiNK_Creator {
  /// <summary>
  ///   Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow {
    private static Settings Settings => Settings.Default;

    private string ScanDirectory => ScanCombo.Text;

    // private readonly NotificationManager _notificationManager = new NotificationManager();



    public MainWindow() {
      InitializeComponent();
      ScanCombo.ItemsSource = Settings.ScanDirectories;
      SearchPatternInputBox.Text = Settings.FileSearchPattern ?? "";
      ScanCombo.SelectedIndex = ScanCombo.Items.IsEmpty ? -1 : 0;
      ShowPage(Page.Main);
    }



    private static void SaveSettingsScanDirectories(
      string addedScanDirectory,
      string fileSearchPattern) {
      var scanDirectories = Settings.ScanDirectories;

      var indexOf = scanDirectories.IndexOf(addedScanDirectory);
      if (indexOf >= 0) // remove if exists
      {
        scanDirectories.RemoveAt(indexOf);
      } else {
        // remove last item if to much items
        var numScanDirectories = Settings.MaxNumScanDirectories;
        if (scanDirectories.Count > Settings.MaxNumScanDirectories) {
          scanDirectories.RemoveAt(numScanDirectories - 1);
        }
      }

      scanDirectories.Insert(0, addedScanDirectory);
      Settings.ScanDirectories = scanDirectories;
      Settings.FileSearchPattern = fileSearchPattern;
      Settings.Save();
    }



    private void ScanBtn_Click(object sender, RoutedEventArgs e) => ScanAndUpdate();



    private void ShowError(string title, string message) {
      // _notificationManager.ShowAsync(
      //     new NotificationContent {Title = title, Message = message},
      //     "WindowArea" // nameof(WindowArea)
      //   );
    }



    private void ScanAndUpdate() {
      var targetRootDirectory = ScanDirectory;
      if (string.IsNullOrEmpty(targetRootDirectory)) {
        return;
      }

      var searchPattern = SearchPatternInputBox.Text;
      IEnumerable<string> apps;
      try {
        apps = Middleware.GetFilesRecursive(targetRootDirectory, searchPattern);
      } catch (IOException e) {
        HandleException(e);
        return;
      } catch (UnauthorizedAccessException e) {
        HandleException(e);
        return;
      }

      var shortcutsForTargetRoot = GetShortcutsForTargetRoot(targetRootDirectory);

      var shortcutsTargets = GetTargets(shortcutsForTargetRoot.Values);
      var appsFiltered = FilterApps(apps, shortcutsTargets);

      var scanDirectory = ScanDirectory;

      ShortcutCandidatesBox.ItemsSource = appsFiltered
                                          .Select(app => new ShortcutCandidatesListBoxItem(app, scanDirectory))
                                          .ToList();
      ShortcutsBox.ItemsSource = ToListBoxItems(shortcutsForTargetRoot, scanDirectory);

      SaveSettingsScanDirectories(scanDirectory, searchPattern);
    }



    private void HandleException(Exception e) {
      ShowError("ERROR", e.Message);
      Console.Error.WriteLine(e);
    }



    private void ScanCombo_KeyUp(object sender, KeyEventArgs e) {
      if (e.Key == Key.Enter) {
        ScanAndUpdate();
      }
    }



    private static ShortcutListBoxItem[] ToListBoxItems(
      IDictionary<string, IWshShortcut> shortcutInfos,
      string scanDirectory) =>
      shortcutInfos
        .AsEnumerable()
        .Select(
          shortcutEntry =>
            new ShortcutListBoxItem(shortcutEntry.Key, shortcutEntry.Value, scanDirectory)
        )
        .OrderBy(shortcutItem => shortcutItem.Item2.TargetPath)
        .ToArray();



    private static ISet<string> GetTargets(IEnumerable<IWshShortcut> linkInfos) =>
      new SortedSet<string>(
        linkInfos
          .Select(linkInfo => linkInfo.TargetPath)
      );



    private static IEnumerable<string> FilterApps(IEnumerable<string> apps, ISet<string> shortcutsTargets) =>
      apps.Where(app => !shortcutsTargets.Contains(app));



    private static IDictionary<string, IWshShortcut> GetShortcutsForTargetRoot(string targetRootDirectory) =>
      Shortcuts.GetStartMenuShortcuts()
               .Where(
                 shortcutEntry => shortcutEntry?.TargetPath?.StartsWith(
                                    targetRootDirectory,
                                    StringComparison.InvariantCultureIgnoreCase
                                  ) ??
                                  false
               )
               .ToDictionary(
                 shortcutEntry => shortcutEntry.TargetPath,
                 shortcutEntry => shortcutEntry
               );



    private void AddShortcutsToUser_Click(object sender, RoutedEventArgs e) =>
      AddShortcutsToUserStartMenuAndBackToMain();



    private void AddShortcutsToComputer_Click(object sender, RoutedEventArgs e) =>
      AddShortcutsToMachineStartMenuAndBackToMain();



    private void ShortcutCandidatesBox_KeyUp(object sender, KeyEventArgs e) {
      if (e.Key == Key.Enter) {
        if (Keyboard.IsKeyDown(Key.LeftShift) ||
            Keyboard.IsKeyDown(Key.RightShift)) {
          AddShortcutsToUserStartMenuAndBackToMain();
        } else {
          AddShortcutsToMachineStartMenuAndBackToMain();
        }
      }
    }



    private void AddShortcutsAndBackToMain(string startMenuPath) {
      SaveShortcuts(startMenuPath);
      ScanAndUpdate(); //TODO meanwhile scan path could be change  
      ShowPage(Page.Main);
    }



    private void AddShortcutsToUserStartMenuAndBackToMain() =>
      AddShortcutsAndBackToMain(Shortcuts.PathStartMenuCurrentUser);



    private void AddShortcutsToMachineStartMenuAndBackToMain() =>
      AddShortcutsAndBackToMain(Shortcuts.PathStartMenuLocalMachine);



    private void ShortcutsBox_KeyUp(object sender, KeyEventArgs e) {
      if (e.Key == Key.Delete) {
        RemoveShortcuts();
      }
    }



    private void ShortcutRemoveBtn_Click(object sender, RoutedEventArgs e) => RemoveShortcuts();



    private void RemoveShortcuts() {
      var selectedShortcutFiles = ShortcutsBox
                                  .SelectedItems
                                  .Cast<ShortcutListBoxItem>()
                                  .Select(shortcut => shortcut.Item1);
      Middleware.DeleteFiles(selectedShortcutFiles);
      ScanAndUpdate();
    }



    private void SaveShortcuts(string startMenuDirectory) {
      var directory = Path.Combine(
        startMenuDirectory,
        Middleware.ShortcutsSubfolder
      );
      var selectedShortcuts = ShortcutCandidatesBox
                              .SelectedItems
                              .Cast<ShortcutCandidatesListBoxItem>()
                              .Select(target => target.Target);
      Middleware.SaveShortcuts(
        selectedShortcuts,
        directory
      );
    }



    private void ShowPage(Page page) => PageControl.SelectedIndex = (int)page;



    private static string ToRelativePath(string filePath, string referencePath) {
      if (filePath.StartsWith(referencePath, StringComparison.InvariantCulture)) {
        return filePath.Remove(0, referencePath.Length);
      }

      throw new ArgumentException(
        $"filePath is not in referencePath: filepath=\"{filePath}\" referencePath=\"{referencePath}\""
      );
    }



    private enum Page {
      Main
    }



    private class ShortcutCandidatesListBoxItem {
      public readonly string Target;
      private readonly string _scanDirectory;



      public ShortcutCandidatesListBoxItem(string target, string scanDirectory) {
        Target = target;
        _scanDirectory = scanDirectory;
      }



      public override string ToString() => ToRelativePath(Target, _scanDirectory);
    }



    private class ShortcutListBoxItem : Tuple<string, IWshShortcut> {
      private readonly string _scanDirectory;



      public ShortcutListBoxItem(string shortcutFile, IWshShortcut shortcutInfo, string scanDirectory)
        : base(shortcutFile, shortcutInfo) =>
        _scanDirectory = scanDirectory;



      public override string ToString() => ToRelativePath(Item2.TargetPath, _scanDirectory);
    }
  }
}
