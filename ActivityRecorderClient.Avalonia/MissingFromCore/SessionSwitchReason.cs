// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Tct.ActivityRecorderClient.Forms;

/// <devdoc>
///    <para> Specifies the reason for the session switch</para>
/// </devdoc>
public enum SessionSwitchReason
{
	/// <devdoc>
	///      A session was connected to the console session.
	/// </devdoc>
	ConsoleConnect = 1,

	/// <devdoc>
	///      A session was disconnected from the console session.
	/// </devdoc>
	ConsoleDisconnect = 2,

	/// <devdoc>
	///      A session was connected to the remote session.
	/// </devdoc>
	RemoteConnect = 3,

	/// <devdoc>
	///      A session was disconnected from the remote session.
	/// </devdoc>
	RemoteDisconnect = 4,

	/// <devdoc>
	///      A user has logged on to the session.
	/// </devdoc>
	SessionLogon = 5,

	/// <devdoc>
	///      A user has logged off the session.
	/// </devdoc>
	SessionLogoff = 6,

	/// <devdoc>
	///      A session has been locked.
	/// </devdoc>
	SessionLock = 7,

	/// <devdoc>
	///      A session has been unlocked.
	/// </devdoc>
	SessionUnlock = 8,

	/// <devdoc>
	///      A session has changed its remote controlled status.
	/// </devdoc>
	SessionRemoteControl = 9
}
