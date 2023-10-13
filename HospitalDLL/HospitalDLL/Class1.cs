using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Collections;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.SqlServer;
using System.Data.SqlClient;

namespace HospitalDLL
{
    public class CodeFirst : DbContext
    {
        public CodeFirst() : base("Server=localhost;Database=Hospital;Trusted_Connection=true")
        {
            Database.SetInitializer<CodeFirst>(null);
        }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<Medicine> Medicine { get; set; }
        public DbSet<Patient> Patient { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Proced> Proced { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new ProcedConfig());
        }
    }
    public class ProcedConfig : EntityTypeConfiguration<Proced>
    {
        public ProcedConfig()
        {
            this.HasOptional(p => p.Equipment)
             .WithMany()
             .HasForeignKey(p => p.Equipment_ID);

            this.HasOptional(p => p.Medicine)
             .WithMany()
             .HasForeignKey(p => p.Medicine_ID);

            this.HasOptional(p => p.Patient)
             .WithMany()
             .HasForeignKey(p => p.Patient_ID);

            this.HasOptional(p => p.Staff)
             .WithMany()
             .HasForeignKey(p => p.Staff_ID);
        }
    }
        [Table("Equipment")]
    public class Equipment
    {
        [Key]
        public int IDeq { get; set; }
        [MinLength(1)]
        public string Name { get; set; }
        public int Quantity { get; set; }
        [MinLength(1)]
        public string Manufacturer { get; set; }
    }

    [Table("Staff")]
    public class Staff
    {
        [Key]
        public int IDstaff { get; set; }
        [MinLength(1)]
        public string Name { get; set; }
        [MinLength(1)]
        public string Last_Name { get; set; }
        [MinLength(1)]
        public string Middle_Name { get; set; }
        public int Age { get; set; }
        [MinLength(1)]
        public string Position { get; set; }
    }

    [Table("Proced")]
    public class Proced
    {
        [Key]
        public int IDproc { get; set; }
        public string Name { get; set; }
        public int Price { get; set; }

        public int? Equipment_ID { get; set; }
        public int? Medicine_ID { get; set; }
        public int? Patient_ID { get; set; }
        public int? Staff_ID { get; set; }

        public virtual Equipment Equipment { get; set; }
        public virtual Medicine Medicine { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual Staff Staff { get; set; }
    }

    [Table("Patient")]
    public class Patient
    {
        [Key]
        public int IDpat { get; set; }
        [MinLength(1)]
        [MaxLength(20, ErrorMessage = "This field cannot exceed limititaion of 20 symbols, or be lover 1 symbol")]
        public string First_Name { get; set; }
        [MinLength(1)]
        [MaxLength(20, ErrorMessage = "This field cannot exceed limititaion of 20 symbols, or be lover 1 symbol")]
        public string Last_Name { get; set; }
        [MinLength(1)]
        [MaxLength(20, ErrorMessage = "This field cannot exceed limititaion of 20 symbols, or be lover 1 symbol")]
        public string Middle_Name { get; set; }
        public int Age { get; set; }
        public string Disease { get; set; }
        public DateTime? Arrival_Date { get; set; }
        public DateTime? Discharge_Date { get; set; }
        public string Ward { get; set; }
        [DefaultValue(1)]
        public int? LuckLevel { get; set; }
        public void Display()
        {
            Console.WriteLine("I am displayed");
        }
    }

    [Table("Medicine")]
    public class Medicine
    {
        [Key]
        public int IDmed { get; set; }
        [MinLength(1)]
        public string Name { get; set; }
        public string Producer { get; set; }
        public DateTime Created_Date { get; set; }
        public DateTime Expiration_Date { get; set; }
    }
    public class App
    {
        public static void AlterDataTables(string tableName, string op, List<Equipment> eq, List<Medicine> med, List<Staff> st, List<Patient> pat, List<Proced> proc)
        {
            using (var dbContext = new CodeFirst())
            {
                switch (tableName)
                {
                    case "dbo.Equipment":
                        if (op == "add") // Визначаємо операцію
                        {
                            Console.WriteLine("Введіть Назву, К-сть та виробника");

                            var temp = new Equipment();
                            try
                            {
                                temp.IDeq = dbContext.Equipment.Count() + 1;
                                temp.Name = Console.ReadLine();
                                temp.Quantity = Convert.ToInt32(Console.ReadLine());
                                temp.Manufacturer = Console.ReadLine();

                                dbContext.Equipment.Add(temp);

                                dbContext.SaveChanges();

                                AuditAdd(temp);
                            }
                            catch (Exception ex) { Console.WriteLine("Дані не пройшли валідацію"); }
                        }
                        if (op == "alter")
                        {
                            Console.WriteLine("Виберіть рядок, що будете редагувати: ");
                            int index = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Введіть Назву, К-сть та виробника");

                            var temp = new Equipment();
                            try
                            {
                                temp.Name = Console.ReadLine();
                                temp.Quantity = Convert.ToInt32(Console.ReadLine());
                                temp.Manufacturer = Console.ReadLine();

                                var pick = dbContext.Equipment.FirstOrDefault(eq => eq.IDeq == index);

                                if (pick != null)
                                {
                                    pick.Name = temp.Name;
                                    pick.Quantity = temp.Quantity;
                                    pick.Manufacturer = temp.Manufacturer;



                                    AuditAlter(pick, dbContext);
                                    dbContext.SaveChanges();

                                }
                                else
                                {
                                    Console.WriteLine("Такого рядка не існує!");
                                }
                            }

                            catch (Exception ex)
                            {
                                Console.WriteLine("Дані не пройшли валідацію");
                            }
                        }
                        if (op == "delete")
                        {
                            Console.WriteLine("Виберіть рядок, що будете видаляти: ");
                            int index = Convert.ToInt32(Console.ReadLine());

                            var pick = dbContext.Equipment.FirstOrDefault(eq => eq.IDeq == index);

                            if (pick != null)
                            {
                                var procedRecordsToDelete = dbContext.Proced.Where(p => p.Equipment_ID == index);
                                dbContext.Proced.RemoveRange(procedRecordsToDelete);

                                eq.Remove(pick);
                                dbContext.Equipment.Remove(pick);



                                dbContext.SaveChanges();

                                AuditDelete(pick);
                            }
                            else
                            {
                                Console.WriteLine("Такого рядка не існує!");
                            }
                        }
                        break;
                    case "dbo.Medicine":
                        if (op == "add") // Визначаємо операцію
                        {
                            Console.WriteLine("Введіть Назву, Виробника та дату Створення/Термін придатності: ");

                            var temp = new Medicine();
                            try
                            {
                                temp.Name = Console.ReadLine();
                                temp.Producer = Console.ReadLine();

                                string dateInput = Console.ReadLine();

                                string date2Input = Console.ReadLine();

                                if (DateTime.TryParse(dateInput, out DateTime createdDate) && DateTime.TryParse(date2Input, out DateTime expDate))
                                {
                                    temp.Created_Date = createdDate;
                                    temp.Expiration_Date = expDate;
                                }
                                else
                                {
                                    Console.WriteLine("Дата не пройшла валідацію, використовуйте рік-місяць-день");
                                }

                                dbContext.Medicine.Add(temp);



                                dbContext.SaveChanges();

                                AuditAdd(temp);
                            }
                            catch (Exception ex) { Console.WriteLine("Дані не пройшли валідацію"); }
                        }
                        if (op == "alter")
                        {
                            Console.WriteLine("Введіть рядок що буде змінено");
                            int index = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Введіть Назву, Виробника та дату Створення/Термін придатності: ");

                            var pick = dbContext.Medicine.FirstOrDefault(med => med.IDmed == index);
                            if (pick != null)
                            {
                                try
                                {
                                    pick.Name = Console.ReadLine();
                                    pick.Producer = Console.ReadLine();

                                    string dateInput = Console.ReadLine();

                                    string date2Input = Console.ReadLine();

                                    if (DateTime.TryParse(dateInput, out DateTime createdDate) && DateTime.TryParse(date2Input, out DateTime expDate))
                                    {
                                        pick.Created_Date = createdDate;
                                        pick.Expiration_Date = expDate;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Дата не пройшла валідацію, використовуйте рік-місяць-день");
                                    }



                                    dbContext.SaveChanges();
                                    AuditAlter(pick, dbContext);
                                    Console.WriteLine();
                                }
                                catch (Exception ex) { Console.WriteLine("Дані не пройшли валідацію"); }
                            }
                        }
                        if (op == "delete")
                        {
                            Console.WriteLine("Виберіть рядок, що будете видаляти: ");

                            int index = Convert.ToInt32(Console.ReadLine());

                            var pick = med.FirstOrDefault(med => med.IDmed == index);

                            if (pick != null)
                            {
                                var procedRecordsToDelete = dbContext.Proced.Where(p => p.Medicine_ID == index);

                                dbContext.Proced.RemoveRange(procedRecordsToDelete);

                                dbContext.Medicine.Remove(pick);



                                dbContext.SaveChanges();

                                AuditDelete(pick);
                            }
                            else
                            {
                                Console.WriteLine("Такого рядка не існує!");
                            }
                        }
                        break;
                    case "dbo.Patient":
                        if (op == "add")
                        {
                            Console.WriteLine("Введіть ПІБ, Вік, Хворобу, Дати прибуття/виписки у форматі рік-місяць-число, відділення та рівень карми:  ");
                            try
                            {
                                var temp = new Patient();
                                temp.Last_Name = Console.ReadLine();
                                temp.First_Name = Console.ReadLine();
                                temp.Middle_Name = Console.ReadLine();
                                temp.Age = Convert.ToInt32(Console.ReadLine());
                                temp.Disease = Console.ReadLine();
                                string Arrival_Date = Console.ReadLine();
                                string Discharge_Date = Console.ReadLine();
                                temp.Ward = Console.ReadLine();
                                var LuckLevel = Console.ReadLine();

                                temp.LuckLevel = LuckLevel == "" ? 1 : Convert.ToInt32(LuckLevel);


                                if (DateTime.TryParse(Arrival_Date, out DateTime createdDate) && DateTime.TryParse(Discharge_Date, out DateTime expDate))
                                {
                                    temp.Arrival_Date = createdDate;
                                    temp.Discharge_Date = expDate;
                                }
                                else
                                {
                                    Console.WriteLine("Дата не пройшла валідацію, використовуйте рік-місяць-день");
                                }
                                dbContext.Patient.Add(temp);



                                dbContext.SaveChanges();

                                AuditAdd(temp);
                            }
                            catch (Exception ex) { Console.WriteLine("Дані не пройшли валідацію"); }

                        }
                        if (op == "alter")
                        {
                            Console.WriteLine("Введіть id рядка, що буде змінений");

                            int index = Convert.ToInt32(Console.ReadLine());
                            var temp = dbContext.Patient.FirstOrDefault(pat => pat.IDpat == index);

                            Console.WriteLine("Введіть ПІБ, Вік, Хворобу, Дати прибуття/виписки у форматі рік-місяць-число, а також відділення ");
                            if (temp != null)
                            {
                                try
                                {
                                    temp.Last_Name = Console.ReadLine();
                                    temp.First_Name = Console.ReadLine();
                                    temp.Middle_Name = Console.ReadLine();
                                    temp.Age = Convert.ToInt32(Console.ReadLine());
                                    temp.Disease = Console.ReadLine();
                                    string Arrival_Date = Console.ReadLine();
                                    string Discharge_Date = Console.ReadLine();
                                    temp.Ward = Console.ReadLine();

                                    if (DateTime.TryParse(Arrival_Date, out DateTime createdDate) && DateTime.TryParse(Discharge_Date, out DateTime expDate))
                                    {
                                        temp.Arrival_Date = createdDate;
                                        temp.Discharge_Date = expDate;



                                        dbContext.SaveChanges();

                                        AuditAlter(temp, dbContext);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Дата не пройшла валідацію, використовуйте рік-місяць-день");
                                    }
                                }
                                catch (Exception ex) { Console.WriteLine("Дані не пройшли валідацію"); }
                            }
                            else { Console.WriteLine("Такого рядка не існує!"); }
                        }
                        if (op == "delete")
                        {
                            Console.WriteLine("Введіть id рядка, що буде видалений");

                            int index = Convert.ToInt32(Console.ReadLine());

                            var temp = dbContext.Patient.FirstOrDefault(pat => pat.IDpat == index);
                            if (temp != null)
                            {
                                var procedRecordsToDelete = dbContext.Proced.Where(p => p.Patient_ID == index);
                                dbContext.Proced.RemoveRange(procedRecordsToDelete);

                                dbContext.Patient.Remove(temp);



                                dbContext.SaveChanges();

                                AuditDelete(temp);
                            }
                            else { Console.WriteLine("Такого рядка не існує"); }
                        }
                        break;
                    case "dbo.Staff":
                        if (op == "add")
                        {
                            var temp = new Staff();

                            Console.WriteLine("Введіть ПІБ, Вік і Посаду ");
                            try
                            {
                                temp.Last_Name = Console.ReadLine();
                                temp.Name = Console.ReadLine();
                                temp.Middle_Name = Console.ReadLine();
                                temp.Age = Convert.ToInt32(Console.ReadLine());
                                temp.Position = Console.ReadLine();

                                dbContext.Staff.Add(temp);



                                dbContext.SaveChanges();

                                AuditAdd(temp);
                            }
                            catch (Exception ex) { Console.WriteLine("Валідація не пройдена"); }
                        }
                        if (op == "alter")
                        {

                            Console.WriteLine("Введіть id рядка, що буде змінений");
                            int index = Convert.ToInt32(Console.ReadLine());

                            Console.WriteLine("Введіть ПІБ, Вік і Посаду ");
                            var pick = dbContext.Staff.FirstOrDefault(st => st.IDstaff == index);
                            if (pick != null)
                            {
                                try
                                {
                                    pick.Last_Name = Console.ReadLine();
                                    pick.Name = Console.ReadLine();
                                    pick.Middle_Name = Console.ReadLine();
                                    pick.Age = Convert.ToInt32(Console.ReadLine());
                                    pick.Position = Console.ReadLine();



                                    dbContext.SaveChanges();

                                    AuditAlter(pick, dbContext);
                                }
                                catch (Exception ex) { Console.WriteLine("Валідація не пройдена"); }
                            }
                        }
                        if (op == "delete")
                        {
                            Console.WriteLine("Введіть id рядка, що буде видалений");

                            int index = Convert.ToInt32(Console.ReadLine());

                            var pick = dbContext.Staff.FirstOrDefault(st => st.IDstaff == index);

                            if (pick != null)
                            {
                                var procedRecordsToDelete = dbContext.Proced.Where(p => p.Staff_ID == index);
                                dbContext.Proced.RemoveRange(procedRecordsToDelete);

                                dbContext.Staff.Remove(pick);



                                dbContext.SaveChanges();

                                AuditDelete(pick);

                                st = dbContext.Staff.ToList();
                            }
                            else { Console.WriteLine("Такого рядка не існує!"); }
                        }
                        break;
                    case "dbo.Proced":
                        if (op == "add")
                        {
                            Console.WriteLine("Введіть ID пацієнта, спорядження, персоналу та ліків, назву та ціну процедури: ");

                            var temp = new Proced();

                            try
                            {
                                temp.Patient_ID = Convert.ToInt32(Console.ReadLine());
                                temp.Equipment_ID = Convert.ToInt32(Console.ReadLine());
                                temp.Staff_ID = Convert.ToInt32(Console.ReadLine());
                                temp.Medicine_ID = Convert.ToInt32(Console.ReadLine());
                                temp.Name = Console.ReadLine();
                                temp.Price = Convert.ToInt32(Console.ReadLine());

                                dbContext.Proced.Add(temp);



                                dbContext.SaveChanges();

                                AuditAdd(temp);
                            }
                            catch (Exception ex) { Console.WriteLine("Дані не пройшли валідацію"); }
                        }
                        if (op == "alter")
                        {
                            Console.WriteLine("Введіть id рядка, що буде змінений");

                            int index = Convert.ToInt32(Console.ReadLine());

                            var pick = dbContext.Proced.FirstOrDefault(proc => proc.IDproc == index);
                            if (pick != null)
                            {
                                try
                                {
                                    Console.WriteLine("Введіть ID пацієнта, спорядження, персоналу та ліків, назву та ціну процедури: ");

                                    pick.Patient_ID = Convert.ToInt32(Console.ReadLine());
                                    pick.Equipment_ID = Convert.ToInt32(Console.ReadLine());
                                    pick.Staff_ID = Convert.ToInt32(Console.ReadLine());
                                    pick.Medicine_ID = Convert.ToInt32(Console.ReadLine());
                                    pick.Name = Console.ReadLine();
                                    pick.Price = Convert.ToInt32(Console.ReadLine());


                                    dbContext.SaveChanges();

                                    AuditAlter(pick, dbContext);
                                }
                                catch (Exception ex) { Console.WriteLine("Дані не пройшли валідацію"); }
                            }
                            else Console.WriteLine("Такого рядка не існує!");
                        }
                        if (op == "delete")
                        {
                            Console.WriteLine("Введіть id рядка, що буде видалений");

                            int index = Convert.ToInt32(Console.ReadLine());
                            var pick = dbContext.Proced.FirstOrDefault(proc => proc.IDproc == index);

                            if (pick != null)
                            {
                                dbContext.Proced.Remove(pick);



                                dbContext.SaveChanges();

                                AuditDelete(pick);
                            }
                            else Console.WriteLine("Такого рядка не існує!");
                        }
                        break;
                    default:
                        Console.WriteLine("Такої таблиці не існує!"); Console.Clear(); throw new ArgumentException("Invalid entity name"); break;
                }
            }
        }
        public static void AuditDelete<T>(T pick) where T : class
        {
            Console.WriteLine("\nВидалено рядок:\n");
            foreach (var propName in pick.GetType().GetProperties())
            {
                if (propName.Name.Contains("ID"))
                {
                    continue;
                }
                Console.WriteLine($"{propName.Name}: {propName.GetValue(pick)}");
            }
            Console.WriteLine("");
        }
        public static void AuditAdd<T>(T pick) where T : class
        {
            Console.WriteLine("\nДодано новий рядок:\n");
            foreach (var propName in pick.GetType().GetProperties())
            {
                if (propName.Name.Contains("ID"))
                {
                    continue;
                }
                Console.WriteLine($"{propName.Name}: {propName.GetValue(pick)}");
            }
            Console.WriteLine("");
        }
        public static void AuditAlter<T>(T pick, CodeFirst dbContext) where T : class
        {
            foreach (var propName in dbContext.Entry(pick).OriginalValues.PropertyNames)
            {
                if (propName.Contains("ID"))
                {
                    Console.WriteLine($"\nЗміни в {dbContext.Entry(pick).CurrentValues[propName]} рядку:\n");
                    continue;
                }
                var originalValue = dbContext.Entry(pick).OriginalValues[propName];
                var currentValue = dbContext.Entry(pick).CurrentValues[propName];
                if (Equals(originalValue, currentValue)) Console.WriteLine($"{propName}: {currentValue} (Без змін)");
                else Console.WriteLine($"{propName}: {originalValue} => {currentValue}");
            }
            Console.WriteLine("");
        }
        public static void Display(string tablename)
        {
            using (var dbContext = new CodeFirst())
            {
                var pat = dbContext.Patient.ToList();
                var eq = dbContext.Equipment.ToList();
                var med = dbContext.Medicine.ToList();
                var st = dbContext.Staff.ToList();
                var proc = dbContext.Proced.ToList();

                if (tablename == "dbo.Patient")
                {
                    Console.WriteLine("Таблиця Пацієнти:");
                    foreach (var p in pat)
                    {
                        Console.WriteLine($"ID: {p.IDpat}");
                        Console.WriteLine($"ПІБ: {p.Last_Name} {p.First_Name} {p.Middle_Name}");
                        Console.WriteLine($"Вік: {p.Age}");
                        Console.WriteLine($"Хвороба: {p.Disease}");
                        Console.WriteLine($"Відділення: {p.Ward}");
                        Console.WriteLine($"Дата прибуття: {p.Arrival_Date}");
                        Console.WriteLine($"Дата виписки: {p.Discharge_Date}");
                        Console.WriteLine($"Рівень карми: {p.LuckLevel}");
                        Console.WriteLine();
                    }
                }
                if (tablename == "dbo.Equipment")
                {
                    Console.WriteLine("Таблиця Спорядження:");
                    foreach (var e in eq)
                    {
                        Console.WriteLine($"ID: {e.IDeq}");
                        Console.WriteLine($"Назва: {e.Name}");
                        Console.WriteLine($"Виробник: {e.Manufacturer}");
                        Console.WriteLine($"Кількість: {e.Quantity}");
                        Console.WriteLine();
                    }
                }
                if (tablename == "dbo.Medicine")
                {
                    Console.WriteLine("Таблиця Ліки:");
                    foreach (var m in med)
                    {
                        Console.WriteLine($"ID: {m.IDmed}");
                        Console.WriteLine($"Назва: {m.Name}");
                        Console.WriteLine($"Виробник: {m.Producer}");
                        Console.WriteLine($"Дата виготовлення: {m.Created_Date}");
                        Console.WriteLine($"Термін придатності: {m.Expiration_Date}");
                        Console.WriteLine();
                    }
                }
                if (tablename == "dbo.Staff")
                {
                    Console.WriteLine("Таблиця Персонал:");
                    foreach (var s in st)
                    {
                        Console.WriteLine($"ID: {s.IDstaff}");
                        Console.WriteLine($"ПІБ: {s.Last_Name} {s.Name} {s.Middle_Name}");
                        Console.WriteLine($"Вік: {s.Age}");
                        Console.WriteLine($"Посада: {s.Position}");
                        Console.WriteLine();
                    }
                }
                if (tablename == "dbo.Proced")
                {
                    Console.WriteLine("Таблиця процедури:");
                    foreach (var p in proc)
                    {
                        Console.WriteLine($"ID: {p.IDproc}");
                        Console.WriteLine($"Пацієнт: {p.Patient.Last_Name} {p.Patient.First_Name} {p.Patient.Middle_Name}");
                        Console.WriteLine($"Тип процедури: {p.Name}");
                        Console.WriteLine($"Спорядження для процедури: {p.Equipment.Name}");
                        Console.WriteLine($"Ліки для процедури: {p.Medicine.Name}");
                        Console.WriteLine($"Процедуру проводив: {p.Staff.Last_Name} {p.Staff.Name} {p.Staff.Middle_Name}");
                        Console.WriteLine($"Ціна: {p.Price}");
                        Console.WriteLine();
                    }
                }
            }
        }
        public static void Menu_operations(string tableName, List<Equipment> eq, List<Medicine> med, List<Staff> st, List<Patient> pat, List<Proced> proc) // Меню операцій
        {
            Console.WriteLine("Виберіть наступну операцію:\n1)Додати дані до таблиці\n2)Видалити дані з таблиці\n3)Редагувати стрічку таблиці\n4)Повернутись");

            string a = Console.ReadLine();
            switch (a)
            {
                case "1":
                    AlterDataTables(tableName, "add", eq, med, st, pat, proc); // таблиця і "вид" операції
                    break;

                case "2":
                    AlterDataTables(tableName, "delete", eq, med, st, pat, proc);
                    break;

                case "3":
                    AlterDataTables(tableName, "alter", eq, med, st, pat, proc);
                    break;

                case "4":
                    Menu_pickTables(eq, med, st, pat, proc);
                    break;

                default:
                    Console.Clear();
                    Console.WriteLine("Неправильний вибір!");
                    break;
            }
        }
        public static List<object> Menu_pickTables(List<Equipment> eq, List<Medicine> med, List<Staff> st, List<Patient> pat, List<Proced> proc) // Меню вибору таблиць
        {
            Console.WriteLine("Введіть назву таблиці\n1)dbo.Equipment \n2)dbo.Medicine \n3)dbo.Staff \n4)dbo.Patient \n5)dbo.Proced\n6)Повернутись");

            string tableName = Console.ReadLine();
            List<object> obj = new List<object>();
            switch (tableName)
            {
                case "1":
                    tableName = "dbo.Equipment";
                    Display(tableName);
                    Menu_operations(tableName, eq, med, st, pat, proc);
                    break;
                case "2":
                    tableName = "dbo.Medicine";
                    Display(tableName);
                    Menu_operations(tableName, eq, med, st, pat, proc);
                    break;
                case "3":
                    tableName = "dbo.Staff";
                    Display(tableName);
                    Menu_operations(tableName, eq, med, st, pat, proc);
                    break;
                case "4":
                    tableName = "dbo.Patient";
                    Display(tableName);
                    Menu_operations(tableName, eq, med, st, pat, proc);
                    break;
                case "5":
                    tableName = "dbo.Proced";
                    Display(tableName);
                    Menu_operations(tableName, eq, med, st, pat, proc);
                    break;
                case "6":
                    Console.Clear();
                    Menu_main(eq, med, st, pat, proc);
                    break;
                default:
                    Console.Clear();
                    Console.WriteLine("Неправильний вибір!");
                    break;
            }
            return obj;
        }
        public static void Menu_main(List<Equipment> eq, List<Medicine> med, List<Staff> st, List<Patient> pat, List<Proced> proc) // Головне меню
        {
            bool men = true;
            while (men = true)
            {
                Console.WriteLine("1)Робота з таблицями\n2)Вихід");
                string choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        Console.Clear();
                        Menu_pickTables(eq, med, st, pat, proc);
                        break;

                    case "2":
                        men = false;
                        return;
                        break;

                    default:
                        Console.WriteLine("Неправильний вибір!");
                        men = false;
                        break;
                }
            }
        }
    }
}