using OMS.Domain.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace OMS.Domain.Entities
{
    [Table("Customers")]
    public class Customer
    {
        public int Id { get; private set; }
        public string Email { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string FullName => $"{FirstName} {LastName}";
        public string PhoneNumber { get; private set; }
        public DateTime RegisteredAt { get; private set; }
        public bool IsActive { get; private set; }
        public string Address { get; private set; }
        public string PasswordHash { get; private set; }
        public virtual ICollection<Order> Orders { get; private set; } = new List<Order>();
        private Customer() { }

        public static Customer Create(
            string email,
            string firstName,
            string lastName,
            string phoneNumber,
            string address,
            string password
            )
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentNullException(nameof(email), "E-posta adresi boş olamaz.");

            if (string.IsNullOrEmpty(firstName))
                throw new ArgumentNullException(nameof(firstName), "Ad boş olamaz.");

            if (string.IsNullOrEmpty(lastName))
                throw new ArgumentNullException(nameof(lastName), "Soyad boş olamaz.");

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password), "password boş olamaz.");

            return new Customer
            {
                Email = email.ToLowerInvariant(),
                FirstName = firstName,
                LastName = lastName,
                PhoneNumber = phoneNumber,
                RegisteredAt = DateTime.UtcNow,
                IsActive = true,
                Address = address,
                PasswordHash = SecurityHelper.CreateMD5Hash(password)
            };
        }

        public void UpdateProfile(string firstName, string lastName, string phoneNumber, string address)
        {
            if (!string.IsNullOrEmpty(firstName))
                FirstName = firstName;

            if (!string.IsNullOrEmpty(lastName))
                LastName = lastName;

            if (!string.IsNullOrEmpty(address))
                Address = address;

            PhoneNumber = phoneNumber;
        }

        public void SetActive(bool isActive)
        {
            IsActive = isActive;
        }
    }
}
