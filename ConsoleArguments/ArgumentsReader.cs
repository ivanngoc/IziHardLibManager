using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace IziHardGames.ConsoleArguments
{
    public class DefaultPath
    {
        public readonly char prefixShort;
        public readonly string prefFull;
        public readonly char description;

        public static readonly DefaultPath Verrbose;
        public static readonly DefaultPath Path;

        static DefaultPath()
        {
            Verrbose = new DefaultPath('v', "verbose", "Show logs in console");
            Path = new DefaultPath('p', "path", "specify path");
        }

        internal DefaultPath(char prefixShort, string prefFull, string desciption)
        {
            this.prefixShort = prefixShort;
            this.prefFull = prefFull;
        }
    }

    /// <summary>
    /// Variants:<br/>
    /// Some.exe -aThisIsValue
    /// Some.exe -a ThisIsValue
    /// Some.exe --all=ThisIsValue
    /// Some.exe --all = ThisIsValue
    /// </summary>
    /*
            await Execute(new string[] { "-e", "asdl;kaodpoapd[a[sd\"asdpkasopkdpksapd" });

            await Execute(new string[] { "--dll", "=", "asdl;kaodpoapd[a[sd\"asdpkasopkdpksapd" });
            await Execute(new string[] { "--dll=", "asdl;kaodpoapd[a[sd\"asdpkasopkdpksapd" });
            await Execute(new string[] { "--dll", "=asdl;kaodpoapd[a[sd\"asdpkasopkdpksapd" });
            await Execute(new string[] { "--dll=asdl;kaodpoapd[a[sd\"asdpkasopkdpksapd" });     
     */
    public class ArgumentsReader
    {
        public const int MAGIC = 44;
        private static readonly List<DefaultPath> byDefaults;
        private readonly List<Argument> arguments = new List<Argument>();
        private Argument? current;

        static ArgumentsReader()
        {
            byDefaults = new List<DefaultPath>();
            byDefaults.Add(DefaultPath.Verrbose);
            byDefaults.Add(DefaultPath.Path);
        }

        public void Parse(string[] args)
        {
            int start = default;
            int next = default;
            bool isWaitEquals = default;
            bool nextArgIsValue = default;
            bool isReadString = default;


            for (int i = 0; i < args.Length; i++)
            {
                bool isReadValue = default;
                string line = args[i];
                Console.WriteLine($"i:{i}. line:{line}. {GetType().FullName}");
                for (int j = 0; j < line.Length; j++)
                {
                    Console.WriteLine($"J: {line[j]}. {GetType().FullName}");
                    var sliceFromJ = line.AsSpan().Slice(j);
                    if (nextArgIsValue)
                    {
                        current!.argRecieved += line;
                        current = null;
                        nextArgIsValue = false;
                        break;
                    }

                    if (current == null)
                    {
                        Argument? argument = default;
                        if (sliceFromJ.StartsWith("--"))
                        {
                            int indexStartK = j + 2;
                            var indexEnd = line.Length;
                            int k = indexStartK;
                            isWaitEquals = true;

                            for (; k < line.Length; k++)
                            {
                                if (line[k] == '=')
                                {
                                    indexEnd = k;
                                    isWaitEquals = false;
                                    break;
                                }
                            }
                            string argFullName = line.Substring(indexStartK, indexEnd - indexStartK).Trim();

                            for (int m = 0; m < arguments.Count; m++)
                            {
                                if (arguments[m].prefixFull == argFullName)
                                {
                                    argument = arguments[m];
                                    break;
                                }
                            }
                            if (argument == null) throw new NullReferenceException($"Not founded for {argFullName}");
                            argument!.prefixRecieved = "--" + argFullName;
                            j = k;
                        }
                        else if (line[j] == '-')
                        {
                            char control = line[j + 1];
                            argument = arguments.First(x => x.prefixShort == control);
                            argument.prefixRecieved = $"-{control}";
                            start = j + 2;
                            j = start;
                        }
                        argument!.isFired = true;
                        current = argument;
                        isReadValue = true;
                    }
                    else
                    {
                        if (isWaitEquals)
                        {
                            if (line[j] == ' ')
                            {
                                continue;
                            }
                            else if (line[j] == '=')
                            {
                                isWaitEquals = false;
                                int indexStartK = j + 1;
                                int length = line.Length - indexStartK;
                                if (length > 0)
                                {
                                    current.argRecieved = line.Substring(indexStartK, length);
                                    current = null;
                                    break;
                                }
                                else
                                {
                                    nextArgIsValue = true;
                                    break;
                                }
                            }
                        }
                        else if (isReadValue)
                        {
                            current.argRecieved += line.Substring(j, line.Length - j);
                            j = line.Length;
                            current = null;
                        }
                        else
                        {
                            if (sliceFromJ.StartsWith("--"))
                            {
                                next = j;
                                this.current = null;
                                j--;
                            }
                            else if (line[j] == '-')
                            {
                                next = j;
                                this.current = null;
                                j--;
                            }
                            else if (line[j] == '"')
                            {
                                isReadString = true;
                                if (line.Last() == '"')
                                {
                                    current!.argRecieved += line.Substring(j + 1, j - (j + 1));
                                    current = null;
                                    j = line.Length;
                                }
                                else
                                {

                                }
                            }
                            else
                            {
                                current!.argRecieved += line[j];
                            }
                        }
                    }
                }
            }
            if (current != null)
            {
                current.isFired = true;
                current = null;
            }
        }


        public void Regist(char shortPrefix, string fullPrefix, Action<Argument> command)
        {
            Check(shortPrefix, fullPrefix);
            var c = new Argument(shortPrefix, fullPrefix)
            {
                handler = command,
            };
            arguments.Add(c);
        }

        public void Regist(char shortPrefix, string fullPrefix)
        {
            Check(shortPrefix, fullPrefix);
            var c = new Argument(shortPrefix, fullPrefix);
            arguments.Add(c);
        }



        public void Regist(DefaultPath defaultPath)
        {
            Check(defaultPath.prefixShort, defaultPath.prefFull);

            var c = new Argument(defaultPath);
            arguments.Add(c);
        }

        private void Check(char shortPrefix, string fullPrefix)
        {
            bool isNotWildCardShort = shortPrefix != '*';
            if (arguments.Any(x => (x.prefixShort == shortPrefix && isNotWildCardShort) || string.Equals(x.prefixFull, fullPrefix, StringComparison.InvariantCultureIgnoreCase))) throw new InvalidOperationException($"Prefix already exists. shortPrefix:{shortPrefix}. fullPrefix:{fullPrefix}");
        }
        public Argument[] GetFiredArguments()
        {
            return arguments.Where(x => x.IsFired).ToArray();
        }

        public string ToStringInfo()
        {
            var v = GetFiredArguments();
            if (v.Length > 0)
            {
                return v.Select(x => x.ToStringInfo()).Aggregate((x, y) => x + Environment.NewLine + y);
            }
            return "No Fired Arguments";
        }

        public void UseDefaultVerbose()
        {
            Regist('v', "verbose", ShowVerbose);
        }
        private void ShowVerbose(Argument command)
        {
            Console.WriteLine($"prefix:{command.prefixRecieved}; arg:{command.argRecieved}; handlerTarget:{command.handler?.Target.GetType().FullName ?? "NoRegisteredTarget"}; method:{command.handler?.Method.Name ?? "No Method"}");
        }

        public Argument Find(string full)
        {
            throw new System.NotImplementedException();
        }
        public Argument Find(char single, string full)
        {
            throw new System.NotImplementedException();
        }
        public Argument Find(char single)
        {
            throw new System.NotImplementedException();
        }
    }

    public class ArgumentArchytype
    {
        public Argument[] arguments = Array.Empty<Argument>();
    }

    public class Argument
    {
        /// <summary>
        /// Start with '-' (always one char)
        /// </summary>
        public readonly char prefixShort;
        /// <summary>
        /// start with '--'
        /// </summary>
        public readonly string prefixFull = string.Empty;

        public string prefixRecieved = string.Empty;
        public string argRecieved = string.Empty;

        internal Action<Argument>? handler;

        internal bool isFired;
        public const char WILDCARD = '*';

        public bool IsFired => isFired;

        internal Argument(DefaultPath defaultPath)
        {
            this.prefixShort = defaultPath.prefixShort;
            this.prefixFull = defaultPath.prefFull;
        }

        internal Argument(char shortPrefix, string fullPrefix)
        {
            this.prefixShort = shortPrefix;
            this.prefixFull = fullPrefix;
        }

        internal string ToStringInfo()
        {
            return $"-{prefixShort}; --{prefixFull}; prefixRecieved:{prefixRecieved}; argRecieved:{argRecieved}";
        }
    }
}
