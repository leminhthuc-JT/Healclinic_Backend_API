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
    public class PatientController : ControllerBase
    {

        private readonly QlBenhvienContext _con;

        public PatientController(QlBenhvienContext con)
        {
            _con = con;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _con.Patients
                .Include(x => x.User)
                .Include(x => x.Appointments)
                .Include(x => x.Chatwithdoctors)
                .Include(x => x.Payments)
                .ToListAsync();
            return Ok(patients);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = await _con.Patients
                .Include(x => x.User)
                .Include(x => x.Appointments)
                .Include(x => x.Chatwithdoctors)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.PatientId == id);
            if (patient == null)
            {
                return NotFound(new { message = "Patient not found!" });
            }
            return Ok(patient);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] Patient patient)
        {
            if (patient == null)
                return BadRequest();

            _con.Patients.Add(patient);
            await _con.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatientById), new { id = patient.PatientId }, patient);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] Patient patient)
        {
            if (id != patient.PatientId) return BadRequest(new { message = "Patient code not found!" });

            var existing = await _con.Patients.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Patient not found!" });

            existing.FullName = patient.FullName;
            existing.Phone = patient.Phone;
            existing.Address = patient.Address;
            existing.DateOfBirth = patient.DateOfBirth;
            existing.Gender = patient.Gender;
            existing.Weight = patient.Weight;
            existing.Height = patient.Height;
            existing.Allergies = patient.Allergies;
            existing.ChronicDiseases = patient.ChronicDiseases;
            existing.BloodType = patient.BloodType;

            _con.Patients.Update(existing);
            await _con.SaveChangesAsync();

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _con.Patients.FindAsync(id);
            if (patient == null) return NotFound(new { message = "Patient not found!" });

            _con.Patients.Remove(patient);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Đã xóa bệnh nhân thành công!" });
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPatients(string search)
        {
            if (string.IsNullOrEmpty(search)) return await GetAllPatients();
            var patients = await _con.Patients
                .Where(x => (x.FullName != null && x.FullName.Contains(search)))
                .ToListAsync();
            return Ok(patients);
        }

        [HttpGet("paging")]
        public async Task<IActionResult> GetPatientsWithPaging(int pageNumber = 1, int pageSize = 10)
        {
            var patients = await _con.Patients.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(patients);
        }

        //Còn thiếu phân loại theo nhóm máu, tình trạng sức khỏe
        //Sắp xếp theo tên, tuổi, nhóm máu, ...

    }
}
