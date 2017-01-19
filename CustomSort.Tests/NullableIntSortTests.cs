using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit;
using Lucene;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Analysis.Standard;
using Version = Lucene.Net.Util.Version;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace CustomSort.Tests
{
	public class NullableIntSortTests
	{
		public NullableIntSortTests()
		{
			CreateTestIndex();
		}

		private IndexWriter writer;
		private IndexSearcher searcher;

		private void CreateWriter()
		{
			if (writer != null)
				writer.Dispose();

			var dir = new RAMDirectory();
			writer = new IndexWriter(dir, new StandardAnalyzer(Version.LUCENE_30), true, IndexWriter.MaxFieldLength.LIMITED);
		}

		private void AddDoc(int id, int? value)
		{
			var doc = new Document();
			doc.Add(new NumericField("id", Field.Store.YES, true).SetIntValue(id));
			doc.Add(new NumericField("data", Field.Store.YES, true).SetIntValue(value));

			writer.AddDocument(doc);
		}

		private void AddTestData()
		{
			AddDoc(1, 100);
			AddDoc(2, 400);
			AddDoc(3, null);
			AddDoc(4, 300);

			writer.Commit();
		}

		private void CreateTestIndex()
		{
			CreateWriter();
			AddTestData();

			searcher = new IndexSearcher(writer.GetReader());
		}

		private class DataDoc
		{
			public int ID { get; set; }
			public int? Data { get; set; }
		}

		private IEnumerable<DataDoc> Search(Sort sort)
		{
			var result = searcher.Search(new MatchAllDocsQuery(), null, 99, sort);

			foreach (var topdoc in result.ScoreDocs)
			{
				var doc = searcher.Doc(topdoc.Doc);
				int id = int.Parse(doc.GetFieldable("id").StringValue);
				int data = int.Parse(doc.GetFieldable("data").StringValue);

				yield return new DataDoc
				{
					ID = id,
					Data = data == int.MinValue ? (int?)null : data
				};
			}
		}

		[Fact]
		public void SortAscending()
		{
			var sort = new Sort(new SortField("data", new NullableIntFieldCompatitorSource()));

			var result = Search(sort).ToList();

			Assert.Equal(4, result.Count);
			Assert.Equal(new int?[] { 100, 300, 400, null }, result.Select(x => x.Data));
		}


		[Fact]
		public void SortDecending()
		{
			var sort = new Sort(new SortField("data", new NullableIntFieldCompatitorSource(),true));

			var result = Search(sort).ToList();

			Assert.Equal(4, result.Count);
			Assert.Equal(new int?[] { 400, 300, 100, null }, result.Select(x => x.Data));
		}
	}
}
