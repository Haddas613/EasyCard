﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Shared.Integration.Resources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class InvoiceEnumsResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal InvoiceEnumsResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Shared.Integration.Resources.InvoiceEnumsResource", typeof(InvoiceEnumsResource).Assembly);
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
        ///   Looks up a localized string similar to Credit Note.
        /// </summary>
        public static string CreditNote {
            get {
                return ResourceManager.GetString("CreditNote", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Pending.
        /// </summary>
        public static string Initial {
            get {
                return ResourceManager.GetString("Initial", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invoice.
        /// </summary>
        public static string Invoice {
            get {
                return ResourceManager.GetString("Invoice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Invoice WIth Payment Info.
        /// </summary>
        public static string InvoiceWithPaymentInfo {
            get {
                return ResourceManager.GetString("InvoiceWithPaymentInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Payment Info.
        /// </summary>
        public static string PaymentInfo {
            get {
                return ResourceManager.GetString("PaymentInfo", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Refund Invoice with Payment Info.
        /// </summary>
        public static string RefundInvoice {
            get {
                return ResourceManager.GetString("RefundInvoice", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sending.
        /// </summary>
        public static string Sending {
            get {
                return ResourceManager.GetString("Sending", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sending Failed.
        /// </summary>
        public static string SendingFailed {
            get {
                return ResourceManager.GetString("SendingFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Sent.
        /// </summary>
        public static string Sent {
            get {
                return ResourceManager.GetString("Sent", resourceCulture);
            }
        }
    }
}
