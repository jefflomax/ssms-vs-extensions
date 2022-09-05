using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SharedSrc.Helpers
{
	public static class ClipboardHelper
	{
		public static bool GetText(out string clipboardText)
		{
			if (Clipboard.ContainsText())
			{
				clipboardText = Clipboard.GetText();
				return true;
			}
			clipboardText = string.Empty;
			return false;
		}

		public static void SetData(string clipboardText)
		{
			Clipboard.SetText(clipboardText);
		}

		public static string GetDataFormats()
		{
			void AddComma(StringBuilder s)
			{
				if (s.Length > 0)
				{
					s.Append(',');
				}
			}

			var sb = new StringBuilder();
			if (Clipboard.ContainsData(DataFormats.Text))
			{
				sb.Append(DataFormats.Text);
			}
			if (Clipboard.ContainsData(DataFormats.CommaSeparatedValue))
			{
				AddComma(sb);
				sb.Append(DataFormats.CommaSeparatedValue);
			}
			if (Clipboard.ContainsData(DataFormats.Html))
			{
				AddComma(sb);
				sb.Append(DataFormats.Html);
			}
			if (Clipboard.ContainsData(DataFormats.StringFormat))
			{
				AddComma(sb);
				sb.Append(DataFormats.StringFormat);
			}
			if (Clipboard.ContainsData(DataFormats.UnicodeText))
			{
				AddComma(sb);
				sb.Append(DataFormats.UnicodeText);
			}
			if (Clipboard.ContainsData(DataFormats.Xaml))
			{
				AddComma(sb);
				sb.Append(DataFormats.Xaml);
			}
			if (Clipboard.ContainsData(DataFormats.XamlPackage))
			{
				AddComma(sb);
				sb.Append(DataFormats.XamlPackage);
			}

			return sb.ToString();
		}

		public static object GetData()
		{
			return Clipboard.GetData(DataFormats.StringFormat);
		}

	}
}
