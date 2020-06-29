using CB.Win32.Registry;
using CB.WPF.Media;
using Drive_Icon_Changer;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CB.Win32 {
  public class Middleware {
    private const string SUBKEY_DEFAULTICON = @"DefaultIcon";

    private readonly Action<string, Exception> errorHandle;

    public Middleware(Action<string, Exception> errorHandle) => this.errorHandle = errorHandle;

    public void WriteDriveIconPathLocalMachine(string drivePath, string iconPath) {
      var regKey = RegistryFactory.Drive.CreateDefaultIconKey(drivePath, RegistryHive.LocalMachine);
      regKey?.SetValue(null, iconPath);
    }



    public void WriteDriveIconPathCurrentUser(string drivePath, string iconPath) {
      //TODO replace  with CB.Win32 method, if it is fixed
      //var regKey = RegistryFactory.Drive.CreateDefaultIconKey(drivePath, RegistryHive.CurrentUser);
      var keyName = RegistryFactory.Drive.GetClassPath(drivePath, RegistryHive.CurrentUser) + "\\" + SUBKEY_DEFAULTICON;
      var regKey = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default).CreateSubKey(keyName);
      regKey?.SetValue(null, iconPath);
    }



    public void DeleteDriveIconPathCurrentUser(string drivePath)
      => DeleteDriveIconPath(drivePath, RegistryHive.CurrentUser);



    public void DeleteDriveIconPathLocalMachine(string drivePath)
      => DeleteDriveIconPath(drivePath, RegistryHive.LocalMachine);



    private void DeleteDriveIconPath(string drivePath, RegistryHive hive) {
      var regPath = RegistryFactory.Drive.GetClassPath(drivePath, hive);
      var regKey =
        RegistryKey.OpenBaseKey(hive, RegistryView.Default)
                   .OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
      regKey?.DeleteSubKey(SUBKEY_DEFAULTICON, false);
    }

    public IEnumerable<Drive> GetDriveIcons(bool anyDrive) {
      foreach (var driveInfo in GetDriveInfos(anyDrive)) {
        Drive drive;
        try {
          using var icon = Icons.ExtractIcon(driveInfo.path, IconSize.Jumbo);
          var iconImage = icon.ToImageSource();
          var label = driveInfo.label != default
            ? $"{driveInfo.label} ({driveInfo.path})"
            : driveInfo.path;
          drive = new Drive {Path = driveInfo.path, Label = label, Icon = iconImage};
        } catch (Exception e) {
          errorHandle?.Invoke(driveInfo.path + " Drive", e);
          continue;
        }
        yield return drive;
      }

    }

    public IEnumerable<(string path, string label)> GetDriveInfos(bool anyDrive){
      if (!anyDrive) {
        foreach (var activeDrive in GetActiveDrives() ) { 
          yield return activeDrive;
        }
        yield break;
      }
      var drives = GetActiveDrives().ToDictionary(drive => drive.path, drive => drive.label);
      for (var c = 'A'; c <= 'Z'; c++) {
        var drivePath = c+":";
        yield return (
          drivePath,
          drives.TryGetValue(drivePath, out var label) ? label : default
        );
      } 
    }

    public IEnumerable<(string path, string label)> GetActiveDrives(){ 
      foreach (var driveInfo in DriveInfo
        .GetDrives()) {
        var driveName = driveInfo.Name.TrimEnd(Path.DirectorySeparatorChar);
        string  volumeLabel;
        try {
          volumeLabel = driveInfo.VolumeLabel;
        } catch (Exception e) {
          errorHandle?.Invoke(driveName+" Drive", e);
          continue;
        }
        yield return (driveName, volumeLabel);
      }
    }
    public bool IsIconFile(string filename) {
      switch (Path.GetExtension(filename)?.ToLower()) {
        case ".ico":
        case ".icl":
        case ".exe":
          return true;
        default:
          return false;
      }
    }

  }
}
