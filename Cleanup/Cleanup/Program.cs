using System;

namespace Cleanup
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Cleanup game = new Cleanup())
            {
                game.Run();
            }
        }
    }
#endif
}

