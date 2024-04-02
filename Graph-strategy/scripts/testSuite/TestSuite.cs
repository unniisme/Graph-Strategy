using System;
using System.Reflection;

namespace Tests
{
    // Custom attribute to mark test functions
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute { }

    // Test runner class
    public static class TestRunner
    {
        public static void RunTests()
        {
            Type testClassType = typeof(TestClass);
            MethodInfo[] methods = testClassType.GetMethods();

            foreach (var method in methods)
            {
                if (method.GetCustomAttributes(typeof(TestAttribute), true).Length > 0)
                {
                    try
                    {
                        method.Invoke(null, null);
                        Console.WriteLine($"Test '{method.Name}' passed.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Test '{method.Name}' failed: {ex.Message}");
                    }
                }
            }
        }
    }

    public class TestClass
    {
    }
}