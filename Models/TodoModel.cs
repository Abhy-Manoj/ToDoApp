namespace TodoApp.Models
{
    public class Todo
    {
        public int TodoId { get; set; }
        public int ProjectId { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
}
