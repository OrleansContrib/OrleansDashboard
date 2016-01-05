using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrleansDashboard
{
    public class Router
    {
        public Router()
        {
            Routes = new Dictionary<string, Func<IOwinContext, IDictionary<string, string>, Task>>();
        }

        public IDictionary<string, Func<IOwinContext, IDictionary<string, string>, Task>> Routes { get; private set; }

        public void Add(string pattern, Func<IOwinContext, IDictionary<string, string>, Task> func)
        {
            Routes.Add(pattern, func);
        }

        public Func<IOwinContext, Task> Match(string path)
        {
            foreach (var route in Routes)
            {
                var result = AreMatch(path, route.Key);
                if (null == result) continue;
                return new Func<IOwinContext, Task>(x => route.Value(x, result));
            }
            return null;
        }

        // i.e matches /foo/bar/baz with /foo/:bar/:baz
        public IDictionary<string, string> AreMatch(string path, string route)
        {
            var pathParts = path.Split('/');
            var routeParts = route.Split('/');

            if (pathParts.Length != routeParts.Length) return null;

            var dictionary = new Dictionary<string, string>();
            for (var i = 0; i < pathParts.Length; i++)
            {
                var routePart = routeParts[i];
                var pathPart = pathParts[i];
                if (routePart.StartsWith(":"))
                {
                    dictionary.Add(routePart.Substring(1), pathPart);
                    continue;
                }
                if (routePart != pathPart)
                {
                    return null;
                }
            }
            return dictionary;
        }


    }
}
