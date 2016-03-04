/*
 * Created by SharpDevelop.
 * User: YLIN68
 * Date: 2/29/2016
 * Time: 4:17 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Text;
using System.Linq;
using System.IO;

namespace sc
{
	/// <summary>
	/// Description of Util.
	/// </summary>
	static public class Util
	{
  
	    public static bool IsWorkstationLocked()
	    {
	      const int DESKTOP_SWITCHDESKTOP = 256;
	      IntPtr hwnd = API.OpenInputDesktop(0, false, DESKTOP_SWITCHDESKTOP);
	
	      if (hwnd == IntPtr.Zero)
	      {
	        // Could not get the input desktop, might be locked already?
	        hwnd = API.OpenDesktop("Default", 0, false, DESKTOP_SWITCHDESKTOP);
	      }
	
	      // Can we switch the desktop?
	      if (hwnd != IntPtr.Zero)
	      {
	        if (API.SwitchDesktop(hwnd))
	        {
	          // Workstation is NOT LOCKED.
	          API.CloseDesktop(hwnd);
	        }
	        else
	        {
	          API.CloseDesktop(hwnd);
	          // Workstation is LOCKED.
	          return true;
	        }
	      }
	
	      return false;
	    }
	
	    // Check if the screensaver is busy running.
		public static bool IsScreensaverRunning()
		{
		  const int SPI_GETSCREENSAVERRUNNING = 114;
		  bool isRunning = false;
		
		  if (!API.SystemParametersInfo(SPI_GETSCREENSAVERRUNNING, 0, ref isRunning, 0))
		  {
		    // Could not detect screen saver status...
		    return false;
		  }
		
		  if (isRunning)
		  {
		    // Screen saver is ON.
		    return true;
		  }
		
		  // Screen saver is OFF.
		  return false;
		}
    	
		public static string Encode(string input)
		{
			/*
			byte[]bytes=Encoding.ASCII.GetBytes(input);
			return string.Join("",bytes.Select(b=>((byte)(~b & 0xff)).ToString("X2")));
			*/
			return string.Join("",Convert.ToBase64String(Encoding.ASCII.GetBytes(input)).TrimEnd(new System.Char[]{'='}).Reverse().ToArray());
		}
		
		public static Stream Decode(Stream input)
		{
			input.Position=0;
			string str=(new StreamReader(input)).ReadToEnd();
			str=Decode(str);
			return new MemoryStream(Encoding.ASCII.GetBytes(str));
		}
		
		public static string Decode(string input)
		{
			/*
			try
			{
				byte[] bytes = new byte[input.Length / 2];
			    for (int i = 0; i < bytes.Length; i++)
			    {
			        string currentHex = input.Substring(i * 2, 2);
			        bytes[i] = Convert.ToByte(currentHex, 16);
			        bytes[i]=(byte)(~bytes[i] & 0xff);
			    }
			    return Encoding.ASCII.GetString(bytes);
			}
			catch
			{
				return "";
			}
			*/
			try
			{
				input=string.Join("",input.Reverse().ToArray());
				if(input.Length%4!=0)
				{
					input+=new string('=',4-(input.Length%4));
				}
				return Encoding.ASCII.GetString(Convert.FromBase64String(input));
			}
			catch
			{
				return "";
			}
			 
		}
	}
}
