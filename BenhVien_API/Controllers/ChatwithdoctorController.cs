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
    public class ChatwithdoctorController : ControllerBase
    {

        private readonly QlBenhvienContext _con;

        public ChatwithdoctorController(QlBenhvienContext con)
        {
            _con = con;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetChatByAccount(int id)
        {
            var accountChat = await _con.Users.
                FirstOrDefaultAsync(x => x.UserId == id);

            if (accountChat == null)
                return NotFound(new { message = "User not found!" });

            var patient = await _con.Patients.Where(x => x.UserId == accountChat.UserId).FirstOrDefaultAsync();
            var docotr = await _con.Doctors.Where(x => x.UserId == accountChat.UserId).FirstOrDefaultAsync();


            var listChat = _con.Chatwithdoctors
                .Include(x => x.Patient)
                .Include(x => x.Doctor).AsQueryable();
            var chats = new List<Chatwithdoctor>();
            if (accountChat.Role == "P" && patient != null)
            {
                chats = await listChat
                .Where(x => x.PatientId == patient.PatientId)
                .ToListAsync();
            }
            else if (accountChat.Role == "D" && docotr != null)
            {
                chats = await listChat
                .Where(x => x.DoctorId == docotr.DoctorId)
                .ToListAsync();
            }
            else
                return BadRequest(new { message = "Invalid user role!" });

            return Ok(chats);
        }

        [HttpGet("search")]
        public async Task<IActionResult> searchChat(string? search, int id)
        {
            var account = await _con.Users.FindAsync(id);

            if (account == null) return NotFound(new { message = "Account not found!" });
            if (string.IsNullOrEmpty(search)) return await GetChatByAccount(id);

            var patient = await _con.Patients.Where(x => x.UserId == account.UserId).FirstOrDefaultAsync();
            var doctor = await _con.Doctors.Where(x => x.UserId == account.UserId).FirstOrDefaultAsync();

            var listChat = _con.Chatwithdoctors
                .Include(x => x.Patient)
                .Include(x => x.Doctor).AsQueryable();

            var chats = new List<Chatwithdoctor>();

            if (account.Role == "P" && patient != null)
            {
                chats = await listChat
                .Where(x => (x.Doctor != null && x.Doctor.FullName.Contains(search) && x.PatientId == patient.PatientId)).ToListAsync();
            }
            else if (account.Role == "D" && doctor != null)
            {
                chats = await listChat
               .Where(x => (x.Patient != null && x.Patient.FullName.Contains(search) && x.DoctorId == doctor.DoctorId)).ToListAsync();
            }
            else
            {
                return BadRequest(new { message = "Invalid user role!" });
            }
            return Ok(chats);

        }

        [HttpPost]
        public async Task<IActionResult> CreateChat([FromBody] Chatwithdoctor chat)
        {
            if (chat == null) return BadRequest(new { message = "Invalid chat data!" });
            await _con.Chatwithdoctors.AddAsync(chat);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Chat created successfully!", chat });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteChat(int id)
        {
            var existingChat = await _con.Chatwithdoctors.FindAsync(id);
            if (existingChat == null) return NotFound(new { message = "Not found Appointment!" });

            _con.Chatwithdoctors.Remove(existingChat);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Đã xóa đoạn chat!" });
        }


    }
}
