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
using System.IO.Compression;

namespace sc
{
	/// <summary>
	/// Description of Storage.
	/// </summary>
	public static class Storage
	{
		static string _archiveName;
		static FileStream _stream;
		static ZipArchive _archive;
		public static bool Zip{get;private set;}
		public static string ZipExtension{get; private set;}
		static Storage()
		{
			Zip=true;
			ZipExtension=".zip";
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
		}
		
		public static void Save(Stream inputStream,string filename)
		{
			inputStream.Position=0;
			if(Zip)
			{
				string newName=DateTime.Now.ToString("yyMMddHHmm")+ZipExtension;
				if(newName!=_archiveName || _stream==null)
				{
					Close();
					_archiveName=newName;
					Open();
				}
				ZipArchiveEntry entry=_archive.CreateEntry(filename);
				using(Stream entryStream=entry.Open())
				{
					inputStream.CopyTo(entryStream);
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
			if(_archive!=null) _archive.Dispose();
			_archive=null;
			if(_stream!=null) _stream.Dispose();
			_stream=null;
		}
		public static void Open()
		{
			_stream=new FileStream(_archiveName,FileMode.Create);
			_archive=new ZipArchive(_stream,ZipArchiveMode.Create,true);
		}
	}
}
