/*
 * Created by SharpDevelop.
 * User: YLIN68
 * Date: 3/3/2016
 * Time: 2:22 PM
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using launcher;

namespace scservice
{
	public class scservice : ServiceBase
	{
		public const string MyServiceName = "scservice";
		System.Diagnostics.EventLog eventLog;
		
		public scservice()
		{
			InitializeComponent();
			CanHandleSessionChangeEvent=true;
			System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
			eventLog = new System.Diagnostics.EventLog();
			if (!System.Diagnostics.EventLog.SourceExists("scservice"))
			{
					System.Diagnostics.EventLog.CreateEventSource(
						"scservice","Application");
			}
			eventLog.Source = "scservice";
			eventLog.Log = "Application";
		}
		
		private void InitializeComponent()
		{
			this.ServiceName = MyServiceName;
		}
		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			// TODO: Add cleanup code here (if required)
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// Start this service.
		/// </summary>
		protected override void OnStart(string[] args)
		{
			// TODO: Add start code here (if required) to start your service.
			if(File.Exists("stop"))
				File.Delete("stop");
			Process p=GetBackgroundProcess();
			if(p!=null)
			{
				eventLog.WriteEntry("Background process already running: "+p.MainModule.FileName);
				return;
			}

			if(!Execute(System.Reflection.Assembly.GetEntryAssembly().Location,"",System.AppDomain.CurrentDomain.BaseDirectory))
			{
				eventLog.WriteEntry("No user is logged-on.");
			}
		}
		
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			// TODO: Add tear-down code here (if required) to stop your service.
			Process p=GetBackgroundProcess();
			if(p!=null)
			{
				Execute(System.Reflection.Assembly.GetEntryAssembly().Location," stop",System.AppDomain.CurrentDomain.BaseDirectory);
	            return;
        	}
			eventLog.WriteEntry("Background process not running.");
		}
		
		Process GetBackgroundProcess()
		{
			Process proc=Process.GetCurrentProcess();
			foreach(Process p in Process.GetProcessesByName(proc.ProcessName))
			{
				if(p.Id!=proc.Id && p.SessionId!=proc.SessionId && p.MainModule.FileName==proc.MainModule.FileName)
				{
					return p;
				}
			}
			return null;
		}
		
		protected override void OnSessionChange(SessionChangeDescription changeDescription)
		{
		    switch (changeDescription.Reason)
		    {
		        case SessionChangeReason.SessionLogoff:
		            break;
		        case SessionChangeReason.SessionLogon:
		            if(changeDescription.SessionId==NativeMethods.WTSGetActiveConsoleSessionId())
		            	OnStart(null);
		            break;
		    }
		}
		
		public bool Execute(string file,string param,string dir)
		{
		    IntPtr sessionTokenHandle = IntPtr.Zero;
		    try
		    {
		        sessionTokenHandle = SessionFinder.GetLocalInteractiveSession();
		        if (sessionTokenHandle != IntPtr.Zero)
		        {
		            ProcessLauncher.StartProcessAsUser(file, param , dir, sessionTokenHandle);
		        }
		    }
		    catch(System.ComponentModel.Win32Exception ex)
		    {
		    	if(ex.NativeErrorCode==1008) //ERROR_NO_TOKEN: No user is logged-on
		    		return false;
		    	throw ex;
		    }
		    catch(Exception ex)
		    {
		        //What are we gonna do?
		        throw ex;
		    }
		    finally
		    {
		        if (sessionTokenHandle != IntPtr.Zero)
		        {
		            NativeMethods.CloseHandle(sessionTokenHandle);
		        }
		    }
		    return true;
		}
		
	}
}
