/*
 * Created by SharpDevelop.
 * User: YLIN68
 * Date: 3/2/2016
 * Time: 2:48 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using NUnit.Framework;
using System.Linq;
using System.IO;
using System.Text;

namespace sc
{
	[TestFixture]
	public class Test1
	{
		[Test]
		public void TestEncoder()
		{
			// TODO: Add your test.
			string input="asfds87s07uvasodn;238ur[0okmqCSDA";
			Assert.AreEqual(input,Util.Decode(Util.Encode(input)));
		}
		[Test]
		public void TestConfig()
		{
			string encoded=Util.Encode(
@"[main]
count=-1
interval=3000
quality=25
skipScreenSaver=true
skipLocked=true
extension=dat
[storage]
zip=true
zipExtension=dat
entries=100
zipPassword=asdf");
			using(StreamWriter sw=new StreamWriter("c.dat",false))
			{
				sw.Write(encoded);
			}
			ScreenShot.Config("c.dat");
			Storage.Config("c.dat");
			Assert.AreEqual(3000,ScreenShot.Interval);
			Assert.AreEqual(".dat",ScreenShot.Extension);
			Assert.AreEqual(true,Storage.Zip);
			Assert.AreEqual(".dat",Storage.ZipExtension);
			Assert.AreEqual(100,Storage.Entries);
		}
		[Test]
		public void BenchmarkLocked()
		{
			DateTime dt=DateTime.Now;
			bool l;
			for(int i=0;i++<100000;)
			{
				l=Util.IsWorkstationLocked();
				l=Util.IsScreensaverRunning();
			}
			Console.WriteLine("Used: {0} ms",(DateTime.Now-dt).TotalMilliseconds);
		}
	}
}
