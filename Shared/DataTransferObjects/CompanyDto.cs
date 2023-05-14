using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DataTransferObjects
{
    // NOTE: if this serializable decorator is the the 
    // xml format will still return thoes self generated properies with 
    // weird codes
    //[Serializable]
    //public record CompanyDto(Guid Id, string Name, String FullAddress);
    // this above code we need to add the forctorparmas inthe mappingprofile class
    // because we are mapping to parameters not properties
    public record CompanyDto
    {
        //the init will add strictness to the property so 
        // it can comeout the same way in the xml code
        public Guid Id { get; init; }
        public string? Name { get; init; }
        public string? FullAddress { get; init; }
    }
}
