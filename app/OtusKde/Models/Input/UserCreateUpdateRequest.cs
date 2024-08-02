using System.ComponentModel.DataAnnotations;

namespace OtusKde.Models.Input;

public class UserCreateUpdateRequest
{
    [EmailAddress] public string Email { get; set; }
    public string Name { get; set; }
}