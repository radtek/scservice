/*
 * Created by SharpDevelop.
 * User: YLIN68
 * Date: 2/29/2016
 * Time: 4:18 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.InteropServices;

namespace sc
{
	/// <summary>
	/// Description of API.
	/// </summary>
	static public class API
	{
		[StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
	    [DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
	    public static extern IntPtr GetDC(IntPtr hWnd);
	
	    [DllImport("user32.dll", ExactSpelling = true)]
	    public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
	
	    [DllImport("gdi32.dll", ExactSpelling = true)]
	    public static extern IntPtr BitBlt(IntPtr hDestDC, int x, int y, int nWidth, int nHeight, IntPtr hSrcDC, int xSrc, int ySrc, int dwRop);

	    [DllImport("gdi32.dll", ExactSpelling = true)]
	    public static extern IntPtr StretchBlt(IntPtr hDestDC, int x, int y, int nWidthDest, int nHeightDest, IntPtr hSrcDC, int xSrc, int ySrc, int nWidthSrc, int nHeightSrc, int dwRop);
	
	    [DllImport("user32.dll", EntryPoint = "GetDesktopWindow")]
	    public static extern IntPtr GetDesktopWindow();
	    
	    [DllImport("user32.dll")]
        public static extern IntPtr GetWindowRect(IntPtr hWnd,ref RECT rect);
        
        public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter

		[DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC,int nWidth, 
            int nHeight);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC,IntPtr hObject);
        
        [DllImport("gdi32.dll")]
  		public static extern IntPtr CreateDIBSection(IntPtr hdc, ref BITMAPINFO bmi, uint Usage, out IntPtr bits, IntPtr hSection, uint dwOffset);
		
  		public static uint BI_RGB = 0;
  		public static uint DIB_RGB_COLORS=0;
  		[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)] 
		public struct BITMAPINFO
		{ 
			public uint biSize;
			public int biWidth, biHeight;
			public short biPlanes, biBitCount;
			public uint biCompression, biSizeImage;
			public int biXPelsPerMeter, biYPelsPerMeter;
			public uint biClrUsed, biClrImportant;
			[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst=256)]
			public uint[] cols;
		}

		public static uint MAKERGB(int r,int g,int b)
		{ 
			return ((uint)(b&255)) | ((uint)((r&255)<<8)) | ((uint)((g&255)<<16));
		}  		
		
		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SystemParametersInfo(uint uAction, 
		                                               uint uParam, 
		                                               ref bool lpvParam,
		                                               int fWinIni);
		
		// Used to check if the workstation is locked
		[DllImport("user32", SetLastError = true)]
		public static extern IntPtr OpenDesktop(string lpszDesktop,
		                                         uint dwFlags,
		                                         bool fInherit,
		                                         uint dwDesiredAccess);
		
		[DllImport("user32", SetLastError = true)]
		public static extern IntPtr OpenInputDesktop(uint dwFlags,
		                                              bool fInherit,
		                                              uint dwDesiredAccess);
		
		[DllImport("user32", SetLastError = true)]
		public static extern IntPtr CloseDesktop(IntPtr hDesktop);
		
		[DllImport("user32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool SwitchDesktop(IntPtr hDesktop);
	}
}
