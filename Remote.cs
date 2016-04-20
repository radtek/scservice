/*
 * Created by SharpDevelop.
 * User: YLIN68
 * Date: 4/15/2016
 * Time: 4:27 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Net.FtpClient;

namespace sc
{
	/// <summary>
	/// Description of Remote.
	/// </summary>
	static public class Remote
	{
		static public bool FTP{get;private set;}
		static public string FTPServer{get;private set;}
		static public string FTPUser{get;private set;}
		static public string FTPPassword{get;private set;}
		static public string FTPPath{get;private set;}

		public static void Config(string file)
		{
			using(FileStream fs=new FileStream(file,FileMode.Open))
			{
				Config(Util.Decode(fs));
			}
		}
		
		static void Config(Stream stream)
		{
			IniFile ini=new IniFile();
			ini.Load(stream);
			FTP=ini.GetBoolean("remote","ftp",FTP);
			if(FTP)
			{
				FTPServer=ini.GetString("remote","ftpserver","127.0.0.1");
				FTPUser=ini.GetString("remote","ftpuser","anonymous");
				FTPPassword=ini.GetString("remote","ftppassword","abc@earth.com");
				FTPPath=ini.GetString("remote","ftppath","");
				FTPPath=FTPPath.Replace('\\','/');
				if(FTPPath!="" && !FTPPath.EndsWith("/")) FTPPath+="/";
			}
		}
		
		static internal bool UploadFile(string filename)
		{
			try
			{
				using(FtpClient client=new FtpClient())
				{
					client.Host=FTPServer;
					client.Credentials=new System.Net.NetworkCredential(FTPUser,FTPPassword);
					using(Stream st=client.OpenWrite(FTPPath+Path.GetFileName(filename)))
					{
						using(FileStream fs=new FileStream(filename,FileMode.Open))
						{
							fs.CopyTo(st);
							return true;
						}
					}
				}
			}
			catch
			{
				return false;
			}
		}
	}
}
