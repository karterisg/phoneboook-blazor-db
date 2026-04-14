namespace PhoneBookApp.Models // namespace gia ta models (domain objects)
{
    public class Contact // entity pou antistoixei se mia epafi (kai se grammi ston pinaka Contacts)
    {
        public int Id { get; set; } // primary key
        public string? Name { get; set; } // onoma epafis 
        public string? Phone { get; set; } // tilefono 
        public string? Email { get; set; } // email 
        public string Gender { get; set; } = "Male"; // filo

    }
}