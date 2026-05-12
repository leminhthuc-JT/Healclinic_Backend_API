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
    public class PaymentappointmentController : ControllerBase
    {

        //Chưa fix
        private readonly QlBenhvienContext _con;

        public PaymentappointmentController(QlBenhvienContext con)
        {
            _con = con;
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllPaymentappointments()
        //{
        //    var items = await _con.Paymentappointments
        //        .Include(pa => pa.Payment)
        //        .Include(pa => pa.Appointment)
        //        .ToListAsync();
        //    return Ok(items);
        //}

        [HttpGet("{paymentId:int}/{appointmentId:int}")]
        public async Task<IActionResult> GetPaymentappointment(int paymentId, int appointmentId)
        {
            var item = await _con.Paymentappointments
                .Include(pa => pa.Payment)
                .Include(pa => pa.Appointment)
                .FirstOrDefaultAsync(pa => pa.PaymentId == paymentId && pa.AppointmentId == appointmentId);

            if (item == null) return NotFound(new { message = "Payment-Appointment not found!" });
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentappointment([FromBody] Paymentappointment model)
        {
            if (model == null) return BadRequest();

            // Optional: ensure referenced Payment and Appointment exist
            var paymentExists = await _con.Payments.AnyAsync(p => p.PaymentId == model.PaymentId);
            var appointmentExists = await _con.Appointments.AnyAsync(a => a.AppointmentId == model.AppointmentId);
            if (!paymentExists || !appointmentExists)
                return BadRequest(new { message = "Referenced Payment or Appointment does not exist." });

            // Prevent duplicate composite key
            var existing = await _con.Paymentappointments.FindAsync(model.PaymentId, model.AppointmentId);
            if (existing != null) return Conflict(new { message = "This payment-appointment link already exists." });

            await _con.Paymentappointments.AddAsync(model);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Payment-Appointment created successfully!", data = model });
        }

        [HttpPut("{paymentId:int}/{appointmentId:int}")]
        public async Task<IActionResult> UpdatePaymentappointment(int paymentId, int appointmentId, [FromBody] Paymentappointment model)
        {
            if (model == null || paymentId != model.PaymentId || appointmentId != model.AppointmentId)
                return BadRequest(new { message = "Payload invalid or keys do not match." });

            var existing = await _con.Paymentappointments
                .FirstOrDefaultAsync(pa => pa.PaymentId == paymentId && pa.AppointmentId == appointmentId);

            if (existing == null) return NotFound(new { message = "Not found payment-appointment!" });

            // No additional fields to update besides relationships, but allow re-linking if needed
            existing.PaymentId = model.PaymentId;
            existing.AppointmentId = model.AppointmentId;

            _con.Paymentappointments.Update(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Payment-Appointment updated successfully!" });
        }

        [HttpDelete("{paymentId:int}/{appointmentId:int}")]
        public async Task<IActionResult> DeletePaymentappointment(int paymentId, int appointmentId)
        {
            var existing = await _con.Paymentappointments
                .FirstOrDefaultAsync(pa => pa.PaymentId == paymentId && pa.AppointmentId == appointmentId);

            if (existing == null) return NotFound(new { message = "Not found payment-appointment!" });

            _con.Paymentappointments.Remove(existing);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Payment-Appointment deleted!" });
        }

        //[HttpGet("paging")]
        //public async Task<IActionResult> GetPaymentappointmentsWithPaging(int pageNumber = 1, int pageSize = 10)
        //{
        //    var items = await _con.Paymentappointments
        //        .Include(pa => pa.Payment)
        //        .Include(pa => pa.Appointment)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    return Ok(items);
        //}

        //[HttpGet("search")]
        //public async Task<IActionResult> SearchPaymentappointments(string? search)
        //{
        //    if (string.IsNullOrEmpty(search)) return await GetAllPaymentappointments();

        //    // Search by payment id, appointment id or patient/doctor info via appointment
        //    var results = await _con.Paymentappointments
        //        .Include(pa => pa.Payment)
        //        .Include(pa => pa.Appointment)
        //            .ThenInclude(a => a.Patient)
        //        .Include(pa => pa.Appointment)
        //            .ThenInclude(a => a.Doctor)
        //        .Where(pa =>
        //            pa.PaymentId.ToString().Contains(search) ||
        //            pa.AppointmentId.ToString().Contains(search) ||
        //            (pa.Appointment.Patient != null && pa.Appointment.Patient.FullName.Contains(search)) ||
        //            (pa.Appointment.Doctor != null && pa.Appointment.Doctor.FullName.Contains(search)))
        //        .ToListAsync();

        //    return Ok(results);
        //}

    }
}
