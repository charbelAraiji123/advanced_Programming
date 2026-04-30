using Final_Project_Adv.Domain.DTO;

namespace Final_Project_Adv.Models
{



   
        public class TaskDashboardVm
        {
            public List<TaskDashboardItemDto> Tasks { get; set; } = new();
        }
    
}
