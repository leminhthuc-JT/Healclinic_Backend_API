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
    public class SpecialtyController : ControllerBase
    {

        //Chưa fix

        private readonly QlBenhvienContext _con;

        public SpecialtyController(QlBenhvienContext con)
        {
            _con = con;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSpecialties()
        {
            var items = await _con.Specialties
                .Include(s => s.Appointments)
                .Include(s => s.Doctors)
                .Include(s => s.Services)
                .ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpecialtyById(string id)
        {
            var item = await _con.Specialties
                .Include(s => s.Appointments)
                .Include(s => s.Doctors)
                .Include(s => s.Services)
                .FirstOrDefaultAsync(s => s.SpecialtyId == id);

            if (item == null) return NotFound(new { message = "Specialty not found!" });
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSpecialty([FromBody] Specialty specialty)
        {
            if (specialty == null) return BadRequest();

            if (string.IsNullOrEmpty(specialty.SpecialtyId))
                specialty.SpecialtyId = Guid.NewGuid().ToString();

            await _con.Specialties.AddAsync(specialty);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Specialty created successfully!", data = specialty });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSpecialty(string id, [FromBody] Specialty specialty)
        {
            if (specialty == null || id != specialty.SpecialtyId)
                return BadRequest(new { message = "Specialty id not found or payload is invalid!" });

            var existing = await _con.Specialties.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Not found specialty!" });

            existing.SpecialtyName = specialty.SpecialtyName;
            existing.Description = specialty.Description;

            _con.Specialties.Update(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Specialty updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpecialty(string id)
        {
            var existing = await _con.Specialties.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Not found specialty!" });

            _con.Specialties.Remove(existing);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Specialty deleted!" });
        }

        [HttpGet("paging")]
        public async Task<IActionResult> GetSpecialtiesWithPaging(int pageNumber = 1, int pageSize = 10)
        {
            var items = await _con.Specialties
                .Include(s => s.Doctors)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchSpecialties(string search)
        {
            if (string.IsNullOrEmpty(search)) return await GetAllSpecialties();

            var results = await _con.Specialties
                .Include(s => s.Doctors)
                .Include(s => s.Services)
                .Where(s =>
                    (s.SpecialtyName != null && s.SpecialtyName.Contains(search)) ||
                    (s.Description != null && s.Description.Contains(search)))
                .ToListAsync();

            return Ok(results);
        }

    }
}
