using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Include için gerekli
using Yazlab2.Data;
using Yazlab2.Models;
using System.Security.Claims;
using Yazlab2.Migrations;

public class ChatController : Controller
{
    private readonly ApplicationDbContext _context;

    public ChatController(ApplicationDbContext context)
    {
        _context = context;
    }

    
    public IActionResult Index()
    {
        // Kullanıcı bilgileriyle beraber mesajları çekiyoruz
        var genelMesajlar = _context.Mesajlar
            .Include(m => m.Gonderici) 
            .Where(m => m.IsGeneralChat)
            .OrderBy(m => m.GonderimZamani)
            .ToList();

        return View(genelMesajlar);
    }
    public IActionResult AdminChat()
    {
        
        var genelMesajlar = _context.Mesajlar
            .Include(m => m.Gonderici) 
            .Where(m => m.IsGeneralChat)
            .OrderBy(m => m.GonderimZamani)
            .ToList();

        return View(genelMesajlar);
    }
    // Yeni Mesaj Gönder
    [HttpPost]
    public IActionResult SendMessage(string mesajMetni)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var mesaj = new Mesaj
        {
            GondericiID = int.Parse(userId),
            MesajMetni = mesajMetni,
            GonderimZamani = DateTime.Now,
            IsGeneralChat = true,
        };
        var bildirim = new Bildirim
        {
            KullanıcıID = mesaj.GondericiID,
            Mesaj = $"Yeni bir mesaj gönderildi: {mesaj.MesajMetni}",
            Tarih = DateTime.Now,
            OkunduMu = false
        };

        _context.Bildirimler.Add(bildirim);

        _context.Mesajlar.Add(mesaj);
        _context.SaveChanges();

        return RedirectToAction("Index");
    }


    [HttpPost]
    public IActionResult SendMessage2(string mesajMetni)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            return Unauthorized();
        }

        var mesaj = new Mesaj
        {
            GondericiID = int.Parse(userId),
            MesajMetni = mesajMetni,
            GonderimZamani = DateTime.Now,
            IsGeneralChat = true,
        };
        var bildirim = new Bildirim
        {
            KullanıcıID = mesaj.GondericiID,
            Mesaj = $"Yeni bir mesaj gönderildi: {mesaj.MesajMetni}",
            Tarih = DateTime.Now,
            OkunduMu = false
        };

        _context.Bildirimler.Add(bildirim);

        _context.Mesajlar.Add(mesaj);
        _context.SaveChanges();

        return RedirectToAction("AdminChat");
    }
}
