using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JobCTRL.Plugins
{
	//this will be moved to and external dll if finalized
	public interface ICaptureExtension
	{
		/// <summary>
		/// Unique id of the extension.
		/// </summary>
		string Id { get; }

		/// <summary>
		/// Returns the parameters which are supported by the plugin.
		/// </summary>
		/// <returns>Name of the parameters</returns>
		IEnumerable<string> GetParameterNames();

		/// <summary>
		/// Sets a parameter for the extension.
		/// </summary>
		/// <param name="name">Name of the parameter</param>
		/// <param name="value">Value of the parameter</param>
		void SetParameter(string name, string value);

		/// <summary>
		/// Returns the unique keys which could be captured by the plugin.
		/// </summary>
		/// <returns>Unique keys</returns>
		IEnumerable<string> GetCapturableKeys();

		/// <summary>
		/// Capture extended data for a window.
		/// </summary>
		/// <param name="hWnd">The top level window handle of the window</param>
		/// <param name="processId">Id of the process</param>
		/// <param name="processName">Name of the process</param>
		/// <returns>The captured data</returns>
		IEnumerable<KeyValuePair<string, string>> Capture(IntPtr hWnd, int processId, string processName);

		//todo do we need new param? <param name="isActive">Indicates whether this window was the active window at the time of the capture</param>
		//todo Method for signaling that new DesktopCapture will be processed (so we don't have to get data for every window) ?
		//todo attribute for getting plugin id without constructing the obj?
	}
}
