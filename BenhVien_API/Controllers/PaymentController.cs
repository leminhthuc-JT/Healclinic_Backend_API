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
    public class PaymentController : ControllerBase
    {

        //Chưa fix
        private readonly QlBenhvienContext _con;

        public PaymentController(QlBenhvienContext con)
        {
            _con = con;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayments()
        {
            var payments = await _con.Payments
                .Include(p => p.Patient)
                .Include(p => p.Paymentappointment)
                .Include(p => p.Paymentprescription)
                .Include(p => p.Paymentservices)
                .ToListAsync();
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaymentById(int id)
        {
            var payment = await _con.Payments
                .Include(p => p.Patient)
                .Include(p => p.Paymentappointment)
                .Include(p => p.Paymentprescription)
                .Include(p => p.Paymentservices)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (payment == null) return NotFound(new { message = "Payment not found!" });
            return Ok(payment);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePayment([FromBody] Payment payment)
        {
            if (payment == null) return BadRequest();

            payment.CreatedAt ??= DateTime.Now;

            // If client supplied child payment services/prescription/appointment, let EF insert them together.
            await _con.Payments.AddAsync(payment);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Payment created successfully!", data = payment });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePayment(int id, [FromBody] Payment payment)
        {
            if (payment == null || id != payment.PaymentId)
                return BadRequest(new { message = "Payment id not found or payload is invalid!" });

            var existing = await _con.Payments
                .Include(p => p.Paymentservices)
                .FirstOrDefaultAsync(p => p.PaymentId == id);

            if (existing == null) return NotFound(new { message = "Not found payment!" });

            existing.PatientId = payment.PatientId;
            existing.PaymentType = payment.PaymentType;
            existing.TotalAmount = payment.TotalAmount;
            existing.UpdateAt = DateTime.Now;
            existing.CreatedAt = payment.CreatedAt ?? existing.CreatedAt;

            // Optional: replace Paymentservices if client provided a list
            if (payment.Paymentservices != null && payment.Paymentservices.Any())
            {
                // remove old ones and add new ones
                _con.Paymentservices.RemoveRange(existing.Paymentservices);
                existing.Paymentservices = payment.Paymentservices;
            }

            _con.Payments.Update(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Payment updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePayment(int id)
        {
            var existing = await _con.Payments.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Not found payment!" });

            _con.Payments.Remove(existing);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Payment deleted!" });
        }

        [HttpGet("paging")]
        public async Task<IActionResult> GetPaymentsWithPaging(int pageNumber = 1, int pageSize = 10)
        {
            var payments = await _con.Payments
                .Include(p => p.Patient)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(payments);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPayments(string search)
        {
            if (string.IsNullOrEmpty(search)) return await GetAllPayments();

            var results = await _con.Payments
                .Include(p => p.Patient)
                .Where(p =>
                    (p.PaymentType != null && p.PaymentType.Contains(search)) ||
                    (p.Patient != null && EF.Functions.Like(p.Patient.FullName, $"%{search}%")) ||
                    (p.TotalAmount != null && p.TotalAmount.ToString().Contains(search)))
                .ToListAsync();

            return Ok(results);
        }

    }
}
