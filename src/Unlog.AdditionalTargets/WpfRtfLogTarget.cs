using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Unlog;
using Unlog.Util;

namespace Unlog.AdditionalTargets
{
	public class WpfRtfLogTarget : ILogTarget
	{
		private readonly RichTextBox _RTF;

		private Color _Fore;
		private Color _Back;

		public WpfRtfLogTarget (RichTextBox rtf)
		{
			_RTF = rtf;
			ResetColors ();
		}

		public void Write (string s)
		{
			// Capture state locally
			var fore = _Fore;
			var back = _Back;

			_RTF.Dispatcher.BeginInvoke ((Action) (() => {
				var range = new TextRange (_RTF.Document.ContentEnd, _RTF.Document.ContentEnd);
				range.Text = s;
				range.ApplyPropertyValue (TextElement.ForegroundProperty, new SolidColorBrush (fore));
				range.ApplyPropertyValue (TextElement.BackgroundProperty, new SolidColorBrush (back));

				_RTF.ScrollToEnd ();
			}));
		}

		public void SetForegroundColor (ConsoleColor c)
		{
			var dcol = c.ToRGB ();
			_Fore = Color.FromArgb (dcol.A, dcol.R, dcol.G, dcol.B);
		}

		public void SetBackgroundColor (ConsoleColor c)
		{
			var dcol = c.ToRGB ();
			_Back = Color.FromArgb (dcol.A, dcol.R, dcol.G, dcol.B);
		}

		public void ResetColors ()
		{
			_Fore = Colors.Black;
			_Back = Colors.White;
		}

		public void Flush ()
		{
			// empty
		}
	}
}
