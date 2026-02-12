// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Tct.ActivityRecorderClient.Forms;

public enum MessageBoxButtons
{
    /// <summary>
    ///  Specifies that the message box contains an OK button.
    /// </summary>
    OK = (int)0x00000000L,

    /// <summary>
    ///  Specifies that the message box contains OK and Cancel buttons.
    /// </summary>
    OKCancel = (int)0x00000001L,

    /// <summary>
    ///  Specifies that the message box contains Abort, Retry, and Ignore buttons.
    /// </summary>
    AbortRetryIgnore = (int)0x00000002L,

    /// <summary>
    ///  Specifies that the message box contains Yes, No, and Cancel buttons.
    /// </summary>
    YesNoCancel = (int)0x00000003L,

    /// <summary>
    ///  Specifies that the message box contains Yes and No buttons.
    /// </summary>
    YesNo = (int)0x00000004L,

    /// <summary>
    ///  Specifies that the message box contains Retry and Cancel buttons.
    /// </summary>
    RetryCancel = (int)0x00000005L,

    /// <summary>
    ///  Specifies that the message box contains Cancel, Try Again, and Continue buttons.
    /// </summary>
    CancelTryContinue = (int)0x00000006L,
}