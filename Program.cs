using Microsoft.EntityFrameworkCore;
using System.Windows.Forms;

using (var db = new AppDbContext())
{
    db.Database.Migrate(); // crée ou met à jour la base si besoin
}

ApplicationConfiguration.Initialize();
Application.Run(new MainForm());