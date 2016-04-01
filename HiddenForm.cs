/*
 * Created by SharpDevelop.
 * User: YLIN68
 * Date: 3/31/2016
 * Time: 12:02 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using launcher;
using sc;

namespace scservice
{
	/// <summary>
	/// Description of HiddenForm.
	/// </summary>
	public class HiddenForm : Form
    {
		Task task;
		Timer timer;
        public HiddenForm()
        {
            InitializeComponent();
        }

		protected override CreateParams CreateParams {
	        get 
	        {
	            var cp = base.CreateParams;
	            cp.ExStyle |= 0x80;  // Turn on WS_EX_TOOLWINDOW
	            return cp;
	        }
        }

       	void HiddenForm_FormClosing(object sender, FormClosingEventArgs e)
        {
       		timer.Dispose();
        	ScreenShot.Stop();
        }
		
       	void HiddenForm_FormLoad(object sender, EventArgs e)
       	{
       		Process p=GetOtherProcess();
       		if(p!=null)
       		{
       			Close();
       			return;
       		}
       		task=Task.Run(()=>ScreenShot.Start());
       		timer.Start();
       	}
       	
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(0, 0);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "SCService";
            this.Text = "SCService";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.ShowInTaskbar=false;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HiddenForm_FormClosing);
            this.Load+=new EventHandler(this.HiddenForm_FormLoad);
            this.ResumeLayout(false);
			timer = new Timer();
        	timer.Interval = 3000;
        	timer.Tick += new EventHandler(timer_Tick);
        }
        
        void timer_Tick(object sender, EventArgs e)
    	{
			if(File.Exists("stop"))
			{
				ScreenShot.Stop();
				Task.WaitAll(task);
				File.Delete("stop");
				Close();
			}
    	}
        
        public void CloseOtherProcess()
        {
        	CloseProcessWindow(GetOtherProcess());
        }
        
		Process GetOtherProcess()
		{
			Process proc=Process.GetCurrentProcess();
			foreach(Process p in Process.GetProcessesByName(proc.ProcessName))
			{
				if(p.Id!=proc.Id && p.SessionId==proc.SessionId && p.MainModule.FileName==proc.MainModule.FileName)
				{
					return p;
				}
			}
			return null;
		}
		
		void CloseProcessWindow(Process p)
		{
			if(p==null) return;
            if (p.MainWindowHandle == IntPtr.Zero)
            {
	            // Try closing application by sending WM_CLOSE to all child windows in all threads.
	            foreach (ProcessThread pt in p.Threads)
	            {
	                NativeMethods.EnumThreadWindows((uint) pt.Id, new NativeMethods.EnumWindowsProc(EnumThreadCallback), p.Id);
	            }
            }
            else
            {
	            // Try to close main window.
	            if(p.CloseMainWindow())
	            {
	                // Free resources used by this Process object.
	                p.Close();
	            }
            }
		}

		bool EnumThreadCallback(IntPtr hWnd, int lParam)
	    {
			string name=NativeMethods.GetWindowText(hWnd);
	        if(name==this.Text)
	        {
	        	NativeMethods.SendMessage(hWnd, NativeMethods.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
	        	return false;
	        }
	        return true;
	    }
    }
}
