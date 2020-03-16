using System;
using System.Runtime.Serialization;
using System.Text;

namespace ClearingHouse.Models
{
    /// <summary>
    /// Personal Details
    /// </summary>
    [DataContract]
    public partial class PersonalDetails
    {
        /// <summary>
        /// Merchant's primary email
        /// </summary>
        [DataMember(Name = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Gets or Sets FirstName
        /// </summary>
        [DataMember(Name = "firstName")]
        public string FirstName { get; set; }

        /// <summary>
        /// Merchant&#39;s contact person family name
        /// </summary>
        [DataMember(Name = "lastName")]
        public string LastName { get; set; }

        /// <summary>
        /// Merchant&#39;s contact person gender
        /// </summary>
        [DataMember(Name = "gender")]
        public string Gender { get; set; }

        /// <summary>
        /// Merchant&#39;s contact person cell. phone
        /// </summary>
        [DataMember(Name = "cellPhone")]
        public string CellPhone { get; set; }

        /// <summary>
        /// Merchant&#39;s contact person national ID number
        /// </summary>
        [DataMember(Name = "nationalIdNumber")]
        public string NationalIdNumber { get; set; }

        /// <summary>
        /// Merchant&#39;s contact person national ID number issue date
        /// </summary>
        [DataMember(Name = "nationalIdNumberDate")]
        public DateTime? NationalIdNumberDate { get; set; }

        /// <summary>
        /// Merchant&#39;s contact person birthdate
        /// </summary>
        [DataMember(Name = "birthDate")]
        public DateTime? BirthDate { get; set; }
    }
}
