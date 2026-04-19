using System;
using System.Collections.Generic;
using TaskStatusEnum = Final_Project_Adv.Domain.Enums.TaskStatus;
namespace Final_Project_Adv.Domain.Entities 
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Users> Users { get; set; } = new();
        public List<TaskItem> Tasks { get; set; } = new();

        // 🔥 Audit
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

