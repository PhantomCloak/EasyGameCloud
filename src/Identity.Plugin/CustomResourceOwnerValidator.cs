using System;
using System.Threading.Tasks;
using IdentityServer4.Validation;

namespace Identity.Plugin
{
    public class CustomResourceOwnerValidator : IResourceOwnerPasswordValidator
    {
        public CustomResourceOwnerValidator()
        {
            Console.WriteLine();
        }
        public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}