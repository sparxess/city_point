using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;

namespace City_Point
{
    class Point
    {
        public DateTime data;
        public int? speed;
        public double? lon;
        public double? lan;

        public Point(string data, string speed, string lon, string lan)
        {
            this.data = DateTime.ParseExact(data, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

            if (speed == "NULL") this.speed = null;
            else this.speed = int.Parse(speed, CultureInfo.InvariantCulture);
            if (lon == "NULL") this.lon = null;
            else this.lon = double.Parse(lon, CultureInfo.InvariantCulture);
            if (lan == "NULL") this.lan = null;
            else this.lan = double.Parse(lan, CultureInfo.InvariantCulture);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                List<Point> point = new List<Point>();
                string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string[] file_list = Directory.GetFiles(path, $"*.csv");

                using (StreamWriter sw = new StreamWriter(@"out.csv", false, Encoding.GetEncoding(1251)))
                {
                    sw.WriteLine("Имя файла;Кол-во Посещений");
                }
                foreach (string f in file_list)
                {
                    if (Path.GetFileName(f) != "out.csv")
                    {
                        using (StreamReader sr = new StreamReader(f))
                        {
                            while (!sr.EndOfStream)
                            {
                                string[] str = sr.ReadLine().Split(';');
                                point.Add(new Point(str[0], str[1], str[2], str[3]));
                            }
                        }

                        //вывод в созданный файл out.csv имени и кол-ва посещений контрольной зоны для каждого файла текующей директории 
                        using (StreamWriter sw = new StreamWriter(@"out.csv", true, Encoding.GetEncoding(1251)))
                        {
                            sw.WriteLine($"{Path.GetFileName(f)};{GetCount(point)}");
                            point.Clear();
                        }
                    }
                }
                Console.WriteLine("Данные успешно добавлены в файл out.csv");      
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            Console.ReadKey();
        }

        // подсчет количества въездов в контрольную зону за указанный период
        public static int GetCount(List<Point> point)
        {
            DateTime StartTime = DateTime.ParseExact("2016-04-01 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            DateTime EndTime = DateTime.ParseExact("2016-10-01 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            double CenterLon = 32.02394;
            double CenterLan = 54.8707;
            double rad = 5000;
            int count = 0;
            double? PrevLon = null;
            double? PrevLan = null;
            double PrevDistance = rad;
            double Distance = rad;
            foreach (Point p in point)
            {
                if ((PrevLon != null) && (PrevLan != null))
                    PrevDistance = GetGeoDistance(PrevLon, PrevLan, CenterLon, CenterLan);
                if ((p.lon != null) && (p.lan != null))
                    Distance = GetGeoDistance(p.lon, p.lan, CenterLon, CenterLan);
                if ((StartTime <= p.data) && (EndTime >= p.data))
                {
                    if ((PrevDistance > rad) && (Distance <= rad))
                    {
                        count++;
                    }
                    //Console.WriteLine($"{p.data}\t{PrevDistance}\t{Distance}");
                }
                PrevLon = p.lon;
                PrevLan = p.lan;
            }
            //Console.WriteLine(count);
            return count;
        }

        // расстояние в метрах между двумя точками
        public static double GetGeoDistance(double? lon1, double? lat1, double lon2, double lat2)
        {
            const double gpsEarthRadius = 6371000;
            const double pi = Math.PI;
            var latRad1 = lat1 * pi / 180;
            var lonRad1 = lon1 * pi / 180;
            var latRad2 = lat2 * pi / 180;
            var lonRad2 = lon2 * pi / 180;
            var angle = Math.Cos((double)latRad1) * Math.Cos((double)latRad2) * Math.Pow(Math.Sin((double)(lonRad1 - lonRad2)
            / 2), 2) + Math.Pow(Math.Sin((double)(latRad1 - latRad2) / 2), 2);
            angle = 2 * Math.Asin(Math.Sqrt(angle));
            return Math.Abs(angle * gpsEarthRadius);
        }
    }
}
