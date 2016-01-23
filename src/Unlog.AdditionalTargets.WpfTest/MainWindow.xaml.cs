using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Unlog.AdditionalTargets.WpfTest
{
	public partial class MainWindow : Window
	{
		public MainWindow ()
		{
			InitializeComponent ();

			var t = new WpfRtfLogTarget (rtfBox);
			Log.Targets.Add (t);
			Log.AllowAsynchronousWriting = false;

			/*
			 * Test basic WriteLine
			 */
			Log.WriteLine ("First line");

			/*
			 * Test combination of Write and empty WriteLine
			 */
			Log.Write ("Second line");
			Log.WriteLine ();
			Log.WriteLine ("Third line");

			/*
			 * Test foreground colors
			 */
			Log.Write ("Foreground: Black,");
			Log.ForegroundColor = ConsoleColor.Red;
			Log.Write ("Red,");
			Log.ForegroundColor = ConsoleColor.Green;
			Log.Write ("Green,");
			Log.ResetColor ();
			Log.Write ("(Reset),");
			Log.ForegroundColor = ConsoleColor.Blue;
			Log.Write ("Blue,");
			Log.ResetColor ();
			Log.Write ("(Reset),");
			Log.WriteLine ();

			/*
			 * Test background colors
			 */
			Log.Write ("Background: White,");
			Log.BackgroundColor = ConsoleColor.Red;
			Log.Write ("Red,");
			Log.BackgroundColor = ConsoleColor.Green;
			Log.Write ("Green,");
			Log.ResetColor ();
			Log.Write ("(Reset),");
			Log.BackgroundColor = ConsoleColor.Blue;
			Log.Write ("Blue,");
			Log.ResetColor ();
			Log.Write ("(Reset),");
			Log.WriteLine ();

			/*
			 * Done!
			 */
			Log.WriteLine ("Test complete");
		}
	}
}
