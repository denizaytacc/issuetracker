using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Models
{
    public class Issue
    {
        public Issue()
        {
            this.Users = new HashSet<User>();
            
        }
        public int Id { get; set; }
        [DisplayName("Issue Name")]
        [MinLength(3, ErrorMessage = "The project name must be longer than 3 characters"), MaxLength(100, ErrorMessage = "The project name must be shorter than 100 characters")]
        [Required(ErrorMessage = "Please enter the issue name")]
        public string? IssueName { get; set; }
        [DisplayName("Description")]
        [MinLength(5, ErrorMessage = "The description must be longer than 5 characters")]
        [Required(ErrorMessage = "Please enter the description")]
        public string Description { get; set; } = "";
        [DisplayName("Estimated Time to Complete(hours)")]
        [Required(ErrorMessage = "Please enter the ETC")]
        public int EstimatedTimeToComplete { get; set; }
        //[DataType(DataType.DateTime)]
        //[Required(ErrorMessage = "Please enter the date")]
        //[DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        //public DateTime FinishDate { get; set; } = Convert.ToDateTime(DateTime.Now); 
        [Required(ErrorMessage = "Please select the priority")]
        public PriorityType Priority { get; set; }
        [Required(ErrorMessage = "Please select the status")]
        public StatusType Status { get; set; }
        [DisplayName("Project")]
        public int ProjectId { get; set; }
        public Project? Project { get; set; }
        public virtual ICollection<User>? Users { get; set; }
        public virtual ICollection<Comment>? Comments { get; set; }
    }
    public enum PriorityType
    {

        [Display(Name = "Low")]
        Low,
        [Display(Name = "Medium")]
        Medium,
        [Display(Name = "High")]
        High,
        [Display(Name = "Immediate")]
        Immediate
    }
    public enum StatusType
    {
        [Display(Name = "Opened")]
        Opened,
        [Display(Name = "Assigned")]
        Assigned,
        [Display(Name = "Fixed")]
        Fixed,
        [Display(Name = "Reopened")]
        Reopened,
        [Display(Name = "Closed")]
        Closed
    }
}
