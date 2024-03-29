﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NXPorts.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NXPorts.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NXP0002: Found multiple export attributes with export alias &quot;{0}&quot;, but different capitalizations. Exports can be case sensitive, but should be avoided..
        /// </summary>
        internal static string Diag_CaseInsensitivelyDuplicateAliasesMessage {
            get {
                return ResourceManager.GetString("Diag_CaseInsensitivelyDuplicateAliasesMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NXP0001: Found multiple export attributes with export alias &quot;{0}&quot;..
        /// </summary>
        internal static string Diag_DuplicateAliasesMessage {
            get {
                return ResourceManager.GetString("Diag_DuplicateAliasesMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NXP0003: No method annotations for export reweaving were found..
        /// </summary>
        internal static string Diag_NoMethodAnnotationsFoundMessage {
            get {
                return ResourceManager.GetString("Diag_NoMethodAnnotationsFoundMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to NXPorts encountered an exception while trying to load the source assembly..
        /// </summary>
        internal static string ExportAttributedAssembly_ExceptionLoadingSourceAssembly {
            get {
                return ResourceManager.GetString("ExportAttributedAssembly_ExceptionLoadingSourceAssembly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The given file path does not exist..
        /// </summary>
        internal static string ExportAttributedAssembly_FileDoesNotExist {
            get {
                return ResourceManager.GetString("ExportAttributedAssembly_FileDoesNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Adjusting PE32 header to reflect the reweaving changes to the assembly file..
        /// </summary>
        internal static string Log_AdjustingPE32Header {
            get {
                return ResourceManager.GetString("Log_AdjustingPE32Header", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Clearing assembly of incompatible assembly flags..
        /// </summary>
        internal static string Log_ClearingFlags {
            get {
                return ResourceManager.GetString("Log_ClearingFlags", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Reweaving method &apos;{0}&apos; with alias &apos;{1}&apos; and calling convention &apos;{2}&apos;.
        /// </summary>
        internal static string Log_ReweavingMethod {
            get {
                return ResourceManager.GetString("Log_ReweavingMethod", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Successfully rewritten assembly at &apos;{0}&apos;..
        /// </summary>
        internal static string Log_SuccessWrittenAt {
            get {
                return ResourceManager.GetString("Log_SuccessWrittenAt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Calling convention &apos;{0}&apos; is not supported by Reverse PInvoke..
        /// </summary>
        internal static string Log_UnsupportedCallingConvention {
            get {
                return ResourceManager.GetString("Log_UnsupportedCallingConvention", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Validation failed - aborting reweave....
        /// </summary>
        internal static string Log_ValidationFailAborting {
            get {
                return ResourceManager.GetString("Log_ValidationFailAborting", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Writing the new assembly file to disk....
        /// </summary>
        internal static string Log_WritingToDisk {
            get {
                return ResourceManager.GetString("Log_WritingToDisk", resourceCulture);
            }
        }
    }
}
