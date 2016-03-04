/*
 * Created by SharpDevelop.
 * User: YLIN68
 * Date: 3/4/2016
 * Time: 8:43 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace launcher
{
	/// <summary>
	/// Description of SessionFinder.
	/// </summary>
	internal static class SessionFinder
	{
	    private const int INT_ConsoleSession = -1;
	
	    internal static IntPtr GetLocalInteractiveSession()
	    {
	        IntPtr tokenHandle = IntPtr.Zero;
	        int sessionID = NativeMethods.WTSGetActiveConsoleSessionId();
	        if (sessionID != INT_ConsoleSession)
	        {
	            if (!NativeMethods.WTSQueryUserToken(sessionID, out tokenHandle))
	            {
	                throw new System.ComponentModel.Win32Exception();
	            }
	        }
	        return tokenHandle;
	    }
	}
}
