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
    public class MessagedetailController : ControllerBase
    {

        //Chưa fix
        private readonly QlBenhvienContext _con;

        public MessagedetailController(QlBenhvienContext con)
        {
            _con = con;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessageById(int id)
        {

            var msg = await _con.Messagedetails
                .Include(m => m.Chat)
                .Where(c => c.ChatId == id).ToListAsync();

            if (msg == null || msg.Count == 0   ) return NotFound(new { message = "Message not found!" });
            return Ok(msg);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromBody] Messagedetail message)
        {
            if (message == null) return BadRequest();

            // set created time if missing (typical messaging behavior)
            message.CreatedAt ??= DateTime.Now;

            await _con.Messagedetails.AddAsync(message);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Message sent successfully!", data = message });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var existing = await _con.Messagedetails.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Not found message!" });

            _con.Messagedetails.Remove(existing);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Message deleted!" });
        }

        // Get a conversation between two participants (both directions)
        [HttpGet("conversation")]
        public async Task<IActionResult> GetConversation(string userA, string userB, int pageNumber = 1, int pageSize = 50)
        {
            if (string.IsNullOrEmpty(userA) || string.IsNullOrEmpty(userB))
                return BadRequest(new { message = "Both participant ids are required." });

            var query = _con.Messagedetails
                .Where(m =>
                    (m.SenderId == userA && m.ReceiverId == userB) ||
                    (m.SenderId == userB && m.ReceiverId == userA))
                .OrderBy(m => m.CreatedAt);

            var messages = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(messages);
        }

        // Get messages involving a user (inbox-like), newest first
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetMessagesForUser(string userId, int pageNumber = 1, int pageSize = 50)
        {
            if (string.IsNullOrEmpty(userId)) return BadRequest();

            var messages = await _con.Messagedetails
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderByDescending(m => m.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(messages);
        }


    }
}
