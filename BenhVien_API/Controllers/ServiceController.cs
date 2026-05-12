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
    public class ServiceController : ControllerBase
    {

        //Chưa fix

        private readonly QlBenhvienContext _con;

        public ServiceController(QlBenhvienContext con)
        {
            _con = con;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            var items = await _con.Services
                .Include(s => s.Specialty)
                .Include(s => s.Paymentservices)
                .ToListAsync();
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceById(string id)
        {
            var item = await _con.Services
                .Include(s => s.Specialty)
                .Include(s => s.Paymentservices)
                .FirstOrDefaultAsync(s => s.ServiceId == id);

            if (item == null) return NotFound(new { message = "Service not found!" });
            return Ok(item);
        }

        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] Service service)
        {
            if (service == null) return BadRequest();

            if (string.IsNullOrEmpty(service.ServiceId))
                service.ServiceId = Guid.NewGuid().ToString();

            await _con.Services.AddAsync(service);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Service created successfully!", data = service });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateService(string id, [FromBody] Service service)
        {
            if (service == null || id != service.ServiceId)
                return BadRequest(new { message = "Service id not found or payload is invalid!" });

            var existing = await _con.Services.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Not found service!" });

            existing.ServiceName = service.ServiceName;
            existing.Price = service.Price;
            existing.Department = service.Department;
            existing.Description = service.Description;
            existing.IsCover = service.IsCover;
            existing.SpecialtyId = service.SpecialtyId;

            _con.Services.Update(existing);
            await _con.SaveChangesAsync();

            return Ok(new { message = "Service updated successfully!" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteService(string id)
        {
            var existing = await _con.Services.FindAsync(id);
            if (existing == null) return NotFound(new { message = "Not found service!" });

            _con.Services.Remove(existing);
            await _con.SaveChangesAsync();
            return Ok(new { message = "Service deleted!" });
        }

        [HttpGet("paging")]
        public async Task<IActionResult> GetServicesWithPaging(int pageNumber = 1, int pageSize = 10)
        {
            var items = await _con.Services
                .Include(s => s.Specialty)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchServices(string search)
        {
            if (string.IsNullOrEmpty(search)) return await GetAllServices();

            var results = await _con.Services
                .Include(s => s.Specialty)
                .Where(s =>
                    (s.ServiceName != null && s.ServiceName.Contains(search)) ||
                    (s.Department != null && s.Department.Contains(search)) ||
                    (s.Description != null && s.Description.Contains(search)) ||
                    (s.Specialty != null && s.Specialty.SpecialtyId != null && s.Specialty.SpecialtyId.Contains(search)))
                .ToListAsync();

            return Ok(results);
        }

    }
}
