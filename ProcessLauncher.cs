/*
 * Created by SharpDevelop.
 * User: YLIN68
 * Date: 3/4/2016
 * Time: 8:40 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Runtime.InteropServices;

namespace launcher
{
	/// <summary>
	/// Description of ProcessLauncher.
	/// </summary>
	internal static class ProcessLauncher
	{
	    internal static void StartProcessAsUser(string executablePath, string commandline, string workingDirectory, IntPtr sessionTokenHandle)
	    {
	        var processInformation = new NativeMethods.PROCESS_INFORMATION();
	        try
	        {
	            var startupInformation = new NativeMethods.STARTUPINFO();
	            startupInformation.length = Marshal.SizeOf(startupInformation);
	            startupInformation.desktop = string.Empty;
	            bool result = NativeMethods.CreateProcessAsUser
	            (
	                sessionTokenHandle,
	                executablePath,
	                commandline,
	                IntPtr.Zero,
	                IntPtr.Zero,
	                false,
	                0,
	                IntPtr.Zero,
	                workingDirectory,
	                ref startupInformation,
	                ref processInformation
	            );
	            if (!result)
	            {
	                int error = Marshal.GetLastWin32Error();
	                string message = string.Format("CreateProcessAsUser Error: {0}", error);
	                throw new ApplicationException(message);
	            }
	        }
	        finally
	        {
	            if (processInformation.processHandle != IntPtr.Zero)
	            {
	                NativeMethods.CloseHandle(processInformation.processHandle);
	            }
	            if (processInformation.threadHandle != IntPtr.Zero)
	            {
	                NativeMethods.CloseHandle(processInformation.threadHandle);
	            }
	            if (sessionTokenHandle != IntPtr.Zero)
	            {
	                NativeMethods.CloseHandle(sessionTokenHandle);
	            }
	        }
	    }
	}
	
}
