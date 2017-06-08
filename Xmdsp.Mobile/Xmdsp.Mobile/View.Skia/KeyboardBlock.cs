
namespace Xmdsp
{
	public class KeyboardBlock
	{
		Presenter pm;
		
		public KeyboardBlock (Presenter pm, int channel)
		{
			this.pm = pm;
			Keyboard = new Keyboard (pm, channel);
			KeyParameters = new KeyboardParameterBlock (pm, channel);
		}

		public Keyboard Keyboard { get; private set; }
		public KeyboardParameterBlock KeyParameters { get; private set; }
	}
}

