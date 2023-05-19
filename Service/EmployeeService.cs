using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;
using Shared.RequestFeatures;
using System.Dynamic;

namespace Service
{
    internal sealed class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;
        private readonly IDataShaper<EmployeeDto> _dataShaper;

        public EmployeeService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper, IDataShaper<EmployeeDto> dataShaper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _dataShaper = dataShaper;
        }

        public async Task<EmployeeDto> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges)
        {
            // Get the company from the repository
            await CheckIfCompanyExists(companyId, trackChanges);

            // Get the employee from the repository
            var employeeDb = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);

            // Map the employee entity to DTO using AutoMapper
            var employee = _mapper.Map<EmployeeDto>(employeeDb);
            return employee;
        }

        public async Task<(IEnumerable<ExpandoObject> employees, MetaData metaData)> GetEmployeesAsync(Guid companyId, EmployeeParameters employeeParameters, bool trackChanges)
        {
            if (!employeeParameters.ValidAgeRange)
                throw new MaxAgeRangeBadRequestException();
            // Get the company from the repository
            await CheckIfCompanyExists(companyId, trackChanges);

            // Get the employees from the repository
            //var employeesFromDb = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges);

            //// Map the employee entities to DTOs using AutoMapper
            //var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
            //return employeesDto;
            var employeesWithMetaData = await _repository.Employee.GetEmployeesAsync(companyId, employeeParameters, trackChanges);
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesWithMetaData);
            //if (string.IsNullOrWhiteSpace(employeeParameters.Fields))
            //{
            //    return (employees: employeesDto, metaData: employeesWithMetaData.MetaData);
            //}
            var sharedData = _dataShaper.ShapedData(employeesDto, employeeParameters.Fields);
            return (employees: sharedData, metaData: employeesWithMetaData.MetaData);
        }

        public async Task<EmployeeDto> CreateEmployeeForCompany(Guid companyId, EmployeeForCreationDto employeeForCreation, bool trackChanges)
        {
            // Get the company from the repository
            await CheckIfCompanyExists(companyId, trackChanges);

            // Map the employee for creation DTO to employee entity using AutoMapper
            var employeeEntity = _mapper.Map<Employee>(employeeForCreation);

            // Create the employee for the company in the repository
            _repository.Employee.CreateEmployeeForCompany(companyId, employeeEntity);
            await _repository.SaveAsync();

            // Map the employee entity to DTO using AutoMapper
            var employeeToReturn = _mapper.Map<EmployeeDto>(employeeEntity);
            return employeeToReturn;
        }

        public async Task DeleteEmployeeForCompanyAsync(Guid companyId, Guid id, bool trackChanges)
        {
            // Get the company from the repository
            await CheckIfCompanyExists(companyId, trackChanges);

            // Get the employee for the company from the repository
            var employeeForCompany = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, trackChanges);

            // Delete the employee from the repository
            _repository.Employee.DeleteEmployee(employeeForCompany);
            await _repository.SaveAsync();
        }

        public async Task UpdateEmployeeForCompanyAsync(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);

            var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);

            _mapper.Map(employeeForUpdate, employeeEntity);
            await _repository.SaveAsync();
        }
        public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatchAsync(
            Guid companyId, Guid id, bool compTrackChanges, bool empTrackChanges)
        {
            await CheckIfCompanyExists(companyId, compTrackChanges);
            var employeeEntity = await GetEmployeeForCompanyAndCheckIfItExists(companyId, id, empTrackChanges);

            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
            return (employeeToPatch, employeeEntity);
        }
        public async Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
        {
            _mapper.Map(employeeToPatch, employeeEntity);
            await _repository.SaveAsync();
        }

        private async Task CheckIfCompanyExists(Guid companyId, bool trackChanges)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);
        }
        private async Task<Employee> GetEmployeeForCompanyAndCheckIfItExists(Guid companyId, Guid id, bool trackChanges)
        {
            var employeeDb = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges);
            if (employeeDb is null)
                throw new EmployeeNotFoundException(id);
            return employeeDb;
        }
    }
}
