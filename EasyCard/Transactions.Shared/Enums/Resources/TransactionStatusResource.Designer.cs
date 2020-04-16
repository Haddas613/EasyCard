﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Transactions.Shared.Enums.Resources {
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
    public class TransactionStatusResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TransactionStatusResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Transactions.Shared.Enums.Resources.TransactionStatusResource", typeof(TransactionStatusResource).Assembly);
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
        ///   Looks up a localized string similar to Cancelled by merchant.
        /// </summary>
        public static string CancelledByMerchant {
            get {
                return ResourceManager.GetString("CancelledByMerchant", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Commited to aggregator.
        /// </summary>
        public static string CommitedToAggregator {
            get {
                return ResourceManager.GetString("CommitedToAggregator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Confirmed by aggregator.
        /// </summary>
        public static string ConfirmedByAggregator {
            get {
                return ResourceManager.GetString("ConfirmedByAggregator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Confirmed by processor.
        /// </summary>
        public static string ConfirmedByProcessor {
            get {
                return ResourceManager.GetString("ConfirmedByProcessor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to cancel to aggregator.
        /// </summary>
        public static string FailedToCancelToAggregator {
            get {
                return ResourceManager.GetString("FailedToCancelToAggregator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to commit by aggregator.
        /// </summary>
        public static string FailedToCommitByAggregator {
            get {
                return ResourceManager.GetString("FailedToCommitByAggregator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to confirm by aggregator.
        /// </summary>
        public static string FailedToConfirmByAggregator {
            get {
                return ResourceManager.GetString("FailedToConfirmByAggregator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Failed to confirm by procesor.
        /// </summary>
        public static string FailedToConfirmByProcesor {
            get {
                return ResourceManager.GetString("FailedToConfirmByProcesor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Initial.
        /// </summary>
        public static string Initial {
            get {
                return ResourceManager.GetString("Initial", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Rejected by aggregator.
        /// </summary>
        public static string RejectedByAggregator {
            get {
                return ResourceManager.GetString("RejectedByAggregator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Rejected by processor.
        /// </summary>
        public static string RejectedByProcessor {
            get {
                return ResourceManager.GetString("RejectedByProcessor", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Rejected to aggregator.
        /// </summary>
        public static string RejectedToAggregator {
            get {
                return ResourceManager.GetString("RejectedToAggregator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Transmission to processor failed.
        /// </summary>
        public static string TransmissionToProcessorFailed {
            get {
                return ResourceManager.GetString("TransmissionToProcessorFailed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Transmitted to processor.
        /// </summary>
        public static string TransmittedToProcessor {
            get {
                return ResourceManager.GetString("TransmittedToProcessor", resourceCulture);
            }
        }
    }
}