/*
 * Created by SharpDevelop.
 * User: YLIN68
 * Date: 3/4/2016
 * Time: 8:42 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace launcher
{
	/// <summary>
	/// Description of NativeMethods.
	/// </summary>
	internal static class NativeMethods
	{
	    [DllImport("kernel32.dll", EntryPoint = "CloseHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
	    internal static extern bool CloseHandle(IntPtr handle);
	
	    [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	    internal static extern bool CreateProcessAsUser(IntPtr tokenHandle, string applicationName, string commandLine, IntPtr processAttributes, IntPtr threadAttributes, bool inheritHandle, int creationFlags, IntPtr envrionment, string currentDirectory, ref STARTUPINFO startupInfo, ref PROCESS_INFORMATION processInformation);
	
	    [DllImport("Kernel32.dll", EntryPoint = "WTSGetActiveConsoleSessionId")]
	    internal static extern int WTSGetActiveConsoleSessionId();

	    [DllImport("WtsApi32.dll", SetLastError = true)]
	    [return: MarshalAs(UnmanagedType.Bool)]
	    internal static extern bool WTSQueryUserToken(int SessionId, out IntPtr phToken);

	    [StructLayout(LayoutKind.Sequential)]
	    internal struct PROCESS_INFORMATION
	    {
	        public IntPtr processHandle;
	        public IntPtr threadHandle;
	        public int processID;
	        public int threadID;
	    }
	
	    [StructLayout(LayoutKind.Sequential)]
	    internal struct STARTUPINFO
	    {
	        public int length;
	        public string reserved;
	        public string desktop;
	        public string title;
	        public int x;
	        public int y;
	        public int width;
	        public int height;
	        public int consoleColumns;
	        public int consoleRows;
	        public int consoleFillAttribute;
	        public int flags;
	        public short showWindow;
	        public short reserverd2;
	        public IntPtr reserved3;
	        public IntPtr stdInputHandle;
	        public IntPtr stdOutputHandle;
	        public IntPtr stdErrorHandle;
	    }
	    
	    [DllImport("user32.dll")]
    	internal static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, int lParam);

		[DllImport("user32.dll")]    	
		internal static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpfn, int lParam);    	
		
		internal delegate bool EnumWindowsProc(IntPtr hwnd, int lParam);
		
		[DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int processId);
		
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		internal static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
		
		internal const UInt32 WM_CLOSE = 0x0010;		

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
		
		[DllImport("user32.dll")]  
		internal static extern int GetWindowTextLength(IntPtr hWnd);
		
		internal static string GetWindowText(IntPtr hWnd)
		{
		    // Allocate correct string length first
		    int length       = GetWindowTextLength(hWnd);
		    StringBuilder sb = new StringBuilder(length + 1);
		    GetWindowText(hWnd, sb, sb.Capacity);
		    return sb.ToString();
		}
	}
}
