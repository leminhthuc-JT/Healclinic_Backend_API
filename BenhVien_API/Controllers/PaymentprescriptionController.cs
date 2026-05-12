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
    public class PaymentprescriptionController : ControllerBase
    {

        //Chưa fix
        private readonly QlBenhvienContext _con;

        public PaymentprescriptionController(QlBenhvienContext con)
        {
            _con = con;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllPaymentprescriptions()
        //{
        //    var items = await _con.Paymentprescriptions
        //        .Include(pp => pp.Payment)
        //        .Include(pp => pp.Prescription)
        //        .ToListAsync();
        //    return Ok(items);
        //}

        [HttpGet("{paymentId:int}/{prescriptionId:int}")]
        public async Task<IActionResult> GetPaymentprescription(int paymentId, int prescriptionId)
        {
            var item = await _con.Paymentprescriptions
                .Include(pp => pp.Payment)
                .Include(pp => pp.Prescription)
                .FirstOrDefaultAsync(pp => pp.PaymentId == paymentId && pp.PrescriptionId == prescriptionId);

            if (item == null) return NotFound(new { message = "Payment-Prescription not found!" });
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentprescription([FromBody] Paymentprescription model)
        {
            if (model == null) return BadRequest();

            var paymentExists = await _con.Payments.AnyAsync(p => p.PaymentId == model.PaymentId);
            var prescriptionExists = await _con.Prescriptions.AnyAsync(p => p.PrescriptionId == model.PrescriptionId);
            if (!paymentExists || !prescriptionExists)
                return BadRequest(new { message = "Referenced Payment or Prescription does not exist." });

            var existing = await _con.Paymentprescriptions.FindAsync(model.PaymentId, model.PrescriptionId);
            if (existing != null) return Conflict(new { message = "This payment-prescription link already exists." });

            await _con.Paymentprescriptions.AddAsync(model);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Payment-Prescription created successfully!", data = model });
        }

        [HttpPut("{paymentId:int}/{prescriptionId:int}")]
        public async Task<IActionResult> UpdatePaymentprescription(int paymentId, int prescriptionId, [FromBody] Paymentprescription model)
        {
            if (model == null || paymentId != model.PaymentId || prescriptionId != model.PrescriptionId)
                return BadRequest(new { message = "Payload invalid or keys do not match." });

            var existing = await _con.Paymentprescriptions
                .FirstOrDefaultAsync(pp => pp.PaymentId == paymentId && pp.PrescriptionId == prescriptionId);

            if (existing == null) return NotFound(new { message = "Not found payment-prescription!" });

            // No additional fields; keep keys consistent (allows re-linking if desired)
            existing.PaymentId = model.PaymentId;
            existing.PrescriptionId = model.PrescriptionId;

            _con.Paymentprescriptions.Update(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Payment-Prescription updated successfully!" });
        }

        [HttpDelete("{paymentId:int}/{prescriptionId:int}")]
        public async Task<IActionResult> DeletePaymentprescription(int paymentId, int prescriptionId)
        {
            var existing = await _con.Paymentprescriptions
                .FirstOrDefaultAsync(pp => pp.PaymentId == paymentId && pp.PrescriptionId == prescriptionId);

            if (existing == null) return NotFound(new { message = "Not found payment-prescription!" });

            _con.Paymentprescriptions.Remove(existing);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Payment-Prescription deleted!" });
        }

        //[HttpGet("paging")]
        //public async Task<IActionResult> GetPaymentprescriptionsWithPaging(int pageNumber = 1, int pageSize = 10)
        //{
        //    var items = await _con.Paymentprescriptions
        //        .Include(pp => pp.Payment)
        //        .Include(pp => pp.Prescription)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    return Ok(items);
        //}

        //[HttpGet("search")]
        //public async Task<IActionResult> SearchPaymentprescriptions(string? search)
        //{
        //    if (string.IsNullOrEmpty(search)) return await GetAllPaymentprescriptions();

        //    var results = await _con.Paymentprescriptions
        //        .Include(pp => pp.Payment)
        //            .ThenInclude(p => p.Patient)
        //        .Include(pp => pp.Prescription)
        //        .Where(pp =>
        //            pp.PaymentId.ToString().Contains(search) ||
        //            pp.PrescriptionId.ToString().Contains(search) ||
        //            (pp.Payment != null && pp.Payment.PaymentType != null && pp.Payment.PaymentType.Contains(search)) ||
        //            (pp.Payment != null && pp.Payment.Patient != null && pp.Payment.Patient.FullName.Contains(search)))
        //        .ToListAsync();

        //    return Ok(results);
        //}

    }
}
