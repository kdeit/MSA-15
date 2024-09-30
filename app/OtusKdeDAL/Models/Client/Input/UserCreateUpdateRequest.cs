using System.ComponentModel.DataAnnotations;

namespace OtusKdeDAL;

public class UserCreateUpdateRequest
{
    [EmailAddress] public string Email { get; set; }
    public string Name { get; set; }
}