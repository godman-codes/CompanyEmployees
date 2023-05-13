using Microsoft.AspNetCore.Mvc;
using Service.Contracts;

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
            try
            {
                var companies =
                    _service.CompanyService.GetAllCompanies(trackChanges: false);
                return Ok(companies);
            }
            catch
            {

                return StatusCode(500, "Internal Server Error");
            }
        }

    }
}
