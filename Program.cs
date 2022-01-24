using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DHCP
{
  class Program
  {
    static List<int> kizart = new List<int>();
    static Dictionary<string, int> lefoglalt = new Dictionary<string, int>();
    static Dictionary<string, int> DHCP = new Dictionary<string, int>();
    static string halozat = "192.168.10.";
    static string muvelet;
    static string MAC;
    static int elsoKioszthatoIP = 100;
    static int utolsoKioszthatoIP = 199;
    static int IPCim;
    static void Main(string[] args)
    {
      Beolvasas("excluded.csv");
      Beolvasas("reserved.csv");
      Beolvasas("dhcp.csv");

      foreach (string be in File.ReadAllLines("test.csv"))
      {
        muvelet = be.Split(';')[0];
        // Ha a művelet "request"
        if (muvelet == "request")
        {
          MAC = be.Split(';')[1];
          // Ha nem tartalmazza a DHCP a MAC-címet
          if (!DHCP.ContainsKey(MAC))
          {
            // Ha a legfoglalt (reserved) tartalmazza a MAC-címet
            if (lefoglalt.ContainsKey(MAC))
            {
              // A lefoglalt IP cím eltárolása az IPCim változóban
              IPCim = lefoglalt[MAC];
              // Ha a DHCP lista nem tartalmazza az eltárolt IP címet
              if (!DHCP.ContainsValue(IPCim))
              {
                // Adja hozzá a DHCP listához a MAC címet az IP címmel együtt
                DHCP.Add(MAC, IPCim);
              }
            }
            // Ha a legfoglalt (reserved) nem tartalmazza a MAC-címet
            else
            {
              // Elkezdjük figyelni a legelső kiosztható IP-címet
              IPCim = elsoKioszthatoIP;
              // Amíg a figyelendő IP cím kisebb, mint az utolsó kiosztható IP
              while (IPCim <= utolsoKioszthatoIP)
              {
                // Ha a DHCP lista nem tartalmazza az IP címet
                if (!DHCP.ContainsValue(IPCim))
                {
                  // Ha az IP cím nincs a kizárások között
                  if (!kizart.Contains(IPCim))
                  {
                    // Ha az IP cím nincs lefoglalva
                    if (!lefoglalt.ContainsValue(IPCim))
                    {
                      // Akkor adja hozzá a DHCP listához a MAC címet és IP címet
                      DHCP.Add(MAC, IPCim);
                      break;
                    }
                  }
                }
                // Ha egyik sem volt, akkor növelje a vizsgálandó IP címet eggyel és újra ellenőrizze
                IPCim++;
              }
              //Ha az IP cím túllépi az utolsó kiosztható IP-t, akkor nem tudott IP címet kiosztani -> hibát dob
              if (IPCim > utolsoKioszthatoIP)
                throw (new Exception("Sikertelen IP-cím kiosztás!"));
            }
          }
        }
        else
        {
          IPCim = int.Parse(be.Split(';', '.')[4]);
          if (DHCP.ContainsValue(IPCim))
          {
            DHCP.Remove(DHCP.First(x => x.Value == IPCim).Key);
          }
        }
      }

      StreamWriter ki = new StreamWriter("dhcp_kesz.csv");
      foreach (var d in DHCP)
      {
        ki.WriteLine($"{d.Key};{halozat}{d.Value}");
      }
      ki.Close();


      Console.ReadKey();
    }

    private static void Beolvasas(string fajlNev)
    {
      StreamReader be = new StreamReader(fajlNev);
      while (!be.EndOfStream)
      {
        if (fajlNev == "excluded.csv")
        {
          kizart.Add(Convert.ToInt32(be.ReadLine().Split('.')[3]));
        }
        else if (fajlNev == "reserved.csv")
        {
          lefoglalt.Add(be.ReadLine().Split(';')[0], Convert.ToInt32(be.ReadLine().Split(';', '.')[4]));
        }
        else if (fajlNev == "dhcp.csv")
        {
          DHCP.Add(be.ReadLine().Split(';')[0], Convert.ToInt32(be.ReadLine().Split(';', '.')[4]));
        }
      }
      be.Close();
    }
  }
}
