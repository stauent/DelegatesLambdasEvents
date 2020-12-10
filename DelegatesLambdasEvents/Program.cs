using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;

// A delegate specifies the template/contract for a method signature
public delegate void SpeakDelegate(string WhatToSay);


public class Program
{
    public static void Main()
    {
        TestClass test = new TestClass();

        // Executing a method directly is pretty straight forward.
        // What if we don't want to execute these methods now,
        // but want to execute them when some event occurs?
        // This is where delegates come in.
        ImSpeakingNow("How are you?");
        MathMethods.MathSpeaks("I'm doing fine");

        // How did we get from methods to delegates to lambdas? How are they related?
        // We use delegates to reference any method that has a specific signature.
        // As long as the signatures match, we can reference any method
        // and execute it using the delegate.

        // Once upon a time you would create a delegate using object constructor syntax.
        // This creates a reference to a method, which can be executed at any time.
        SpeakDelegate me = new SpeakDelegate(ImSpeakingNow);
        SpeakDelegate math = new SpeakDelegate(MathMethods.MathSpeaks);
        Action<string,int> xxx = MathMethods.NewMethod;
        xxx("hello",9);

        Action<string, int[]> newAction = MathMethods.NewMethod2;
        int[] myints = new int[]{9,8,7};
        newAction("I an another action", myints);

        Func<string, int, int> myFunc = MathMethods.myFunc;
        myFunc += MathMethods.myFunc2;
        int length = myFunc("Paul",60);

        // Now execute the method you're referencing using the delegate it's mapped to.
        me("What a sunny day");
        math("I like to count");

        // Using the object constructor syntax was a little cumbersome, so 
        // "implied conversion" was introduced. The compiler knows that 
        // the method "TalkingTest" has the same signature as the SpeakDelegate
        // so it performs all the heavy lifting under the covers and allows
        // you to simply assign a method to a delegate.
        SpeakDelegate abc = test.TalkingTest;
        abc("I'm new");
        me = test.TalkingTest;

        // A Multicast Delegate is a delegate that holds the references of more than one function. 
        // When a multicast delegate is executed, then all the functions which are referenced by 
        // the delegate are going to be executed.
        me += ImSpeakingNow;
        me += MathMethods.MathSpeaks;

        // Notice that all 3 methods that this deletate references are executed with one line of code.
        // Also notice that all 3 methods are called synchronously!
        me("We're speaking the same language");

        // Example of passing a delegate as a parameter to a method
        ILikeDelegates(me, "All my delegates should say this");
        ILikeDelegates(ImSpeakingNow, "All my delegates should say this");

        // We can remove method references from the delegate to have as few or as many
        // references in the delegate that we want.
        me -= ImSpeakingNow;
        me -= MathMethods.MathSpeaks;

        me("Just me now");

        // Here are a couple more examples of using delegates
        MathMethods.DoMathDelegate doMath = test.AddThese;
        int Total = doMath(4, 8);
        Console.WriteLine($"Total of 4+8 = {Total}");

        // An "Action" is a predefined delegate that takes 0 or more parameters, does SOMETHING and returns void.
        // An Action can take no parameter or 
        Action someAction = test.DoSomething;
        someAction();

        // Events help implement the "publisher/subscriber" model.
        // Any object can publish a set of events to which other objects can subscribe.
        // Let's say that we want to be notified whenever a method in the 
        // TestClass class completes.  That class has an event called OperationCompleteEvent
        // that is fired to tell anyone listening about that event.
        test.OperationCompleteEvent += OnOperationComplete;

        // Now that our event has been hooked up, let's execute the same
        // code as before, but this time the events will fire and we will
        // be notified by having our event handlers called.
        doMath(4, 8);
        someAction();

        // Don't want to be notified of these events anymore
        test.OperationCompleteEvent -= OnOperationComplete;


        // There are many times when we want to execute code in some method
        // but it will only ever be called in one place. It seems like a 
        // real waste to have to declare a method like we did
        // with "ImSpeakingNow(string SayThis)" just for that purpose.
        // To that end, the "Anonymous" method was created. 
        // Anonymous methods provide a way to write unnamed inline 
        // statement blocks that can be executed in a delegate invocation.

        List<String> names = new List<String>();
        names.Add("Fred");
        names.Add("Sam");
        names.Add("Bob");

        // The following demonstrates the anonymous method feature of C#
        // to display the contents of the list to the console 
        names.ForEach(delegate (String name)
        {
            Console.WriteLine(name);
        });

        me = delegate (string Something) { Console.WriteLine($"Anonymous says: {Something}"); };
        me("I am here!");

        // A lambda expression is nothing more than syntactic sugar for an anonymous method.
        // The following lambda expression is EXACTLY the same as the anonymous method above.
        // The type of the parameter "Something" is inferred by the compiler.
        me = (Something) => { Console.WriteLine($"Lambda says: {Something}"); };
        me("I am here!");

        Func<int,int, int> ReturnSomething = ( x,  y) => { return x + y; };
        int value = ReturnSomething(9, 8);
        Console.WriteLine($"Value is {value}");

        // The signature of the method called is:
        // 		public static int Calculate(DoMathDelegate DoMath, int first, int second)
        //
        // The first parameter is a lambda expression matching the delegate signature:
        //		public delegate int DoMathDelegate(int first, int second)
        //
        // The next 2 parameters are the values consumed by the DoMathDelegate
        Console.WriteLine($"Value is {MathMethods.Calculate((a, b) => a + b, 1, 2)} using lambda");
        Console.WriteLine($"Value is {MathMethods.Calculate((x, z) => x * z, 1, 2)}");
        Console.WriteLine($"Value is {MathMethods.Calculate((q, r) => q - r, 1, 2)}");
        Console.WriteLine($"Value is {MathMethods.Calculate((f, h) => f / h, 1, 2)}");

        // Parameter delegates are often designed to work on data that is internal to the class/type.
        // The delegate is typically used to iterate over the internal data values to 
        // produce some kind of result or filter the data in some way.
        MathMethods.AppendValue(2);
        MathMethods.AppendValue(3);
        MathMethods.AppendValue(4);
        MathMethods.AppendValue(5);
        MathMethods.AppendValue(6);
        MathMethods.AppendValue(7);
        MathMethods.AppendValue(8);
        Console.WriteLine($"CalculateTotal addition is {MathMethods.CalculateTotal((a, b) => a + b)}");
        Console.WriteLine($"CalculateTotal multiplication is {MathMethods.CalculateTotal((a, b) => a * b)}");


        // Here we will create a lambda expression that will be used to filter out all even numbers
        List<int> even = MathMethods.RunFilter(i => i % 2 == 0);
        foreach (int x in even)
        {
            Console.WriteLine($"Even {x}");
        }

        // Here we will create a lambda expression that will be used to filter out all odd numbers
        List<int> odd = MathMethods.RunFilter(i => i % 2 == 1);
        foreach (int x in odd)
        {
            Console.WriteLine($"Odd {x}");
        }

        /// A Predicate is a delegate like the Func and Action delegates. 
        /// It represents a method that checks whether the passed parameter meets a set of criteria.
        /// A predicate delegate methods must take one input parameter and return a boolean - true or false.
        /// You'll find that built in delegate types like "Action", "Func<>" and "Predicate<>" can be used
        /// instead of creating your own custom delegates most of the time. Here's an example of using
        /// a built-in "Predicate<int>" instead of custom "FilterDelegate".
        List<int> lessThan5 = MathMethods.RunFilterPredicate(i => i < 5);
        Console.WriteLine($"Values less than 5 using predicate");
        foreach (int x in lessThan5)
        {
            Console.WriteLine($"{x}");
        }

        //----------------- What's happening under the hood? Expression Trees!
        System.Linq.Expressions.Expression<Func<int, int>> myExpression = x => x * x;
        string lambdaString = myExpression.ToString();
        Func<int, int> compiledDelegate = myExpression.Compile();
        int parameter = 8;
        int answer = compiledDelegate(parameter);
        Console.WriteLine($"Result of calling '{lambdaString}' using parameter '{parameter}' is '{answer}'");
        myExpression.DumpExpression();

        Expression<Func<int, bool>> expr = i => i % 2 == 0;
        expr.DumpExpression();
        Expression<Func<string, string, string>> tree = (a, b) => a.ToLower() + b.ToUpper();
        tree.DumpExpression();

        Expression<SpeakDelegate> myDelegate = (sayThis) => Console.WriteLine(sayThis);
        myDelegate.DumpExpression();

        FilmCritic.DemonstrateDeferredExecution("Rambo", "First", new DateTime(2009, 1, 1));

    }

    static void ILikeDelegates(SpeakDelegate someDelegate, string whatToSay)
    {
        Console.WriteLine("\r\n\r\nI'm doing some work here");
        someDelegate(whatToSay);
        Console.WriteLine("Finished my work\r\n\r\n");
    }


static void ImSpeakingNow(string SayThis)
    {
        Console.WriteLine($"Main says: {SayThis}");
    }

    /// <summary>
    /// Event handler that consumes "Operation Complete" messages.
    /// </summary>
    /// <param name="sender">The object that fired the event</param>
    /// <param name="args">Custom object derived from EventArgs that will provide information about the event</param>
    static void OnOperationComplete(object sender, ProcessEventArgs args)
    {
        Console.WriteLine($"Event handler OnOperationComplete received event from:  {sender.GetType().Name}: {args.Message} at {args.CompletionTime}");
    }
}




//---------Test classes ----------------
public class ProcessEventArgs : EventArgs
{
    public string Message { get; set; }
    public DateTime CompletionTime { get; set; }

    public ProcessEventArgs(string Message = "Process Complete")
    {
        this.Message = Message;
        CompletionTime = DateTime.Now;
    }
}

public class TestClass
{
    // Declare an event that will be fired whenever
    // a method completes.
    public event EventHandler<ProcessEventArgs> OperationCompleteEvent;

    public void DoSomething()
    {
        for (int i = 0; i < 10; ++i)
        {
            Console.WriteLine($"DoSomething printing {i}");
            Thread.Sleep(500);
        }

        OperationComplete(MethodBase.GetCurrentMethod().Name);
    }

    public int AddThese(List<int> NumbersToAdd)
    {
        int total = 0;
        foreach (int i in NumbersToAdd)
            total += i;

        OperationComplete(MethodBase.GetCurrentMethod().Name);

        return (total);
    }

    public int AddThese(int first, int second)
    {
        OperationComplete(MethodBase.GetCurrentMethod().Name);

        return (first + second);
    }

    public void TalkingTest(string SayThat)
    {
        Console.WriteLine($"TestClass says: {SayThat}");
    }

    public void OperationComplete(string NameOfOperation)
    {
        if(OperationCompleteEvent != null)
            Console.WriteLine("OperationComplete firing an event. This is a SYNCHRONOUS operation.");

        ProcessEventArgs args = new ProcessEventArgs($"{NameOfOperation} is now complete");
        OperationCompleteEvent?.Invoke(this, args);

        if (OperationCompleteEvent != null)
            Console.WriteLine("OperationComplete continuing AFTER event was fired");
    }
}

public static class MathMethods
{
    public delegate int DoMathDelegate(int first, int second);
    public delegate bool FilterDelegate(int i);

    public static int Calculate(DoMathDelegate DoMath, int first, int second)
    {
        return DoMath(first, second);
    }

    public static List<int> internalValues = new List<int>();
    public static void AppendValue(int value) => internalValues.Add(value);

    public static int CalculateTotal(DoMathDelegate DoMath)
    {
        int total = 0;
        int count = internalValues.Count();
        if (count > 1)
        {
            for (int i = 0; i < count - 1; ++i)
            {
                total += DoMath(internalValues[i], internalValues[i + 1]);
            }
        }

        return (total);
    }

    /// <summary>
    /// LINQ can interact with any type implementing the IEnumerable<T> interface.
    /// More importantly, you have seen how the C# LINQ query operators are simply shorthand notations
    /// for making calls on static members of the System.Linq.Enumerable type.As shown, most members of
    /// Enumerable operate on Func<T> delegate types, which can take literal method addresses, anonymous
    /// methods, or lambda expressions as input to evaluate the query.
    /// 
    /// In this method, we are manually doing what LINQ does for us under the covers on the IEnumerable<T> interface.
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static List<int> RunFilter(FilterDelegate filter)
    {
        List<int> retVal = new List<int>();
        foreach (int v in internalValues)
        {
            if (filter(v))
                retVal.Add(v);
        }
        return (retVal);
    }


    /// <summary>
    /// Predicate is the delegate like Func and Action delegates. 
    /// It represents a method that checks whether the passed parameter meets a set of criteria.
    /// A predicate delegate methods must take one input parameter and return a boolean - true or false.
    /// 
    ///     Predicate signature: public delegate bool Predicate<in T>(T obj);
    ///     
    /// </summary>
    /// <param name="filter">A predicate that takes a single integer input
    /// parameter and returns true if the integer meets the criteria specified in the predicate</param>
    /// <returns>All integers that met the criteria of the specified predicate</returns>
    public static List<int> RunFilterPredicate(Predicate<int> filter)
    {
        return internalValues.FindAll(filter);
    }

    public static void MathSpeaks(string SayThat)
    {
        Console.WriteLine($"MathMethods says: {SayThat}");
    }
    public static void NewMethod(string SayThat, int Age)
    {
        Console.WriteLine($"MathMethods says: {SayThat}, I am {Age} years old");
    }

    public static void NewMethod2(string SayThat, int[] Ages)
    {
        Console.WriteLine($"MathMethods says: {SayThat}");
    }

    public static int myFunc(string MyName, int age)
    {
        Console.WriteLine($"My age is {age}");
        return (MyName.Length);
    }

    public static int myFunc2(string MyName, int age)
    {
        Console.WriteLine($"My func2 saying {age}");
        return (800);
    }
}

/// <summary>
/// There are two different things the compiler can generate for an operator’s lambda
/// expression: IL code or an expression tree. 
/// Extension methods on IEnumerable<T> sequences have IL code emitted by the compiler. Extension
/// methods on IQueryable<T> sequences have expression trees emitted by the compiler. 
/// </summary>
public static class WalkExpressionTree
{
    public static void DumpExpression<T>(this Expression<T> expr)
    {
        Console.WriteLine("\r\nParamter Count: {0}", expr.Parameters.Count);
        foreach (var param in expr.Parameters)
        {
            Console.WriteLine("\tParameter Name: {0}", param.Name);
        }

        var body = expr.Body;
        Console.WriteLine("Expression Type: {0}", body.NodeType);

        switch(body)
        {
            case BinaryExpression be:
                {
                    Console.WriteLine("Method to be called: {0}", be.Method);
                }
                break;
            case MethodCallExpression mce:
                {
                    Console.WriteLine("Method to be called: {0}", mce.Method);
                }
                break;
        }

        Console.WriteLine("Return Type: {0}", expr.ReturnType);
    }
}




public static class FilmCritic
{
    public class FilmDto
    {
        public string Name { get; set; }
        public DateTime ReleaseDate { get; set; }
    }

    static IList<FilmDto> myFilms = new List<FilmDto>
                 {
                     new FilmDto { Name = "Gravity", ReleaseDate = new DateTime(2013, 10, 15) },
                     new FilmDto { Name = "Blade Runner", ReleaseDate = new DateTime(1975, 2, 28) },
                     new FilmDto { Name = "Superman", ReleaseDate = new DateTime(1985, 6, 6) },
                     new FilmDto { Name = "Rambo First Blood", ReleaseDate = new DateTime(1982, 1, 8) },
                     new FilmDto { Name = "Rambo First Blood Part 2", ReleaseDate = new DateTime(1985, 2, 3) },
                     new FilmDto { Name = "Rambo III", ReleaseDate = new DateTime(1988, 2, 3) },
                     new FilmDto { Name = "Rambo", ReleaseDate = new DateTime(2008, 5, 4) },
                     new FilmDto { Name = "Rambo Last Blood", ReleaseDate = new DateTime(2019, 12, 1) },
                     new FilmDto { Name = "Eli", ReleaseDate = new DateTime(2018, 7, 1) }
                 };

    public static IEnumerable<FilmDto> GetMoviesByName(this IEnumerable<FilmDto> source, string MovieName)
    {
        foreach (FilmDto film in source)
        {
            if (film.Name.StartsWith(MovieName))
            {
                Console.WriteLine($"\t\t\tMatching film {film.Name} found");
                yield return film;
            }
        }
    }

    public static void DemonstrateDeferredExecution(string MoviedName, string AlsoContains, DateTime ReleaseDateAfter)
    {
        Console.WriteLine("\r\nStarting LINQ deferred execution example");
        IEnumerable<FilmDto> retVal = from film in myFilms.GetMoviesByName(MoviedName) select film;
        Console.WriteLine($"GetMoviesByName for {MoviedName} was called");

        retVal = retVal.Where(film => film.Name.Contains(AlsoContains));
        Console.WriteLine($"Filtering movie name to contain {AlsoContains}");

        retVal = retVal.Where(film => film.ReleaseDate > ReleaseDateAfter);
        Console.WriteLine($"Filtering movie date to be after {ReleaseDateAfter}");

        Console.WriteLine($"Enumerating Results for filter");

        List <FilmDto> filtered = retVal.ToList();
        foreach (FilmDto dto in filtered)
        {
            Console.WriteLine($"Movie selected was {dto.Name} released on {dto.ReleaseDate}");
        }
        Console.WriteLine("END LINQ deferred execution example\r\n");
    }
}
