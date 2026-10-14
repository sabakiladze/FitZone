using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitZone.Domain.Entities.Models
{
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;  // fluent api ში მივუთითო რომ მხოლოდ user and admin
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
