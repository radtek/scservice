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
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using sc;

namespace scservice
{
	static class Program
	{
		/// <summary>
		/// This method starts the service.
		/// </summary>
		static void Main(string[]args)
		{
			// To run more than one service you have to add them here
			if(!System.Environment.UserInteractive)
				ServiceBase.Run(new ServiceBase[] { new scservice() });
			else if(args.Length==1)
			{
				if(!File.Exists(args[0])) return;
				using(StreamReader sr=new StreamReader(args[0]))
				{
					using(StreamWriter sw=new StreamWriter(args[0]+".dat"))
					{
						sw.Write(Util.Encode(sr.ReadToEnd()));
					}
				}
			}
			else
			{
				System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
				Task task=Task.Run(()=>ScreenShot.Start());
				while(true)
				{
					if(File.Exists("stop"))
					{
						ScreenShot.Stop();
						Task.WaitAll(task);
						File.Delete("stop");
						break;
					}
					Thread.Sleep(3000);
				}
			}
		}
	}
}
