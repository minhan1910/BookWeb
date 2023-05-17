using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWeb.Models.ViewModels
{
    public class UserVM
    {
        [Required]
        public string UserID { get; set; }
        
        [ValidateNever]
        public ApplicationUser? ApplicationUser { get; set; }
        
        public string? RoleId { get; set; }
        [ValidateNever]
        //public IEnumerable<SelectListItem>? Roles { get; set; }
        public IEnumerable<IdentityRole>? Roles { get; set; }

        public string? CompanyId { get; set; }
        [ValidateNever]
        //public IEnumerable<SelectListItem>? Companies { get; set; }
        public IEnumerable<Company>? Companies { get; set; }
    }
}
