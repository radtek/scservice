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
		
		public scservice()
		{
			InitializeComponent();
			System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
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
			Execute(System.Reflection.Assembly.GetEntryAssembly().Location,System.AppDomain.CurrentDomain.BaseDirectory);
		}
		
		/// <summary>
		/// Stop this service.
		/// </summary>
		protected override void OnStop()
		{
			// TODO: Add tear-down code here (if required) to stop your service.
			using(StreamWriter sw=new StreamWriter("stop"))
			{
				sw.Write("1");
			}
			while(File.Exists("stop"))
			{
				Thread.Sleep(100);
			}
		}
		
		public void Execute(string file,string dir)
		{
		    IntPtr sessionTokenHandle = IntPtr.Zero;
		    try
		    {
		        sessionTokenHandle = SessionFinder.GetLocalInteractiveSession();
		        if (sessionTokenHandle != IntPtr.Zero)
		        {
		            ProcessLauncher.StartProcessAsUser(file, "" , dir, sessionTokenHandle);
		        }
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
		}
		
	}
}
