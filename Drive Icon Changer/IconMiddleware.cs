using CB.Win32.Registry;
using Microsoft.Win32;



namespace CB.Win32 {
  public static class IconMiddleware {
    private const string SUBKEY_DEFAULTICON = @"DefaultIcon";



    public static void WriteDriveIconPathLocalMachine(string drivePath, string iconPath) {
      var regKey = RegistryFactory.Drive.CreateDefaultIconKey(drivePath, RegistryHive.LocalMachine);
      regKey?.SetValue(null, iconPath);
    }



    public static void WriteDriveIconPathCurrentUser(string drivePath, string iconPath) {
      var regKey = RegistryFactory.Drive.CreateDefaultIconKey(drivePath, RegistryHive.CurrentUser);
      regKey?.SetValue(null, iconPath);
    }



    public static void DeleteDriveIconPathCurrentUser(string drivePath)
      => DeleteDriveIconPath(drivePath, RegistryHive.CurrentUser);



    public static void DeleteDriveIconPathLocalMachine(string drivePath)
      => DeleteDriveIconPath(drivePath, RegistryHive.LocalMachine);



    private static void DeleteDriveIconPath(string drivePath, RegistryHive hive) {
      var regPath = RegistryFactory.Drive.GetClassPath(drivePath, hive);
      var regKey =
        RegistryKey.OpenBaseKey(hive, RegistryView.Default)
                   .OpenSubKey(regPath, RegistryKeyPermissionCheck.ReadWriteSubTree);
      regKey?.DeleteSubKey(SUBKEY_DEFAULTICON, false);
    }
  }
}
