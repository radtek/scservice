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
using System.Windows.Forms;
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
				if(args[0].ToLower()=="stop")
				{
					CloseOtherProcess();
				}
				else
				{
					EncodeFile(args[0]);
				}
			}
			else
			{
				System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
				Application.Run(new HiddenForm());
			}
		}
		
		static void CloseOtherProcess()
		{
			HiddenForm form=new HiddenForm();
			form.CloseOtherProcess();
			form.Close();
		}
		
		static void EncodeFile(string filename)
		{
			if(!File.Exists(filename)) return;
			using(StreamReader sr=new StreamReader(filename))
			{
				using(StreamWriter sw=new StreamWriter(filename+".dat"))
				{
					sw.Write(Util.Encode(sr.ReadToEnd()));
				}
			}
		}
	}
}
