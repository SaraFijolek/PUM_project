using System;
using System.Collections.Generic;
using System.Text;

namespace App1.Models
{
    public class Profile
    {

        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime BirthDate { get; set; }
        public string Gender { get; set; }
        public string HeightCm { get; set; }
        public string WeightKg { get; set; }
        public string AvatarUrl { get; set; }
        public string PreferredLanguage { get; set; }
        public bool IsAdmin { get; set; }

    }
}
