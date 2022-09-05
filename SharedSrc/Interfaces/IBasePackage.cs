using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedSrc.Interfaces
{
	/// <summary>
	/// Methods added to shared package
	/// </summary>
	/// <typeparam name="O"></typeparam>
	public interface IBasePackage<O>
	{
		O GetOptionsState();
		Task ToOutputPaneAsync( string message );
		Task ToOutputPaneAsync( bool show, string message );
	}
}
