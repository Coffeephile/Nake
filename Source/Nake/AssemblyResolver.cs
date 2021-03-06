﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Nake
{
    class AssemblyResolver
    {
        public static volatile bool Resolve = false;

        static readonly List<RoslynAssembly> roslynAssemblies;
        static readonly ConcurrentDictionary<string, Assembly> resolvedAssemblies = new ConcurrentDictionary<string, Assembly>();

        static readonly string currentDir;

        static AssemblyResolver()
        {
            currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            roslynAssemblies = new List<RoslynAssembly>
            {
                new RoslynAssembly("Roslyn.Compilers", "Roslyn.Compilers.Common"),
                new RoslynAssembly("Roslyn.Compilers.CSharp", "Roslyn.Compilers.CSharp")
            };
        }

        public static void Register()
        {
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly; 
        }

        public static void Unregister()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveAssembly; 
        }

        static Assembly ResolveAssembly(object sender, ResolveEventArgs args)
        {
            if (!Resolve)
                return null;

            return resolvedAssemblies.GetOrAdd(args.Name, name =>
            {
                var assemblyName = new AssemblyName(args.Name);

                var roslynAssembly = roslynAssemblies
                    .Find(x => x.AssemblyName == assemblyName.Name);

                Unregister();

                return roslynAssembly.Load();
            });
        }

        class RoslynAssembly
        {
            public readonly string AssemblyName;
            readonly string assemblyPath;

            public RoslynAssembly(string assemblyName, string packageName)
            {
                AssemblyName = assemblyName;

                assemblyPath = Path.Combine(currentDir, 
                    string.Format(@"Roslyn\{0}.1.2.20906.2\lib\net45\{1}.dll", packageName, assemblyName));
            }

            public Assembly Load()
            {
                return Assembly.LoadFrom(assemblyPath);
            }
        }
    }
}
