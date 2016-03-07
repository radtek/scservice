/*
 * Created by SharpDevelop.
 * User: ylin68
 * Date: 3/4/2016
 * Time: 11:16 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using NUnit.Framework;
using Ionic.Zip;

namespace scservice
{
	[TestFixture]
	public class Test2
	{
		[Test]
		public void TestZip()
		{
			// TODO: Add your test.
			string Content = "This string will be the content of the Readme.txt file in the zip archive.";
			using (ZipFile zip1 = new ZipFile())
			{
			  zip1.AddEntry("Readme.txt", Content);
			  zip1.Comment = "This zip file was created at " + System.DateTime.Now.ToString("G");
			  zip1.Save("Content.zip");
			}				
		}
	}
}
