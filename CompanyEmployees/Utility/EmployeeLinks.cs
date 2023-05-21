using Contracts;
using Entities.LinkModels;
using Entities.Models;
using Shared.DataTransferObjects;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace CompanyEmployees.Utility
{
    public class EmployeeLinks : IEmployeeLinks
    {
        private readonly LinkGenerator _linkGenerator;
        private readonly IDataShaper<EmployeeDto> _dataShaper;

        public EmployeeLinks(
            LinkGenerator linkGenerator,
            IDataShaper<EmployeeDto> dataShaper
        )
        {
            _linkGenerator = linkGenerator;
            _dataShaper = dataShaper;
        }

        // Generates HATEOAS links for employees
        public LinkResponse TryGenerateLinks(
            IEnumerable<EmployeeDto> employeesDto,
            string fields, Guid companyId,
            HttpContext httpContext)
        {
            // Shape the employee data based on requested fields
            var shapedEmployees = ShapeData(employeesDto, fields);

            // Check if links should be generated based on the requested media type
            if (ShouldGenerateLinks(httpContext))
                return ReturnLinkedEmployees(employeesDto, fields, companyId, httpContext, shapedEmployees);

            // If links should not be generated, return the shaped employees without links
            return ReturnShapedEmployees(shapedEmployees);
        }

        // Shape the employee data based on the requested fields
        private List<Entity> ShapeData(IEnumerable<EmployeeDto> employeesDto, string fields) =>
            _dataShaper.ShapedData(employeesDto, fields).Select(e => e.Entity).ToList();

        // Check if links should be generated based on the requested media type
        private bool ShouldGenerateLinks(HttpContext httpContext)
        {
            // Retrieve the media type from the Accept header
            var mediaType = (MediaTypeHeaderValue)httpContext.Items["AcceptHeaderMediaType"];

            // Check if the media type indicates HATEOAS support
            return mediaType.SubTypeWithoutSuffix.EndsWith("hateoas", StringComparison.InvariantCultureIgnoreCase);
        }

        // Return the shaped employees without links
        private LinkResponse ReturnShapedEmployees(List<Entity> shapedEmployees) =>
            new LinkResponse { ShapedEntities = shapedEmployees };

        // Return the shaped employees with generated links
        private LinkResponse ReturnLinkedEmployees(IEnumerable<EmployeeDto> employeesDto,
            string fields, Guid companyId, HttpContext httpContext,
            List<Entity> shapedEmployees)
        {
            var employeeDtoList = employeesDto.ToList();
            for (var index = 0; index < employeeDtoList.Count(); index++)
            {
                // Generate links for each employee and add them to the corresponding Entity
                var employeeLinks = CreateLinksForEmployee(httpContext, companyId,
                    employeeDtoList[index].Id, fields);
                shapedEmployees[index].Add("Links", employeeLinks);
            }

            // Create a LinkCollectionWrapper for the shaped employees and generate links for the employee collection
            var employeeCollection = new LinkCollectionWrapper<Entity>(shapedEmployees);
            var linkedEmployees = CreateLinksForEmployees(httpContext, employeeCollection);

            // Return the shaped employees with generated links
            return new LinkResponse
            {
                HasLinks = true,
                LinkedEntities = linkedEmployees
            };
        }

        // Generate links for a specific employee
        private List<Link> CreateLinksForEmployee(HttpContext httpContext,
            Guid companyId, Guid id, string fields = "")
        {
            var links = new List<Link>
            {
                // Generate self link for retrieving a specific employee
                new Link(_linkGenerator.GetUriByAction(
                    httpContext, "GetEmployeeForCompany",
                    values: new { companyId, id, fields }),
                    "self", "GET"),

                // Generate delete link for deleting a specific employee
                new Link(_linkGenerator.GetUriByAction(
                        httpContext, "DeleteEmployeeForCompany",
                        values: new { companyId, id }),
                    "delete_employee", "DELETE"),

                // Generate update link for updating a specific employee
                new Link(_linkGenerator.GetUriByAction(
                    httpContext, "UpdateEmployeeForCompany",
                    values: new { companyId, id }),
                    "update_employee", "PUT"),

                // Generate partial update link for partially updating a specific employee
                new Link(_linkGenerator.GetUriByAction(
                    httpContext, "PartiallyUpdateEmployeeForCompany",
                    values: new { companyId, id }),
                    "partially_update_employee", "PATCH")
            };
            return links;
        }

        // Generate links for the employee collection
        private LinkCollectionWrapper<Entity> CreateLinksForEmployees(
            HttpContext httpContext,
            LinkCollectionWrapper<Entity> employeesWrapper)
        {
            // Generate self link for retrieving all employees for a company
            employeesWrapper.Links.Add(
                new Link(_linkGenerator.GetUriByAction(
                    httpContext, "GetEmployeesForCompany",
                    values: new { }), "self", "GET"
                ));
            return employeesWrapper;
        }
    }
}
