using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ClsidInfoCollector {
  class ExtractIcon {
    // HIDE INSTANCE CONSTRUCTOR
    private ExtractIcon() {
    }

    [DllImport( "Shell32", CharSet = CharSet.Auto )]
    private static unsafe extern int ExtractIconEx(
      string lpszFile,
      int nIconIndex,
      IntPtr[] phIconLarge,
      IntPtr[] phIconSmall,
      int nIcons);

    [DllImport( "user32.dll", EntryPoint = "DestroyIcon", SetLastError = true )]
    private static unsafe extern int DestroyIcon(IntPtr hIcon);

    public static Icon ExtractIconFromExe(string file, bool large, int iconIndex = 0) {
      unsafe
      {
        var hIconSmall = new IntPtr[1] { IntPtr.Zero };
        var hIconLarge = new IntPtr[1] { IntPtr.Zero };

        try {
          var readIconCount = ExtractIconEx( file, iconIndex, hIconLarge, hIconSmall, 1 );
          var hIcon = large ? hIconLarge : hIconSmall;

          return readIconCount > 0
          && hIcon[0] != IntPtr.Zero
            // GET FIRST EXTRACTED ICON
            ? (Icon)Icon.FromHandle( hIcon[0] ).Clone()
            : null;

        } catch (Exception ex) {
          /* EXTRACT ICON ERROR */

          // BUBBLE UP
          throw new ApplicationException( "Could not extract icon", ex );
        } finally {
          // RELEASE RESOURCES
          DestroyIcons( hIconSmall );
          DestroyIcons( hIconLarge );
        }
      }
    }

    private static void DestroyIcons(IntPtr[] iconPtr) {
      foreach (var ptr in iconPtr)
        if (ptr != IntPtr.Zero)
          DestroyIcon( ptr );
    }


  }
}
