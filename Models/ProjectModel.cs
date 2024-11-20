namespace TodoApp.Models
{
    public class Project
    {
        public int ProjectId { get; set; }
        public string Title { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UserId { get; set; }
    }

}
