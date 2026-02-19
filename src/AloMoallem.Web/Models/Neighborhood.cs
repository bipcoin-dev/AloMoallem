namespace AloMoallem.Web.Models;

public class Neighborhood
{
    public int Id { get; set; }

    public int GovernorateId { get; set; }
    public Governorate Governorate { get; set; } = default!;

    public string Name { get; set; } = "";
}
