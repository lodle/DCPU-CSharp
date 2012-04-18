
public class Notch
{
    public static void Main()
    {
        for (short a=0; a<0x5; a = a + 1)
        {
            Console.Write(0, a, "Hi!");

            if (a == 0x3)
            {
                break;
            }
            else
            {
                continue;
            }
        }
    }
}
