using System;

namespace ParticleTest2
{
    static class Program
    {
        static string[] particleCountFlags =    { "-particles", "/particles", "-p", "/p", };
        static string[] pullStrengthFlags =     { "-mousePull", "/mousePull", "-m", "/m", };
        static string[] gravityStrengthFlags =  { "-gravity"  , "/gravity",   "-g", "/g", };
        static string helpFlag = "--help";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            int particleCount = 512;
            int pullStrength = 100;
            int gravityStrength = 450000;
            if (args.Length > 0)
            {
                if(args[0].Equals(helpFlag))
                {
                    printHelp();
                    return;
                }

                for (int i = 0; i < args.Length; i++)
                {
                    foreach( string particleFlag in particleCountFlags)
                    {
                        if( args[i].StartsWith(particleFlag))
                        {
                            string[] particleArgs = new string[2];
                            particleArgs[0] = args[i];
                            particleArgs[1] = i+1 < args.Length ? args[i+1] : "";
                            particleCount = getIntArgValue(particleArgs, particleCountFlags);
                            break;
                        }
                    }

                    foreach( string pullFlag in pullStrengthFlags)
                    {
                        if (args[i].StartsWith(pullFlag))
                        {
                            string[] pullArgs = new string[2];
                            pullArgs[0] = args[i];
                            pullArgs[1] = i + 1 < args.Length ? args[i + 1] : "";
                            pullStrength = getIntArgValue(pullArgs, pullStrengthFlags);
                            break;
                        }
                    }

                    foreach (string gravityFlag in gravityStrengthFlags)
                    {
                        if (args[i].StartsWith(gravityFlag))
                        {
                            string[] gravityArgs = new string[2];
                            gravityArgs[0] = args[i];
                            gravityArgs[1] = i + 1 < args.Length ? args[i + 1] : "";
                            gravityStrength = getIntArgValue(gravityArgs, gravityStrengthFlags);
                            break;
                        }
                    }
                }
            }

            using (Game1 game = new Game1(particleCount, pullStrength, gravityStrength))
            {
                game.Run();
            }
        }

        static int getIntArgValue(string[] args, string[] argFlags)
        {
            bool argMatch = false;
            string matchStart = "";
            foreach (string start in argFlags)
            {
                if (args[0].StartsWith(start))
                {
                    argMatch = true;
                    matchStart = start;
                    break;
                }
            }

            if (!argMatch)
                return 0;

            //check if the second arg is a good number
            int temp;
            if (Int32.TryParse(args[1], out temp))
            {
                return temp;
            }

            string numValue = args[0].Substring(matchStart.Length);

            if (numValue.StartsWith("=") || numValue.StartsWith(":"))
            {
                numValue = numValue.Substring(1);
            }

            if (Int32.TryParse(numValue, out temp))
            {
                return temp;
            }

            return 0;
        }

        static void printHelp()
        {
            Console.WriteLine("===== Welcome to Particle Simulator =====");
            Console.WriteLine("Requirements to compile code: Visual Studio 2008, XNA Framework 3.1");
            Console.WriteLine();
            Console.WriteLine(
                "===== Command Line Arguments =====\n" +
                "To use args:\n"+
                "     description -> example format \n" +
                "   dash  before argName, value in next arg  -> -arg value\n"+
                "   slash before argName, value in next arg  -> /arg value\n" +
                "   colon  after Argname, followed by value  -> -arg:value\n" +
                "   equals after Argname, followed by value  -> -arg=value\n" +
                "   Argnames can be full name or first letter, noted in \n"+
                "   Args section of each parameter listed below.\n" +
                "\n"+
                "   particleCount: particleCount^2 is the number of particles rendered.\n"+
                "        Default: 512 (262144 total particles)\n" +
                "        Args: " + string.Join(" ", particleCountFlags) + "\n" +
                "   pullStrength : Power of Mouse-click gravity(left)/antigravity(right).\n"+
                "        Default: 100 \n"+
                "        Note: Pull power weakens linearly over greater distance, rather\n" +
                "              than exponentially, like gravity \n" +
                "        Args: " + string.Join(" ", pullStrengthFlags) + "\n" +
                "   CoreGravity  : Power of Gravity in Center of space (mode 3).\n"+
                "        Default: 45000 \n"+ 
                "        Args: " + string.Join(" ", gravityStrengthFlags));
            Console.WriteLine();
            Console.WriteLine(
                "===== Controls =====\n" +
                "Modes\n" +
                "   1: No Gravity\n"+
                "      press 1 to set to this mode\n" + 
                "   2: Downward Gravity: gravity pulls down to bottom of window\n"+
                "      press 2 to set to this mode\n" +
                "   3: Core Gravity, gravity pulls toward center of window\n" +
                "      press 3 to set to this mode\n" +
                "Other Controls\n" +
                "   Enter       - Reset particles\n"+
                "   Left-click  - Pull particles to mouse\n"+
                "   Right-click - Push particles away from mouse\n"+
                "   Shift(left) - Toggle mouse gravity power\n"+
                "                 Default: Normal\n"+
                "                 Normal: Pull power = pullStrength(default 100)\n"+
                "                 Boost : Pull power = 5 * pullStrength, for extra pull\n"+
                "   4           - Toggle Orbit/Spiral on Core Gravity Reset\n"+
                "                 Default: Spiral\n"+
                "                 Spiral - particles slowly spiral toward center.\n"+
                "                 Orbit  - particles orbit center, don't get closer or futher\n"+
                "                 Note: This change shows when you reset the particles,\n"+
                "                    and only on Core Gravity Mode. Example:\n"+
                "                    hit 3 to get to core gravity mode\n"+
                "                    hit Enter to reset particles to spiral velocities\n"+
                "                    hit 4 toggle to orbital reset velocities\n"+
                "                    hit Enter to reset with orbital velocities\n"+
                "                    hit 4 then Enter to toggle back to spiral velocities"
                );
            Console.WriteLine();
        }

    }
}

