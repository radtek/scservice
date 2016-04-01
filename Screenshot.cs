/*
 * Created by SharpDevelop.
 * User: YLIN68
 * Date: 2/29/2016
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Threading;
using System.IO;

namespace sc
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	
	static internal class ScreenShot
	{		
		static ImageCodecInfo imageCodecInfo;
		static EncoderParameters encoderParams=new EncoderParameters(1);
		static bool stopped=true;
		
        static IntPtr handle;
        static IntPtr hdcSrc;
        static API.RECT windowRect;
        static int width;
        static int height;
        static IntPtr hdcDest;
        static IntPtr hBitmap; 
        static IntPtr hOld;
        static Bitmap bmp=null,prevBmp=null;
        static object locker=new object();

		public static bool SkipLocked{get;private set;}
		public static bool SkipScreenSaver{get;private set;}
		public static int Count{get;private set;}
		public static int Interval{get;private set;}
		public static long Quality{get;private set;}
		public static string Extension{get;private set;}
		
		static ScreenShot()
		{
			 imageCodecInfo = GetEncoderInfo("image/jpeg");
			 SkipLocked=SkipScreenSaver=true;
			 Count=-1;
			 Interval=3000;
			 Quality=25L;
			 Extension=".jpg";
		}
		static void SetEncoderParameters(long quality)
		{
			EncoderParameter qualParam = new EncoderParameter(Encoder.Quality, quality);
			encoderParams.Param[0]=qualParam;			
		}
		
		public static void Config(int c,int i,long q)
		{
			Count=c;
			Interval=i;
			Quality=q;
		}
		
		static void Config(Stream stream)
		{
			IniFile ini=new IniFile();
			ini.Load(stream);
			Count=ini.GetInt("main","count",Count);
			Interval=ini.GetInt("main","interval",Interval);
			Quality=ini.GetLong("main","quality",Quality);
			SkipLocked=ini.GetBoolean("main","skipLocked",SkipLocked);
			SkipScreenSaver=ini.GetBoolean("main","skipScreenSaver",SkipScreenSaver);
			Extension=ini.GetString("main","extension",Extension);
			if(Extension!="" && Extension[0]!='.') Extension="."+Extension;
		}
		
		public static void Config(string file)
		{
			using(FileStream fs=new FileStream(file,FileMode.Open))
			{
				Config(Util.Decode(fs));
			}
		}
		
		public static void Stop()
		{
			lock(locker)
			{
				if(stopped) return;
	            if(bmp!=null) bmp.Dispose();
	            // clean up 
	            API.DeleteDC(hdcDest);
	            API.ReleaseDC(handle,hdcSrc);
	            API.DeleteObject(hBitmap);
	            Storage.Close();
				stopped=true;
			}
		}
		
		public static void Start()
		{
			try
			{
				Config("c.dat");
				Storage.Config("c.dat");
			}
			catch
			{
			}
			SetEncoderParameters(Quality);
			lock(locker)
			{
	            handle=API.GetDesktopWindow();
	            hdcSrc = API.GetWindowDC(handle);
	            windowRect = new API.RECT();
	            API.GetWindowRect(handle,ref windowRect);
	            width = windowRect.right - windowRect.left;
	            height = windowRect.bottom - windowRect.top;
	            hdcDest = API.CreateCompatibleDC(hdcSrc);
	            hBitmap = API.CreateCompatibleBitmap(hdcSrc,width,height);
	            stopped=false;
			}
			int i=0;
			while(true)
			{
				lock(locker)
				{
					if(stopped) break;
					Take();
				}
				if(Count>0)
				{
					i++;
					if(i>=Count) break;
				}
				lock(locker)
				{
					if(stopped) break;
				}
				Thread.Sleep(Interval);
			}
			if(!stopped) Stop();
		}
		
		public static void Take()
		{
			if((SkipScreenSaver && Util.IsScreensaverRunning()) ||
			   (SkipLocked && Util.IsWorkstationLocked())
			  )
			{
				return;
			}
        	hOld = API.SelectObject(hdcDest,hBitmap);
            API.BitBlt(hdcDest,0,0,width,height,hdcSrc,0,0,API.SRCCOPY);
            API.SelectObject(hdcDest,hOld);
			bmp = Image.FromHbitmap(hBitmap);
			if(prevBmp==null || IsDifferent(prevBmp,bmp))
			{
				using(MemoryStream stream=new MemoryStream())
				{
					bmp.Save(stream,imageCodecInfo,encoderParams);
					Storage.Save(stream,DateTime.Now.ToString("yyMMddHHmmssfff")+Extension);
				}
			}
			if(prevBmp!=null) prevBmp.Dispose();
			prevBmp=bmp;
		}

		public static bool IsDifferent(Bitmap bmp1,Bitmap bmp2)
		{
			bool same = true;
			if(bmp1.Width!=bmp2.Width  || bmp1.Height!=bmp2.Height) 
				return true;
			Rectangle rect = new Rectangle(0, 0, bmp1.Width, bmp1.Height);
			BitmapData bmpData1 = bmp1.LockBits(rect, ImageLockMode.ReadOnly, bmp1.PixelFormat);
			BitmapData bmpData2 = bmp2.LockBits(rect, ImageLockMode.ReadOnly, bmp2.PixelFormat);
			int stride1=bmpData1.Stride/4;
			int stride2=bmpData2.Stride/4;
			unsafe {
				int* ptr1 = (int*)bmpData1.Scan0.ToPointer();
			  	int* ptr2 = (int*)bmpData2.Scan0.ToPointer();
				for (int y = 0; same && y < rect.Height; y++) 
				{
					for (int x = 0; x < Math.Abs(stride1); x++)
				    {
				      if (*ptr1 != *ptr2) 
				      {
				        same = false;
				        break;
				      }
				      ptr1++;
				      ptr2++;
				    }
				  	if(stride1<0)
				  		ptr1 -= Math.Abs(stride1)*2;
				  	if(stride2<0)
				  		ptr2 -= Math.Abs(stride2)*2;
				}
			}
			bmp1.UnlockBits(bmpData1);
			bmp2.UnlockBits(bmpData2);
			return !same;
		}
		
		public static Image CaptureWindow(IntPtr handle)
        {
            // get te hDC of the target window
            IntPtr hdcSrc = API.GetWindowDC(handle);
            // get the size
            API.RECT windowRect = new API.RECT();
            API.GetWindowRect(handle,ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            IntPtr hdcDest = API.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = API.CreateCompatibleBitmap(hdcSrc,width,height); 
            // select the bitmap object
            IntPtr hOld = API.SelectObject(hdcDest,hBitmap);
            // bitblt over
            API.BitBlt(hdcDest,0,0,width,height,hdcSrc,0,0,API.SRCCOPY);
            // restore selection
            API.SelectObject(hdcDest,hOld);
            // clean up 
            API.DeleteDC(hdcDest);
            API.ReleaseDC(handle,hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            API.DeleteObject(hBitmap);
            return img;
        }
		
		/// <summary>
		/// Copies a bitmap into a 1bpp/8bpp bitmap of the same dimensions, fast
		/// </summary>
		/// <param name="b">original bitmap</param>
		/// <param name="bpp">1 or 8, target bpp</param>
		/// <returns>a 1bpp copy of the bitmap</returns>
		static System.Drawing.Bitmap CopyToBpp(Bitmap b, int bpp)
		{ 
			if (bpp!=1 && bpp!=8) throw new System.ArgumentException("1 or 8","bpp");
		
			// Plan: built into Windows GDI is the ability to convert
			// bitmaps from one format to another. Most of the time, this
			// job is actually done by the graphics hardware accelerator card
			// and so is extremely fast. The rest of the time, the job is done by
			// very fast native code.
			// We will call into this GDI functionality from C#. Our plan:
			// (1) Convert our Bitmap into a GDI hbitmap (ie. copy unmanaged->managed)
			// (2) Create a GDI monochrome hbitmap
			// (3) Use GDI "BitBlt" function to copy from hbitmap into monochrome (as above)
			// (4) Convert the monochrone hbitmap into a Bitmap (ie. copy unmanaged->managed)
			
			int w=b.Width, h=b.Height;
			IntPtr hbm = b.GetHbitmap(); // this is step (1)
			//
			// Step (2): create the monochrome bitmap.
			// "BITMAPINFO" is an interop-struct which we define below.
			// In GDI terms, it's a BITMAPHEADERINFO followed by an array of two RGBQUADs
			API.BITMAPINFO bmi = new API.BITMAPINFO();
			bmi.biSize=40;  // the size of the BITMAPHEADERINFO struct
			bmi.biWidth=w;
			bmi.biHeight=h;
			bmi.biPlanes=1; // "planes" are confusing. We always use just 1. Read MSDN for more info.
			bmi.biBitCount=(short)bpp; // ie. 1bpp or 8bpp
			bmi.biCompression=API.BI_RGB; // ie. the pixels in our RGBQUAD table are stored as RGBs, not palette indexes
			bmi.biSizeImage = (uint)(((w+7)&0xFFFFFFF8)*h/8);
			bmi.biXPelsPerMeter=1000000; // not really important
			bmi.biYPelsPerMeter=1000000; // not really important
			// Now for the colour table.
			uint ncols = (uint)1<<bpp; // 2 colours for 1bpp; 256 colours for 8bpp
			bmi.biClrUsed=ncols;
			bmi.biClrImportant=ncols;
			bmi.cols=new uint[256]; // The structure always has fixed size 256, even if we end up using fewer colours
			if (bpp==1) 
			{
				bmi.cols[0]=API.MAKERGB(0,0,0); 
				bmi.cols[1]=API.MAKERGB(255,255,255);
			}
			else {for (int i=0; i<ncols; i++) bmi.cols[i]=API.MAKERGB(i,i,i);}
			// For 8bpp we've created an palette with just greyscale colours.
			// You can set up any palette you want here. Here are some possibilities:
			// greyscale: for (int i=0; i<256; i++) bmi.cols[i]=MAKERGB(i,i,i);
			// rainbow: bmi.biClrUsed=216; bmi.biClrImportant=216; int[] colv=new int[6]{0,51,102,153,204,255};
			//          for (int i=0; i<216; i++) bmi.cols[i]=MAKERGB(colv[i/36],colv[(i/6)%6],colv[i%6]);
			// optimal: a difficult topic: http://en.wikipedia.org/wiki/Color_quantization
			// 
			// Now create the indexed bitmap "hbm0"
			IntPtr bits0; // not used for our purposes. It returns a pointer to the raw bits that make up the bitmap.
			IntPtr hbm0 = API.CreateDIBSection(IntPtr.Zero,ref bmi,API.DIB_RGB_COLORS,out bits0,IntPtr.Zero,0);
			//
			// Step (3): use GDI's BitBlt function to copy from original hbitmap into monocrhome bitmap
			// GDI programming is kind of confusing... nb. The GDI equivalent of "Graphics" is called a "DC".
			IntPtr sdc = API.GetDC(IntPtr.Zero);       // First we obtain the DC for the screen
			// Next, create a DC for the original hbitmap
			IntPtr hdc = API.CreateCompatibleDC(sdc); 
			API.SelectObject(hdc,hbm);
			// and create a DC for the monochrome hbitmap
			IntPtr hdc0 = API.CreateCompatibleDC(sdc); 
			API.SelectObject(hdc0,hbm0);
			// Now we can do the BitBlt:
			API.BitBlt(hdc0,0,0,w,h,hdc,0,0,API.SRCCOPY);
			// Step (4): convert this monochrome hbitmap back into a Bitmap:
			Bitmap b0 = Bitmap.FromHbitmap(hbm0);
			//
			// Finally some cleanup.
			API.DeleteDC(hdc);
			API.DeleteDC(hdc0);
			API.ReleaseDC(IntPtr.Zero,sdc);
			API.DeleteObject(hbm);
			API.DeleteObject(hbm0);
			//
			return b0;
		}

		
		private static Rectangle GetBoundingBoxForChanges(Bitmap _prevBitmap,Bitmap _newBitmap)
		{
		    // The search algorithm starts by looking
		    //    for the top and left bounds. The search
		    //    starts in the upper-left corner and scans
		    //    left to right and then top to bottom. It uses
		    //    an adaptive approach on the pixels it
		    //    searches. Another pass is looks for the
		    //    lower and right bounds. The search starts
		    //    in the lower-right corner and scans right
		    //    to left and then bottom to top. Again, an
		    //    adaptive approach on the search area is used.
		    //
		 
		    // Note: The GetPixel member of the Bitmap class
		    //    is too slow for this purpose. This is a good
		    //    case of using unsafe code to access pointers
		    //    to increase the speed.
		    //
		 
		    // Validate the images are the same shape and type.
		    //
		    if (_prevBitmap.Width != _newBitmap.Width ||
		        _prevBitmap.Height != _newBitmap.Height ||
		        _prevBitmap.PixelFormat != _newBitmap.PixelFormat)
		    {
		        // Not the same shape...can't do the search.
		        //
		        return Rectangle.Empty;
		    }
		 
		    // Init the search parameters.
		    //
		    int width = _newBitmap.Width;
		    int height = _newBitmap.Height;
		    int left = width;
		    int right = 0;
		    int top = height;
		    int bottom = 0;
		 
		    BitmapData bmNewData = null;
		    BitmapData bmPrevData = null;
		    try
		    {
		        // Lock the bits into memory.
		        //
		        bmNewData = _newBitmap.LockBits(
		            new Rectangle(0, 0, _newBitmap.Width, _newBitmap.Height),
		            ImageLockMode.ReadOnly, _newBitmap.PixelFormat);
		        bmPrevData = _prevBitmap.LockBits(
		            new Rectangle(0, 0, _prevBitmap.Width, _prevBitmap.Height),
		            ImageLockMode.ReadOnly, _prevBitmap.PixelFormat);
		 
		        // The images are ARGB (4 bytes)
		        //
		        int numBytesPerPixel = 4;
		 
		        // Get the number of integers (4 bytes) in each row
		        //    of the image.
		        //
		        int strideNew = bmNewData.Stride / numBytesPerPixel;
		        int stridePrev = bmPrevData.Stride / numBytesPerPixel;
		 
		        // Get a pointer to the first pixel.
		        //
		        // Note: Another speed up implemented is that I don't
		        //    need the ARGB elements. I am only trying to detect
		        //    change. So this algorithm reads the 4 bytes as an
		        //    integer and compares the two numbers.
		        //
		        System.IntPtr scanNew0 = bmNewData.Scan0;
		        System.IntPtr scanPrev0 = bmPrevData.Scan0;
		 
		        // Enter the unsafe code.
		        //
		        unsafe
		        {
		            // Cast the safe pointers into unsafe pointers.
		            //
		            int* pNew = (int*)(void*)scanNew0;
		            int* pPrev = (int*)(void*)scanPrev0;
		 
		            // First Pass - Find the left and top bounds
		            //    of the minimum bounding rectangle. Adapt the
		            //    number of pixels scanned from left to right so
		            //    we only scan up to the current bound. We also
		            //    initialize the bottom & right. This helps optimize
		            //    the second pass.
		            //
		            // For all rows of pixels (top to bottom)
		            //
		            for (int y = 0; y < _newBitmap.Height; ++y)
		            {
		                // For pixels up to the current bound (left to right)
		                //
		                for (int x = 0; x < left; ++x)
		                {
		                    // Use pointer arithmetic to index the
		                    //    next pixel in this row.
		                    //
		                    if ((pNew + x)[0] != (pPrev + x)[0])
		                    {
		                        // Found a change.
		                        //
		                        if (x < left)
		                        {
		                            left = x;
		                        }
		                        if (x > right)
		                        {
		                            right = x;
		                        }
		                        if (y < top)
		                        {
		                            top = y;
		                        }
		                        if (y > bottom)
		                        {
		                            bottom = y;
		                        }
		                    }
		                }
		 
		                // Move the pointers to the next row.
		                //
		                pNew += strideNew;
		                pPrev += stridePrev;
		            }
		 
		            // If we did not find any changed pixels
		            //    then no need to do a second pass.
		            //
		            if (left != width)
		            {
		                // Second Pass - The first pass found at
		                //    least one different pixel and has set
		                //    the left & top bounds. In addition, the
		                //    right & bottom bounds have been initialized.
		                //    Adapt the number of pixels scanned from right
		                //    to left so we only scan up to the current bound.
		                //    In addition, there is no need to scan past
		                //    the top bound.
		                //
		 
		                // Set the pointers to the first element of the
		                //    bottom row.
		                //
		                pNew = (int*)(void*)scanNew0;
		                pPrev = (int*)(void*)scanPrev0;
		                pNew += (_newBitmap.Height - 1) * strideNew;
		                pPrev += (_prevBitmap.Height - 1) * stridePrev;
		 
		                // For each row (bottom to top)
		                //
		                for (int y = _newBitmap.Height - 1; y > top; y--)
		                {
		                    // For each column (right to left)
		                    //
		                    for (int x = _newBitmap.Width - 1; x > right; x--)
		                    {
		                        // Use pointer arithmetic to index the
		                        //    next pixel in this row.
		                        //
		                        if ((pNew + x)[0] != (pPrev + x)[0])
		                        {
		                            // Found a change.
		                            //
		                            if (x > right)
		                            {
		                                right = x;
		                            }
		                            if (y > bottom)
		                            {
		                                bottom = y;
		                            }
		                        }
		                    }
		 
		                    // Move up one row.
		                    //
		                    pNew -= strideNew;
		                    pPrev -= stridePrev;
		                }
		            }
		        }
		    }
		    catch (Exception ex)
		    {
		        int xxx = 0;
		    }
		    finally
		    {
		        // Unlock the bits of the image.
		        //
		        if (bmNewData != null)
		        {
		            _newBitmap.UnlockBits(bmNewData);
		        }
		        if (bmPrevData != null)
		        {
		            _prevBitmap.UnlockBits(bmPrevData);
		        }
		    }
		 
		    // Validate we found a bounding box. If not
		    //    return an empty rectangle.
		    //
		    int diffImgWidth = right - left + 1;
		    int diffImgHeight = bottom - top + 1;
		    if (diffImgHeight < 0 || diffImgWidth < 0)
		    {
		        // Nothing changed
		        return Rectangle.Empty;
		    }
		 
		    // Return the bounding box.
		    //
		    return new Rectangle(left, top, diffImgWidth, diffImgHeight);
		}		
	    static ImageCodecInfo GetEncoderInfo(String mimeType)
	    {
	        int j;
	        ImageCodecInfo[] encoders;
	        encoders = ImageCodecInfo.GetImageEncoders();
	        for(j = 0; j < encoders.Length; ++j)
	        {
	            if(encoders[j].MimeType == mimeType)
	                return encoders[j];
	        }
	        return null;
	    }
		
	}
}
