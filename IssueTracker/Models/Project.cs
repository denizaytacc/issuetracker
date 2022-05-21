using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace IssueTracker.Models
{
    public class Project
    {
        public Project()
        {
            this.Users = new HashSet<User>();
        }
        public int Id { get; set; }
        [DisplayName("Project Name")]
        [MinLength(5, ErrorMessage = "The project name must be longer than 5 characters"), MaxLength(50, ErrorMessage = "The project name must be shorter than 50 characters")]
        [Required(ErrorMessage = "You must enter the project name")]
        public string? ProjectName { get; set; }
        [DisplayName("Project Description")]
        [MinLength(5, ErrorMessage = "The project description must be longer than 5 characters"), MaxLength(300, ErrorMessage = "The project name must be shorter than 30 characters")]
        [Required(ErrorMessage = "You must enter the project description")]
        public string? ProjectDescription { get; set; }
        [Required]
        [Range(0, 100, ErrorMessage = "The progress must be between 1 and 100")]
        public int Progress { get; set; }
        // List of issues(MtO) and members(MtM) this project has
        public virtual ICollection<Issue>? Issues { get; set; }
        public virtual ICollection<User>? Users { get; set; }
    }
}
