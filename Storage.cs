/*
 * Created by SharpDevelop.
 * User: YLIN68
 * Date: 3/2/2016
 * Time: 8:49 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Text;
using Ionic.Zip;

namespace sc
{
	/// <summary>
	/// Description of Storage.
	/// </summary>
	public static class Storage
	{
		static string _archiveName;
		static ZipFile _archive;
		static int _entries=0;
		
		public static bool Zip{get;private set;}
		public static string ZipExtension{get; private set;}
		public static string ZipPassword{get;private set;}
		public static int Entries{get;private set;}
		public static bool FTP{get;private set;}
		public static bool DeleteAfterUpload{get;private set;}
		
		static Storage()
		{
			Zip=true;
			ZipExtension=".zip";
			ZipPassword="";
			Entries=50;
			FTP=false;
			DeleteAfterUpload=false;
		}

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
			Zip=ini.GetBoolean("storage","zip",Zip);
			ZipExtension=ini.GetString("storage","zipExtension",ZipExtension);
			if(ZipExtension!="" && ZipExtension[0]!='.') ZipExtension="."+ZipExtension;
			ZipPassword=ini.GetString("storage","zipPassword",ZipPassword);
			Entries=ini.GetInt("storage","entries",Entries);
			FTP=ini.GetBoolean("remote","ftp",FTP);
			DeleteAfterUpload=ini.GetBoolean("remote","DeleteAfterUpload",DeleteAfterUpload);
			if(Entries<=0) Entries=50;
		}
		
		public static void Save(Stream inputStream,string filename)
		{
			inputStream.Position=0;
			if(Zip)
			{
				if(_entries>=Entries || _archive==null)
				{
					Close();
					_archiveName=System.Guid.NewGuid().ToString()+ZipExtension;
					Open();
					_entries=0;
				}
				using(MemoryStream ms=new MemoryStream())
				{
					inputStream.CopyTo(ms);
					ZipEntry entry=_archive.AddEntry(filename,ms.ToArray());
				}
				_entries++;
			}
			else
			{
				using(FileStream sw=new FileStream(filename,FileMode.Create))
				{
					inputStream.CopyTo(sw);
				}
			}
		}
		public static void Close()
		{
			if(_archive!=null)
			{
				_archive.Save();
				if(FTP)
				{
					if(Remote.UploadFile(_archiveName)&&DeleteAfterUpload)
					{
						File.Delete(_archiveName);
					}
				}
				_archive.Dispose();
				_archive=null;
			}
			
		}
		static void Open()
		{
			_archive=new ZipFile(_archiveName);
			if(ZipPassword!="")
			{
				_archive.Password=ZipPassword;
			}
		}
	}
}
