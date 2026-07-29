using Xunit.Abstractions;
using Xunit.Sdk;

namespace EventTests.Tests;

public sealed class PriorityOrder : ITestCaseOrderer
{
	public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
		where TTestCase : ITestCase
	{
		return testCases
			.Select(tc =>
			{
				var attr = tc.TestMethod.Method
					.GetCustomAttributes(typeof(TestPriorityAttribute).AssemblyQualifiedName)
					.FirstOrDefault();

				int priority = attr?.GetNamedArgument<int>("Priority") ?? 0;

				return new {TestCase = tc, Priority = priority};
			})
			.OrderBy(x => x.Priority)
			.ThenBy(x => x.TestCase.TestMethod.Method.Name)
			.Select(x => x.TestCase);
	}
}