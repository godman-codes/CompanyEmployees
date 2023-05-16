using CompanyEmployees.Presentation.ModelBinders;
using Microsoft.AspNetCore.Mvc;
using Service.Contracts;
using Shared.DataTransferObjects;

namespace CompanyEmployees.Presentation.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly IServiceManager _service;

        public CompanyController(IServiceManager service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetCompanies()
        {
            //throw new Exception("Exception");
            var companies = _service.CompanyService.GetAllCompanies(trackChanges: false);
            return Ok(companies);
        }


        [HttpGet("{id:guid}", Name="CompanyById")]
        public IActionResult GetCompany(Guid id)
        {
            var company = _service.CompanyService.GetCompany(companyId: id, trackChanges: false);
            return Ok(company);
        }


        [HttpGet("collection/({ids})", Name = "CompanyCollection")] 
        public IActionResult GetCompanyCollection(
            [ModelBinder(BinderType=typeof(ArrayModelBinder))] IEnumerable<Guid> ids
            ) 
        { 
            var companies = _service.CompanyService.GetByIds(ids, trackChanges: false);
            return Ok(companies); 
        }

        [HttpPost]
        public IActionResult CreateCompany([FromBody] CompanyForCreationDto company)
        {
            if (company is null)
                return BadRequest("CompanyForCreationDto object is null");

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);
            var createdCompany = _service.CompanyService.CreateCompany(company);

            return CreatedAtRoute("CompanyById", new { id = createdCompany.Id }, createdCompany);
        }

        [HttpPost("collection")]
        public IActionResult CreateCompanyCollection([FromBody] IEnumerable<CompanyForCreationDto> companyCollection)
        { 
            var result = _service.CompanyService.CreateCompanyCollection(companyCollection);
            return CreatedAtRoute("CompanyCollection", new { result.ids }, result.companies); 
        }

        [HttpDelete("{companyId}")]
        public IActionResult DeleteCompany(Guid companyId)
        {
            _service.CompanyService.DeleteCompany(companyId, trackChanges: false);
            return NoContent();
        }

        [HttpPut("{id:guid}")]
        public IActionResult UpdateCompany(Guid id, [FromBody] CompanyForUpdateDto company)
        {
            if (company is null)
                return BadRequest("CompanyForUpdateDto object is null");

            if (!ModelState.IsValid)
                return UnprocessableEntity(ModelState);
            _service.CompanyService.UpdateCompany(id, company, trackChanges: true);
            return NoContent(); }
    }
}
