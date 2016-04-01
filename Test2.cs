/*
 * Created by SharpDevelop.
 * User: ylin68
 * Date: 3/4/2016
 * Time: 11:16 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using NUnit.Framework;
using Ionic.Zip;
using System.Diagnostics;
using launcher;

namespace scservice
{
	[TestFixture]
	public class Test2
	{
		[Test]
		public void TestZip()
		{
			// TODO: Add your test.
			string Content = "This string will be the content of the Readme.txt file in the zip archive.";
			using (ZipFile zip1 = new ZipFile())
			{
			  zip1.AddEntry("Readme.txt", Content);
			  zip1.Comment = "This zip file was created at " + System.DateTime.Now.ToString("G");
			  zip1.Save("Content.zip");
			}				
		}

		[Test]
		public void GetProcessWindow()
		{
			foreach(Process p in Process.GetProcessesByName("scservice"))
			{
	            // Check if main window exists. If the window is minimized to the tray this might be not the case.
	            Console.WriteLine("Process found, Name:{0} id:{1}",p.ProcessName,p.Id);
	            if (p.MainWindowHandle == IntPtr.Zero)
	            {
		            // Try closing application by sending WM_CLOSE to all child windows in all threads.
		            foreach (ProcessThread pt in p.Threads)
		            {
		                NativeMethods.EnumThreadWindows((uint) pt.Id, new NativeMethods.EnumWindowsProc(EnumThreadCallback), p.Id);
		            }
	            }
	            else
	            {
		            // Try to close main window.
		            if(p.CloseMainWindow())
		            {
		                // Free resources used by this Process object.
		                p.Close();
		            }
	            }
        	}
		}
		
		bool EnumThreadCallback(IntPtr hWnd, int lParam)
	    {
			string name=NativeMethods.GetWindowText(hWnd);
	        Console.WriteLine("Found Window {0} Handle {1} under process id {2}",name,hWnd,lParam);
	        if(name=="SCService")
	        {
	        	Console.WriteLine("Closing window");
	        	NativeMethods.SendMessage(hWnd, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
	        }
	        return true;
	    }
	}
}
