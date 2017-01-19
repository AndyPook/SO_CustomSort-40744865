using System;
using Lucene.Net.Search;
using Lucene.Net.Index;

namespace CustomSort
{

	public class NullableIntComparator : FieldComparator
	{
		private int[] values;
		private int[] currentReaderValues;
		private string field;
		private IntParser parser;
		private int bottom; // Value of bottom of queue
		private bool reversed;

		public NullableIntComparator(int numHits, string field, Parser parser, bool reversed)
		{
			values = new int[numHits];
			this.field = field;
			this.parser = (IntParser)parser;
			this.reversed = reversed;
		}

		public override int Compare(int slot1, int slot2)
		{
			// TODO: there are sneaky non-branch ways to compute
			// -1/+1/0 sign
			// Cannot return values[slot1] - values[slot2] because that
			// may overflow
			int v1 = values[slot1];
			int v2 = values[slot2];

			if (v1 == int.MinValue)
				return reversed ? -1 : 1;
			if (v2 == int.MinValue)
				return reversed ? 1 : -1;

			if (v1 > v2)
			{
				return 1;
			}
			else if (v1 < v2)
			{
				return -1;
			}
			else
			{
				return 0;
			}
		}

		public override int CompareBottom(int doc)
		{
			if (bottom == int.MinValue)
				return reversed ? -1 : 1;

			// TODO: there are sneaky non-branch ways to compute
			// -1/+1/0 sign
			// Cannot return bottom - values[slot2] because that
			// may overflow
			int v2 = currentReaderValues[doc];

			if (v2 == int.MinValue)
				return reversed ? 1 : -1;

			if (bottom > v2)
			{
				return 1;
			}
			else if (bottom < v2)
			{
				return -1;
			}
			else
			{
				return 0;
			}
		}

		public override void Copy(int slot, int doc)
		{
			values[slot] = currentReaderValues[doc];
		}

		public override void SetNextReader(IndexReader reader, int docBase)
		{
			currentReaderValues = FieldCache_Fields.DEFAULT.GetInts(reader, field, parser);
		}

		public override void SetBottom(int bottom)
		{
			this.bottom = values[bottom];
		}

		public override IComparable this[int slot] => values[slot];
	}
}