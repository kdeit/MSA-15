using System.ComponentModel.DataAnnotations;

namespace OtusKdeDAL;

public class BaseEntity
{
    [Key] public int Id { get; set; }
}