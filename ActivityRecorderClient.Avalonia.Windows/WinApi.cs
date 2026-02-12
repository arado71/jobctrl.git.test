using Accessibility;
using Microsoft.Win32.SafeHandles;
/**
 * MetroFramework - Modern UI for WinForms
 * 
 * The MIT License (MIT)
 * Copyright (c) 2011 Sven Walter, http://github.com/viperneo
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of 
 * this software and associated documentation files (the "Software"), to deal in the 
 * Software without restriction, including without limitation the rights to use, copy, 
 * modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, 
 * and to permit persons to whom the Software is furnished to do so, subject to the 
 * following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in 
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
 * INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
 * PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
 * HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE 
 * OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace Tct.ActivityRecorderClient
{
	[SuppressUnmanagedCodeSecurity]
	public static class WinApi
	{
		// ReSharper disable InconsistentNaming
		#region Consts
		public const int MAX_PATH = 260;
		#endregion

		#region Structs

		[StructLayout(LayoutKind.Sequential)]

		public struct POINT
		{
			public Int32 x;
			public Int32 y;

			public POINT(Int32 x, Int32 y) { this.x = x; this.y = y; }
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SIZE
		{
			public Int32 cx;
			public Int32 cy;

			public SIZE(Int32 cx, Int32 cy) { this.cx = cx; this.cy = cy; }
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct ARGB
		{
			public byte Blue;
			public byte Green;
			public byte Red;
			public byte Alpha;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct BLENDFUNCTION
		{
			public byte BlendOp;
			public byte BlendFlags;
			public byte SourceConstantAlpha;
			public byte AlphaFormat;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct TCHITTESTINFO
		{
			public Point pt;
			public uint flags;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct NCCALCSIZE_PARAMS
		{
			public RECT rect0;
			public RECT rect1;
			public RECT rect2;
			public IntPtr lppos;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MINMAXINFO
		{
			public POINT ptReserved;
			public POINT ptMaxSize;
			public POINT ptMaxPosition;
			public POINT ptMinTrackSize;
			public POINT ptMaxTrackSize;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct APPBARDATA
		{
			public uint cbSize;
			public IntPtr hWnd;
			public uint uCallbackMessage;
			public ABE uEdge;
			public RECT rc;
			public int lParam;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WindowPos
		{
			public int hwnd;
			public int hWndInsertAfter;
			public int x;
			public int y;
			public int cx;
			public int cy;
			public int flags;
		}

		//Structure for Terminal Service Client IP Address
		[StructLayout(LayoutKind.Sequential)]
		[System.Reflection.Obfuscation(Exclude = true)]
		public struct WTS_CLIENT_ADDRESS
		{
			public int iAddressFamily;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
			public byte[] bAddress;
		}

		//Structure for Terminal Service Session Info
		[StructLayout(LayoutKind.Sequential)]
		[System.Reflection.Obfuscation(Exclude = true)]
		public struct WTS_SESSION_INFO
		{
			public int iSessionID;
			[MarshalAs(UnmanagedType.LPStr)]
			public string sWinsWorkstationName;
			public WTS_CONNECTSTATE_CLASS oState;
		}

		//Structure for Terminal Service Session Client Display
		[StructLayout(LayoutKind.Sequential)]
		[System.Reflection.Obfuscation(Exclude = true)]
		public struct WTS_CLIENT_DISPLAY
		{
			public int iHorizontalResolution;
			public int iVerticalResolution;
			//1 = The display uses 4 bits per pixel for a maximum of 16 colors.
			//2 = The display uses 8 bits per pixel for a maximum of 256 colors.
			//4 = The display uses 16 bits per pixel for a maximum of 2^16 colors.
			//8 = The display uses 3-byte RGB values for a maximum of 2^24 colors.
			//16 = The display uses 15 bits per pixel for a maximum of 2^15 colors.
			public int iColorDepth;
		}

		[System.Reflection.ObfuscationAttribute(Exclude = true)]
		public struct TokenElevation
		{
			public UInt32 TokenIsElevated;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct OBJECT_TYPE_INFORMATION
		{
			public UNICODE_STRING Name;
			public int TotalNumberOfObjects;
			public int TotalNumberOfHandles;
			public int TotalPagedPoolUsage;
			public int TotalNonPagedPoolUsage;
			public int TotalNamePoolUsage;
			public int TotalHandleTableUsage;
			public int HighWaterNumberOfObjects;
			public int HighWaterNumberOfHandles;
			public int HighWaterPagedPoolUsage;
			public int HighWaterNonPagedPoolUsage;
			public int HighWaterNamePoolUsage;
			public int HighWaterHandleTableUsage;
			public int InvalidAttributes;
			public GENERIC_MAPPING GenericMapping;
			public int ValidAccess;
			public byte SecurityRequired;
			public byte MaintainHandleCount;
			public ushort MaintainTypeList;
			public int PoolType;
			public int PagedPoolUsage;
			public int NonPagedPoolUsage;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct UNICODE_STRING
		{
			public ushort Length;
			public ushort MaximumLength;
			public IntPtr Buffer;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct GENERIC_MAPPING
		{
			public int GenericRead;
			public int GenericWrite;
			public int GenericExecute;
			public int GenericAll;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SystemHandleEntry
		{
			public ushort OwnerProcessId;
			public ushort CreatorBackTraceIndex;
			public byte ObjectTypeNumber;
			public byte Flags;
			public ushort Handle;
			public IntPtr Object;
			public int GrantedAccess;

			public override string ToString()
			{
				return "pid: " + OwnerProcessId + " type: " + ObjectTypeNumber + " flags: " + Flags + " handle: " + Handle + " gacc: " + GrantedAccess;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct LASTINPUTINFO
		{
			public static readonly uint Size = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));

			[MarshalAs(UnmanagedType.U4)]
			public UInt32 cbSize;
			[MarshalAs(UnmanagedType.U4)]
			public UInt32 dwTime;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct IconInfo
		{
			public bool fIcon;
			public int xHotspot;
			public int yHotspot;
			public IntPtr hbmMask;
			public IntPtr hbmColor;
		}

		// DWORD -> uint, WORD -> ushort
		[StructLayout(LayoutKind.Sequential)]
		public struct BitmapInfoHeader
		{
			public uint biSize;
			public int biWidth;
			public int biHeight;
			public ushort biPlanes;
			public ushort biBitCount;
			public uint biCompression;
			public uint biSizeImage;
			public uint biXPelsPerMeter;
			public uint biYPelsPerMeter;
			public uint biClrUsed;
			public uint biClrImportant;
		}

		private struct LUID
		{
			public int LowPart;
			public int HighPart;
		}
		private struct LUID_AND_ATTRIBUTES
		{
			public LUID pLuid;
			public int Attributes;
		}
		private struct TOKEN_PRIVILEGES
		{
			public int PrivilegeCount;
			public LUID_AND_ATTRIBUTES Privileges;
		}

		#endregion

		#region Enums

		[Flags]
		public enum WS_EX
		{
			TOPMOST = 0x00000008,
			WS_VISIBLE = 0x10000000,
		}

		public enum ABM : uint
		{
			New = 0x00000000,
			Remove = 0x00000001,
			QueryPos = 0x00000002,
			SetPos = 0x00000003,
			GetState = 0x00000004,
			GetTaskbarPos = 0x00000005,
			Activate = 0x00000006,
			GetAutoHideBar = 0x00000007,
			SetAutoHideBar = 0x00000008,
			WindowPosChanged = 0x00000009,
			SetState = 0x0000000A,
		}

		public enum ABE : uint
		{
			Left = 0,
			Top = 1,
			Right = 2,
			Bottom = 3
		}

		public enum ScrollBar
		{
			SB_HORZ = 0,
			SB_VERT = 1,
			SB_CTL = 2,
			SB_BOTH = 3,
		}

		public enum HitTest
		{
			HTNOWHERE = 0,
			HTCLIENT = 1,
			HTCAPTION = 2,
			HTGROWBOX = 4,
			HTSIZE = HTGROWBOX,
			HTMINBUTTON = 8,
			HTMAXBUTTON = 9,
			HTLEFT = 10,
			HTRIGHT = 11,
			HTTOP = 12,
			HTTOPLEFT = 13,
			HTTOPRIGHT = 14,
			HTBOTTOM = 15,
			HTBOTTOMLEFT = 16,
			HTBOTTOMRIGHT = 17,
			HTREDUCE = HTMINBUTTON,
			HTZOOM = HTMAXBUTTON,
			HTSIZEFIRST = HTLEFT,
			HTSIZELAST = HTBOTTOMRIGHT,
			HTTRANSPARENT = -1
		}

		public enum TabControlHitTest
		{
			TCHT_NOWHERE = 1,
		}

		public enum Messages : uint
		{
			WM_NULL = 0x0,
			WM_CREATE = 0x1,
			WM_DESTROY = 0x2,
			WM_MOVE = 0x3,
			WM_SIZE = 0x5,
			WM_ACTIVATE = 0x6,
			WM_SETFOCUS = 0x7,
			WM_KILLFOCUS = 0x8,
			WM_ENABLE = 0xa,
			WM_SETREDRAW = 0xb,
			WM_SETTEXT = 0xc,
			WM_GETTEXT = 0xd,
			WM_GETTEXTLENGTH = 0xe,
			WM_PAINT = 0xf,
			WM_CLOSE = 0x10,
			WM_QUERYENDSESSION = 0x11,
			WM_QUERYOPEN = 0x13,
			WM_ENDSESSION = 0x16,
			WM_QUIT = 0x12,
			WM_ERASEBKGND = 0x14,
			WM_SYSCOLORCHANGE = 0x15,
			WM_SHOWWINDOW = 0x18,
			WM_WININICHANGE = 0x1a,
			WM_SETTINGCHANGE = WM_WININICHANGE,
			WM_DEVMODECHANGE = 0x1b,
			WM_ACTIVATEAPP = 0x1c,
			WM_FONTCHANGE = 0x1d,
			WM_TIMECHANGE = 0x1e,
			WM_CANCELMODE = 0x1f,
			WM_SETCURSOR = 0x20,
			WM_MOUSEACTIVATE = 0x21,
			WM_CHILDACTIVATE = 0x22,
			WM_QUEUESYNC = 0x23,
			WM_GETMINMAXINFO = 0x24,
			WM_PAINTICON = 0x26,
			WM_ICONERASEBKGND = 0x27,
			WM_NEXTDLGCTL = 0x28,
			WM_SPOOLERSTATUS = 0x2a,
			WM_DRAWITEM = 0x2b,
			WM_MEASUREITEM = 0x2c,
			WM_DELETEITEM = 0x2d,
			WM_VKEYTOITEM = 0x2e,
			WM_CHARTOITEM = 0x2f,
			WM_SETFONT = 0x30,
			WM_GETFONT = 0x31,
			WM_SETHOTKEY = 0x32,
			WM_GETHOTKEY = 0x33,
			WM_QUERYDRAGICON = 0x37,
			WM_COMPAREITEM = 0x39,
			WM_GETOBJECT = 0x3d,
			WM_COMPACTING = 0x41,
			WM_COMMNOTIFY = 0x44,
			WM_WINDOWPOSCHANGING = 0x46,
			WM_WINDOWPOSCHANGED = 0x47,
			WM_POWER = 0x48,
			WM_COPYDATA = 0x4a,
			WM_CANCELJOURNAL = 0x4b,
			WM_NOTIFY = 0x4e,
			WM_INPUTLANGCHANGEREQUEST = 0x50,
			WM_INPUTLANGCHANGE = 0x51,
			WM_TCARD = 0x52,
			WM_HELP = 0x53,
			WM_USERCHANGED = 0x54,
			WM_NOTIFYFORMAT = 0x55,
			WM_CONTEXTMENU = 0x7b,
			WM_STYLECHANGING = 0x7c,
			WM_STYLECHANGED = 0x7d,
			WM_DISPLAYCHANGE = 0x7e,
			WM_GETICON = 0x7f,
			WM_SETICON = 0x80,
			WM_NCCREATE = 0x81,
			WM_NCDESTROY = 0x82,
			WM_NCCALCSIZE = 0x83,
			WM_NCHITTEST = 0x84,
			WM_NCPAINT = 0x85,
			WM_NCACTIVATE = 0x86,
			WM_GETDLGCODE = 0x87,
			WM_SYNCPAINT = 0x88,
			WM_NCMOUSEMOVE = 0xa0,
			WM_NCLBUTTONDOWN = 0xa1,
			WM_NCLBUTTONUP = 0xa2,
			WM_NCLBUTTONDBLCLK = 0xa3,
			WM_NCRBUTTONDOWN = 0xa4,
			WM_NCRBUTTONUP = 0xa5,
			WM_NCRBUTTONDBLCLK = 0xa6,
			WM_NCMBUTTONDOWN = 0xa7,
			WM_NCMBUTTONUP = 0xa8,
			WM_NCMBUTTONDBLCLK = 0xa9,
			WM_NCXBUTTONDOWN = 0xab,
			WM_NCXBUTTONUP = 0xac,
			WM_NCXBUTTONDBLCLK = 0xad,
			WM_INPUT = 0xff,
			WM_KEYFIRST = 0x100,
			WM_KEYDOWN = 0x100,
			WM_KEYUP = 0x101,
			WM_CHAR = 0x102,
			WM_DEADCHAR = 0x103,
			WM_SYSKEYDOWN = 0x104,
			WM_SYSKEYUP = 0x105,
			WM_SYSCHAR = 0x106,
			WM_SYSDEADCHAR = 0x107,
			WM_UNICHAR = 0x109,
			WM_KEYLAST = 0x108,
			WM_IME_STARTCOMPOSITION = 0x10d,
			WM_IME_ENDCOMPOSITION = 0x10e,
			WM_IME_COMPOSITION = 0x10f,
			WM_IME_KEYLAST = 0x10f,
			WM_INITDIALOG = 0x110,
			WM_COMMAND = 0x111,
			WM_SYSCOMMAND = 0x112,
			WM_TIMER = 0x113,
			WM_HSCROLL = 0x114,
			WM_VSCROLL = 0x115,
			WM_INITMENU = 0x116,
			WM_INITMENUPOPUP = 0x117,
			WM_MENUSELECT = 0x11f,
			WM_MENUCHAR = 0x120,
			WM_ENTERIDLE = 0x121,
			WM_MENURBUTTONUP = 0x122,
			WM_MENUDRAG = 0x123,
			WM_MENUGETOBJECT = 0x124,
			WM_UNINITMENUPOPUP = 0x125,
			WM_MENUCOMMAND = 0x126,
			WM_CHANGEUISTATE = 0x127,
			WM_UPDATEUISTATE = 0x128,
			WM_QUERYUISTATE = 0x129,
			WM_CTLCOLOR = 0x19,
			WM_CTLCOLORMSGBOX = 0x132,
			WM_CTLCOLOREDIT = 0x133,
			WM_CTLCOLORLISTBOX = 0x134,
			WM_CTLCOLORBTN = 0x135,
			WM_CTLCOLORDLG = 0x136,
			WM_CTLCOLORSCROLLBAR = 0x137,
			WM_CTLCOLORSTATIC = 0x138,
			WM_MOUSEFIRST = 0x200,
			WM_MOUSEMOVE = 0x200,
			WM_LBUTTONDOWN = 0x201,
			WM_LBUTTONUP = 0x202,
			WM_LBUTTONDBLCLK = 0x203,
			WM_RBUTTONDOWN = 0x204,
			WM_RBUTTONUP = 0x205,
			WM_RBUTTONDBLCLK = 0x206,
			WM_MBUTTONDOWN = 0x207,
			WM_MBUTTONUP = 0x208,
			WM_MBUTTONDBLCLK = 0x209,
			WM_MOUSEWHEEL = 0x20a,
			WM_XBUTTONDOWN = 0x20b,
			WM_XBUTTONUP = 0x20c,
			WM_XBUTTONDBLCLK = 0x20d,
			WM_MOUSELAST = 0x20d,
			WM_PARENTNOTIFY = 0x210,
			WM_ENTERMENULOOP = 0x211,
			WM_EXITMENULOOP = 0x212,
			WM_NEXTMENU = 0x213,
			WM_SIZING = 0x214,
			WM_CAPTURECHANGED = 0x215,
			WM_MOVING = 0x216,
			WM_POWERBROADCAST = 0x218,
			WM_DEVICECHANGE = 0x219,
			WM_MDICREATE = 0x220,
			WM_MDIDESTROY = 0x221,
			WM_MDIACTIVATE = 0x222,
			WM_MDIRESTORE = 0x223,
			WM_MDINEXT = 0x224,
			WM_MDIMAXIMIZE = 0x225,
			WM_MDITILE = 0x226,
			WM_MDICASCADE = 0x227,
			WM_MDIICONARRANGE = 0x228,
			WM_MDIGETACTIVE = 0x229,
			WM_MDISETMENU = 0x230,
			WM_ENTERSIZEMOVE = 0x231,
			WM_EXITSIZEMOVE = 0x232,
			WM_DROPFILES = 0x233,
			WM_MDIREFRESHMENU = 0x234,
			WM_IME_SETCONTEXT = 0x281,
			WM_IME_NOTIFY = 0x282,
			WM_IME_CONTROL = 0x283,
			WM_IME_COMPOSITIONFULL = 0x284,
			WM_IME_SELECT = 0x285,
			WM_IME_CHAR = 0x286,
			WM_IME_REQUEST = 0x288,
			WM_IME_KEYDOWN = 0x290,
			WM_IME_KEYUP = 0x291,
			WM_MOUSEHOVER = 0x2a1,
			WM_MOUSELEAVE = 0x2a3,
			WM_NCMOUSELEAVE = 0x2a2,
			WM_WTSSESSION_CHANGE = 0x2b1,
			WM_TABLET_FIRST = 0x2c0,
			WM_TABLET_LAST = 0x2df,
			WM_CUT = 0x300,
			WM_COPY = 0x301,
			WM_PASTE = 0x302,
			WM_CLEAR = 0x303,
			WM_UNDO = 0x304,
			WM_RENDERFORMAT = 0x305,
			WM_RENDERALLFORMATS = 0x306,
			WM_DESTROYCLIPBOARD = 0x307,
			WM_DRAWCLIPBOARD = 0x308,
			WM_PAINTCLIPBOARD = 0x309,
			WM_VSCROLLCLIPBOARD = 0x30a,
			WM_SIZECLIPBOARD = 0x30b,
			WM_ASKCBFORMATNAME = 0x30c,
			WM_CHANGECBCHAIN = 0x30d,
			WM_HSCROLLCLIPBOARD = 0x30e,
			WM_QUERYNEWPALETTE = 0x30f,
			WM_PALETTEISCHANGING = 0x310,
			WM_PALETTECHANGED = 0x311,
			WM_HOTKEY = 0x312,
			WM_PRINT = 0x317,
			WM_PRINTCLIENT = 0x318,
			WM_APPCOMMAND = 0x319,
			WM_THEMECHANGED = 0x31a,
			WM_HANDHELDFIRST = 0x358,
			WM_HANDHELDLAST = 0x35f,
			WM_AFXFIRST = 0x360,
			WM_AFXLAST = 0x37f,
			WM_PENWINFIRST = 0x380,
			WM_PENWINLAST = 0x38f,
			WM_USER = 0x400,
			WM_REFLECT = 0x2000,
			WM_APP = 0x8000,
			WM_DWMCOMPOSITIONCHANGED = 0x031E,

			SC_MOVE = 0xF010,
			SC_MINIMIZE = 0XF020,
			SC_MAXIMIZE = 0xF030,
			SC_RESTORE = 0xF120
		}

		public enum Bool
		{
			False = 0,
			True
		};

		[System.Reflection.Obfuscation(Exclude = true)]
		public enum WTS_CONNECTSTATE_CLASS
		{
			WTSActive,
			WTSConnected,
			WTSConnectQuery,
			WTSShadow,
			WTSDisconnected,
			WTSIdle,
			WTSListen,
			WTSReset,
			WTSDown,
			WTSInit
		}

		[System.Reflection.Obfuscation(Exclude = true)]
		public enum WTS_INFO_CLASS
		{
			WTSInitialProgram,
			WTSApplicationName,
			WTSWorkingDirectory,
			WTSOEMId,
			WTSSessionId,
			WTSUserName,
			WTSWinStationName,
			WTSDomainName,
			WTSConnectState,
			WTSClientBuildNumber,
			WTSClientName,
			WTSClientDirectory,
			WTSClientProductId,
			WTSClientHardwareId,
			WTSClientAddress,
			WTSClientDisplay,
			WTSClientProtocolType,
			WTSIdleTime,
			WTSLogonTime,
			WTSIncomingBytes,
			WTSOutgoingBytes,
			WTSIncomingFrames,
			WTSOutgoingFrames,
			WTSClientInfo,
			WTSSessionInfo,
			WTSConfigInfo,
			WTSValidationInfo,
			WTSSessionAddressV4,
			WTSIsRemoteSession
		}

		public enum GetWindowCmd : uint
		{
			GW_HWNDFIRST = 0,
			GW_HWNDLAST = 1,
			GW_HWNDNEXT = 2,
			GW_HWNDPREV = 3,
			GW_OWNER = 4,
			GW_CHILD = 5,
			GW_ENABLEDPOPUP = 6
		}

		public enum DWMWINDOWATTRIBUTE
		{
			DWMWA_NCRENDERING_ENABLED = 1,
			DWMWA_NCRENDERING_POLICY,
			DWMWA_TRANSITIONS_FORCEDISABLED,
			DWMWA_ALLOW_NCPAINT,
			DWMWA_CAPTION_BUTTON_BOUNDS,
			DWMWA_NONCLIENT_RTL_LAYOUT,
			DWMWA_FORCE_ICONIC_REPRESENTATION,
			DWMWA_FLIP3D_POLICY,
			DWMWA_EXTENDED_FRAME_BOUNDS,
			DWMWA_HAS_ICONIC_BITMAP,
			DWMWA_DISALLOW_PEEK,
			DWMWA_EXCLUDED_FROM_PEEK,
			DWMWA_CLOAK,
			DWMWA_CLOAKED,
			DWMWA_FREEZE_REPRESENTATION,
			DWMWA_LAST
		}

		/// <summary>
		/// Passed to <see cref="GetTokenInformation"/> to specify what
		/// information about the token to return.
		/// </summary>
		public enum TokenInformationClass
		{
			TokenUser = 1,
			TokenGroups,
			TokenPrivileges,
			TokenOwner,
			TokenPrimaryGroup,
			TokenDefaultDacl,
			TokenSource,
			TokenType,
			TokenImpersonationLevel,
			TokenStatistics,
			TokenRestrictedSids,
			TokenSessionId,
			TokenGroupsAndPrivileges,
			TokenSessionReference,
			TokenSandBoxInert,
			TokenAuditPolicy,
			TokenOrigin,
			TokenElevationType,
			TokenLinkedToken,
			TokenElevation,
			TokenHasRestrictions,
			TokenAccessInformation,
			TokenVirtualizationAllowed,
			TokenVirtualizationEnabled,
			TokenIntegrityLevel,
			TokenUiAccess,
			TokenMandatoryPolicy,
			TokenLogonSid,
			MaxTokenInfoClass
		}

		/// <summary>
		/// The elevation type for a user token.
		/// </summary>
		public enum TokenElevationType
		{
			TokenElevationTypeDefault = 1,
			TokenElevationTypeFull,
			TokenElevationTypeLimited
		}

		//http://msdn.microsoft.com/en-us/library/windows/desktop/ms684880(v=vs.85).aspx
		public enum ProcessAccessFlags
		{
			PROCESS_QUERY_LIMITED_INFORMATION = 0x1000,
			SYNCHRONIZE = 0x00100000,
		}

		public enum GetAncestorFlags
		{
			/// <summary>
			/// Retrieves the parent window. This does not include the owner, as it does with the GetParent function.
			/// </summary>
			GetParent = 1,
			/// <summary>
			/// Retrieves the root window by walking the chain of parent windows.
			/// </summary>
			GetRoot = 2,
			/// <summary>
			/// Retrieves the owned root window by walking the chain of parent and owner windows returned by GetParent.
			/// </summary>
			GetRootOwner = 3
		}

		public enum NT_STATUS
		{
			STATUS_SUCCESS = 0x00000000,
			STATUS_BUFFER_OVERFLOW = unchecked((int)0x80000005L),
			STATUS_INFO_LENGTH_MISMATCH = unchecked((int)0xC0000004L)
		}

		public enum SYSTEM_INFORMATION_CLASS
		{
			SystemBasicInformation = 0,
			SystemPerformanceInformation = 2,
			SystemTimeOfDayInformation = 3,
			SystemProcessInformation = 5,
			SystemProcessorPerformanceInformation = 8,
			SystemHandleInformation = 16,
			SystemInterruptInformation = 23,
			SystemExceptionInformation = 33,
			SystemRegistryQuotaInformation = 37,
			SystemLookasideInformation = 45
		}

		public enum OBJECT_INFORMATION_CLASS
		{
			ObjectBasicInformation = 0,
			ObjectNameInformation = 1,
			ObjectTypeInformation = 2,
			ObjectAllTypesInformation = 3,
			ObjectHandleInformation = 4
		}

		[Flags]
		public enum ProcessAccessRights
		{
			PROCESS_DUP_HANDLE = 0x00000040
		}

		[Flags]
		public enum DuplicateHandleOptions
		{
			NONE = 0x0,
			DUPLICATE_CLOSE_SOURCE = 0x1,
			DUPLICATE_SAME_ACCESS = 0x2
		}

		public enum FileType : uint
		{
			FileTypeChar = 0x0002,
			FileTypeDisk = 0x0001,
			FileTypePipe = 0x0003,
			FileTypeRemote = 0x8000,
			FileTypeUnknown = 0x0000,
		}

		[FlagsAttribute]
		public enum EXECUTION_STATE : uint
		{
			ES_SYSTEM_REQUIRED = 0x00000001,
			ES_DISPLAY_REQUIRED = 0x00000002,
			// Legacy flag, should not be used.
			// ES_USER_PRESENT   = 0x00000004,
			ES_AWAYMODE_REQUIRED = 0x00000040, //Windows Server 2003 and Windows XP/2000 is not supported.
			ES_CONTINUOUS = 0x80000000,
		}

		public enum SystemEventContants : uint
		{
			EVENT_SYSTEM_SOUND = 0x1,
			EVENT_SYSTEM_ALERT = 0x2,
			EVENT_SYSTEM_FOREGROUND = 0x3,
			EVENT_SYSTEM_MENUSTART = 0x4,
			EVENT_SYSTEM_MENUEND = 0x5,
			EVENT_SYSTEM_MENUPOPUPSTART = 0x6,
			EVENT_SYSTEM_MENUPOPUPEND = 0x7,
			EVENT_SYSTEM_CAPTURESTART = 0x8,
			EVENT_SYSTEM_CAPTUREEND = 0x9,
			EVENT_SYSTEM_MOVESIZESTART = 0xa,
			EVENT_SYSTEM_MOVESIZEEND = 0xb,
			EVENT_SYSTEM_CONTEXTHELPSTART = 0xc,
			EVENT_SYSTEM_CONTEXTHELPEND = 0xd,
			EVENT_SYSTEM_DRAGDROPSTART = 0xe,
			EVENT_SYSTEM_DRAGDROPEND = 0xf,
			EVENT_SYSTEM_DIALOGSTART = 0x10,
			EVENT_SYSTEM_DIALOGEND = 0x11,
			EVENT_SYSTEM_SCROLLINGSTART = 0x12,
			EVENT_SYSTEM_SCROLLINGEND = 0x13,
			EVENT_SYSTEM_SWITCHSTART = 0x14,
			EVENT_SYSTEM_SWITCHEND = 0x15,
			EVENT_SYSTEM_MINIMIZESTART = 0x16,
			EVENT_SYSTEM_MINIMIZEEND = 0x17
		}

		public enum ObjectEventContants : uint
		{
			EVENT_OBJECT_CREATE = 0x8000,
			EVENT_OBJECT_DESTROY = 0x8001,
			EVENT_OBJECT_SHOW = 0x8002,
			EVENT_OBJECT_HIDE = 0x8003,
			EVENT_OBJECT_REORDER = 0x8004,
			EVENT_OBJECT_FOCUS = 0x8005,
			EVENT_OBJECT_SELECTION = 0x8006,
			EVENT_OBJECT_SELECTIONADD = 0x8007,
			EVENT_OBJECT_SELECTIONREMOVE = 0x8008,
			EVENT_OBJECT_SELECTIONWITHIN = 0x8009,
			EVENT_OBJECT_STATECHANGE = 0x800A,
			EVENT_OBJECT_LOCATIONCHANGE = 0x800B,
			EVENT_OBJECT_NAMECHANGE = 0x800C,
			EVENT_OBJECT_DESCRIPTIONCHANGE = 0x800D,
			EVENT_OBJECT_VALUECHANGE = 0x800E,
			EVENT_OBJECT_PARENTCHANGE = 0x800F,
			EVENT_OBJECT_HELPCHANGE = 0x8010,
			EVENT_OBJECT_DEFACTIONCHANGE = 0x8011,
			EVENT_OBJECT_ACCELERATORCHANGE = 0x8012
		}

		public enum WinEventFlags
		{
			WINEVENT_OUTOFCONTEXT = 0x0000,
			WINEVENT_SKIPOWNTHREAD = 0x0001,
			WINEVENT_SKIPOWNPROCESS = 0x0002,
			WINEVENT_INCONTEXT = 0x0004
		}

		#endregion

		#region Fields
		public delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

		public const int Autohide = 0x0000001;
		public const int AlwaysOnTop = 0x0000002;

		public const Int32 MfByposition = 0x400;
		public const Int32 MfRemove = 0x1000;

		public const int TCM_HITTEST = 0x1313;

		public const Int32 ULW_COLORKEY = 0x00000001;
		public const Int32 ULW_ALPHA = 0x00000002;
		public const Int32 ULW_OPAQUE = 0x00000004;

		public const byte AC_SRC_OVER = 0x00;
		public const byte AC_SRC_ALPHA = 0x01;

		// GetWindow() constants
		public const int GW_HWNDFIRST = 0;
		public const int GW_HWNDLAST = 1;
		public const int GW_HWNDNEXT = 2;
		public const int GW_HWNDPREV = 3;
		public const int GW_OWNER = 4;
		public const int GW_CHILD = 5;
		public const int HC_ACTION = 0;
		public const int WH_CALLWNDPROC = 4;
		public const int GWL_WNDPROC = -4;
		public const int GWL_EXSTYLE = -20;
		public const int GWL_STYLE = -16;

		// ExStyle constants
		public const int WS_EX_NOACTIVATE = 0x8000000;
		//public const int WS_EX_COMPOSITED = 0x02000000; //don't use this one as it has many side effects
		public const int WS_EX_TRANSPARENT = 0x20;
		public const int WS_EX_LAYERED = 0x80000;

		public const int WM_SETTINGCHANGE = 0x001A;
		public const short SPI_WORKAREA = 47;

		// Style contants
		public const int WS_MINIMIZEBOX = 0x20000;
		public const int WS_CLIPCHILDREN = 0x02000000;

		public const int CS_DBLCLKS = 0x8;

		public const uint ECM_FIRST = 0x1500;
		public const uint EM_SETCUEBANNER = ECM_FIRST + 1;

		public const uint WM_GETTEXT = 0x000D;
		public const uint WM_GETTEXTLENGTH = 0x000E;

		public const int STANDARD_RIGHTS_REQUIRED = 0xF0000;
		public const int TOKEN_ASSIGN_PRIMARY = 0x1;
		public const int TOKEN_DUPLICATE = 0x2;
		public const int TOKEN_IMPERSONATE = 0x4;
		public const int TOKEN_QUERY = 0x8;
		public const int TOKEN_QUERY_SOURCE = 0x10;
		public const int TOKEN_ADJUST_GROUPS = 0x40;
		public const int TOKEN_ADJUST_PRIVILEGES = 0x20;
		public const int TOKEN_ADJUST_SESSIONID = 0x100;
		public const int TOKEN_ADJUST_DEFAULT = 0x80;
		public const int TOKEN_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED | TOKEN_ASSIGN_PRIMARY | TOKEN_DUPLICATE | TOKEN_IMPERSONATE | TOKEN_QUERY | TOKEN_QUERY_SOURCE | TOKEN_ADJUST_PRIVILEGES | TOKEN_ADJUST_GROUPS | TOKEN_ADJUST_SESSIONID | TOKEN_ADJUST_DEFAULT);

		public const int INFINITE = -1;
		public const int WAIT_ABANDONED = 0x80;
		public const int WAIT_OBJECT_0 = 0x00;
		public const int WAIT_TIMEOUT = 0x102;
		public const int WAIT_FAILED = -1;

		public const string WM_HTML_GETOBJECT = "WM_HTML_GETOBJECT";
		public const Int32 SMTO_ABORTIFHUNG = 2;

		public const int HWND_TOPMOST = -1;
		public const uint SWP_NOSIZE = 0x0001;
		public const uint SWP_NOMOVE = 0x0002;
		public const uint SWP_NOACTIVATE = 0x0010;

		#endregion

		#region API Calls

		[DllImport("advapi32.dll")]
		static extern int OpenProcessToken(IntPtr ProcessHandle,
			int DesiredAccess, out IntPtr TokenHandle);

		[DllImport("advapi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AdjustTokenPrivileges(IntPtr TokenHandle,
			[MarshalAs(UnmanagedType.Bool)] bool DisableAllPrivileges,
			ref TOKEN_PRIVILEGES NewState,
			UInt32 BufferLength,
			IntPtr PreviousState,
			IntPtr ReturnLength);

		[DllImport("advapi32.dll")]
		static extern int LookupPrivilegeValue(string lpSystemName,
			string lpName, out LUID lpLuid);

		[DllImport("user32.dll", SetLastError = true)]
		static extern int ExitWindowsEx(uint uFlags, uint dwReason);
		[DllImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache")]
		public static extern uint DnsFlushResolverCache();

		[DllImport("user32.dll", ExactSpelling = true, SetLastError = true, EntryPoint = "UpdateLayeredWindow")]
		public static extern Bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pprSrc, Int32 crKey, ref BLENDFUNCTION pblend, Int32 dwFlags);

		[DllImport("user32.dll", ExactSpelling = true, SetLastError = true, EntryPoint = "GetDC")]
		public static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true, EntryPoint = "CreateCompatibleDC")]
		public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true, EntryPoint = "DeleteDC")]
		public static extern Bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll", ExactSpelling = true, EntryPoint = "SelectObject")]
		public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

		[DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true, EntryPoint = "DeleteObject")]
		public static extern Bool DeleteObject(IntPtr hObject);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLong")]
		public static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "SetWindowLong")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);

		[DllImport("user32.dll", EntryPoint = "GetSystemMenu")]
		public static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

		[DllImport("user32.dll", EntryPoint = "GetMenuItemCount")]
		public static extern int GetMenuItemCount(IntPtr hMenu);

		[DllImport("user32.dll", EntryPoint = "GetForegroundWindow")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", EntryPoint = "DrawMenuBar")]
		public static extern bool DrawMenuBar(IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "RemoveMenu")]
		public static extern bool RemoveMenu(IntPtr hMenu, uint uPosition, uint uFlags);

		[DllImport("user32.dll", EntryPoint = "ReleaseCapture")]
		public static extern bool ReleaseCapture();

		[DllImport("user32.dll", EntryPoint = "SetCapture")]
		public static extern IntPtr SetCapture(IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "SendMessage")]
		public static extern int SendMessage(IntPtr wnd, int msg, bool param, int lparam);

		[DllImport("shell32.dll", SetLastError = true, EntryPoint = "SHAppBarMessage")]
		public static extern IntPtr SHAppBarMessage(ABM dwMessage, [In] ref APPBARDATA pData);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "FindWindow")]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll", EntryPoint = "SetForeGroundWindow")]
		public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "GetDCEx")]
		public static extern IntPtr GetDCEx(IntPtr hwnd, IntPtr hrgnclip, uint fdwOptions);

		[DllImport("user32.dll", EntryPoint = "ShowScrollBar")]
		public static extern bool ShowScrollBar(IntPtr hWnd, int bar, int cmd);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowDC")]
		public static extern IntPtr GetWindowDC(IntPtr handle);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "ReleaseDC")]
		public static extern IntPtr ReleaseDC(IntPtr handle, IntPtr hDC);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "GetClassName")]
		public static extern int GetClassName(IntPtr hwnd, char[] className, int maxCount);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "GetClassName")]
		public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, EntryPoint = "GetWindowTextLength")]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true, EntryPoint = "GetWindowText")]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString,
			int nMaxCount);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowThreadProcessId")]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

		[DllImport("user32.dll", EntryPoint = "EnumChildWindows")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumChildWindows(IntPtr hwndParent, EnumWindowsProc lpEnumFunc, IntPtr lParam);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindow")]
		public static extern IntPtr GetWindow(IntPtr hwnd, int uCmd);

		[DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "IsWindowVisible")]
		public static extern bool IsWindowVisible(IntPtr hwnd);

		[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "GetClientRect")]
		public static extern int GetClientRect(IntPtr hwnd, [In, Out] ref RECT rect);

		[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "MoveWindow")]
		public static extern bool MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

		[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "UpdateWindow")]
		public static extern bool UpdateWindow(IntPtr hwnd);

		[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "InvalidateRect")]
		public static extern bool InvalidateRect(IntPtr hwnd, ref RECT rect, bool bErase);

		[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "ValidateRect")]
		public static extern bool ValidateRect(IntPtr hwnd, ref RECT rect);

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi, EntryPoint = "GetFocus")]
		internal static extern IntPtr GetFocus();

		[DllImport("user32.dll", EntryPoint = "WindowFromPoint")]
		internal static extern IntPtr WindowFromPoint(Point p);

		[DllImport("wtsapi32.dll", EntryPoint = "WTSEnumerateSessions")]
		public static extern int WTSEnumerateSessions(
			IntPtr pServer,
			[MarshalAs(UnmanagedType.U4)] int iReserved,
			[MarshalAs(UnmanagedType.U4)] int iVersion,
			ref IntPtr pSessionInfo,
			[MarshalAs(UnmanagedType.U4)] ref int iCount);

		[DllImport("Wtsapi32.dll", EntryPoint = "WTSQuerySessionInformation")]
		public static extern bool WTSQuerySessionInformation(
			System.IntPtr pServer,
			int iSessionID,
			WTS_INFO_CLASS oInfoClass,
			out System.IntPtr pBuffer,
			out uint iBytesReturned);

		[DllImport("wtsapi32.dll", EntryPoint = "WTSFreeMemory")]
		public static extern void WTSFreeMemory(
			IntPtr pMemory);

		[DllImport("gdi32.dll", SetLastError = true, EntryPoint = "BitBlt")]
		public static extern int BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);
		[DllImport("gdi32.dll", SetLastError = true, EntryPoint = "CreateCompatibleBitmap")]
		public static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);
		[DllImport("user32.dll", SetLastError = true, EntryPoint = "GetDesktopWindow")]
		public static extern IntPtr GetDesktopWindow();

		// Retrieves the address of the specified interface for the object 
		// associated with the specified window.
		[DllImport("oleacc.dll", PreserveSig = false, CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "AccessibleObjectFromWindow")]
		[return: MarshalAs(UnmanagedType.Interface)]
		public static extern object AccessibleObjectFromWindow(IntPtr hwnd, uint id, ref Guid iid);

		// Retrieves the child ID or IDispatch of each child within an accessible 
		// container object.
		[DllImport("oleacc.dll", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "AccessibleChildren")]
		public static extern int AccessibleChildren(
			IAccessible paccContainer,
			int iChildStart,
			int cChildren,
			[Out] [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)]
			object[] rgvarChildren,
			ref int pcObtained);

		[DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll", EntryPoint = "GetWindow", CharSet = CharSet.Auto)]
		public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "SendMessage")]
		public static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, StringBuilder lParam);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "SendMessage")]
		public static extern int SendMessage(IntPtr hWnd, uint msg, int wParam, int lParam);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "SendMessage")]
		public static extern int SendMessage(IntPtr hWnd, Messages msg, int wParam, int lParam);

		[DllImport("user32.dll", EntryPoint = "EnumWindows")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, int lParam);

		[DllImport("user32.dll", EntryPoint = "FindWindowEx")]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, IntPtr windowTitle);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "IsIconic")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsIconic(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "IsZoomed")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsZoomed(IntPtr hWnd);

		[DllImport("user32.dll", EntryPoint = "GetWindowRect")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll", EntryPoint = "ClientToScreen")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool ClientToScreen(IntPtr hWnd, ref POINT lpPoint);

		[DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId")]
		public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int processId);

		[DllImport("dwmapi.dll", EntryPoint = "DwmGetWindowAttribute")]
		public static extern int DwmGetWindowAttribute(IntPtr hwnd, DWMWINDOWATTRIBUTE dwAttribute, out int pvAttribute, int cbAttribute);

		[DllImport("user32.dll", EntryPoint = "EnumChildWindows")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool EnumChildWindows(IntPtr parentHandle, EnumWindowsProc lpEnumFunc, int lParam);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "GetWindow")]
		public static extern IntPtr GetWindow(IntPtr hWnd, GetWindowCmd uCmd);

		[ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true, EntryPoint = "CloseHandle")]
		public static extern bool CloseHandle(IntPtr handle);

		[DllImport("user32.dll", ExactSpelling = true, EntryPoint = "GetAncestor")]
		public static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "FindWindowEx")]
		public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetFileType")]
		internal static extern FileType GetFileType(SafeObjectHandle hFile);

		[DllImport("ntdll.dll", SetLastError = true, EntryPoint = "NtQuerySystemInformation")]
		internal static extern NT_STATUS NtQuerySystemInformation(
			[In] SYSTEM_INFORMATION_CLASS SystemInformationClass,
			[In] IntPtr SystemInformation,
			[In] int SystemInformationLength,
			[Out] out int ReturnLength);

		[DllImport("ntdll.dll", SetLastError = true, EntryPoint = "NtQueryObject")]
		internal static extern NT_STATUS NtQueryObject(
			[In] SafeObjectHandle Handle,
			[In] OBJECT_INFORMATION_CLASS ObjectInformationClass,
			[In] IntPtr ObjectInformation,
			[In] int ObjectInformationLength,
			[Out] out int ReturnLength);

		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "OpenProcess")]
		internal static extern SafeProcessHandle OpenProcess(
			[In] ProcessAccessRights dwDesiredAccess,
			[In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
			[In] int dwProcessId);

		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "DuplicateHandle")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool DuplicateHandle(
			[In] SafeProcessHandle hSourceProcessHandle,
			[In] IntPtr hSourceHandle,
			[In] IntPtr hTargetProcessHandle,
			[Out] out SafeObjectHandle lpTargetHandle,
			[In] int dwDesiredAccess,
			[In, MarshalAs(UnmanagedType.Bool)] bool bInheritHandle,
			[In] DuplicateHandleOptions dwOptions);

		[DllImport("kernel32.dll", EntryPoint = "GetCurrentProcess")]
		internal static extern IntPtr GetCurrentProcess();

		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "GetProcessId")]
		internal static extern int GetProcessId(
			[In] IntPtr Process);

		[DllImport("kernel32.dll", SetLastError = true, EntryPoint = "QueryDosDevice")]
		internal static extern int QueryDosDevice(
			[In] string lpDeviceName,
			[Out] StringBuilder lpTargetPath,
			[In] int ucchMax);

		[DllImport("user32.dll", EntryPoint = "GetClipboardSequenceNumber")]
		public static extern uint GetClipboardSequenceNumber();

		[DllImport("kernel32", CharSet = CharSet.Unicode, EntryPoint = "WritePrivateProfileString")]
		public static extern long WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

		[DllImport("kernel32", CharSet = CharSet.Unicode, EntryPoint = "GetPrivateProfileString")]
		public static extern int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "GetCurrentProcessId")]
		public static extern int GetCurrentProcessId();

		[DllImport("user32", EntryPoint = "RegisterWindowMessageA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern Int32 RegisterWindowMessage(string lpString);

		[DllImport("user32", EntryPoint = "SendMessageTimeoutA", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		public static extern Int32 SendMessageTimeout(IntPtr hWnd, Int32 msg, Int32 wParam, Int32 lParam, Int32 fuFlags, Int32 uTimeout, ref Int32 lpdwResult);

		//[DllImport("oleacc", EntryPoint = "ObjectFromLresult", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		//public static extern Int32 ObjectFromLresult(Int32 lResult, ref Guid riid, Int32 wParam, ref IEController.IHTMLDocument2 ppvObject);

		[DllImport("user32.dll", EntryPoint = "IsWindow")]
		public static extern bool IsWindow(IntPtr hWnd);

		[DllImport("msvcrt.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl, EntryPoint = "memcmp")]
		public static extern int memcmp(IntPtr b1, IntPtr b2, long count);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "GetLastInputInfo")]
		public static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

		[DllImport("User32", EntryPoint = "GetGuiResources")]
		public static extern int GetGuiResources(IntPtr hProcess, int uiFlags);

		[DllImport("kernel32", EntryPoint = "GetTickCount64")]
		public static extern ulong GetTickCount64();

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "RegisterHotKey")]
		public static extern int RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, Keys vk);

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "UnregisterHotKey")]
		public static extern int UnregisterHotKey(IntPtr hWnd, int id);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "GetModuleHandle")]
		public static extern IntPtr GetModuleHandle(string moduleName);

		[DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "GetProcAddress")]
		public static extern IntPtr GetProcAddress(IntPtr hModule,
			[MarshalAs(UnmanagedType.LPStr)]string procName);

		[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi, EntryPoint = "IsWow64Process")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool IsWow64Process([In] IntPtr process, [Out] out bool wow64Process);

		//http://www.pinvoke.net/default.aspx/user32/SetWindowPos.html
		//http://stackoverflow.com/questions/156046/show-a-form-without-stealing-focus-in-c
		[DllImport("user32.dll", EntryPoint = "SetWindowPos")]
		public static extern bool SetWindowPos(
			IntPtr hWnd,         // window handle
			int hWndInsertAfter, // placement-order handle
			int X,               // horizontal position
			int Y,               // vertical position
			int cx,              // width
			int cy,              // height
			uint uFlags);        // window positioning flags

		[DllImport("user32", EntryPoint = "PostMessage")]
		public static extern bool PostMessage(IntPtr hwnd, int msg, IntPtr wparam, IntPtr lparam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "SetThreadExecutionState")]
		public static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

		[DllImport("advapi32.dll", SetLastError = true, EntryPoint = "GetTokenInformation")]
		public static extern bool GetTokenInformation(IntPtr tokenHandle, TokenInformationClass tokenInformationClass, IntPtr tokenInformation, int tokenInformationLength, out int returnLength);

		[DllImport("user32.dll", EntryPoint = "HideCaret")]
		public static extern bool HideCaret(IntPtr hWnd);
		[DllImport("user32.dll", EntryPoint = "ShowCaret")]
		public static extern bool ShowCaret(IntPtr hWnd);

		[DllImport("user32", EntryPoint = "DeleteMenu")]
		public static extern bool DeleteMenu(IntPtr hMenu, int uPosition, int uFlags);

		[DllImport("user32.dll", EntryPoint = "GetCaretPos")]
		public static extern int GetCaretPos(ref Point lpPoint);

		[DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, EntryPoint = "GetKeyState")]
		public static extern short GetKeyState(int vKey);

		[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "DestroyIcon")]
		public static extern bool DestroyIcon(IntPtr handle);

		[DllImport("gdi32.dll", EntryPoint = "CreateDIBSection")]
		public static extern IntPtr CreateDIBSection(IntPtr hdc, [In] ref BitmapInfoHeader pbmi, uint iUsage,
			out IntPtr ppvBits, IntPtr hSection, uint dwOffset);

		[DllImport("user32.dll", EntryPoint = "CreateIconIndirect")]
		public static extern IntPtr CreateIconIndirect(ref IconInfo icon);

		[DllImport("user32.dll", EntryPoint = "GetIconInfo")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

		[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory")]
		public static extern bool RtlMoveMemory(IntPtr dest, IntPtr source, int dwcount);

		public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
		IntPtr hwnd, uint idObject, uint idChild, uint dwEventThread, uint dwmsEventTime);

		[DllImport("user32.dll")]
		public static extern IntPtr SetWinEventHook(SystemEventContants eventMin, SystemEventContants eventMax, IntPtr
	   hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
	   uint idThread, WinEventFlags dwFlags);

		[DllImport("user32.dll")]
		public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

		[DllImport("oleacc.dll")]
		public static extern uint AccessibleObjectFromEvent(IntPtr hwnd, uint dwObjectID, uint dwChildID, out IAccessible ppacc, [MarshalAs(UnmanagedType.Struct)] out object pvarChild);

		[DllImport("user32.dll", EntryPoint = "EnumDesktopWindows", ExactSpelling = false, CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumWindowsProc lpEnumCallbackFunction, IntPtr lParam);

		#endregion

		#region Helper Methods

		public static int LoWord(int dwValue)
		{
			return dwValue & 0xffff;
		}

		public static int HiWord(int dwValue)
		{
			return (dwValue >> 16) & 0xffff;
		}

		public static bool CheckCursorIsInsideControl(this Control control)
		{
			var hwnd = WindowFromPoint(Cursor.Position);
			var c = Control.FromHandle(hwnd);
			if (c == null) return false;
			while (c.Parent != null && c.Handle != control.Handle) c = c.Parent;
			return control.Handle == c.Handle;
		}

		[DllImport("user32.dll", SetLastError = true, EntryPoint = "SystemParametersInfo")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, ref bool pvParam, uint fWinIni);

		private const uint SPIF_SENDCHANGE = 0x02;
		private const uint SPI_GETSCREENREADER = 70;
		private const uint SPI_SETSCREENREADER = 71;

		// ScreenReader mode detection & setting
		// https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-systemparametersinfoa
		// https://stackoverflow.com/questions/8079716/c-sharp-how-to-detect-if-screen-reader-is-running
		// https://stackoverflow.com/questions/1650838/getting-the-windows-system-error-code-title-description-from-its-hex-number

		public static bool IsScreenReaderRunning()
		{
			var bScreenReader = false;
			if (!SystemParametersInfo(SPI_GETSCREENREADER, 0, ref bScreenReader, 0))
				throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
			return bScreenReader;
		}

		public static void ScreenReaderOn()
		{
			var pvParam = false;
			if (!SystemParametersInfo(SPI_SETSCREENREADER, 1, ref pvParam, SPIF_SENDCHANGE))
				throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
		}

		public static void ScreenReaderOff()
		{
			var pvParam = false;
			if (!SystemParametersInfo(SPI_SETSCREENREADER, 0, ref pvParam, SPIF_SENDCHANGE))
				throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
		}

		public static void Shutdown()
		{
			const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";
			const short SE_PRIVILEGE_ENABLED = 2;
			const int EWX_LOGOFF = 0x00000000;
			const int EWX_SHUTDOWN = 0x00000001;
			const int EWX_REBOOT = 0x00000002;
			const int EWX_FORCE = 0x00000004;
			const int EWX_POWEROFF = 0x00000008;
			const int EWX_FORCEIFHUNG = 0x00000010;
			const short TOKEN_ADJUST_PRIVILEGES = 32;
			const short TOKEN_QUERY = 8;
			IntPtr hToken;
			TOKEN_PRIVILEGES tkp;

			// Get shutdown privileges...
			OpenProcessToken(Process.GetCurrentProcess().Handle,
				TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, out hToken);
			tkp.PrivilegeCount = 1;
			tkp.Privileges.Attributes = SE_PRIVILEGE_ENABLED;
			LookupPrivilegeValue("", SE_SHUTDOWN_NAME, out tkp.Privileges.pLuid);
			AdjustTokenPrivileges(hToken, false, ref tkp, 0U, IntPtr.Zero,
				IntPtr.Zero);

			// Now we have the privileges, shutdown Windows
			ExitWindowsEx(EWX_SHUTDOWN | EWX_FORCE | EWX_FORCEIFHUNG, 0);
		}

		#endregion

		#region classes
		[SuppressUnmanagedCodeSecurity]
		internal sealed class SafeObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			private SafeObjectHandle()
				: base(true)
			{ }

			internal SafeObjectHandle(IntPtr preexistingHandle, bool ownsHandle)
				: base(ownsHandle)
			{
				base.SetHandle(preexistingHandle);
			}

			protected override bool ReleaseHandle()
			{
				return WinApi.CloseHandle(base.handle);
			}
		}
		#endregion
		// ReSharper restore InconsistentNaming
	}
}