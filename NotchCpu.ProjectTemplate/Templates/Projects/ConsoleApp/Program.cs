
public class NotchCpu
{
    public static void Main()
    {
        short a = 1;
        Util.ConsoleWrite(5, 6, a);

        printFib();
    }

    public static short fib(short n)
    {
        if (n == 0)
            return 0;

        if (n == 1)
            return 1;

        return fib(n - 1) + fib(n - 2);
    }

    public static void printFib()
    {
        short z = fib(6);
        Util.ConsoleWrite(3, 0xF, z);
    }
}

public class Util
{
    public static void ConsoleWrite(short x, short y, string text)
    {

    }
}