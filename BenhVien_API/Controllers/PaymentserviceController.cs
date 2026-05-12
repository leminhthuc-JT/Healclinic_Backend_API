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
    public class PaymentserviceController : ControllerBase
    {

        //Chưa fix
        private readonly QlBenhvienContext _con;

        public PaymentserviceController(QlBenhvienContext con)
        {
            _con = con;
        }

        [HttpGet]
        //public async Task<IActionResult> GetAllPaymentservices()
        //{
        //    var items = await _con.Paymentservices
        //        .Include(ps => ps.Payment)
        //            .ThenInclude(p => p.Patient)
        //        .Include(ps => ps.Service)
        //        .ToListAsync();

        //    return Ok(items);
        //}

        [HttpGet("{paymentId:int}/{serviceId}")]
        public async Task<IActionResult> GetPaymentservice(int paymentId, string serviceId)
        {
            var item = await _con.Paymentservices
                .Include(ps => ps.Payment)
                    .ThenInclude(p => p.Patient)
                .Include(ps => ps.Service)
                .FirstOrDefaultAsync(ps => ps.PaymentId == paymentId && ps.ServiceId == serviceId);

            if (item == null) return NotFound(new { message = "Payment-Service link not found!" });
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentservice([FromBody] Paymentservice model)
        {
            if (model == null) return BadRequest();

            // Validate referenced entities
            var paymentExists = await _con.Payments.AnyAsync(p => p.PaymentId == model.PaymentId);
            var serviceExists = await _con.Services.AnyAsync(s => s.ServiceId == model.ServiceId);
            if (!paymentExists || !serviceExists)
                return BadRequest(new { message = "Referenced Payment or Service does not exist." });

            // Prevent duplicate composite key
            var existing = await _con.Paymentservices.FindAsync(model.PaymentId, model.ServiceId);
            if (existing != null) return Conflict(new { message = "This payment-service link already exists." });

            await _con.Paymentservices.AddAsync(model);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Payment-Service created successfully!", data = model });
        }

        [HttpPut("{paymentId:int}/{serviceId}")]
        public async Task<IActionResult> UpdatePaymentservice(int paymentId, string serviceId, [FromBody] Paymentservice model)
        {
            if (model == null || paymentId != model.PaymentId || serviceId != model.ServiceId)
                return BadRequest(new { message = "Payload invalid or keys do not match." });

            var existing = await _con.Paymentservices
                .FirstOrDefaultAsync(ps => ps.PaymentId == paymentId && ps.ServiceId == serviceId);

            if (existing == null) return NotFound(new { message = "Not found payment-service!" });

            // Update mutable fields
            existing.Quantity = model.Quantity;

            _con.Paymentservices.Update(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Payment-Service updated successfully!" });
        }

        [HttpDelete("{paymentId:int}/{serviceId}")]
        public async Task<IActionResult> DeletePaymentservice(int paymentId, string serviceId)
        {
            var existing = await _con.Paymentservices
                .FirstOrDefaultAsync(ps => ps.PaymentId == paymentId && ps.ServiceId == serviceId);

            if (existing == null) return NotFound(new { message = "Not found payment-service!" });

            _con.Paymentservices.Remove(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Payment-Service deleted!" });
        }

        //[HttpGet("paging")]
        //public async Task<IActionResult> GetPaymentservicesWithPaging(int pageNumber = 1, int pageSize = 10)
        //{
        //    var items = await _con.Paymentservices
        //        .Include(ps => ps.Payment)
        //            .ThenInclude(p => p.Patient)
        //        .Include(ps => ps.Service)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    return Ok(items);
        //}

        //[HttpGet("search")]
        //public async Task<IActionResult> SearchPaymentservices(string? search)
        //{
        //    if (string.IsNullOrEmpty(search)) return await GetAllPaymentservices();

        //    var results = await _con.Paymentservices
        //        .Include(ps => ps.Payment)
        //            .ThenInclude(p => p.Patient)
        //        .Include(ps => ps.Service)
        //        .Where(ps =>
        //            ps.PaymentId.ToString().Contains(search) ||
        //            (ps.ServiceId != null && ps.ServiceId.Contains(search)) ||
        //            (ps.Service != null && ps.Service.ServiceName != null && ps.Service.ServiceName.Contains(search)) ||
        //            (ps.Payment != null && ps.Payment.PaymentType != null && ps.Payment.PaymentType.Contains(search)) ||
        //            (ps.Payment != null && ps.Payment.Patient != null && ps.Payment.Patient.FullName.Contains(search)))
        //        .ToListAsync();

        //    return Ok(results);
        //}




    }
}
