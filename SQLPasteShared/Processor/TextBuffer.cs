using System;

namespace SQLPaste.Processor
{
	public class TextBuffer
	{
		private char[] _buffer;
		private int _offset;

		public TextBuffer( int size )
		{
			_buffer = new char[ size ];
			_offset = 0;
		}

		public void Add( char ch )
		{
			_buffer[ _offset++ ] = ch;
		}

		public void Add( string s )
		{
			Array.Copy( s.ToCharArray(), 0, _buffer, _offset, s.Length );
			_offset += s.Length;
		}

		public void Add( ReadOnlySpan<char> span )
		{
			var length = span.Length;
			span.CopyTo( _buffer.AsSpan<char>( _offset, length ) );
			_offset += length;
		}

		public void Add( string format, ReadOnlySpan<char> span )
		{
			throw new NotImplementedException();
		}

		public string Result()
		{
			var result = new string( _buffer );
			_buffer = null;
			_offset = 0;
			return result;
		}
	}
}
