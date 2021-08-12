namespace Api.GUI
{
	public interface IGUIBuilderFactory
	{
		IGUIBuilder New(params dynamic[] args);
	}
}