namespace PhoneBookApp.Models
{
    public static class ContactsRepository
    {
        private static List<Contact> contacts = new List<Contact>()
        {
            new Contact{ Id=1, Name="Γιώργος", Phone="6912345678" },
            new Contact{ Id=2, Name="Κώστας", Phone="6971234567" },
            new Contact{ Id=3, Name="Λευτέρης", Phone="6998765432" },
            new Contact{ Id=4, Name="Μαρία", Phone="6934567890" },
            new Contact{ Id=5, Name="Ελένη", Phone="6945678901" },
            new Contact{ Id=6, Name="Νίκος", Phone="6956789012" },
            new Contact{ Id=7, Name="Δημήτρης", Phone="6967890123" },
            new Contact{ Id=8, Name="Αντώνης", Phone="6978901234" },
            new Contact{ Id=9, Name="Παναγιώτης", Phone="6989012345" },
            new Contact{ Id=10, Name="Σοφία", Phone="6990123456" },
            new Contact{ Id=11, Name="Χρήστος", Phone="6911122233" },
            new Contact{ Id=12, Name="Βασίλης", Phone="6922233344" },
            new Contact{ Id=13, Name="Αγγελική", Phone="6933344455" },
            new Contact{ Id=14, Name="Ιωάννα", Phone="6944455566" },
            new Contact{ Id=15, Name="Θανάσης", Phone="6955566677" }



        };


        //epistrefei th lista apo epafes 
        public static List<Contact> GetContacts() => contacts;



        //vriskei epafh me sygkekrimeno id, epistrefei null an den vrethei
        public static Contact? GetContactById(int id)
        {
            return contacts.FirstOrDefault(c => c.Id == id);
        }

        //prosthetei nea epafh, me id pou einai to megalitero id + 1
        public static void AddContact(Contact contact)
        {
            int maxId = contacts.Any() ? contacts.Max(c => c.Id) : 0;
            contact.Id = maxId + 1;
            contacts.Add(contact);
        }


        //kanei update se epafh me sygkekrimeno id, an vrethei, alliws den kanei tipota
        public static void UpdateContact(Contact contact)
        {
            var existing = contacts.FirstOrDefault(c => c.Id == contact.Id);

            if (existing != null)
            {
                existing.Name = contact.Name;
                existing.Phone = contact.Phone;
                existing.Gender = contact.Gender;
            }
        }


        //diagrafei epafh me sygkekrimeno id, an vrethei, alliws den kanei tipota
        public static void DeleteContact(int id)
        {
            var c = contacts.FirstOrDefault(x => x.Id == id);
            if (c != null) contacts.Remove(c);
        }


        //epistrefei lista me epafes pou to onoma tous periexei to contactfilter, xwris na exei shmasia ta kefalaia-kleia
        public static List<Contact> SearchContacts(string filter)
        {
            return contacts.Where(c =>
                (c.Name != null && c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)) ||
                (c.Phone != null && c.Phone.Contains(filter))
            ).ToList();
        }

    }

}


