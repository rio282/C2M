using System;
using System.Threading;
using SharpDX.XInput;
using WindowsInput;

namespace C2M
{
	class C2M
	{
		private readonly Controller controller;
		private readonly IMouseSimulator mouse;
		private readonly ButtonHandler buttonHandler;

		private Timer frameTimer;
		private const int RefreshRate = 60;

		public C2M() 
		{
			controller = new Controller(UserIndex.One);
			mouse = new InputSimulator().Mouse;
			buttonHandler = new ButtonHandler(this, controller, mouse);
		}

		private void Update()
		{
			if (!controller.IsConnected)
				return;

			controller.GetState(out var state);
			buttonHandler.Handle(state);
		}

		public void Start() 
		{
			frameTimer = new Timer(e => Update());
			frameTimer.Change(0, 1000 / RefreshRate);

			Console.WriteLine("Started C2M!");
			Console.WriteLine("Keep this window open.");
			Console.WriteLine();
			Console.Write("Running...");
			Console.Read();
		}

		public void Stop()
		{
			frameTimer.Change(Timeout.Infinite, Timeout.Infinite);
			frameTimer.Dispose();
		}
	}
}
