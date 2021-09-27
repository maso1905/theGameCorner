using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AuthSystem.Models
{
    public class Employee
    {
        public int EmployeeId { get; set; }
        public string Name { get; set; }
        public string WorkTitle { get; set; }
        [NotMapped]
        public string ImageName { get; set; }
        public IEnumerable<Employee> Employees { get; set; }

    }
}