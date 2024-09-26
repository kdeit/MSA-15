using System.ComponentModel.DataAnnotations;

namespace OtusKdeDAL;

public class User : BaseEntity
{
    public string Name { get; set; }
    [EmailAddress] public string Email { get; set; }
}