using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Mail Activity Tracker Addin")]
[assembly: AssemblyDescription("JobCTRL Mail Activity Tracker Addin for Outlook")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("JobCTRL Inc.")]
[assembly: AssemblyProduct("JobCTRL Mail Activity Tracker Addin")]
[assembly: AssemblyCopyright("Copyright © JobCTRL 2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("27537acc-24c9-4f38-a59a-bf8c62316303")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.1.0")]
[assembly: AssemblyFileVersion("1.0.1.0")]

[assembly: log4net.Config.XmlConfigurator(Watch = true)]