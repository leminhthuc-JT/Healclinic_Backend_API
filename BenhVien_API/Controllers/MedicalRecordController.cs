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
    public class MedicalRecordController : ControllerBase
    {

        //Chưa fix

        private readonly QlBenhvienContext _con;

        public MedicalRecordController(QlBenhvienContext con)
        {
            _con = con;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMedicalRecords()
        {
            var records = await _con.MedicalRecords
                .Include(r => r.Appointment)
                .Include(r => r.Prescriptions)
                .ToListAsync();
            return Ok(records);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedicalRecordById(int id)
        {
            var record = await _con.MedicalRecords
                .Include(r => r.Appointment)
                .Include(r => r.Prescriptions)
                .FirstOrDefaultAsync(r => r.RecordId == id);

            if (record == null)
                return NotFound(new { message = "Medical record not found!" });

            return Ok(record);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMedicalRecord([FromBody] MedicalRecord record)
        {
            if (record == null)
                return BadRequest();

            // Ensure CreatedAt is set if not provided
            record.CreatedAt ??= DateTime.Now;

            await _con.MedicalRecords.AddAsync(record);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Medical record created successfully!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedicalRecord(int id, [FromBody] MedicalRecord record)
        {
            if (record == null || id != record.RecordId)
                return BadRequest(new { message = "Record id not found or payload is invalid!" });

            var existing = await _con.MedicalRecords.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Not found medical record!" });

            existing.AppointmentId = record.AppointmentId;
            existing.Diagnosis = record.Diagnosis;
            existing.Symptoms = record.Symptoms;
            existing.Treatment = record.Treatment;
            existing.IsCover = record.IsCover;
            existing.PercentCover = record.PercentCover;
            existing.CreatedAt = record.CreatedAt ?? existing.CreatedAt;

            _con.MedicalRecords.Update(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Medical record updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedicalRecord(int id)
        {
            var existing = await _con.MedicalRecords.FindAsync(id);
            if (existing == null)
                return NotFound(new { message = "Not found medical record!" });

            _con.MedicalRecords.Remove(existing);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Medical record deleted!" });
        }


    }
}
