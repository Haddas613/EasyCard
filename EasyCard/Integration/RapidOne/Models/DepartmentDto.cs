using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOne.Models
{
    public class DepartmentDto
    {
        public int? DepartmentID { get; set; }
        public int? BranchID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public bool RequireForeignName { get; set; }
        public string Note { get; set; }
        public int Type { get; set; }
        public BranchDto Branch { get; set; }
        public bool LinkPricelists { get; set; }
        public bool LinkCategories { get; set; }
    }
}
