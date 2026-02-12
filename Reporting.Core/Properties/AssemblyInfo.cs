using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Reporting.Core")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Reporting.Core")]
[assembly: AssemblyCopyright("Copyright ©  2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("cb35a776-5e6b-4d4c-ba3c-447de89eb09c")]

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
//[assembly: AssemblyFileVersion("1.0.0.0")]

#if DEBUG
[assembly: InternalsVisibleTo("Reporting.Test")]
#else
[assembly: InternalsVisibleTo("Reporting.Test,PublicKey=" +
                              "0024000004800000940000000602000000240000525341310004000001000100cd37301afc7091" +
                              "587687d059326011193c84a1547423fa68d30f1230dfe8c2797fb160072ed0f6765a580eef242b" +
                              "9f2429e46be340d7eb711dffc4ee78e89a63f0d651f51bd25c3f63304eb64e10761631c4d22bcf" +
                              "f281fcf0b179110df8f2a17a1407621695d80b2cf113008e0d1e8ddc56027f56ed4323a870a2f9" +
                              "09b38abf")]
#endif
[assembly: AssemblyFileVersionAttribute("1.0.0.0")]
