using System;
using System.Threading.Tasks;
using NUnit.Framework;
using SQLPaste.Processor;

namespace UnitTests
{
	public class StringProcessorTests
	{
		private StringProcessor sp = null;
		private string nl = Environment.NewLine;

		// TODO:
		// Option to control Date Format?
		// What should be done with empty?

		[SetUp]
		public void Setup()
		{
			sp = new StringProcessor();
		}

#if false
		[Test]
		public void RegexTest()
		{
			var key = "EnableIntellisense";
			var intellisenseEnabled = RegistryHelper.GetStringBooleanValue( key );

			//RegistryHelper.SetStringValueFromBool( key, false );

			//var updated = RegistryHelper.GetStringBooleanValue( key );
			Assert.AreEqual( SSMSIntellisense.Enabled, intellisenseEnabled );
		}
#endif

		[Test]
		public void IntegerDecimalAlphaAlphanumericConsistent()
		{
			var test = "123\t123.4\tabc\ta1b2c3" + '\n' +
				"123\t123.4\tabc\ta2b2c3" + '\r' +
				"123\t123.4\tabc\ta3b2c3" + '\r' + '\n' +
				"123\t123.4\tabc\ta4b2c3";

			var (lineTypes, lines, maxColumn) = sp.Process( test );

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "IDAN", lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 4 );
			Assert.AreEqual( lines, 4 );
			Assert.AreEqual( maxColumn, 6 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText( test, Direction.Horizontal );
			Assert.IsNotNull( output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;

			var expected =
				$"123,123.4,'abc','a1b2c3'{nl}123,123.4,'abc','a2b2c3'{nl}" +
				$"123,123.4,'abc','a3b2c3'{nl}123,123.4,'abc','a4b2c3'{nl}";
			Assert.AreEqual( expected, output );

			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}


		[Test]
		public void IntegerSingleColumnConsistent()
		{
			var test = "1" + '\r' + '\n' +
				"2" +'\r' + '\n' +
				"3" + '\r' + '\n' +
				"4";

			var (lineTypes, lines, maxColumn) = sp.Process( test );

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "I", lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 4 );
			Assert.AreEqual( lines, 4 );
			Assert.AreEqual( maxColumn, 1 );

			var formatSettings = FormatHorizontalCsv();

			var output = sp.GetFormattedText( test, Direction.Horizontal );
			Assert.IsNotNull( output );
			Assert.AreEqual( "1,2,3,4", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void IntegerSingleColumnQuotedConsistent()
		{
			var test = "1" + '\r' + '\n' +
				"2" +'\r' + '\n' +
				"3" + '\r' + '\n' +
				"4";

			var (lineTypes, lines, maxColumn) = sp.Process
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv,
				quoteIntegerColumns: true
			);

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "I", lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 4 );
			Assert.AreEqual( lines, 4 );
			Assert.AreEqual( maxColumn, 1 );

			var formatSettings = FormatHorizontalCsv(quoteIntegers:true);

			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv,
				quoteIntegers:true
			);
			Assert.IsNotNull( output );
			Assert.AreEqual( "'1','2','3','4'", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}


		// single column becomes row w/ , delimeters (! Matrix)
		[Test]
		public void AlphaNumericMinusSignSingleColumnConsistent()
		{
			var test = "1" + '\r' + '\n' +
				"2-1" +'\r' + '\n' +
				"3" + '\r' + '\n' +
				"4";

			var (lineTypes, lines, maxColumn) = sp.Process( test );

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "N", lineTypes[0].GetTemplate() );

			Assert.AreEqual( lineTypes[0].GetLines(), 4 );
			Assert.AreEqual( lines, 4 );
			Assert.AreEqual( maxColumn, 3 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText( test, Direction.Horizontal );
			Assert.IsNotNull( output );
			Assert.AreEqual( "'1','2-1','3','4'", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void NegativeIntegerMinusSignSingleColumnConsistent()
		{
			var test = "1" + '\r' + '\n' +
				"-21" +'\r' + '\n' +
				"3" + '\r' + '\n' +
				"4";

			var (lineTypes, lines, maxColumn) = sp.Process( test );

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( lineTypes[0].GetTemplate(), "I" );

			Assert.AreEqual( lineTypes[0].GetLines(), 4 );
			Assert.AreEqual( lines, 4 );
			Assert.AreEqual( maxColumn, 3 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText( test, Direction.Horizontal );
			Assert.IsNotNull( output );
			Assert.AreEqual( "1,-21,3,4", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}


		[Test]
		public void IntegerSingleRowConsistent()
		{
			var test = "1\t2\t3\t4\t5\t6\t7\t8\t9";

			var (lineTypes, lines, maxColumn) = sp.Process( test );

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( new string( 'I', 9 ), lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 1 );
			Assert.AreEqual( lines, 1 );
			Assert.AreEqual( maxColumn, 1 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText( test, Direction.Horizontal );
			Assert.IsNotNull( output );
			Assert.AreEqual( "1,2,3,4,5,6,7,8,9", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var totalChars = test.Length + extraChars;
			Assert.AreEqual( output.Length, totalChars );
		}

		[Test]
		public void IntegerSingleRowsConsistent()
		{
			var test = "1\t2\t3\t4\t5\t6\t7\t8\t9" + '\r' + '\n' +
				"2\t3\t4\t5\t6\t7\t8\t9\t10" ;

			var (lineTypes, lines, maxColumn) = sp.Process( test );

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( new string( 'I', 9 ), lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 2 );
			Assert.AreEqual( lines, 2 );
			// TODO: BUG Assert.AreEqual( maxColumn, 2 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );
			Assert.AreEqual( $"1,2,3,4,5,6,7,8,9{nl}2,3,4,5,6,7,8,9,10{nl}", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void EmptyFirstColumnConsistent()
		{
			// Nullable column is not quoted
			var test =
				"\tabc\t1234\tNULL" +'\n' +
				"\tdef\t2345\t-12.345";
			var (lineTypes, lines, _) = sp.Process( test );

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "EAId", lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 2 );
			Assert.AreEqual( lines, 2 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );
			Assert.AreEqual( $",'abc',1234,NULL{nl},'def',2345,-12.345{nl}", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void EmptyLastColumnConsistent()
		{
			var test =
				"abc\t1234\tNULL\t" +'\n' +
				"def\t2345\t12.345\t";
			var (lineTypes, lines, _) = sp.Process( test );

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "AIdE", lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 2 );
			Assert.AreEqual( lines, 2 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );
			Assert.AreEqual( $"'abc',1234,NULL,{nl}'def',2345,12.345,{nl}", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void MultipleEmptyLastColumnConsistent()
		{
			var test =
				"abc\t1234\t\t" +'\n' +
				"def\t2345\t\t";
			var (lineTypes, lines, _) = sp.Process( test );

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "AIEE", lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 2 );
			Assert.AreEqual( lines, 2 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );
			Assert.AreEqual( $"'abc',1234,,{nl}'def',2345,,{nl}", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void IntegerDateGuidConsistent()
		{
			var test =
				"CD7FA59F-50C6-42F6-8C68-5B78C68069FC\t2022-02-12 11:51:35.660\t1234\t1234\t12.340" +'\n' +
				"3DCFF513-BE7C-4403-8105-4E28C30C043A\t2022-02-11 11:51:35.660\t12345\t12345\t12.345";

			var (lineTypes, lines, maxColumn) = sp.Process( test );

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "GTIID", lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 2 );
			Assert.AreEqual( lines, 2 );
			Assert.AreEqual( maxColumn, 36 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );

			var expected =
				$"'CD7FA59F-50C6-42F6-8C68-5B78C68069FC','2022-02-12 11:51:35.660',1234,1234,12.340{nl}" +
				$"'3DCFF513-BE7C-4403-8105-4E28C30C043A','2022-02-11 11:51:35.660',12345,12345,12.345{nl}";
			Assert.AreEqual( expected, output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void NullOneOfEachTypeConsistent()
		{
			// Test has NULL's which would not be quoted in columns
			// where the non null data would be - Extra Chars for
			// buffer size cannot be dead-reckoned
			var test =
				"NULL\tb\tNULL" +'\r' +
				"2F70A769-CF1A-4343-A21B-119C4BEB304C\ta\t12" +'\r' +
				"B5461C71-6E1B-4B9E-995D-AB3613B44F97\tNULL\t123";

			var (lineTypes, lines, maxColumn) = sp.Process( test );
			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "gai", lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 3 );
			Assert.AreEqual( lines, 3 );
			Assert.AreEqual( maxColumn, 36 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );

			var expected =
				$"NULL,'b',NULL{nl}" +
				$"'2F70A769-CF1A-4343-A21B-119C4BEB304C','a',12{nl}" +
				$"'B5461C71-6E1B-4B9E-995D-AB3613B44F97',NULL,123{nl}";
			Assert.AreEqual( expected, output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void NullColumnConsistent()
		{
			var test =
				"NULL\tb\tNULL" +'\r' +
				"2F70A769-CF1A-4343-A21B-119C4BEB304C\ta\tNULL" +'\r' +
				"B5461C71-6E1B-4B9E-995D-AB3613B44F97\tNULL\tNULL";

			var (lineTypes, lines, maxColumn) = sp.Process( test );
			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( lineTypes[0].GetTemplate(), "gaX" );
			Assert.AreEqual( lineTypes[0].GetLines(), 3 );
			Assert.AreEqual( lines, 3 );
			Assert.AreEqual( maxColumn, 36 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );

			var expected =
				$"NULL,'b',NULL{nl}" +
				$"'2F70A769-CF1A-4343-A21B-119C4BEB304C','a',NULL{nl}" +
				$"'B5461C71-6E1B-4B9E-995D-AB3613B44F97',NULL,NULL{nl}";
			Assert.AreEqual( expected, output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void NullAndEmptyColumnConsistent()
		{
			var test =
				"NULL\t\t\t\t" + '\r' +
				"\tNULL\t\t\t" + '\r' + '\n' +
				"\t\tNULL\t\t" + '\n' +
				"\t\t\t\t";

			var (lineTypes, lines, maxColumn) = sp.Process( test );
			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( lineTypes[ 0 ].GetTemplate(), "eeeEE" );
			Assert.AreEqual( lineTypes[ 0 ].GetLines(), 4 );
			Assert.AreEqual( lines, 4 );
			Assert.AreEqual( maxColumn, 4 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );

			var expected =
				$"NULL,,,,{nl}" +
				$",NULL,,,{nl}" +
				$",,NULL,,{nl}" +
				$",,,,{nl}";
			Assert.AreEqual( expected, output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		// TODO: "Empty" here is correct CSV, but how would that make
		// sense in SQL - likely Empty needs to become '' or NULL
		[Test]
		public void EmptyColumnConsistent()
		{
			var test =
				"\t\t\t\t" + '\r' +
				"\t\t\t\t" + '\r' + '\n' +
				"\t\t\t\t" + '\n' +
				"\t\t\t\t";

			var (lineTypes, lines, maxColumn) = sp.Process( test );
			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( lineTypes[ 0 ].GetTemplate(), "EEEEE" );
			Assert.AreEqual( lineTypes[ 0 ].GetLines(), 4 );
			Assert.AreEqual( lines, 4 );
			Assert.AreEqual( maxColumn, 0 );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );

			var expected =
				$",,,,{nl}" +
				$",,,,{nl}" +
				$",,,,{nl}" +
				$",,,,{nl}";
			Assert.AreEqual( expected, output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void EscapedString()
		{
			var test =
				"apos'trophe" + '\t' +
				"apostrophe'" + '\t' +
				"'apostrophe'";

			var (lineTypes, lines, maxColumn) = sp.Process( test );

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "AAA", lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 1 );
			Assert.AreEqual( lines, 1 );

			var formatSettings = FormatDirType(Direction.Horizontal, DelimiterType.Csv, isMatrix:false);
			var output = sp.GetFormattedText( test, Direction.Horizontal );
			Assert.IsNotNull( output );
			Assert.AreEqual( "'apos''trophe','apostrophe''','''apostrophe'''", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void AlphaNumericConversionAfterOtherColumnConvertsAndEscapedChar()
		{
			var test = "131313" + '\t' + "Kensing, LLC" + '\t' + $"30 ROCKFELLER PLZ FL 54th{nl}" +
				"13131313" + '\t' + "marissa's company" + '\t' + $"Address{nl}" +
				"131314" + '\t' + "Great Waters Financial, LLC" + '\t' + $"510 1st N Ave{nl}" +
				"131315" + '\t' + "Itria Restarant Group LLC" + '\t' + $"645 Haight St{nl}" +
				"131316" + '\t' + "Curtis F Davis Enterprises" + '\t' + $"4225 Northgate Blvd{nl}" +
				"131317" + '\t' + "QUALIS TELECOM INC" + '\t' + $"4225 Northgate Blvd{nl}" +
				"131318" + '\t' + "FlexTech Alliance, Inc." + '\t' + $"2244 Blach Place{nl}" +
				"P200000" + '\t' + "Selective Insurance(WC)" + '\t' + "NULL";

			var (lineTypes, lines, maxColumn) = sp.Process(test);

			Assert.AreEqual(lineTypes.Count, 1);
			Assert.AreEqual("NNn", lineTypes[0].GetTemplate());
			Assert.AreEqual(lineTypes[0].GetLines(), 8);
			Assert.AreEqual(lines, 8);

			var formatSettings = FormatDirType(Direction.Horizontal, DelimiterType.Csv, isMatrix: false);
			var output = sp.GetFormattedText(test, Direction.Horizontal);
			Assert.IsNotNull(output);

			var expected = "'131313'" + ',' + "'Kensing, LLC'" + ',' + $"'30 ROCKFELLER PLZ FL 54th'{nl}" +
				"'13131313'" + ',' + "'marissa''s company'" + ',' + $"'Address'{nl}" +
				"'131314'" + ',' + "'Great Waters Financial, LLC'" + ',' + $"'510 1st N Ave'{nl}" +
				"'131315'" + ',' + "'Itria Restarant Group LLC'" + ',' + $"'645 Haight St'{nl}" +
				"'131316'" + ',' + "'Curtis F Davis Enterprises'" + ',' + $"'4225 Northgate Blvd'{nl}" +
				"'131317'" + ',' + "'QUALIS TELECOM INC'" + ',' + $"'4225 Northgate Blvd'{nl}" +
				"'131318'" + ',' + "'FlexTech Alliance, Inc.'" + ',' + $"'2244 Blach Place'{nl}" +
				"'P200000'" + ',' + "'Selective Insurance(WC)'" + ',' + $"NULL{nl}";

			Assert.AreEqual(expected, output);

			var extraChars = sp.GetExtraChars(formatSettings);
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual(output.Length, bufferSizeFromInputAndExtra);
		}

		[Test]
		public void ValuesHorizontal()
		{
#if false
-- example of values
SELECT * FROM 
(
VALUES 
	('a', 1),
	('b', 2)
)
as tab(k,v)
#endif
			var test =
				"a" + '\t' + "1" +'\r' + '\n' +
				"b" + '\t' + "2";

			var (lineTypes, lines, maxColumn) = sp.Process
			(
				test,
				Direction.Horizontal,
				DelimiterType.Values
			);

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "AI", lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 2 );
			Assert.AreEqual( lines, 2 );

			var formatSettings = FormatHorizontalValues();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Values
			);
			Assert.IsNotNull( output );
			Assert.AreEqual( $"('a',1),('b',2){nl}", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void ValuesVertical()
		{
			// Vertical must include , Newline on all but the last row
			var test =
				"a" + '\t' + "1" +'\r' + '\n' +
				"b" + '\t' + "2" + '\r' + '\n' +
				"c" + '\t' + "3";

			var (lineTypes, lines, maxColumn) = sp.Process
			(
				test,
				Direction.Vertical,
				DelimiterType.Values
			);

			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "AI", lineTypes[0].GetTemplate() );
			Assert.AreEqual( lineTypes[0].GetLines(), 3 );
			Assert.AreEqual( lines, 3 );

			var formatSettings = FormatDirType(Direction.Vertical, DelimiterType.Values);
			var output = sp.GetFormattedText
			(
				test,
				Direction.Vertical,
				DelimiterType.Values
			);
			Assert.IsNotNull( output );
			Assert.AreEqual( $"('a',1),{nl}('b',2),{nl}('c',3){nl}", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[Test]
		public void CoerceIntegerToAlphaNumeric()
		{
			var test =
				"1" + '\t' + "1" + '\r' + '\n' +
				"2" + '\t' + "2" + '\r' + '\n' +
				"a" + '\t' + "3" + '\r' + '\n' +
				"3" + '\t' + "4" + '\r' + '\n' +
				"b" + '\t' + "5";

			var (lineTypes, lines, maxColumn) = sp.Process
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.AreEqual(lineTypes.Count, 1);
			Assert.AreEqual("NI", lineTypes[0].GetTemplate());

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );
			Assert.AreEqual($"'1',1{nl}'2',2{nl}'a',3{nl}'3',4{nl}'b',5{nl}", output);

			var extraChars = sp.GetExtraChars(formatSettings);
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual(output.Length, bufferSizeFromInputAndExtra);
		}

		[Test]
		public void CoerceNullAndIntegerToAlphaNumeric()
		{
			var test =
				"NULL" + '\t' + "1" + '\r' + '\n' +
				"2" + '\t' + "2" + '\r' + '\n' +
				"3" + '\t' + "3" + '\r' + '\n' +
				"a" + '\t' + "4" + '\r' + '\n' +
				"b" + '\t' + "5" + '\r' + '\n' +
				"NULL" + '\t' + "6";

			var (lineTypes, lines, maxColumn) = sp.Process
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.AreEqual(lineTypes.Count, 1);
			var lineType = lineTypes[ 0 ];
			Assert.AreEqual("nI", lineType.GetTemplate());

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );
			Assert.AreEqual($"NULL,1{nl}'2',2{nl}'3',3{nl}'a',4{nl}'b',5{nl}NULL,6{nl}", output);

			var extraChars = sp.GetExtraChars(formatSettings);
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual(output.Length, bufferSizeFromInputAndExtra);

			// This proves that when the nullable int column encountered
			// an alpha and we included 4 quote chars in the coercion, we
			// did not include quotes on the NULL
			var internals = lineType.GetInternalsForUnitTests();
			Assert.AreEqual( 4, internals.extraCoercedChars );
		}

		[Test]
		public void IntegerWithNoCarryoverCoerceToAlphaNumericWithRightBufferSize()
		{
			var test = "1"+ '\t' + "alph" + '\t' + "alp" + '\r' + '\n'+
				"2"+ '\t' +   "alpha" + '\t' +   "123" + '\r' + '\n' +
				"3"+ '\t' +  "alpha" + '\t' + "alp123";

			var (lineTypes, lines, maxColumn) = sp.Process
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "IAN", lineTypes[0].GetTemplate() );

			var formatSettings = FormatHorizontalCsv();
			var output = sp.GetFormattedText
			(
				test,
				Direction.Horizontal,
				DelimiterType.Csv
			);
			Assert.IsNotNull( output );
			Assert.AreEqual( $"1,'alph','alp'{nl}2,'alpha','123'{nl}3,'alpha','alp123'{nl}", output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}

		[TestCase( Direction.Horizontal )]
		[TestCase( Direction.Vertical )]
		public void PasteIntValuesVertical( Direction direction )
		{
			var test = "1" + '\r' + '\n' +
				"2" + '\r' + '\n' +
				"3" ;
			var formatSettings = FormatDirType
			(
				direction,
				DelimiterType.Values,
				isMatrix: false,
				quoteIntegers: true
			);

			var (lineTypes, lines, maxColumn) = sp.Process
			(
				test,
				direction,
				DelimiterType.Values,
				quoteIntegerColumns: true
			);
			Assert.AreEqual( lineTypes.Count, 1 );
			Assert.AreEqual( "I", lineTypes[ 0 ].GetTemplate() );

			var output = sp.GetFormattedText
			(
				test,
				direction,
				DelimiterType.Values,
				quoteIntegers: true
			);
			Assert.IsNotNull( output );

			var extraChars = sp.GetExtraChars( formatSettings );
			var bufferSizeFromInputAndExtra = test.Length + extraChars;
			Assert.AreEqual( output.Length, bufferSizeFromInputAndExtra );
		}


		private static FormatSettings FormatHorizontalCsv(bool quoteIntegers = false) =>
			new FormatSettings( Direction.Horizontal, DelimiterType.Csv, isMatrix: true, quoteIntegers );

		private static FormatSettings FormatHorizontalValues(bool quoteIntegers = false) =>
			new FormatSettings( Direction.Horizontal, DelimiterType.Values, isMatrix: true, quoteIntegers );
		
		private static FormatSettings FormatDirType
		(
			Direction direction = Direction.Horizontal,
			DelimiterType delimiterType = DelimiterType.Csv,
			bool isMatrix = true,
			bool quoteIntegers = false
		) =>
			new FormatSettings( direction, delimiterType, isMatrix: isMatrix, quoteIntegers );

	}
}