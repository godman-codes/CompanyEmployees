using CompanyEmployees.Presentation.ActionFilters;
using CompanyEmployees.Presentation.Helper;
using Entities.LinkModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        [ServiceFilter(typeof(ValidateMediaTypeAttribute))]
        public async Task<IActionResult> GetEmployeesForCompany(Guid companyId, [FromQuery] EmployeeParameters employeeParameters)
        {
            // Retrieve employees for a specific company
            //var employees = await _service.EmployeeService.GetEmployeesAsync(companyId, employeeParameters, trackChanges: false);
            //var response = PaginationHelper.CreatePaginatedResponse(Request, employees, employeeParameters.PageNumber, employeeParameters.PageSize);

            var linkParams = new LinkParameters(employeeParameters, HttpContext);
            var pagedResult = await _service.EmployeeService.GetEmployeesAsync(companyId, linkParams, trackChanges: false);
            Response.Headers.Add("X-Pagination", JsonSerializer.Serialize(pagedResult.metaData));
            //return Ok(pagedResult.employees);
            //return Ok(employees);
            //return new HttpResponseMessageCustom(response);
            JsonSerializer.Serialize(pagedResult.metaData);
            return pagedResult.linkResponse.HasLinks ? Ok(pagedResult.linkResponse.LinkedEntities) :
                Ok(pagedResult.linkResponse.ShapedEntities);
        }

        // GET: api/companies/{companyId}/employees/{id}
        [HttpGet("{id:guid}", Name = "GetEmployeeForCompany")]
        public async Task<IActionResult> GetEmployeeForCompany(Guid companyId, Guid id)
        {
            // Retrieve a specific employee for a company
            var employee = await _service.EmployeeService.GetEmployeeAsync(companyId, id, trackChanges: false);
            return Ok(employee);
        }

        // POST: api/companies/{companyId}/employees
        [HttpPost]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public IActionResult CreateEmployeeForCompany(Guid companyId, [FromBody] EmployeeForCreationDto employee)
        {

            // Create a new employee for a company
            var employeeToReturn = _service.EmployeeService.CreateEmployeeForCompany(companyId, employee, trackChanges: false);

            // Return the created employee in the response with the appropriate route
            return CreatedAtRoute("GetEmployeeForCompany", new { companyId, id = employeeToReturn.Id }, employeeToReturn);
        }

        // DELETE: api/companies/{companyId}/employees/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteEmployeeForCompany(Guid companyId, Guid id)
        {
            // Delete a specific employee for a company
            await _service.EmployeeService.DeleteEmployeeForCompanyAsync(companyId, id, trackChanges: false);

            // Return a successful response with no content
            return NoContent();
        }

        [HttpPut("{id:guid}")]
        [ServiceFilter(typeof(ValidationFilterAttribute))]
        public async Task<IActionResult> UpdateEmployeeForCompany([FromBody] EmployeeForUpdateDto employee,Guid companyId, Guid id)
        {
            await _service.EmployeeService.UpdateEmployeeForCompanyAsync(
                companyId: companyId,id: id,employeeForUpdate: employee, compTrackChanges: false, empTrackChanges: true
                );
            return NoContent();

        }

        [HttpPatch("{id:guid}")]
        public async Task<IActionResult> PartiallyUpdateEmployeeForCompany(
            Guid companyId, Guid id, [FromBody] JsonPatchDocument<EmployeeForUpdateDto> patchDoc) 
        {
            if (patchDoc is null)
                return BadRequest("patchDoc object sent from client is null.");
            var result = await _service.EmployeeService.GetEmployeeForPatchAsync(companyId, id, compTrackChanges: false, empTrackChanges: true);
            patchDoc.ApplyTo(result.employeeToPatch, ModelState);

            TryValidateModel(result.employeeToPatch);
            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);
            await _service.EmployeeService.SaveChangesForPatchAsync(result.employeeToPatch, result.employeeEntity);
            return NoContent(); }

    }
}
