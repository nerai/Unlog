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

		private System.Drawing.Color? _Fore = null;
		private System.Drawing.Color? _Back = null;

		public WpfRtfLogTarget (RichTextBox rtf)
		{
			_RTF = rtf;
		}

		public void Write (string s)
		{
			_RTF.Dispatcher.Invoke ((Action) (() => {
				var range = new TextRange (_RTF.Document.ContentEnd, _RTF.Document.ContentEnd);
				range.Text = s;

				if (_Fore != null) {
					var dcol = _Fore.Value;
					var mcol = Color.FromArgb (dcol.A, dcol.R, dcol.G, dcol.B);
					var brush = new SolidColorBrush (mcol);
					range.ApplyPropertyValue (TextElement.ForegroundProperty, brush);
				}
				if (_Back != null) {
					var dcol = _Back.Value;
					var mcol = Color.FromArgb (dcol.A, dcol.R, dcol.G, dcol.B);
					var brush = new SolidColorBrush (mcol);
					range.ApplyPropertyValue (TextElement.BackgroundProperty, brush);
				}

				_RTF.ScrollToEnd ();
			}));
		}

		public void SetForegroundColor (ConsoleColor c)
		{
			_Fore = c.ToRGB ();
		}

		public void SetBackgroundColor (ConsoleColor c)
		{
			_Back = c.ToRGB ();
		}

		public void ResetColors ()
		{
			_Fore = null;
			_Back = null;
		}

		public void Flush ()
		{
			// empty
		}
	}
}
