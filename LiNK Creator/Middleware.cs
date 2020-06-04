using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;



namespace CB.Win32 {
  public static class Middleware {
    public const string ShortcutsSubfolder = "Portables";



    public static void DeleteFiles([NotNull] [ItemNotNull] IEnumerable<string> files) {
      foreach (var file in files) {
        File.Delete(file);
      }
    }



    public static IEnumerable<string> GetFilesRecursive(string rootDirectory, string searchPattern) {
      if (Directory.Exists(rootDirectory)) {
        return Directory.GetFiles(rootDirectory, searchPattern, SearchOption.AllDirectories);
      }

      throw new IOException("Folder does not exist: " + rootDirectory);
    }



    public static void SaveShortcuts([NotNull] [ItemNotNull] IEnumerable<string> targets, string directory) {
      Directory.CreateDirectory(directory);
      foreach (var target in targets) {
        var filename = Path.Combine(directory, Path.GetFileNameWithoutExtension(target) + Shortcuts.EXTENSION);
        var shortcut = Shortcuts.CreateShortcut(filename);
        shortcut.TargetPath = target;
        shortcut.Save();
      }
    }
  }
}
