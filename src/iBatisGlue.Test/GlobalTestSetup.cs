using System;
using NUnit.Framework;

namespace iBatisGlue.Test
{
	[SetUpFixture]
	public class GlobalTestSetup
	{
		[OneTimeSetUp]
		public void TestFixtureSetUp()
		{
			Environment.CurrentDirectory = TestContext.CurrentContext.TestDirectory;
		}
	}
}