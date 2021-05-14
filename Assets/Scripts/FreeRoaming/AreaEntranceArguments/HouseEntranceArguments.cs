using System;
using System.Collections.Generic;

namespace FreeRoaming.AreaEntranceArguments
{
    public static class HouseEntranceArguments
    {

        public static bool argumentsSet = false;
        public static int houseId = 0;

        public static void SetArguments(int houseId)
        {
            argumentsSet = true;
            HouseEntranceArguments.houseId = houseId;
        }

    }
}
