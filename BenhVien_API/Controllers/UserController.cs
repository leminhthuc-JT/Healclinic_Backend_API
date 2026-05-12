using BenhVien_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace ConnectionStringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly QlBenhvienContext _con;

        public UserController(QlBenhvienContext con)
        {
            _con = con;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _con.Users
                .Include(x => x.Patients)
                .Include(x => x.Doctors)
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsersById(int id)
        {
            var user = await _con.Users
                .Include(x => x.Patients)
                .Include(x => x.Doctors)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound(new { message = "User not found!" });

            }
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user, string fullName)
        {
            if (user == null || string.IsNullOrEmpty(fullName))
                return BadRequest();

            _con.Users.Add(user);

            if (user.Role == "P")
            {
                var createPatient = new Patient();
                createPatient.FullName = fullName;
                createPatient.UserId = user.UserId;
                _con.Patients.Add(createPatient);
            }
            else if(user.Role == "D")
            {
                var createDoctor = new Doctor();
                createDoctor.FullName = fullName;
                createDoctor.UserId = user.UserId;
                _con.Doctors.Add(createDoctor);
            }

            await _con.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUsersById), new { id = user.UserId }, user);

        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] User user)
        {
            if (id != user.UserId) return BadRequest(new { message = "Acount code not found" });

            var exitingUser = await _con.Users
                .Include(x => x.Patients)
                .Include(x => x.Doctors)
                .FirstOrDefaultAsync(u => u.UserId == id);
            if (exitingUser == null)
            {
                return NotFound(new { message = "User not found!" });
            }

            exitingUser.Email = user.Email;
            exitingUser.PassWord = user.PassWord;
            exitingUser.Phone = user.Phone;
            exitingUser.Role = user.Role;

            await _con.SaveChangesAsync();
            return Ok(exitingUser);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _con.Users.FindAsync(id);

            if (user == null) return NotFound(new { message = "User not found!" });

            _con.Users.Remove(user);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Đã xóa tài khoản thành công!" });
        }


        // Search user by email or full name
        [HttpGet("search")]
        public async Task<IActionResult> SearchEmail(string search)
        {
            if (string.IsNullOrEmpty(search)) return await GetAllUsers();
            var user = await _con.Users.Where(x => x.Email.Contains(search)).ToListAsync();
            return Ok(user);
        }

        //Paging
        [HttpGet("paging")]
        public async Task<IActionResult> GetUsersWithPaging(int pageNumber = 1, int pageSize = 10)
        {
            var users = await _con.Users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(users);

        }

        //Login
        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _con.Users.FirstOrDefaultAsync(u => u.Email == email && u.PassWord == password);
            if (user == null)
            {
                return Unauthorized(new { message = "Email hoặc mật khẩu không chính xác!" });
            }
            return Ok(new { message = "Đăng nhập thành công!", user });
        }

        //Change password

        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(string email, string currentPassword, string newPassword)
        {
            var user = await _con.Users.FirstOrDefaultAsync(u => u.Email == email && u.PassWord == currentPassword);
            if (user == null)
            {
                return Unauthorized(new { message = "Tài khoản hoặc mật kahaur không chính xác!" });
            }
            user.PassWord = newPassword;
            await _con.SaveChangesAsync();
            return Ok(new { message = "Đổi mật khẩu thành công!" });
        }

        //Thiếu các đăng ký đăng nhập bằng xác thực ,..
        //Phân loại role


    }
}
