//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;

//namespace Disco.Models.Repository
//{
//    // Added 2012-10-23 G# - DBv5 Migration
//    public class JobAssignment
//    {
//        [Key, Required, ColumnAttribute(Order = 0)]
//        public int JobId { get; set; }
//        [Key, Required, ColumnAttribute(Order = 1)]
//        public string TechUserId { get; set; }
//        [Key, Required, ColumnAttribute(Order = 2)]
//        public DateTime AssignedDate { get; set; }

//        public DateTime? UnassignedDate { get; set; }

//        public DateTime? TargetCompletionDate { get; set; }

//        [ForeignKey("JobId"), InverseProperty("JobAssignments")]
//        public virtual Job Job { get; set; }

//        [ForeignKey("TechUserId")]
//        public User TechUser { get; set; }
//    }
//}
