using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTracker.Models
{
    public class User
    {
        public User()
        {
            this.Projects = new HashSet<Project>();
            this.Issues = new HashSet<Issue>();
        }
        public int Id { get; set; }
        [EmailAddress]
        public string EmailAddress { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string ProfileImage { get; set; }
        [Required]
        [DisplayName("Phone number")]
        [DataType(DataType.PhoneNumber)]
        public string PhoneNumber { get; set; }
        [NotMapped]
        [DisplayName("Upload Profile Image")]
        public IFormFile? ImageFile { get; set; }
        public virtual ICollection<Project> Projects { get; set; }
        public virtual ICollection<Issue> Issues { get; set; }
    }


}
