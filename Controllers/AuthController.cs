using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult LandingPage()
        {
            ViewBag.IsLoggedIn = HttpContext.Session.GetInt32("UserId") != null;
            return View();
        }

        public IActionResult Login() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "Please enter username and password.";
                return View();
            }

            var user = GetUser(username, password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                return RedirectToAction("Index", "Project");
            }

            ViewBag.Error = "Invalid username or password.";
            return View();
        }

        public IActionResult Signup() => View();

        [HttpPost]
        public IActionResult Signup(string username, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "All fields are required.";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Passwords do not match.";
                return View();
            }

            if (RegisterUser(username, password))
            {
                return RedirectToAction("Login");
            }
            else
            {
                ViewBag.Error = "User already exists!";
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("LandingPage");
        }

        private User? GetUser(string username, string password)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand("ValidateUser", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);

            connection.Open();
            using var reader = command.ExecuteReader();
            return reader.Read() ? new User { UserId = reader.GetInt32(0), Username = reader.GetString(1) } : null;
        }

        private bool RegisterUser(string username, string password)
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");
            using var connection = new SqlConnection(connectionString);
            var command = new SqlCommand("RegisterUser", connection)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", password);
            SqlParameter successParam = command.Parameters.Add("@Success", SqlDbType.Bit);
            successParam.Direction = ParameterDirection.Output;

            connection.Open();
            command.ExecuteNonQuery();

            return (bool)successParam.Value;
        }
    }
}
