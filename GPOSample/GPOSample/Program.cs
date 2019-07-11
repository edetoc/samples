using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.GroupPolicy;

namespace GPOSample
{
    class Program
    {
        static void Main(string[] args)
        {
            try 
            {
                var domain = new GPDomain("edetoc.lab");

                var gpo = domain.GetGpo("xxxGPO");
                var somcollection = domain.SearchSoms (gpo);

                foreach (var som in somcollection)
                {
                    //Console.WriteLine(som.Name);
                    //Console.WriteLine(som.Path);

                    foreach (var link in som.GpoLinks )
                    {
                        if (Guid.Equals(gpo.Id, link.GpoId))
                        {
                            Console.WriteLine("Need to suppress link to "
                                                    + link.DisplayName // GPO name
                                                    + " in "
                                                    + link.Target);    // path

                            link.Delete();
                            Console.WriteLine("link suppressed");

                        }
                        
                    }
                }

                // We have suppressed all links to the GPO in the domain
                // we can now suppress the GPO object itself

                gpo.Delete ();
                Console.WriteLine("xxxGPO deleted with success.");
            
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            
            
        }
    }
}
