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
			private string _Name = null;

			public string Name
			{
				get
				{
					if (_Name == null) {
						var t = Create ();
						_Name = t.GetType ().Name;
						Cleanup ();
					}
					return _Name;
				}
			}

			internal abstract ILogTarget Create ();

			internal virtual void Cleanup ()
			{
			}

			private readonly List<double> Times = new List<double> ();
			private readonly List<double> BacklogTimes = new List<double> ();

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

			public void Run (string[] data)
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
					Thread.Sleep (10);
				}
				var t2 = DateTime.UtcNow;

				var dt1 = t1.Subtract (t0).TotalMilliseconds;
				var dt2 = t2.Subtract (t0).TotalMilliseconds;
				Times.Add (dt1);
				BacklogTimes.Add (dt2);

				Cleanup ();
			}
		}

		private class TestNull : Tester
		{
			internal override ILogTarget Create ()
			{
				return new NullLogTarget ();
			}
		}

		private class TestConsole : Tester
		{
			internal override ILogTarget Create ()
			{
				return new ConsoleLogTarget ();
			}
		}

		private class TestFile : Tester
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

		static void Main (string[] args)
		{
			Log.WriteLine ("Generating test data");
			const int DataCount = 1000;
			var data = Enumerable
				.Range (0, DataCount)
				.Select (i => GenerateNoise (i * i % 361 + i % 169))
				.ToArray ();
			var results = new Tester[] {
				new TestNull (),
				new TestConsole (),
				new TestFile (),
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
			Log.WriteLine (data.Sum (s => s.Length) / data.Length + " bytes per write");

			foreach (var tester in results) {
				var dt1 = tester.MedianTime (false) / DataCount * 1000;
				var dt2 = tester.MedianTime (true) / DataCount * 1000;
				Console.WriteLine (""
					+ tester.Name + ": "
					+ dt1.ToString ("0.0") + " / " + dt2.ToString ("0.0") + " ns");
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
