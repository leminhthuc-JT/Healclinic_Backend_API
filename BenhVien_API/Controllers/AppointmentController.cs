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
    public class AppointmentController : ControllerBase
    {
        private readonly QlBenhvienContext _con;

        public AppointmentController(QlBenhvienContext con)
        {
            _con = con;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _con.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.MedicalRecords)
                .ToListAsync();
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var appointment = await _con.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.MedicalRecords)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);
            if (appointment == null)
            {
                return NotFound(new { message = "Appointment not found!" });
            }
            return Ok(appointment);
        }


        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
        {
            if (appointment == null)
                return BadRequest();

            await _con.Database.ExecuteSqlRawAsync(
                "EXEC sp_CreateAppointment @PatientId, @DoctorId, @AppointmentDate, @SpecialtyId, @Note, @Price",
                new SqlParameter("@PatientId", appointment.PatientId),
                new SqlParameter("@DoctorId", (object?)appointment.DoctorId ?? DBNull.Value),
                new SqlParameter("@AppointmentDate", appointment.AppointmentDate),
                new SqlParameter("@TimeExpected", (object?)appointment.TimeExpected ?? DBNull.Value),
                new SqlParameter("@SpecialtyId", (object?)appointment.SpecialtyId ?? DBNull.Value),
                new SqlParameter("@Note", (object?)appointment.Note ?? DBNull.Value),
                new SqlParameter("@IsCover", (object?)appointment.IsCover ?? DBNull.Value),
                new SqlParameter("@Price", (object?)appointment.Price ?? DBNull.Value)
            );

            return Ok(new { message = "Appointment created successfully!" });
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> ApdateAppointment(int id, [FromBody] Appointment app)
        {
            if (id != app.AppointmentId) return BadRequest(new { message = "Appointment code not found!" });

            var existingApp = await _con.Appointments.FindAsync(id);
            if (existingApp == null) return NotFound(new { message = "Not found Appointment!" });

            existingApp.PatientId = app.PatientId;
            existingApp.DoctorId = app.DoctorId;
            existingApp.AppointmentDate = app.AppointmentDate;
            existingApp.TimeExpected = app.TimeExpected;
            existingApp.SpecialtyId = app.SpecialtyId;
            existingApp.QueueNumber = app.QueueNumber;
            existingApp.Status = app.Status;
            existingApp.Note = app.Note;
            existingApp.IsCover = app.IsCover;
            existingApp.Price = app.Price;

            _con.Appointments.Update(existingApp);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Appointment updated successfully!" });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var existingApp = await _con.Appointments.FindAsync(id);
            if (existingApp == null) return NotFound(new { message = "Not found Appointment!" });

            _con.Appointments.Remove(existingApp);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Đã xóa lịch khám!" });
        }


        [HttpGet("paging")]
        public async Task<IActionResult> GetAppointmentsWithPaging(int pageNumber = 1, int pageSize = 10)
        {
            var appointments = await _con.Appointments.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return Ok(appointments);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAppointments(string search)
        {
            if (string.IsNullOrEmpty(search))
            {
                return await GetAllAppointments();
            }
            var appointments = await _con.Appointments
                .Include(x => x.Patient)
                .Include(x => x.Doctor)
                .Where(x => (x.Patient != null && x.Patient.FullName.Contains(search)) || (x.Doctor != null && x.Doctor.FullName.Contains(search))).ToListAsync();
            return Ok(appointments);
        }


        //Phân loại theo bác sĩ, bệnh nhân
        //sắp xếp theo số thứ tự,...


    }
}
