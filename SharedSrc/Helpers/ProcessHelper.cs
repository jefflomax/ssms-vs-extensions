using System.Diagnostics;
using System.Threading.Tasks;

namespace SharedSrc.Helpers
{
	public static class ProcessHelper
	{
		public static async Task<string> RunProcessAsync
		(
			string executable,
			string arguments,
			string workingDirectory
		)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = executable,
					Arguments = arguments,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true,
					WorkingDirectory = workingDirectory
				}
			};

			var response = await Task.Run( () => {
				var output = "failed";

				process.Start();
				while (!process.StandardOutput.EndOfStream)
				{
					string line = process.StandardOutput.ReadLine();
					output = line;
				}
				return output;
			} );
			return response;
		}
	}
}
