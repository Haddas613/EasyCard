﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Merchants.Api.Models.Integrations.EasyInvoice {
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
    public class ECInvoiceDocumentTypeResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal ECInvoiceDocumentTypeResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Merchants.Api.Models.Integrations.EasyInvoice.ECInvoiceDocumentTypeResource", typeof(ECInvoiceDocumentTypeResource).Assembly);
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
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Credit note.
        /// </summary>
        public static string CREDIT_NOTE {
            get {
                return ResourceManager.GetString("CREDIT_NOTE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invoice.
        /// </summary>
        public static string INVOICE {
            get {
                return ResourceManager.GetString("INVOICE", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invoice with Payment Info.
        /// </summary>
        public static string INVOICE_WITH_PAYMENT_INFO {
            get {
                return ResourceManager.GetString("INVOICE_WITH_PAYMENT_INFO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Payment Info.
        /// </summary>
        public static string PAYMENT_INFO {
            get {
                return ResourceManager.GetString("PAYMENT_INFO", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Refund Invoice with Payment Info.
        /// </summary>
        public static string REFUND_INVOICE_WITH_PAYMENT_INFO {
            get {
                return ResourceManager.GetString("REFUND_INVOICE_WITH_PAYMENT_INFO", resourceCulture);
            }
        }
    }
}