using Ice.Project.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example {
    class Program {
        static void Main(string[] args) {
            string dir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../data");
            LoaderHelp.Load(dir);

            int[] heroIds = { 1, 2, 3 };
            foreach(int id in heroIds) {
                var heroTemplate = HeroTemplateManager.Instance.GetItemTemplate(id);
                if(heroTemplate != null) {
                    Console.WriteLine("id is {0}, sex is {1}, height is {2}", heroTemplate.Id, heroTemplate.Sex, heroTemplate.Height);
                }
            }
        }
    }
}
