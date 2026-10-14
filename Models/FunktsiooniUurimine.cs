using System.ComponentModel.DataAnnotations;

namespace FunktsiooniUurimineWeb.Models;

public class FunktsiooniUurimine
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Valem on kohustuslik.")]
    [StringLength(500, ErrorMessage = "Valem võib olla kuni 500 märki pikk.")]
    [Display(Name = "Valem")]
    public string Valem { get; set; } = string.Empty;

    [Display(Name = "Määramispiirkond")]
    public string Maaramispiirkond { get; set; } = string.Empty;

    [Display(Name = "Nullkohad")]
    public string Nullkohad { get; set; } = string.Empty;

    [Display(Name = "Tuletis")]
    public string Tuletis { get; set; } = string.Empty;

    [Display(Name = "Kriitilised punktid")]
    public string KriitilisedPunktid { get; set; } = string.Empty;

    [Display(Name = "Ekstreemumid")]
    public string Ekstreemumid { get; set; } = string.Empty;

    [Display(Name = "Loodud aeg")]
    public DateTime LuodudAeg { get; set; } = DateTime.UtcNow;
}
