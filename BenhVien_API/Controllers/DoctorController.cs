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
    public class DoctorController : ControllerBase
    {
        private readonly QlBenhvienContext _con;

        public DoctorController(QlBenhvienContext con)
        {
            _con = con;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _con.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Include(d => d.Appointments)
                .ToListAsync();
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            if (id == null) return BadRequest();

            var doctor = await _con.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .Include(d => d.Appointments)
                .FirstOrDefaultAsync(d => d.DoctorId == id);
            if (doctor == null) return NotFound(new { message = "Doctor not found!" });

            return Ok(doctor);

        }

        [HttpPost]
        public async Task<IActionResult> createDoctor([FromBody] Doctor doctor)
        {
            if (doctor == null) return BadRequest();

            _con.Doctors.Add(doctor);
            await _con.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDoctorById), new { id = doctor.DoctorId }, doctor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> updateDoctor(int id, [FromBody] Doctor doctor)
        {
            if (id != doctor.DoctorId) return BadRequest(new { message = "Doctor code not found!" });

            var existing = await _con.Doctors.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Doctor not found!" });

            existing.FullName = doctor.FullName;
            existing.Specialty = doctor.Specialty;
            existing.ExperienceYears = doctor.ExperienceYears;
            existing.Bio = doctor.Bio;
            existing.AvatarUrl = doctor.AvatarUrl;
            existing.WorkStartTime = doctor.WorkStartTime;
            existing.WorkEndTime = doctor.WorkEndTime;

            _con.Doctors.Update(existing);
            await _con.SaveChangesAsync();
            return Ok(existing);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> deleteDoctor(int id)
        {
            var doctor = await _con.Doctors.FindAsync(id);
            if (doctor == null) return NotFound(new { message = "Doctor not found!" });

            _con.Doctors.Remove(doctor);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Doctor deleted successfully!" });
        }


        [HttpGet("search")]
        public async Task<IActionResult> searchDoctor(string search)
        {
            if (string.IsNullOrEmpty(search)) return await GetAllDoctors();

            var doctor = await _con.Doctors.Where(x => x.FullName.Contains(search)).ToListAsync();
            return Ok(doctor);


        }


        [HttpGet("paging")]
        public async Task<IActionResult> GetDoctorWithBtPaging(int pagpNumber = 1, int pageSize = 10)
        {
            var doctors = await _con.Doctors.Skip((pagpNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(doctors);
        }


        //Còn thiếu lọc theo phòng ban
        //Sắp xếp theo tên, kinh nghiệm



    }
}
