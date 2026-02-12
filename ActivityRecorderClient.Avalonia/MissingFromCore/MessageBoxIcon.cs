// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Tct.ActivityRecorderClient.Forms;

public enum MessageBoxIcon
{
    /// <summary>
    ///  Specifies that the message box contain no symbols.
    /// </summary>
    None = 0,

    /// <summary>
    ///  Specifies that the message box contains a hand symbol.
    /// </summary>
    Hand = (int)0x00000010L,

    /// <summary>
    ///  Specifies that the message box contains a question mark symbol.
    /// </summary>
    Question = (int)0x00000020L,

    /// <summary>
    ///  Specifies that the message box contains an exclamation symbol.
    /// </summary>
    Exclamation = (int)0x00000030L,

    /// <summary>
    ///  Specifies that the message box contains an asterisk symbol.
    /// </summary>
    Asterisk = (int)0x00000040L,

    /// <summary>
    ///  Specifies that the message box contains a hand icon. This field is constant.
    /// </summary>
    Stop = Hand,

    /// <summary>
    ///  Specifies that the message box contains a hand icon.
    /// </summary>
    Error = Hand,

    /// <summary>
    ///  Specifies that the message box contains an exclamation icon.
    /// </summary>
    Warning = Exclamation,

    /// <summary>
    ///  Specifies that the message box contains an asterisk icon.
    /// </summary>
    Information = Asterisk,
}