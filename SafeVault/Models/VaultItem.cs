using System.ComponentModel.DataAnnotations;

namespace SafeVault.Models
{
    // a "vault item" is just some piece of data a user wants stored securely
    // e.g. a note, a credential entry, whatever
    public class VaultItem
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = "";

        [Required]
        [StringLength(2000)]
        public string Content { get; set; } = "";

        public string OwnerId { get; set; } = "";
    }
}
