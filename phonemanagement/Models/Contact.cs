namespace PhoneBookApp.Models // namespace gia ta models (domain objects)
{
    public class Contact // entity pou antistoixei se mia epafi (kai se grammi ston pinaka Contacts)
    {
        public int Id { get; set; } // primary key
        public string? Name { get; set; } // onoma epafis 
        public string? Phone { get; set; } // tilefono 
        public string? Email { get; set; } // email 
        public string Gender { get; set; } = "Male"; // filo//default

        /// <summary>True when this row is a shared directory card created by a user (visible to all in /api/directory).</summary>
        public bool IsUserContribution { get; set; }

        /// <summary>Stable id for directory API / UI when <see cref="IsUserContribution"/> is true; null for legacy mirrored rows.</summary>
        public Guid? DirectoryListingId { get; set; }

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    }
}