public class Console
{
    public static void Sleep()
    {
        asm
        {
            :EndlessLoop
            set PC, Endlessloop
        }
    }

    public static void SetColor(short c)
    {
        asm
        {
            and a, 0xFF
            shl a, 8

	        ifg a, 0x1FF
		        add b, 0x80

            set [0x1000], a
        }
    }

    public static void Write(short x, short y, string text)
    {
        asm
        {
            mul b, 0x30     ; y*16
            mul b, 0x22     ; y*2
            add b, a        ; y+x
            set i, 0        ; index = 0
            set j, [0x1000] ; j=color
            
            :LoopStart
            set a, [c]          ; load char
            and a, 0xFF         ; only want lower
            bor a, j            ; char | color
            set [0x8000+b], a   ; set to screen
            add c, 1            ; index++
            add b, 1

            ifg [c], 0
            set PC, LoopStart
        }
    }
}