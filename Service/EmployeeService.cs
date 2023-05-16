using AutoMapper;
using Contracts;
using Entities.Exceptions;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal sealed class EmployeeService : IEmployeeService
    {
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public EmployeeService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<EmployeeDto> GetEmployeeAsync(Guid companyId, Guid id, bool trackChanges)
        {
            // Get the company from the repository
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

            // Get the employee from the repository
            var employeeDb = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges);
            if (employeeDb is null)
                throw new EmployeeNotFoundException(id);

            // Map the employee entity to DTO using AutoMapper
            var employee = _mapper.Map<EmployeeDto>(employeeDb);
            return employee;
        }

        public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(Guid companyId, bool trackChanges)
        {
            // Get the company from the repository
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

            // Get the employees from the repository
            var employeesFromDb = await _repository.Employee.GetEmployeesAsync(companyId, trackChanges);

            // Map the employee entities to DTOs using AutoMapper
            var employeesDto = _mapper.Map<IEnumerable<EmployeeDto>>(employeesFromDb);
            return employeesDto;
        }

        public async Task<EmployeeDto> CreateEmployeeForCompany(Guid companyId, EmployeeForCreationDto employeeForCreation, bool trackChanges)
        {
            // Get the company from the repository
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

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
            var company = await _repository.Company.GetCompanyAsync(companyId, trackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

            // Get the employee for the company from the repository
            var employeeForCompany = await _repository.Employee.GetEmployeeAsync(companyId, id, trackChanges);
            if (employeeForCompany is null)
                throw new EmployeeNotFoundException(id);

            // Delete the employee from the repository
            _repository.Employee.DeleteEmployee(employeeForCompany);
           await  _repository.SaveAsync();
        }

        public async Task UpdateEmployeeForCompanyAsync(Guid companyId, Guid id, EmployeeForUpdateDto employeeForUpdate, bool compTrackChanges, bool empTrackChanges)
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, compTrackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);

            var employeeEntity = await _repository.Employee.GetEmployeeAsync(companyId, id, empTrackChanges);
            if (employeeEntity is null)
                throw new EmployeeNotFoundException(id);

            _mapper.Map(employeeForUpdate, employeeEntity);
            await _repository.SaveAsync();
        }
        public async Task<(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)> GetEmployeeForPatchAsync(
            Guid companyId, Guid id, bool compTrackChanges, bool empTrackChanges) 
        {
            var company = await _repository.Company.GetCompanyAsync(companyId, compTrackChanges);
            if (company is null)
                throw new CompanyNotFoundException(companyId);
            var employeeEntity = await _repository.Employee.GetEmployeeAsync(companyId, id, empTrackChanges);
            if (employeeEntity is null)
                throw new EmployeeNotFoundException(companyId);
            var employeeToPatch = _mapper.Map<EmployeeForUpdateDto>(employeeEntity);
            return (employeeToPatch, employeeEntity); }
        public async Task SaveChangesForPatchAsync(EmployeeForUpdateDto employeeToPatch, Employee employeeEntity)
        {
            _mapper.Map(employeeToPatch, employeeEntity);
            await _repository.SaveAsync();
        }
    }
}
