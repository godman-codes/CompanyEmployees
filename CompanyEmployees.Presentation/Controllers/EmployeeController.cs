using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompanyEmployees.Presentation.Controllers
{
    [Route("api/companies/{companyId}/employees")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        private readonly IServiceManager _service;

        public EmployeeController(IServiceManager service)
        {
            _service = service;
        }

        // GET: api/companies/{companyId}/employees
        [HttpGet]
        public IActionResult GetEmployeesForCompany(Guid companyId)
        {
            // Retrieve employees for a specific company
            var employees = _service.EmployeeService.GetEmployees(companyId, trackChanges: false);
            return Ok(employees);
        }

        // GET: api/companies/{companyId}/employees/{id}
        [HttpGet("{id:guid}", Name = "GetEmployeeForCompany")]
        public IActionResult GetEmployeeForCompany(Guid companyId, Guid id)
        {
            // Retrieve a specific employee for a company
            var employee = _service.EmployeeService.GetEmployee(companyId, id, trackChanges: false);
            return Ok(employee);
        }

        // POST: api/companies/{companyId}/employees
        [HttpPost]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
        {
            if (employee is null)
                return BadRequest("EmployeeForCreationDto object is null");

            // Create a new employee for a company
            var employeeToReturn = _service.EmployeeService.CreateEmployeeForCompany(companyId, employee, trackChanges: false);

            // Return the created employee in the response with the appropriate route
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }

        // DELETE: api/companies/{companyId}/employees/{id}
        [HttpDelete("{id:guid}")]
        public IActionResult DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            // Delete a specific employee for a company
            _service.EmployeeService.DeleteEmployeeForCompany(companyId, id, trackChanges: false);

            // Return a successful response with no content
            return NoContent();
        }
    }
}
