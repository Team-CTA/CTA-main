using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameModeData
{
    public class Normal
    {
        public static int MaxPlayers = 2;
        public static bool IsOpen = true;
        public static bool IsVisible = true;
    }
    public class Ranked
    {
        public static int MaxPlayers = 2;
        public static bool IsOpen = true;
        public static bool IsVisible = true;
    }
    public class Custom
    {
        public static int MaxPlayers = 2;
        public static bool IsOpen = false;
        public static bool IsVisible = false;
    }
}
