using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace EasyInvoice.Models
{
    [DataContract]
    public class ECInvoiceDocumentResponse
    {
        [DataMember(Name = "documentUrl")]
        public string DocumentUrl { get; set; }

        [DataMember(Name = "documentCopyUrl")]
        public string DocumentCopyUrl { get; set; }

        [DataMember(Name = "documentNumber")]
        public long? DocumentNumber { get; set; }

        [DataMember(Name = "timestamp")]
        public DateTime Timestamp { get; set; }

        [DataMember(Name = "status")]
        public int Status { get; set; }

        [DataMember(Name = "error")]
        public string Error { get; set; }

        [DataMember(Name = "exception")]
        public string Exception { get; set; }

        [DataMember(Name = "errors")]
        public IList<ECInvoiceError> Errors { get; set; }

        [DataMember(Name = "message")]
        public string Message { get; set; }

        [DataMember(Name = "path")]
        public string Path { get; set; }
    }
}
