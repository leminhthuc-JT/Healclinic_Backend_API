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
    public class PrescriptionController : ControllerBase
    {

        //Chưa fix
        private readonly QlBenhvienContext _con;

        public PrescriptionController(QlBenhvienContext con)
        {
            _con = con;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPrescriptions()
        {
            var items = await _con.Prescriptions
                .Include(p => p.Record)
                .Include(p => p.Prescriptiondetails)
                    .ThenInclude(d => d.Medication)
                .Include(p => p.Paymentprescriptions)
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPrescriptionById(int id)
        {
            var item = await _con.Prescriptions
                .Include(p => p.Record)
                .Include(p => p.Prescriptiondetails)
                    .ThenInclude(d => d.Medication)
                .Include(p => p.Paymentprescriptions)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (item == null) return NotFound(new { message = "Prescription not found!" });
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePrescription([FromBody] Prescription model)
        {
            if (model == null) return BadRequest();

            model.CreatedAt ??= DateTime.Now;

            // Ensure referenced medical record exists
            var recordExists = await _con.MedicalRecords.AnyAsync(r => r.RecordId == model.RecordId);
            if (!recordExists) return BadRequest(new { message = "Referenced medical record does not exist." });

            await _con.Prescriptions.AddAsync(model);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Prescription created successfully!", data = model });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePrescription(int id, [FromBody] Prescription model)
        {
            if (model == null || id != model.PrescriptionId)
                return BadRequest(new { message = "Prescription id not found or payload is invalid!" });

            var existing = await _con.Prescriptions
                .Include(p => p.Prescriptiondetails)
                .FirstOrDefaultAsync(p => p.PrescriptionId == id);

            if (existing == null) return NotFound(new { message = "Not found prescription!" });

            existing.RecordId = model.RecordId;
            existing.Note = model.Note;
            existing.CreatedAt = model.CreatedAt ?? existing.CreatedAt;

            // If client provided details, replace them
            if (model.Prescriptiondetails != null && model.Prescriptiondetails.Any())
            {
                _con.Prescriptiondetails.RemoveRange(existing.Prescriptiondetails);
                existing.Prescriptiondetails = model.Prescriptiondetails;
            }

            _con.Prescriptions.Update(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Prescription updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var existing = await _con.Prescriptions.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Not found prescription!" });

            _con.Prescriptions.Remove(existing);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Prescription deleted!" });
        }

        //[HttpGet("paging")]
        //public async Task<IActionResult> GetPrescriptionsWithPaging(int pageNumber = 1, int pageSize = 10)
        //{
        //    var items = await _con.Prescriptions
        //        .Include(p => p.Record)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    return Ok(items);
        //}

        //[HttpGet("search")]
        //public async Task<IActionResult> SearchPrescriptions(string search)
        //{
        //    if (string.IsNullOrEmpty(search)) return await GetAllPrescriptions();

        //    var results = await _con.Prescriptions
        //        .Include(p => p.Record)
        //        .Include(p => p.Prescriptiondetails)
        //            .ThenInclude(d => d.Medication)
        //        .Where(p =>
        //            (p.Note != null && p.Note.Contains(search)) ||
        //            (p.Record != null && p.Record.Diagnosis != null && p.Record.Diagnosis.Contains(search)) ||
        //            p.Prescriptiondetails.Any(d => d.Medication != null && d.Medication.MedicineName.Contains(search)))
        //        .ToListAsync();

        //    return Ok(results);
        //}

    }
}
