using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace TrimTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var strings = new List<string>(){"Testing String 1     ", "Testing String 2  ", "Testing String 3"};
            Test test = new Test(strings, "Non Collection String   ", 10, "Nested String      ");
            Console.WriteLine("Pre trim:");
            Console.WriteLine("\tStrings:");
            Console.WriteLine($"\t\t\"{test.Strings.ElementAt(0)}\"");
            Console.WriteLine($"\t\t\"{test.Strings.ElementAt(1)}\"");
            Console.WriteLine($"\t\t\"{test.Strings.ElementAt(2)}\"");
            Console.WriteLine($"\tNon Collection String: \"{test.TestStr}\"");
            Console.WriteLine($"\tInt: {test.Int}");
            Console.WriteLine($"\tNested Object:");
            Console.WriteLine($"\t\tNested Object String: \"{test.NestedObj.NestedString}\"");
            Console.WriteLine($"\t\tNested Object Non-String: {test.NestedObj.TestDateTime:d}");
            test = test.TrimProp();
            Console.WriteLine("\nPost Trim:");
            Console.WriteLine("\tStrings:");
            Console.WriteLine($"\t\t\"{test.Strings.ElementAt(0)}\"");
            Console.WriteLine($"\t\t\"{test.Strings.ElementAt(1)}\"");
            Console.WriteLine($"\t\t\"{test.Strings.ElementAt(2)}\"");
            Console.WriteLine($"\tNon Collection String: \"{test.TestStr}\"");
            Console.WriteLine($"\tInt: {test.Int}");
            Console.WriteLine($"\tNested Object:");
            Console.WriteLine($"\t\tNested Object String: \"{test.NestedObj.NestedString}\"");
            Console.WriteLine($"\t\tNested Object Non-String: {test.NestedObj.TestDateTime:d}");

            Console.ReadLine();
        }
    }

    public class DoTrim : System.Attribute
    {
        public bool ShouldDoTrim { get; }

        public DoTrim()
        {
            this.ShouldDoTrim = true;
        }

        public DoTrim(bool shouldDoTrim)
        {
            this.ShouldDoTrim = shouldDoTrim;
        }
    }

    public class Test
    {
        [DoTrim]
        public ICollection<string> Strings { get; set; }
        public string TestStr { get; set; }
        public int Int { get; set; }
        [DoTrim]
        public Test2 NestedObj { get; set; }
        public Test(ICollection<string> strings, string testStr, int i, string nestedString)
        {
            Strings = strings;
            TestStr = testStr;
            Int = i;
            NestedObj = new Test2(nestedString);
        }

        public override string ToString()
        {
            return
                $"Strings: \n\t'{Strings.ElementAt(0)}'\n\t'{Strings.ElementAt(1)}'\n\t'{Strings.ElementAt(2)}'\nNonColString: {TestStr}\nInt: {Int}\nNested Obj:\n\tString: {NestedObj.NestedString}\n\tDateTime: {NestedObj.TestDateTime:d}";
        }
    }

    public class Test2
    {
        public Test2(string nestedString)
        {
            NestedString = nestedString;
            TestDateTime = DateTime.Now;
        }

        public string NestedString { get; set; }
        public DateTime TestDateTime { get; set; }
        public override string ToString()
        {
            return $"NestedString: \n\t'{NestedString}' \n\tNested Object: {TestDateTime:d}";
        }
    }

    static class TrimTest
    {
        public static TSelf TrimProp<TSelf>(this TSelf input)
        {
            var properties = input
                .GetType()
                .GetProperties();

            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string))
                {
                    var cVal = (string)property.GetValue(input, null);
                    if (cVal != null)
                    {
                        property.SetValue(input, cVal.Trim(), null);
                    }
                }
                else if (!property.PropertyType.IsPrimitive)
                {
                    DoTrim dotrim = property
                        .GetCustomAttributes(true)
                        .SingleOrDefault(obj => obj is DoTrim) as DoTrim;
                    if (dotrim == null || !dotrim.ShouldDoTrim) continue;
                    if (property.IsCollection())
                    {
                        var values = property.GetValue(input, null) as IList;
                        if (values == null || values.Count <= 0) continue;
            
                        if (values[0] is string)
                        {
                            property.SetValue(input, (values as ICollection<string>).TrimCollection(), null);
                        }else if (!values[0].GetType().IsPrimitive)
                        {
                            foreach (var value in values)
                            {
                                values[values.IndexOf(value)] = values[values.IndexOf(value)].TrimProp();
                            }
                            property.SetValue(input, values, null);
                        }
                    }
                    else
                    {
                        property.SetValue(input, property.GetValue(input, null).TrimProp(), null);
                    }
                }
            }
            return input;
        }

        public static ICollection<string> TrimCollection(this ICollection<string> collection)
        {
            ICollection<string> newCol = new List<string>();
            foreach (var str in collection)
            {
                newCol.Add(str.Trim());
            }
            return newCol;
        }

        public static bool IsCollection(this PropertyInfo propertyInfo)
        {
            return propertyInfo.PropertyType.IsGenericType &&
                   propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>);
        }
    }
}
