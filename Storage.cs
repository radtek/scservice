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
		
		public static bool Zip{get;private set;}
		public static string ZipExtension{get; private set;}
		public static string ZipPassword{get;private set;}
		static Storage()
		{
			Zip=true;
			ZipExtension=".zip";
			ZipPassword="";
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
			if(ZipExtension[0]!='.') ZipExtension="."+ZipExtension;
			ZipPassword=ini.GetString("storage","zipPassword",ZipPassword);
		}
		
		public static void Save(Stream inputStream,string filename)
		{
			inputStream.Position=0;
			if(Zip)
			{
				string newName=DateTime.Now.ToString("yyMMddHHmm")+ZipExtension;
				if(newName!=_archiveName || _archive==null)
				{
					Close();
					_archiveName=newName;
					Open();
				}
				using(MemoryStream ms=new MemoryStream())
				{
					inputStream.CopyTo(ms);
					ZipEntry entry=_archive.AddEntry(filename,ms.ToArray());
				}
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
