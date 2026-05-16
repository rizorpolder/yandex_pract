
namespace EventTests.Tests;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class TestPriorityAttribute : Attribute
{
	public int Priority { get; }

	public TestPriorityAttribute(int priority)
	{
		Priority = priority;
	}
}