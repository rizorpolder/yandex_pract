using yandex_pract.MockDB;

namespace TestProject1.Fixture;

public class TestDBFixture
{
	public MockDB Db { get; }

	public TestDBFixture()
	{
		Db = new MockDB();
	}
}