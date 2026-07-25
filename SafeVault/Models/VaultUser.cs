using Microsoft.AspNetCore.Identity;

namespace SafeVault.Models
{
    // extends the built in Identity user, nothing crazy added yet
    public class VaultUser : IdentityUser
    {
        public string? Department { get; set; }
    }
}
