using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Elia.Unity.Infrastructure
{
	public interface IAdsProvider// : IUnityAdsListener
	{
		Action<string> AdReady { get; set; }
		Action<string> AdStarted { get; set; }
		//Action<string, ShowResult> AdFinished { get; set; }
		Action<string> AdError { get; set; }
	}
}
