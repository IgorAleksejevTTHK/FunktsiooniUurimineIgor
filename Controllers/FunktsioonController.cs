using FunktsiooniUurimineWeb.Data;
using FunktsiooniUurimineWeb.Models;
using FunktsiooniUurimineWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FunktsiooniUurimineWeb.Controllers;

public class FunktsioonController : Controller
{
    private readonly RakenduseKontekst _kontekst;
    private readonly FunktsiooniAnalyys _analüüs;

    public FunktsioonController(RakenduseKontekst kontekst, FunktsiooniAnalyys analüüs)
    {
        _kontekst = kontekst;
        _analüüs = analüüs;
    }

    [HttpGet]
    public IActionResult Index() => View(new FunktsiooniUurimine());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(FunktsiooniUurimine olem)
    {
        if (!ModelState.IsValid) return View(olem);
        try
        {
            var tulemus = _analüüs.Uuri(olem.Valem);
            olem.Maaramispiirkond = tulemus.Maaramispiirkond;
            olem.Nullkohad = tulemus.Nullkohad;
            olem.Tuletis = tulemus.Tuletis;
            olem.KriitilisedPunktid = tulemus.KriitilisedPunktid;
            olem.Ekstreemumid = tulemus.Ekstreemumid;
            ViewBag.AnaluusTehtud = true;
            return View(olem);
        }
        catch (FormatException ex)
        {
            ModelState.AddModelError(nameof(olem.Valem), ex.Message);
            return View(olem);
        }
    }

    public async Task<IActionResult> History()
    {
        var funktsioonid = await _kontekst.FunktsiooniUurimised
            .OrderByDescending(x => x.LuodudAeg)
            .ToListAsync();
        return View(funktsioonid);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id is null) return NotFound();
        var olem = await _kontekst.FunktsiooniUurimised.FirstOrDefaultAsync(x => x.Id == id);
        if (olem is null) return NotFound();
        return View(olem);
    }

    public IActionResult Create() => View(new FunktsiooniUurimine());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(FunktsiooniUurimine olem)
    {
        if (!ModelState.IsValid) return View(olem);
        try
        {
            var tulemus = _analüüs.Uuri(olem.Valem);
            olem.Maaramispiirkond = tulemus.Maaramispiirkond;
            olem.Nullkohad = tulemus.Nullkohad;
            olem.Tuletis = tulemus.Tuletis;
            olem.KriitilisedPunktid = tulemus.KriitilisedPunktid;
            olem.Ekstreemumid = tulemus.Ekstreemumid;
            olem.LuodudAeg = DateTime.UtcNow;
            await _kontekst.FunktsiooniUurimised.AddAsync(olem);
            await _kontekst.SaveChangesAsync();
            TempData["Teade"] = "Funktsiooni analüüs salvestati edukalt.";
            return RedirectToAction(nameof(Details), new { id = olem.Id });
        }
        catch (FormatException ex)
        {
            ModelState.AddModelError(nameof(olem.Valem), ex.Message);
            return View(olem);
        }
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id is null) return NotFound();
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);
        if (olem is null) return NotFound();
        return View(olem);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, FunktsiooniUurimine olem)
    {
        if (id != olem.Id) return NotFound();
        if (!ModelState.IsValid) return View(olem);
        try
        {
            var tulemus = _analüüs.Uuri(olem.Valem);
            olem.Maaramispiirkond = tulemus.Maaramispiirkond;
            olem.Nullkohad = tulemus.Nullkohad;
            olem.Tuletis = tulemus.Tuletis;
            olem.KriitilisedPunktid = tulemus.KriitilisedPunktid;
            olem.Ekstreemumid = tulemus.Ekstreemumid;
            _kontekst.Update(olem);
            await _kontekst.SaveChangesAsync();
            TempData["Teade"] = "Funktsiooni analüüs uuendati.";
            return RedirectToAction(nameof(Details), new { id = olem.Id });
        }
        catch (FormatException ex)
        {
            ModelState.AddModelError(nameof(olem.Valem), ex.Message);
            return View(olem);
        }
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id is null) return NotFound();
        var olem = await _kontekst.FunktsiooniUurimised.FirstOrDefaultAsync(x => x.Id == id);
        if (olem is null) return NotFound();
        return View(olem);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var olem = await _kontekst.FunktsiooniUurimised.FindAsync(id);
        if (olem is not null)
        {
            _kontekst.FunktsiooniUurimised.Remove(olem);
            await _kontekst.SaveChangesAsync();
        }
        TempData["Teade"] = "Kirje kustutati.";
        return RedirectToAction(nameof(History));
    }

    public IActionResult Viga() => View("Error");
}
