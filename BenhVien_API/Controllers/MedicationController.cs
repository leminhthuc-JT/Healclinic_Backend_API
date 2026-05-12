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
    public class MedicationController : ControllerBase
    {

        //Chưa fix

        private readonly QlBenhvienContext _con;

        public MedicationController(QlBenhvienContext con)
        {
            _con = con;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMedications()
        {
            var meds = await _con.Medications
                .Include(m => m.Prescriptiondetails)
                .ToListAsync();
            return Ok(meds);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMedicationById(string id)
        {
            var med = await _con.Medications
                .Include(m => m.Prescriptiondetails)
                .FirstOrDefaultAsync(m => m.MedicationId == id);

            if (med == null) return NotFound(new { message = "Medication not found!" });
            return Ok(med);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMedication([FromBody] Medication medication)
        {
            if (medication == null) return BadRequest();

            // ensure key exists
            if (string.IsNullOrEmpty(medication.MedicationId))
                medication.MedicationId = Guid.NewGuid().ToString();

            // optional defaults (keep nullable handling)
            medication.StartDate ??= DateTime.Now;

            await _con.Medications.AddAsync(medication);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Medication created successfully!" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedication(string id, [FromBody] Medication medication)
        {
            if (medication == null || id != medication.MedicationId)
                return BadRequest(new { message = "Medication id not found or payload is invalid!" });

            var existing = await _con.Medications.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Not found medication!" });

            existing.MedicineName = medication.MedicineName;
            existing.Dosage = medication.Dosage;
            existing.Frequency = medication.Frequency;
            existing.StartDate = medication.StartDate;
            existing.EndDate = medication.EndDate;
            existing.Quantity = medication.Quantity;
            existing.Unit = medication.Unit;
            existing.Country = medication.Country;
            existing.Manufacturer = medication.Manufacturer;
            existing.Price = medication.Price;
            existing.IsCover = medication.IsCover;
            existing.Note = medication.Note;

            _con.Medications.Update(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Medication updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMedication(string id)
        {
            var existing = await _con.Medications.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Not found medication!" });

            _con.Medications.Remove(existing);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Medication deleted!" });
        }

        [HttpGet("paging")]
        public async Task<IActionResult> GetMedicationsWithPaging(int pageNumber = 1, int pageSize = 10)
        {
            var meds = await _con.Medications
                .Include(m => m.Prescriptiondetails)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(meds);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchMedications(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return await GetAllMedications();
            }

            var meds = await _con.Medications
                .Include(m => m.Prescriptiondetails)
                .Where(m =>
                    (m.MedicineName != null && m.MedicineName.Contains(search)) ||
                    (m.Manufacturer != null && m.Manufacturer.Contains(search)) ||
                    (m.Country != null && m.Country.Contains(search)) ||
                    (m.Note != null && m.Note.Contains(search)))
                .ToListAsync();

            return Ok(meds);
        }

    }
}
