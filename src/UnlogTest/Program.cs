using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Unlog;

namespace UnlogTest
{
	class Program
	{
		private abstract class Tester
		{
			public string Name
			{
				get {
					return GetType ().Name.Replace ("Test", "");
				}
			}

			internal abstract ILogTarget Create ();

			internal virtual void Cleanup ()
			{
			}

			protected readonly List<double> Times = new List<double> ();
			protected readonly List<double> BacklogTimes = new List<double> ();

			public double MedianTime (bool includeBacklog)
			{
				var ts = includeBacklog
					? BacklogTimes
					: Times;
				return ts
					.OrderBy (x => x)
					.Skip (Times.Count / 2)
					.First ();
			}

			public virtual void Run (string[] data)
			{
				Log.ClearTargets ();
				Log.AddTarget (Create ());

				var t0 = DateTime.UtcNow;
				int n = data.Length;
				for (int i = 0; i < n; i++) {
					Log.WriteLine (data[i]);
				}
				var t1 = DateTime.UtcNow;

				while (Log.MeasureWriteBacklog () != 0) {
					Thread.Sleep (1);
				}
				var t2 = DateTime.UtcNow;

				var dt1 = t1.Subtract (t0).TotalSeconds;
				var dt2 = t2.Subtract (t0).TotalSeconds;
				Times.Add (dt1);
				BacklogTimes.Add (dt2);

				Cleanup ();
			}
		}

		private class TestLogNull : Tester
		{
			internal override ILogTarget Create ()
			{
				return new NullLogTarget ();
			}
		}

		private class TestLogConsole : Tester
		{
			internal override ILogTarget Create ()
			{
				return new ConsoleLogTarget ();
			}
		}

		private class TestLogFile : Tester
		{
			private string path;
			private FileLogTarget target;

			internal override ILogTarget Create ()
			{
				path = "unlog test file " + DateTime.UtcNow.Ticks + ".log";
				target = new FileLogTarget (path);
				return target;
			}

			internal override void Cleanup ()
			{
				target.Dispose ();

				int i = 0;
				while (File.Exists (path) && i++ < 10) {
					try {
						File.Delete (path);
					}
					catch (IOException) {
						// ignore, repeat
						Thread.Sleep (10);
					}
				}
			}
		}

		private class TestDirectConsole : Tester
		{
			internal override ILogTarget Create ()
			{
				throw new NotSupportedException ();
			}

			public override void Run (string[] data)
			{
				var t0 = DateTime.UtcNow;
				int n = data.Length;
				for (int i = 0; i < n; i++) {
					Console.WriteLine (data[i]);
				}
				var t1 = DateTime.UtcNow;
				var dt1 = t1.Subtract (t0).TotalSeconds;
				Times.Add (dt1);
				BacklogTimes.Add (dt1);
			}
		}

		static void Main (string[] args)
		{
			Log.WriteLine ("Generating test data");
			const int DataCount = 1000;
			var data = Enumerable
				.Range (0, DataCount)
				.Select (i => GenerateNoise (i * i % 361 + i % 169))
				.ToArray ();
			var results = new Tester[] {
				new TestDirectConsole (),
				new TestLogNull (),
				new TestLogConsole (),
				new TestLogFile (),
			};

			Log.WriteLine ("Begin measurement");
			for (int testrun = 0; testrun < 3; testrun++) {
				foreach (var tester in results) {
					tester.Run (data);
				}
			}

			Console.Clear ();
			Log.ClearTargets ();
			Log.AddTarget (new ConsoleLogTarget ());

			Log.WriteLine ("Measurements completed");
			var totalBytes = data.Sum (s => s.Length);
			var bytesPerCall = 1.0 * totalBytes / data.Length;
			Log.WriteLine (bytesPerCall.ToString ("0.0") + " bytes per write:");

			foreach (var tester in results) {
				var t1 = tester.MedianTime (false);
				var t2 = tester.MedianTime (true);
				var dt1 = t1 / DataCount * 1000 * 1000;
				var dt2 = t2 / DataCount * 1000 * 1000;
				var kcallsPerSecond1 = DataCount / t1 / 1000;
				var kcallsPerSecond2 = DataCount / t2 / 1000;
				Console.WriteLine (""
					+ tester.Name.PadLeft (13) + ": "
					+ dt1.ToString ("0.0") + " / " + dt2.ToString ("0.0") + " µs, "
					+ kcallsPerSecond1.ToString ("0.0") + " / " + kcallsPerSecond2.ToString ("0.0") + " kHz");
			}

			Console.ReadKey (true);
		}

		private static Random _R = new Random ();

		private static string GenerateNoise (int length)
		{
			var sb = new StringBuilder (length);
			for (int i = 0; i < length; i++) {
				sb.Append ((char) (_R.Next ('A', 'Z')));
			}
			return sb.ToString ();
		}
	}
}
