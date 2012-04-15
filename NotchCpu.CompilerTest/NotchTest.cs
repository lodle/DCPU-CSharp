
public class Notch
{
    public static void Main()
    {
        Console.SetColor(0x170);
        Console.Write(0, 0, "Hello Bob!!");

        Console.SetColor(0x107);
        Console.Write(11, 0, ", How are you?");

        Console.Sleep();
    }

    //public static short fib(short n)
    //{
    //    if (n == 0)
    //        return 0;

    //    if (n == 1)
    //        return 1;

    //    return fib(n - 1) + fib(n - 2);
    //}

    //public static void printFib()
    //{
    //    short z = fib(6);
    //    Util.ConsoleWrite(3, 0xF, z);
    //}
}