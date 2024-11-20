using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    public class ProjectController : Controller
    {
        private readonly IConfiguration _config;

        public ProjectController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
            ViewBag.Username = HttpContext.Session.GetString("Username");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Auth");

            var projects = GetProjects(userId.Value);
            return View(projects);
        }

        [HttpPost]
        public IActionResult Create(string title)
        {
            ExecuteStoredProcedure("InsertProject", new SqlParameter[]
            {
                new SqlParameter("@Title", title),
                new SqlParameter("@UserId", HttpContext.Session.GetInt32("UserId"))
            });

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(int projectId, string title)
        {
            ExecuteStoredProcedure("UpdateProject", new SqlParameter[]
            {
                new SqlParameter("@ProjectId", projectId),
                new SqlParameter("@Title", title)
            });

            return RedirectToAction("Index");
        }

        public IActionResult Delete(int id)
        {
            ExecuteStoredProcedure("DeleteProject", new SqlParameter[]
            {
                new SqlParameter("@ProjectId", id)
            });

            return RedirectToAction("Index");
        }

        private IEnumerable<Project> GetProjects(int userId)
        {
            var projects = new List<Project>();
            var connectionString = _config.GetConnectionString("DefaultConnection");

            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand("GetProjectsByUserId", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@UserId", userId);

            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                projects.Add(new Project
                {
                    ProjectId = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    CreatedDate = reader.GetDateTime(2)
                });
            }

            return projects;
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
    }
}
