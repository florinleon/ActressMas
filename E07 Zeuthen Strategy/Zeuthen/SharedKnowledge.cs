/**************************************************************************
 *                                                                        *
 *  Website:     https://github.com/florinleon/ActressMas                 *
 *  Description: Zeuthen strategy using the ActressMas framework          *
 *  Copyright:   (c) 2018, Florin Leon                                    *
 *                                                                        *
 *  This program is free software; you can redistribute it and/or modify  *
 *  it under the terms of the GNU General Public License as published by  *
 *  the Free Software Foundation. This program is distributed in the      *
 *  hope that it will be useful, but WITHOUT ANY WARRANTY; without even   *
 *  the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR   *
 *  PURPOSE. See the GNU General Public License for more details.         *
 *                                                                        *
 **************************************************************************/

namespace Zeuthen
{
    public class SharedKnowledge
    {
        public static double Utility1(double deal)
        {
            return 5.0 - deal;
        }

        public static double Utility2(double deal)
        {
            return 2.0 / 3.0 * deal;
        }

        public static double Risk1(double d1, double d2)
        {
            if (Utility1(d1) > 0)
                return 1.0 - Utility1(d2) / Utility1(d1);
            else
                return -1;
        }

        public static double Risk2(double d2, double d1)
        {
            if (Utility2(d2) > 0)
                return 1.0 - Utility2(d1) / Utility2(d2);
            else
                return -1;
        }

        public static double NewProposal1(double d1, double d2)
        {
            for (double d = d1; d <= d2; d += Utils.Eps)
                if (Utility1(d) * Utility2(d) > Utility1(d2) * Utility2(d2))
                    return d;

            return d2;
        }

        public static double NewProposal2(double d2, double d1)
        {
            for (double d = d2; d >= d1; d -= Utils.Eps)
                if (Utility1(d) * Utility2(d) > Utility1(d1) * Utility2(d1))
                    return d;

            return d1;
        }
    }
}