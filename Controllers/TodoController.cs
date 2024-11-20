using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    public class TodoController : Controller
    {
        private readonly IConfiguration _config;

        public TodoController(IConfiguration config)
        {
            _config = config;
        }

        [Route("Todo/Details/{projectId}")]
        public IActionResult Details(int projectId)
        {
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var project = GetProjectById(projectId);
            if (project == null || project.UserId != userId.Value)
            {
                ViewBag.Error = "Unauthorized access.";
                return RedirectToAction("Index", "Project");
            }

            var todos = GetTodosByProject(projectId);
            ViewBag.ProjectName = project.Title;
            ViewBag.ProjectId = projectId;
            return View(todos);
        }

        [HttpPost]
        public IActionResult Create(int projectId, string description)
        {
            ExecuteStoredProcedure("InsertTodo", new SqlParameter[]
            {
                new SqlParameter("@ProjectId", projectId),
                new SqlParameter("@Description", description)
            });

            return RedirectToAction("Details", new { projectId });
        }

        [HttpPost]
        public IActionResult Update(Todo todo)
        {
            ExecuteStoredProcedure("UpdateTodo", new SqlParameter[]
            {
                new SqlParameter("@TodoId", todo.TodoId),
                new SqlParameter("@Description", todo.Description),
                new SqlParameter("@Status", todo.Status)
            });

            return RedirectToAction("Details", new { projectId = todo.ProjectId });
        }

        public IActionResult Delete(int id)
        {
            var todo = GetTodoById(id);
            if (todo == null)
            {
                ViewBag.Error = "Todo not found.";
                return RedirectToAction("Index", "Project");
            }

            ExecuteStoredProcedure("DeleteTodo", new SqlParameter[]
            {
                new SqlParameter("@TodoId", id)
            });

            return RedirectToAction("Details", new { projectId = todo.ProjectId });
        }

        private IEnumerable<Todo> GetTodosByProject(int projectId)
        {
            var todos = new List<Todo>();
            var connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand("GetTodosByProject", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ProjectId", projectId);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                todos.Add(new Todo
                {
                    TodoId = reader.GetInt32(0),
                    Description = reader.GetString(1),
                    Status = reader.GetString(2)
                });
            }

            return todos;
        }
        private Project? GetProjectById(int projectId)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand("GetProjectById", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@ProjectId", projectId);

            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read()
                ? new Project
                {
                    ProjectId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    CreatedDate = reader.GetDateTime(2),
                    UserId = reader.GetInt32(3)
                }
                : null;
        }

        [HttpPost]
        public IActionResult ToggleStatus(int todoId)
        {
            var todo = GetTodoById(todoId);
            if (todo == null)
            {
                ViewBag.Error = "Todo not found.";
                return RedirectToAction("Index", "Project");
            }

            todo.Status = todo.Status == "Pending" ? "Completed" : "Pending";

            ExecuteStoredProcedure("UpdateTodo", new SqlParameter[]
            {
                new SqlParameter("@TodoId", todo.TodoId),
                new SqlParameter("@Description", todo.Description),
                new SqlParameter("@Status", todo.Status)
            });

            return RedirectToAction("Details", new { projectId = todo.ProjectId });
        }

        private Todo? GetTodoById(int todoId)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand("GetTodoById", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@TodoId", todoId);

            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read()
                ? new Todo
                {
                    TodoId = reader.GetInt32(0),
                    Description = reader.GetString(1),
                    Status = reader.GetString(2),
                    ProjectId = reader.GetInt32(3)
                }
                : null;
        }

        private void ExecuteStoredProcedure(string procedureName, SqlParameter[] parameters)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand(procedureName, connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.AddRange(parameters);
            connection.Open();
            command.ExecuteNonQuery();
        }

        public IActionResult ExportToGist(int projectId)
        {
            var project = GetProjectById(projectId);
            if (project == null)
            {
                ViewBag.Error = "Project not found.";
                return RedirectToAction("Index", "Project");
            }

            var todos = GetTodosByProject(projectId);
            string markdownContent = GenerateMarkdownContent(project, todos);

            string gistUrl = UploadGistToGitHub($"{project.Title}.md", markdownContent);

            if (string.IsNullOrEmpty(gistUrl))
            {
                ViewBag.Error = "Failed to create Gist.";
                return RedirectToAction("Details", new { projectId });
            }

            SaveMarkdownLocally($"{project.Title}.md", markdownContent);

            TempData["Message"] = "Gist created successfully!";
            TempData["GistUrl"] = gistUrl;
            return RedirectToAction("Details", new { projectId });
        }

        private string GenerateMarkdownContent(Project project, IEnumerable<Todo> todos)
        {
            var completedTodos = todos.Where(todo => todo.Status == "Completed").ToList();
            var pendingTodos = todos.Where(todo => todo.Status == "Pending").ToList();

            var content = new StringBuilder();
            content.AppendLine($"# {project.Title}");
            content.AppendLine();
            content.AppendLine($"**Summary:** {completedTodos.Count} / {todos.Count()} completed.");
            content.AppendLine();
            content.AppendLine("## Pending Todos");
            foreach (var todo in pendingTodos)
            {
                content.AppendLine($"- [ ] {todo.Description}");
            }
            content.AppendLine();
            content.AppendLine("## Completed Todos");
            foreach (var todo in completedTodos)
            {
                content.AppendLine($"- [x] {todo.Description}");
            }

            return content.ToString();
        }

        private string UploadGistToGitHub(string fileName, string content)
        {
            var gist = new
            {
                description = "Project Summary",
                @public = false,
                files = new Dictionary<string, object>
                {
                    { fileName, new { content } }
                }
            };

            var json = JsonConvert.SerializeObject(gist);
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "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");
            client.DefaultRequestHeaders.Add("User-Agent", "TodoApp");

            var response = client.PostAsync("https://api.github.com/gists",
                new StringContent(json, Encoding.UTF8, "application/json")).Result;

            if (response.IsSuccessStatusCode)
            {
                var responseBody = response.Content.ReadAsStringAsync().Result;
                dynamic result = JsonConvert.DeserializeObject(responseBody);
                return result.html_url;
            }

            Console.WriteLine(response.Content.ReadAsStringAsync().Result);

            return null;
        }

        private void SaveMarkdownLocally(string fileName, string content)
        {
            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var filePath = Path.Combine(downloadsPath, fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            System.IO.File.WriteAllText(filePath, content);
        }


    }
}
