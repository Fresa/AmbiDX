namespace PixelCapturer
{
    public static class Config
    {
        public const bool EnableSerialCommunication = false;
        public const int SerialBaud = 115200;
        public const Port SerialPort = Port.Com3;

        // Minimum LED brightness; some users prefer a small amount of backlighting
        // at all times, regardless of screen content.  Higher values are brighter,
        // or set to 0 to disable this feature.

        public const short MinBrightness = 120;

        // LED transition speed; it's sometimes distracting if LEDs instantaneously
        // track screen contents (such as during bright flashing sequences), so this
        // feature enables a gradual fade to each new LED state.  Higher numbers yield
        // slower transitions (max of 255), or set to 0 to disable this feature
        // (immediate transition of all LEDs).

        public const short Fade = 75;

        public const short PixelSize = 25;
        // PER-DISPLAY INFORMATION ---------------------------------------------------

        // This array contains details for each display that the software will
        // process.  If you have screen(s) attached that are not among those being
        // "Adalighted," they should not be in this list.  Each triplet in this
        // array represents one display.  The first number is the system screen
        // number...typically the "primary" display on most systems is identified
        // as screen #1, but since arrays are indexed from zero, use 0 to indicate
        // the first screen, 1 to indicate the second screen, and so forth.  This
        // is the ONLY place system screen numbers are used...ANY subsequent
        // references to displays are an index into this list, NOT necessarily the
        // same as the system screen number.  For example, if you have a three-
        // screen setup and are illuminating only the third display, use '2' for
        // the screen number here...and then, in subsequent section, '0' will be
        // used to refer to the first/only display in this list.
        // The second and third numbers of each triplet represent the width and
        // height of a grid of LED pixels attached to the perimeter of this display.
        // For example, '9,6' = 9 LEDs across, 6 LEDs down.

        public static readonly int[][] Displays = {
            new[] {0,89,13}
            //new[] {0,31,15}
        };

        // PER-LED INFORMATION -------------------------------------------------------

        // This array contains the 2D coordinates corresponding to each pixel in the
        // LED strand, in the order that they're connected (i.e. the first element
        // here belongs to the first LED in the strand, second element is the second
        // LED, and so forth).  Each triplet in this array consists of a display
        // number (an index into the display array above, NOT necessarily the same as
        // the system screen number) and an X and Y coordinate specified in the grid
        // units given for that display.  {0,0,0} is the top-left corner of the first
        // display in the array.
        // For our example purposes, the coordinate list below forms a ring around
        // the perimeter of a single screen, with a one pixel gap at the bottom to
        // accommodate a monitor stand.  Modify this to match your own setup:

        public static readonly int[][] Leds =
        {
            new[] {0,29,12}, new[] {0,28,12}, new[] {0,27,12}, new[] {0,26,12}, new[] {0,25,12}, new[] {0,24,12}, new[] {0,23,12}, new[] {0,22,12}, new[] {0,21,12}, new[] {0,20,12}, new[] {0,19,12}, new[] {0,18,12}, new[] {0,17,12}, new[] {0,16,12}, new[] {0,15,12}, new[] {0,14,12}, new[] {0,13,12}, new[] {0,12,12}, new[] {0,11,12}, new[] {0,10,12}, new[] {0,9,12}, new[] {0,8,12}, new[] {0,7,12}, new[] {0,6,12}, new[] {0,5,12}, new[] {0,4,12}, new[] {0,3,12}, new[] {0,2,12}, new[] {0,1,12}, new[] {0,0,12}, new[] {0,0,11}, new[] {0,0,10}, new[] {0,0,9}, new[] {0,0,8}, new[] {0,0,7}, new[] {0,0,6}, new[] {0,0,5}, new[] {0,0,4}, new[] {0,0,3}, new[] {0,0,2}, new[] {0,0,1}, new[] {0,0,0}, new[] {0,1,0}, new[] {0,2,0}, new[] {0,3,0}, new[] {0,4,0}, new[] {0,5,0}, new[] {0,6,0}, new[] {0,7,0}, new[] {0,8,0}, new[] {0,9,0}, new[] {0,10,0}, new[] {0,11,0}, new[] {0,12,0}, new[] {0,13,0}, new[] {0,14,0}, new[] {0,15,0}, new[] {0,16,0}, new[] {0,17,0}, new[] {0,18,0}, new[] {0,19,0}, new[] {0,20,0}, new[] {0,21,0}, new[] {0,22,0}, new[] {0,23,0}, new[] {0,24,0}, new[] {0,25,0}, new[] {0,26,0}, new[] {0,27,0}, new[] {0,28,0}, new[] {0,29,0}, new[] {0,30,0}, new[] {0,31,0}, new[] {0,32,0}, new[] {0,33,0}, new[] {0,34,0}, new[] {0,35,0}, new[] {0,36,0}, new[] {0,37,0}, new[] {0,38,0}, new[] {0,39,0}, new[] {0,40,0}, new[] {0,41,0}, new[] {0,42,0}, new[] {0,43,0}, new[] {0,44,0}, new[] {0,45,0}, new[] {0,46,0}, new[] {0,47,0}, new[] {0,48,0}, new[] {0,49,0}, new[] {0,50,0}, new[] {0,51,0}, new[] {0,52,0}, new[] {0,53,0}, new[] {0,54,0}, new[] {0,55,0}, new[] {0,56,0}, new[] {0,57,0}, new[] {0,58,0}, new[] {0,59,0}, new[] {0,60,0}, new[] {0,61,0}, new[] {0,62,0}, new[] {0,63,0}, new[] {0,64,0}, new[] {0,65,0}, new[] {0,66,0}, new[] {0,67,0}, new[] {0,68,0}, new[] {0,69,0}, new[] {0,70,0}, new[] {0,71,0}, new[] {0,72,0}, new[] {0,73,0}, new[] {0,74,0}, new[] {0,75,0}, new[] {0,76,0}, new[] {0,77,0}, new[] {0,78,0}, new[] {0,79,0}, new[] {0,80,0}, new[] {0,81,0}, new[] {0,82,0}, new[] {0,83,0}, new[] {0,84,0}, new[] {0,85,0}, new[] {0,86,0}, new[] {0,87,0}, new[] {0,88,0}, new[] {0,88,1}, new[] {0,88,2}, new[] {0,88,3}, new[] {0,88,4}, new[] {0,88,5}, new[] {0,88,6}, new[] {0,88,7}, new[] {0,88,8}, new[] {0,88,9}, new[] {0,88,10}, new[] {0,88,11}, new[] {0,88,12}, new[] {0,87,12}, new[] {0,86,12}, new[] {0,85,12}, new[] {0,84,12}, new[] {0,83,12}, new[] {0,82,12}, new[] {0,81,12}, new[] {0,80,12}, new[] {0,79,12}, new[] {0,78,12}, new[] {0,77,12}, new[] {0,76,12}, new[] {0,75,12}, new[] {0,74,12}, new[] {0,73,12}, new[] {0,72,12}, new[] {0,71,12}, new[] {0,70,12}, new[] {0,69,12}, new[] {0,68,12}, new[] {0,67,12}, new[] {0,66,12}, new[] {0,65,12}, new[] {0,64,12}, new[] {0,63,12}, new[] {0,62,12}, new[] {0,61,12}, new[] {0,60,12}, new[] {0,59,12}, new[] {0,58,12}, new[] {0,57,12}, new[] {0,56,12}, new[] {0,55,12}, new[] {0,54,12}, new[] {0,53,12}, new[] {0,52,12}, new[] {0,51,12}, new[] {0,50,12}, new[] {0,49,12}, new[] {0,48,12}, new[] {0,47,12}, new[] {0,46,12}, new[] {0,45,12}, new[] {0,44,12}, new[] {0,43,12}, new[] {0,42,12}, new[] {0,41,12}, new[] {0,40,12}, new[] {0,39,12}, new[] {0,38,12}, new[] {0,37,12}, new[] {0,36,12}, new[] {0,35,12}, new[] {0,34,12}, new[] {0,33,12}, new[] {0,32,12}, new[] {0,31,12}, new[] {0,30,12}
            //new[]{0,1,0},new[]{0,2,0},new[]{0,3,0},new[]{0,4,0},new[]{0,5,0},new[]{0,6,0},new[]{0,7,0},new[]{0,8,0},new[]{0,9,0},new[]{0,10,0},new[]{0,11,0},new[]{0,12,0},new[]{0,13,0},new[]{0,14,0},new[]{0,15,0},new[]{0,16,0},new[]{0,17,0},new[]{0,18,0},new[]{0,19,0},new[]{0,20,0},new[]{0,21,0},new[]{0,22,0},new[]{0,23,0},new[]{0,24,0},new[]{0,25,0},new[]{0,26,0},new[]{0,27,0},new[]{0,28,0},new[]{0,29,0},new[]{0,30,1},new[]{0,30,2},new[]{0,30,3},new[]{0,30,4},new[]{0,30,5},new[]{0,30,6},new[]{0,30,7},new[]{0,30,8},new[]{0,30,9},new[]{0,30,10},new[]{0,30,11},new[]{0,30,12},new[]{0,30,13},new[]{0,29,14},new[]{0,28,14},new[]{0,27,14},new[]{0,26,14},new[]{0,25,14},new[]{0,24,14},new[]{0,23,14},new[]{0,22,14},new[]{0,21,14},new[]{0,20,14},new[]{0,19,14},new[]{0,18,14},new[]{0,17,14},new[]{0,16,14},new[]{0,15,14},new[]{0,14,14},new[]{0,13,14},new[]{0,12,14},new[]{0,11,14},new[]{0,10,14},new[]{0,9,14},new[]{0,8,14},new[]{0,7,14},new[]{0,6,14},new[]{0,5,14},new[]{0,4,14},new[]{0,3,14},new[]{0,2,14},new[]{0,1,14},new[]{0,0,13},new[]{0,0,12},new[]{0,0,11},new[]{0,0,10},new[]{0,0,9},new[]{0,0,8},new[]{0,0,7},new[]{0,0,6},new[]{0,0,5},new[]{0,0,4},new[]{0,0,3},new[]{0,0,2},new[]{0,0,1}
        };
        //static final int leds[][] = new int[][] {{0,29,12},{0,28,12},{0,27,12},{0,26,12},{0,25,12},{0,24,12},{0,23,12},{0,22,12},{0,21,12},{0,20,12},{0,19,12},{0,18,12},{0,17,12},{0,16,12},{0,15,12},{0,14,12},{0,13,12},{0,12,12},{0,11,12},{0,10,12},{0,9,12},{0,8,12},{0,7,12},{0,6,12},{0,5,12},{0,4,12},{0,3,12},{0,2,12},{0,1,12},{0,0,12},{0,0,11},{0,0,10},{0,0,9},{0,0,8},{0,0,7},{0,0,6},{0,0,5},{0,0,4},{0,0,3},{0,0,2},{0,0,1},{0,0,0},{0,1,0},{0,2,0},{0,3,0},{0,4,0},{0,5,0},{0,6,0},{0,7,0},{0,8,0},{0,9,0},{0,10,0},{0,11,0},{0,12,0},{0,13,0},{0,14,0},{0,15,0},{0,16,0},{0,17,0},{0,18,0},{0,19,0},{0,20,0},{0,21,0},{0,22,0},{0,23,0},{0,24,0},{0,25,0},{0,26,0},{0,27,0},{0,28,0},{0,29,0},{0,30,0},{0,30,1},{0,30,2},{0,30,3},{0,30,4},{0,30,5},{0,30,6},{0,30,7},{0,30,8},{0,30,9},{0,30,10},{0,30,11},{0,30,12}};
        //static final int leds[][] = new int[][] {{0,6,5},{0,5,5},{0,4,5},{0,3,5},{0,2,5},{0,1,5},{0,0,5},{0,0,4},{0,0,3},{0,0,2},{0,0,1},{0,0,0},{0,1,0},{0,2,0},{0,3,0},{0,4,0},{0,5,0},{0,6,0},{0,7,0},{0,7,1},{0,7,2},{0,7,3},{0,7,4},{0,7,5}};
        //public readonly int[][] leds = {new int[] {0,0,1}, new int[] {0,0,0}, new int[] {0,1,0}, new int[] {0,1,1}};
        /* Hypothetical second display has the same arrangement as the first.
           But you might not want both displays completely ringed with LEDs;
           the screens might be positioned where they share an edge in common.
         ,{1,3,5}, {1,2,5}, {1,1,5}, {1,0,5}, // Bottom edge, left half
          {1,0,4}, {1,0,3}, {1,0,2}, {1,0,1}, // Left edge
          {1,0,0}, {1,1,0}, {1,2,0}, {1,3,0}, {1,4,0}, // Top edge
                   {1,5,0}, {1,6,0}, {1,7,0}, {1,8,0}, // More top edge
          {1,8,1}, {1,8,2}, {1,8,3}, {1,8,4}, // Right edge
          {1,8,5}, {1,7,5}, {1,6,5}, {1,5,5}  // Bottom edge, right half
        */

    }
}