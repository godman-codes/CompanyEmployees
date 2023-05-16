using AutoMapper;
using Entities.Models;
using Shared.DataTransferObjects;

namespace CompanyEmployees.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // always use forctorparams when you are using record class with only params
            CreateMap<Company, CompanyDto>()
                .ForMember(c => c.FullAddress,
                opt => opt.MapFrom(x => string.Join(' ', x.Address, x.Country)));
            CreateMap<Employee, EmployeeDto>();
            CreateMap<CompanyForCreationDto, Company>();
            CreateMap<EmployeeForCreationDto, Employee>();
            //CreateMap<EmployeeForUpdateDto, Employee>()
            //    // this mapping allows you to pass in a json of ony the prperty 
            //    // you want to update 
            //    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<CompanyForUpdateDto, Company>();
               //.ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmployeeForUpdateDto, Employee>().ReverseMap();
            //CreateMap<CompanyForUpdateDto, Company>()

        }
    }
}
