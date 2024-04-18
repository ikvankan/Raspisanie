﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raspisanie.Models
{
    public class Teacher
    {
        [Key]
        public int Id { get; set; }
        public string TeacherName { get; set; }
        public int AuditoryId { get; set; }

        [ForeignKey ("AuditoryId")]
        public virtual Auditoria Auditoria { get; set; }

    }
}
