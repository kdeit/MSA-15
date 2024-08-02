using System.ComponentModel.DataAnnotations;

namespace OtusKde.DAL;

public class User : BaseEntity
{
    public string Name { get; set; }
    [EmailAddress] public string Email { get; set; }
}