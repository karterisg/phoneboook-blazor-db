namespace PhoneBookApp.Models // namespace gia ta models (domain objects)
{
    public class Contact // entity pou antistoixei se mia epafi (kai se grammi ston pinaka Contacts)
    {
        public int Id { get; set; } // primary key (Identity stin SQL)
        public string? Name { get; set; } // onoma epafis (mporei na einai null)
        public string? Phone { get; set; } // tilefono (mporei na einai null)
        public string? Email { get; set; } // email (mporei na einai null)
        public string Gender { get; set; } = "Male"; // filo, default Male (den einai null)

    }
}