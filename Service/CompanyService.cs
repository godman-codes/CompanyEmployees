using AutoMapper;
using Contracts;
using Entities.Models;
using Service.Contracts;
using Shared.DataTransferObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    internal sealed class CompanyService : ICompanyService
    { 
        private readonly IRepositoryManager _repository;
        private readonly ILoggerManager _logger;
        private readonly IMapper _mapper;

        public CompanyService(IRepositoryManager repository, ILoggerManager logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public IEnumerable<CompanyDto> GetAllCompanies(bool trackChanges)
        {
            try
            {
                var companies = _repository.Company.GetAllCompanies(trackChanges);
                var comapaniesDto = _mapper.Map<IEnumerable<CompanyDto>>(companies);
                return comapaniesDto;
            }
            catch (Exception ex)
            {
                // log the error
                _logger.LogError(
                    $"Somethingt went wong in the {nameof(GetAllCompanies)} service method {ex}"
                    );
                throw;
            }
        }
    }
}
