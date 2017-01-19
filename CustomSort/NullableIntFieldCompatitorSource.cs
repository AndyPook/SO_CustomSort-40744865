using Lucene.Net.Search;

namespace CustomSort
{
	public class NullableIntFieldCompatitorSource : FieldComparatorSource
	{
		public override FieldComparator NewComparator(string fieldname, int numHits, int sortPos, bool reversed)
		{
			return new NullableIntComparator(numHits, fieldname, FieldCache_Fields.NUMERIC_UTILS_INT_PARSER, reversed);
		}
	}
}
