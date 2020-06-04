using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CB.System;
using Microsoft.Win32;



namespace CB.Win32 {
  public static class IconX {
    private const string SUBKEY_SEP = @"\";
    private const string SUBKEY_DEFAULTICON = @"DefaultIcon";



    [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteObject([In] IntPtr hObject);



    public static ImageSource ToImageSource(this Icon icon) {
      var bitmap = icon.ToBitmap();
      var hBitmap = bitmap.GetHbitmap();

      ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
        hBitmap,
        IntPtr.Zero,
        Int32Rect.Empty,
        BitmapSizeOptions.FromEmptyOptions()
      );

      if (!DeleteObject(hBitmap)) {
        throw new Win32Exception();
      }

      return wpfBitmap;
    }



    public static Icon ExtractAssociatedIcon(string path, ushort size) {
      if (File.Exists(path)) {
        return ExtractIconFromResource(path, size);
      }

      var dirInfo = new DirectoryInfo(path.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
      if (dirInfo.Parent == null) {
        return ExtractAssociatedDriveIcon(path, size);
      }

      throw new NotImplementedException("Reading associated icon in not yet supported for " + path);
    }



    private static Icon ExtractAssociatedDriveIcon(string path, ushort size) {
      var iconFilename = ReadDriveIconPathCurrentUser(path) ??
                         ReadDriveIconPathLocalMachine(path) ??
                         ReadDefaultDriveIcon();
      Console.WriteLine(iconFilename);

      try {
        return iconFilename != null
                 ? ExtractIconFromResource(iconFilename, size)
                 : null;
      } catch (FileNotFoundException e) {
        Console.Error.WriteLine(e);
        return null;
      }
    }



    public static string ReadDriveIconPathLocalMachine(string drivePath) {
      var regkeyPath = GetRegKeyPathToDriveLocalMachine(drivePath) + SUBKEY_SEP + SUBKEY_DEFAULTICON;

      return Registry
             .LocalMachine
             .OpenSubKey(regkeyPath)
             ?.GetValue(null)
             .ToString();
    }



    public static string ReadDriveIconPathCurrentUser(string drivePath) {
      var regkeyPath = GetRegKeyPathToDriveCurrentUser(drivePath) + SUBKEY_SEP + SUBKEY_DEFAULTICON;

      return Registry
             .CurrentUser
             .OpenSubKey(regkeyPath)
             ?.GetValue(null)
             ?.ToString();
    }



    private static string ReadDefaultDriveIcon() =>
      Registry
        .ClassesRoot
        .OpenSubKey(@"Drive" + SUBKEY_SEP + SUBKEY_DEFAULTICON)
        ?.GetValue(null)
        ?.ToString();



    public static void WriteDriveIconPathLocalMachine(string drivePath, string iconPath) {
      var regkeyPath = GetRegKeyPathToDriveLocalMachine(drivePath) + SUBKEY_SEP + SUBKEY_DEFAULTICON;
      Registry
        .LocalMachine
        .CreateSubKey(regkeyPath)
        ?.SetValue(null, iconPath);
    }



    public static void WriteDriveIconPathCurrentUser(string drivePath, string iconPath) {
      var regkeyPath = GetRegKeyPathToDriveCurrentUser(drivePath) + SUBKEY_SEP + SUBKEY_DEFAULTICON;
      Registry
        .CurrentUser
        .CreateSubKey(regkeyPath)
        ?.SetValue(null, iconPath);
    }



    public static void DeleteDriveIconPathCurrentUser(string drivePath) {
      var regkeyPath = GetRegKeyPathToDriveCurrentUser(drivePath);
      Registry
        .CurrentUser
        .OpenSubKey(regkeyPath, RegistryKeyPermissionCheck.ReadWriteSubTree)
        ?.DeleteSubKey(SUBKEY_DEFAULTICON, false);
    }



    public static void DeleteDriveIconPathLocalMachine(string drivePath) {
      var regkeyPath = GetRegKeyPathToDriveLocalMachine(drivePath);
      Registry
        .CurrentUser
        .OpenSubKey(regkeyPath)
        ?.DeleteSubKey(SUBKEY_DEFAULTICON, false);
    }



    private static string GetRegKeyPathToDriveCurrentUser(string drivePath) {
      var driveName = drivePath
                      .TrimEnd(Path.DirectorySeparatorChar)
                      .TrimEnd(Path.VolumeSeparatorChar);
      return $@"Software\Classes\Applications\Explorer.exe\Drives\{driveName}";
    }



    private static string GetRegKeyPathToDriveLocalMachine(string drivePath) {
      var driveName = drivePath
                      .TrimEnd(Path.DirectorySeparatorChar)
                      .TrimEnd(Path.VolumeSeparatorChar);
      return $@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\DriveIcons\{driveName}";
    }



    [DllImport("Shell32", CharSet = CharSet.Auto)]
    private static extern int ExtractIconEx(
      string lpszFile,
      int nIconIndex,
      IntPtr[] phIconLarge,
      IntPtr[] phIconSmall,
      uint nIconSize);



    [DllImport("user32.dll", EntryPoint = "DestroyIcon", SetLastError = true)]
    private static extern int DestroyIcon(IntPtr hIcon);



    public static Icon ExtractIconFromResource(
      string filename,
      int iconIndex,
      ushort size) {
      IntPtr[] hDummy = {IntPtr.Zero};
      IntPtr[] hIconEx = {IntPtr.Zero};

      try {
        var readIconCount = ExtractIconEx(filename, iconIndex, hIconEx, hDummy, 1);

        return readIconCount > 0 &&
               hIconEx[0] != IntPtr.Zero
                 ? (Icon)Icon.FromHandle(hIconEx[0]).Clone()
                 : null;
      } catch (Exception ex) {
        /* EXTRACT ICON ERROR */

        // BUBBLE UP
        throw new ApplicationException($"Could not extract icon: {filename},{iconIndex}", ex);
      } finally {
        // RELEASE RESOURCES
        foreach (var ptr in hIconEx) {
          if (ptr != IntPtr.Zero) {
            DestroyIcon(ptr);
          }
        }

        foreach (var ptr in hDummy) {
          if (ptr != IntPtr.Zero) {
            DestroyIcon(ptr);
          }
        }
      }
    }



    public static Icon ExtractIconFromResource(
      string filename,
      ushort size) {
      var (left, right) = filename.Separate(',');
      var iconIndex = right != null
                        ? int.Parse(right)
                        : 0;
      return ExtractIconFromResource(left, iconIndex, size);
    }
  }
}
