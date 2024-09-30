using System.ComponentModel.DataAnnotations;

namespace OtusKdeDAL;

public class User : BaseEntity
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    [EmailAddress] public string Email { get; set; }
    
    //public int CompanyId { get; set; }
    //public virtual Company Company { get; set; }
}