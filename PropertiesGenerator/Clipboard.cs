using System.Runtime.InteropServices;

namespace PropertiesGenerator;

public static class Clipboard
{

    [DllImport("user32.dll")]
    public static extern IntPtr GetClipboardData(uint uFormat);

    [DllImport("user32.dll")]
    public static extern bool IsClipboardFormatAvailable(uint format);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool OpenClipboard(IntPtr hWndNewOwner);

    [DllImport("user32.dll", SetLastError = true)]
    public static extern bool CloseClipboard();

    [DllImport("kernel32.dll")]
    public static extern IntPtr GlobalLock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    public static extern bool GlobalUnlock(IntPtr hMem);

    [DllImport("kernel32.dll")]
    public static extern int GlobalSize(IntPtr hMem);

    [DllImport("user32.dll")]
    public static extern bool EmptyClipboard();

    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr SetClipboardData(uint uFormat, IntPtr hMem);

    [DllImport("kernel32.dll")]
    public static extern IntPtr GlobalAlloc(uint uFlags, IntPtr dwBytes);

    public static string ReadFromClipboard()
    {
        string clipboardText = string.Empty;

        try
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                if (IsClipboardFormatAvailable(13)) // 13 - CF_UNICODETEXT
                {
                    IntPtr hClipboardData = GetClipboardData(13); // 13 - CF_UNICODETEXT

                    if (hClipboardData != IntPtr.Zero)
                    {
                        IntPtr pClipboardData = GlobalLock(hClipboardData);
                        int size = GlobalSize(hClipboardData);

                        if (pClipboardData != IntPtr.Zero)
                        {
                            byte[] buffer = new byte[size];
                            Marshal.Copy(pClipboardData, buffer, 0, size);

                            clipboardText = System.Text.Encoding.Unicode.GetString(buffer).TrimEnd('\0');

                            GlobalUnlock(pClipboardData);
                        }
                    }
                }

                CloseClipboard();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        return clipboardText;
    }

    public static void WriteToClipboard(string text)
    {
        try
        {
            if (OpenClipboard(IntPtr.Zero))
            {
                EmptyClipboard();

                int byteCount = (text.Length + 1) * 2; // Multiply by 2 to account for Unicode characters

                IntPtr hGlobal = GlobalAlloc(0x2000, (IntPtr)byteCount); // 0x2000 - GMEM_MOVEABLE

                if (hGlobal != IntPtr.Zero)
                {
                    IntPtr pGlobal = GlobalLock(hGlobal);

                    if (pGlobal != IntPtr.Zero)
                    {
                        byte[] buffer = System.Text.Encoding.Unicode.GetBytes(text);

                        Marshal.Copy(buffer, 0, pGlobal, buffer.Length);

                        GlobalUnlock(pGlobal);

                        SetClipboardData(13, hGlobal); // 13 - CF_UNICODETEXT
                    }
                }

                CloseClipboard();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Clipboard write error: " + ex.Message);
        }
    }
}