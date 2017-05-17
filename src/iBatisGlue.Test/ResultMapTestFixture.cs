using System;
using iBatisGlue.Parser;
using NUnit.Framework;
using Shouldly;

namespace iBatisGlue.Test
{
	public class ResultMapTestFixture
	{
		[Test]
		public void ParseResultMap_Test1()
		{
			var fileMapList = iBatisGlueUtils.GetMapFileList(
				iBatisGlueUtils.iBatisMapConfigFile,
				iBatisGlueUtils.iBatisMapFilesBasePath
			);
			var result = iBatisGlueParser.GetResult(fileMapList, "SomeChildClass-results");
			Console.WriteLine("---------------------------------------------------------------");
			Console.WriteLine(result);
			Console.WriteLine("---------------------------------------------------------------");
			result.ShouldContain( "SomeChildClass-results" );
			result.ShouldContain( "SomeClass-results" );
		}

		[Test]
		public void ParseResultMap_Test2()
		{
			var fileMapList = iBatisGlueUtils.GetMapFileList(
				iBatisGlueUtils.iBatisMapConfigFile,
				iBatisGlueUtils.iBatisMapFilesBasePath
			);
			var result = iBatisGlueParser.GetResult(fileMapList, "RequestInfoWC-results");
			Console.WriteLine("---------------------------------------------------------------");
			Console.WriteLine(result);
			Console.WriteLine("---------------------------------------------------------------");
			result.ShouldContain("Persistence1.Persistence2.RequestInfoWC-results");
		}
	}
}