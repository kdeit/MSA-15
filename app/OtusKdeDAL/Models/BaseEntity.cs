using System.ComponentModel.DataAnnotations;

namespace OtusKde.DAL;

public class BaseEntity
{
    [Key] public int Id { get; set; }
}