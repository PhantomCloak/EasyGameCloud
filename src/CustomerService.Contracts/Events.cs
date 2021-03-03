using System;

namespace CustomerService.Contracts
{
    public class CustomerCreated
    {
        public string Id { get; set; }
    }

    public class CustomerDeleted
    {
        public string Id { get; set; }
    }
}