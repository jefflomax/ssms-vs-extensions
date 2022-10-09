
namespace SQLPaste.Processor
{
	public class FormatBase
	{
		public FormatBase
		(
			Direction direction,
			DelimiterType delimiterType,
			bool quoteIntegers
		)
		{
			Horizontal = direction == Direction.Horizontal;
			Values = delimiterType == DelimiterType.Values;
			CSV = delimiterType == DelimiterType.Csv;
			QuoteIntegers = quoteIntegers;
		}

		public FormatBase
		(
			FormatBase formatBase
		)
		{
			Horizontal = formatBase.Horizontal;
			Values = formatBase.Values;
			CSV = formatBase.CSV;
			QuoteIntegers = formatBase.QuoteIntegers;
		}

		public bool Horizontal { get; }
		public bool QuoteIntegers { get; }
		public bool CSV { get; }
		public bool Values { get; } // TODO: better
	}
}
