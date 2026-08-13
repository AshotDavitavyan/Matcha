using Xunit;

namespace Tests;

[CollectionDefinition(Name, DisableParallelization = true)]
public class ApplicationConfigurationCollection
{
	public const string Name = "Application configuration";
}
