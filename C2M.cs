using System;
using System.Threading;
using C2M.utils;
using SharpDX.XInput;
using WindowsInput;

namespace C2M
{
	class C2M
	{
		private readonly Controller controller;
		private readonly IMouseSimulator mouse;
		private readonly ControllerHandler controllerHandler;

		private Timer frameTimer;
		private const int RefreshRate = 60;

		public C2M()
		{
			controller = new Controller(UserIndex.One);
			mouse = new InputSimulator().Mouse;
			controllerHandler = new ControllerHandler(this, controller, mouse);
		}

		private void Update()
		{
			if (!controller.IsConnected)
			{
				Console.Title = "C2M (Not Connected.)";
				return;
			}
			Console.Title = "C2M (Connected.)";

			controller.GetState(out var state);
			controllerHandler.Handle(state);
		}

		public void Start()
		{
			frameTimer = new Timer(e => Update());
			frameTimer.Change(0, 1000 / RefreshRate);

			Console.WriteLine("Started C2M!");
			Console.WriteLine();
			Console.WriteLine("Keep this window open.");
			Console.WriteLine("Running...");
			Console.Read();
			Stop();
		}

		public void Stop()
		{
			frameTimer.Change(Timeout.Infinite, Timeout.Infinite);
			frameTimer.Dispose();
		}
	}
}
