using System.Threading.Tasks;
using CustomerService.Contracts.CommandModels;

namespace CustomerService.Repository
{
    public class ICustomerRepository
    {
        Task CreateCustomerAsync(CreateCustomerCommand createCustomerCommand)
        {
            
        }
    }
}