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
    public class PrescriptiondetailController : ControllerBase
    {

        //Chưa fix
        private readonly QlBenhvienContext _con;

        public PrescriptiondetailController(QlBenhvienContext con)
        {
            _con = con;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllPrescriptiondetails()
        //{
        //    var items = await _con.Prescriptiondetails
        //        .Include(d => d.Medication)
        //        .Include(d => d.Prescription)
        //            .ThenInclude(p => p.Record)
        //        .ToListAsync();

        //    return Ok(items);
        //}

        [HttpGet("{prescriptionId:int}/{medicationId}")]
        public async Task<IActionResult> GetPrescriptiondetail(int prescriptionId, string medicationId)
        {
            var item = await _con.Prescriptiondetails
                .Include(d => d.Medication)
                .Include(d => d.Prescription)
                .FirstOrDefaultAsync(d => d.PrescriptionId == prescriptionId && d.MedicationId == medicationId);

            if (item == null) return NotFound(new { message = "Prescription detail not found!" });
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePrescriptiondetail([FromBody] Prescriptiondetail model)
        {
            if (model == null) return BadRequest();

            // Validate referenced entities exist
            var presExists = await _con.Prescriptions.AnyAsync(p => p.PrescriptionId == model.PrescriptionId);
            var medExists = await _con.Medications.AnyAsync(m => m.MedicationId == model.MedicationId);
            if (!presExists || !medExists)
                return BadRequest(new { message = "Referenced Prescription or Medication does not exist." });

            // Prevent duplicate composite key
            var existing = await _con.Prescriptiondetails.FindAsync(model.PrescriptionId, model.MedicationId);
            if (existing != null) return Conflict(new { message = "This prescription detail already exists." });

            await _con.Prescriptiondetails.AddAsync(model);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Prescription detail created successfully!", data = model });
        }

        [HttpPut("{prescriptionId:int}/{medicationId}")]
        public async Task<IActionResult> UpdatePrescriptiondetail(int prescriptionId, string medicationId, [FromBody] Prescriptiondetail model)
        {
            if (model == null || prescriptionId != model.PrescriptionId || medicationId != model.MedicationId)
                return BadRequest(new { message = "Payload invalid or keys do not match." });

            var existing = await _con.Prescriptiondetails
                .FirstOrDefaultAsync(d => d.PrescriptionId == prescriptionId && d.MedicationId == medicationId);

            if (existing == null) return NotFound(new { message = "Not found prescription detail!" });

            existing.Duration = model.Duration;
            existing.Quantity = model.Quantity;
            existing.Note = model.Note;

            _con.Prescriptiondetails.Update(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Prescription detail updated successfully!" });
        }

        [HttpDelete("{prescriptionId:int}/{medicationId}")]
        public async Task<IActionResult> DeletePrescriptiondetail(int prescriptionId, string medicationId)
        {
            var existing = await _con.Prescriptiondetails
                .FirstOrDefaultAsync(d => d.PrescriptionId == prescriptionId && d.MedicationId == medicationId);

            if (existing == null) return NotFound(new { message = "Not found prescription detail!" });

            _con.Prescriptiondetails.Remove(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Prescription detail deleted!" });
        }

        //[HttpGet("paging")]
        //public async Task<IActionResult> GetPrescriptiondetailsWithPaging(int pageNumber = 1, int pageSize = 10)
        //{
        //    var items = await _con.Prescriptiondetails
        //        .Include(d => d.Medication)
        //        .Include(d => d.Prescription)
        //            .ThenInclude(p => p.Record)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    return Ok(items);
        //}

        //[HttpGet("search")]
        //public async Task<IActionResult> SearchPrescriptiondetails(string? search)
        //{
        //    if (string.IsNullOrEmpty(search)) return await GetAllPrescriptiondetails();

        //    var results = await _con.Prescriptiondetails
        //        .Include(d => d.Medication)
        //        .Include(d => d.Prescription)
        //            .ThenInclude(p => p.Record)
        //        .Where(d =>
        //            (d.Medication != null && d.Medication.MedicineName != null && d.Medication.MedicineName.Contains(search)) ||
        //            (d.Note != null && d.Note.Contains(search)) ||
        //            (d.Prescription != null && d.Prescription.Note != null && d.Prescription.Note.Contains(search)) ||
        //            (d.Medication != null && d.Medication.Manufacturer != null && d.Medication.Manufacturer.Contains(search)))
        //        .ToListAsync();

        //    return Ok(results);
        //}

    }
}
