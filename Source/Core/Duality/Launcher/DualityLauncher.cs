using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using Duality.Backend;
using Duality.Backend.DefaultOpenTK;
using Duality.Resources;
using OpenTK;
using OpenTK.Graphics;

namespace Duality.Launcher
{
	/// <summary>
	/// A class that allows you to easily initialize duality, run it and clean it up afterwards.
	/// As static state is used under the hood please make sure to only have 1 instance at a time of this class.
	/// </summary>
	public class DualityLauncher : IDisposable
	{
		private readonly List<ILogOutput> logOutputs = new List<ILogOutput>();
		private readonly Stack<IDisposable> disposables = new Stack<IDisposable>();
		private readonly GameWindow window;

		public int Width
		{
			get { return this.window.ClientSize.Width; }
		}
		public int Height
		{
			get { return this.window.ClientSize.Height; }
		}
		public Point2 Size
		{
			get { return new Point2(this.Width, this.Height); }
		}

		/// <summary>
		/// Initializes duality but does not yet run it.
		/// </summary>
		/// <param name="launcherArgs"></param>
		public DualityLauncher(LauncherArgs launcherArgs = null)
		{
			if (launcherArgs == null)
			{
				launcherArgs = new LauncherArgs();
			}

			System.Threading.Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
			System.Threading.Thread.CurrentThread.CurrentUICulture = System.Globalization.CultureInfo.InvariantCulture;

			// Set up console logging
			this.AddLogOutput(new ConsoleLogOutput());

			// Set up file logging
			try
			{
				StreamWriter logfileWriter = new StreamWriter("logfile.txt");
				logfileWriter.AutoFlush = true;
				this.disposables.Push(logfileWriter);

				this.AddLogOutput(new TextWriterLogOutput(logfileWriter));
			}
			catch (Exception e)
			{
				Logs.Core.WriteWarning("Text Logfile unavailable: {0}", LogFormat.Exception(e));
			}

			// Set up a global exception handler to log errors
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

			// Write initial log message before actually booting Duality
			Logs.Core.Write("Running DualityLauncher with flags: {1}{0}",
				Environment.NewLine,
				launcherArgs);

			// Initialize the Duality core
			DualityApp.Init(
				DualityApp.ExecutionEnvironment.Launcher,
				DualityApp.ExecutionContext.Game,
				new DefaultAssemblyLoader(),
				launcherArgs);

			// Open up a new window
			this.refreshMode = launcherArgs.IsProfiling ? RefreshMode.NoSync : DualityApp.UserData.Instance.WindowRefreshMode;

			this.window = DualityApp.OpenWindow(DualityApp.UserData.Instance.WindowSize.X, DualityApp.UserData.Instance.WindowSize.Y);
			window.UpdateFrame += Test_UpdateFrame;
			window.RenderFrame += Test_RenderFrame;
			window.Resize += Window_Resize;

			// Register events and input
			this.HookIntoDuality();
		}

		internal void HookIntoDuality()
		{
			DualityApp.Mouse.Source = new GameWindowMouseInputSource(this.window);
			DualityApp.Keyboard.Source = new GameWindowKeyboardInputSource(this.window);
			DualityApp.UserData.Applying += this.OnUserDataApplying;
		}
		internal void UnhookFromDuality()
		{
			if (DualityApp.Mouse.Source is GameWindowMouseInputSource)
				DualityApp.Mouse.Source = null;
			if (DualityApp.Keyboard.Source is GameWindowKeyboardInputSource)
				DualityApp.Keyboard.Source = null;
			DualityApp.UserData.Applying -= this.OnUserDataApplying;
		}

		private void OnUserDataApplying(object sender, EventArgs e)
		{
			// Early-out, if no display is connected / available anyway
			if (DisplayDevice.Default == null) return;

			// Determine the target state for our window
			MouseCursor targetCursor = DualityApp.UserData.Instance.SystemCursorVisible ? MouseCursor.Default : MouseCursor.Empty;
			WindowState targetWindowState = this.window.WindowState;
			WindowBorder targetWindowBorder = this.window.WindowBorder;
			Size targetSize = this.window.ClientSize;
			switch (DualityApp.UserData.Instance.WindowMode)
			{
				case ScreenMode.Window:
					targetWindowState = WindowState.Normal;
					targetWindowBorder = WindowBorder.Resizable;
					targetSize = new Size(DualityApp.UserData.Instance.WindowSize.X, DualityApp.UserData.Instance.WindowSize.Y);
					break;

				case ScreenMode.FixedWindow:
					targetWindowState = WindowState.Normal;
					targetWindowBorder = WindowBorder.Fixed;
					targetSize = new Size(DualityApp.UserData.Instance.WindowSize.X, DualityApp.UserData.Instance.WindowSize.Y);
					break;

				case ScreenMode.FullWindow:
				case ScreenMode.Fullscreen:
					targetWindowState = WindowState.Fullscreen;
					targetWindowBorder = WindowBorder.Hidden;
					targetSize = new Size(DisplayDevice.Default.Width, DisplayDevice.Default.Height);
					break;

				default:
					throw new ArgumentOutOfRangeException();
			}

			// Apply the target state to the game window wherever values changed
			if (this.window.WindowState != targetWindowState)
				this.window.WindowState = targetWindowState;
			if (this.window.WindowBorder != targetWindowBorder)
				this.window.WindowBorder = targetWindowBorder;
			if (this.window.ClientSize != targetSize)
				this.window.ClientSize = targetSize;
			if (this.window.Cursor != targetCursor)
				this.window.Cursor = targetCursor;

			DualityApp.WindowSize = new Point2(this.window.ClientSize.Width, this.window.ClientSize.Height);
		}

		private void Window_Resize(object sender, EventArgs e)
		{
			DualityApp.Resize(Size.X, Size.Y);
			//DualityApp.GraphicsBackend.Resize(Size.X, Size.Y);

			//if(DualityApp.ShadowRenderer != null) DualityApp.ShadowRenderer.Resize(Size.X, Size.Y);
			//if(DualityApp.DeferredRenderer != null) DualityApp.DeferredRenderer.Resize(Size.X, Size.Y);
			//if (DualityApp.ShadowBufferRenderer != null) DualityApp.ShadowBufferRenderer.Resize(Size.X, Size.Y);
			//if (DualityApp.PostEffectManager != null) DualityApp.PostEffectManager.Resize(Size.X, Size.Y);
			//if(DualityApp.SpriteRenderer != null) DualityApp.SpriteRenderer.Resize(Size.X, Size.Y);
			//DrawDevice.RenderVoid(new Rect(this.Size));
		}

		private RefreshMode refreshMode;
		private Stopwatch frameLimiterWatch = new Stopwatch();
		private void Test_UpdateFrame(object sender, FrameEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Terminated)
			{
				this.window.Close();
				return;
			}

			// Give the processor a rest if we have the time, don't use 100% CPU even without VSync
			if (this.frameLimiterWatch.IsRunning && this.refreshMode == RefreshMode.ManualSync)
			{
				while (this.frameLimiterWatch.Elapsed.TotalMilliseconds < Time.MillisecondsPerFrame)
				{
					// Enough leftover time? Risk a short sleep, don't burn CPU waiting.
					if (this.frameLimiterWatch.Elapsed.TotalMilliseconds < Time.MillisecondsPerFrame * 0.75)
						System.Threading.Thread.Sleep(0);
				}
			}
			this.frameLimiterWatch.Restart();
			DualityApp.Update();
		}

		private void Test_RenderFrame(object sender, FrameEventArgs e)
		{
			if (DualityApp.ExecContext == DualityApp.ExecutionContext.Terminated) return;

			Vector2 imageSize;
			Rect viewportRect;
			DualityApp.CalculateGameViewport(this.Size, out viewportRect, out imageSize);

			DualityApp.Render();
			//Profile.TimeRender.BeginMeasure();
			//Profile.TimeSwapBuffers.BeginMeasure();
			this.window.SwapBuffers();
			//Profile.TimeSwapBuffers.EndMeasure();
			//Profile.TimeRender.EndMeasure();
		}

		public void Dispose()
		{
			this.UnhookFromDuality();

			this.window.Dispose();

			// Shut down the Duality core
			DualityApp.Terminate();

			AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;

			foreach (ILogOutput logOutput in this.logOutputs)
			{
				Logs.RemoveGlobalOutput(logOutput);
			}
			this.logOutputs.Clear();

			foreach (IDisposable disposable in this.disposables)
			{
				disposable.Dispose();
			}
			this.disposables.Clear();
		}

		/// <summary>
		/// Runs duality. This will block till the game ends.
		/// Don't call this if you want full control of the update loop (such as in unit tests).
		/// </summary>
		public void Run()
		{
			// Load the starting Scene
			Scene.SwitchTo(DualityApp.AppData.Instance.StartScene);

			// Enter the game loop
			this.window.Run();
		}

		/// <summary>
		/// Adds a global log output and also makes sure its removed when <see cref="Dispose"/> is called
		/// </summary>
		/// <param name="logOutput"></param>
		private void AddLogOutput(ILogOutput logOutput)
		{
			this.logOutputs.Add(logOutput);
			Logs.AddGlobalOutput(logOutput);
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			try
			{
				Logs.Core.WriteError(LogFormat.Exception(e.ExceptionObject as Exception));
			}
			catch (Exception) { /* Ensure we're not causing any further exception by logging... */ }
		}
	}
}