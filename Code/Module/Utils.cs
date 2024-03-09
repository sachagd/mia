using System;
using System.Linq;
using NumSharp;

namespace Celeste.Mod.Mia.UtilsClass
{
    public class Utils
    {
        public static void Print(params object[] arguments)
        {
            if (!arguments.Any())
            {
                throw new System.Exception("You need to provide at least 1 argument");
            }
            String text = "";
            foreach (object arg in arguments)
            {
                text += arg.ToString();
                text += " ";
            }
            Logger.Log("Mia", text);
        }

        public static bool[] GetWhatThingToMove(NDArray y_pred)
        {
            bool[] movements = new bool[7]; 
            int n = np.argmax(y_pred);
            int q = n / 9;
            int r = n % 9;
            if (q == 1)
            {
                movements[4] = true;
            }
            else if (q == 2)
            {
                movements[5] = true;
            }
            else if (q == 3)
            {
                movements[6] = true;
            }
            else if (q == 4)
            {
                movements[4] = true;
                movements[5] = true;
            }
            else if (q == 5)
            {
                movements[4] = true;
                movements[6] = true;
            }
            if (r == 1)
            {
                movements[2] = true;
            }
            else if (r == 2)
            {
                movements[0] = true;
            }
            else if (r == 3)
            {
                movements[3] = true;
            }
            else if (r == 4)
            {
                movements[1] = true;
            }
            else if (r == 5)
            {
                movements[2] = true;
                movements[0] = true;
            }
            else if (r == 6)
            {
                movements[2] = true;
                movements[1] = true;
            }
            else if (r == 7)
            {
                movements[3] = true;
                movements[0] = true;
            }
            else if (r == 8)
            {
                movements[3] = true;
                movements[1] = true;
            }
            return movements;
        }

        public static int[] GetInputs()
        {
            int[] inputs = new int[7];

            if (Input.MoveX.Value == -1) inputs[0] = 1;
            if (Input.MoveX.Value == 1) inputs[1] = 1;
            if (Input.MoveY.Value == 1) inputs[2] = 1;
            if (Input.MoveY.Value == -1) inputs[3] = 1;
            if (Input.Grab.Check) inputs[4] = 1;
            if (Input.DashPressed) inputs[5] = 1;
            if (Input.Jump.Check) inputs[6] = 1;
            return inputs;

        }
    }
}