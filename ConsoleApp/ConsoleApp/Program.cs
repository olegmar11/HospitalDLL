using HospitalDLL;
using System.IO;

public class ConsoleApp
{
    public static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.InputEncoding = System.Text.Encoding.Unicode;
        using (var dbContext = new CodeFirst())
        {
            var pat = dbContext.Patient.ToList();
            var eq = dbContext.Equipment.ToList();
            var med = dbContext.Medicine.ToList();
            var st = dbContext.Staff.ToList();
            var proc = dbContext.Proced.ToList();
            App.Menu_main(eq, med, st, pat, proc);
        }
    }
}