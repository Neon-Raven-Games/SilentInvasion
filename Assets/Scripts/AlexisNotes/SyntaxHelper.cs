using UnityEngine;

namespace DefaultNamespace
{
    // the syntax helper class
    public class SyntaxHelper : MonoBehaviour
    {
        // public property, other classes can use: Shows in the inspector
        public bool isTrueOrFalse;

        // float: 0.235f
        private float _floatingPointNumber;

        // int: 1, 2, 3
        // private property, other classes cannot use
        // other classes cannot use, but shows in the inspector
        [SerializeField] private int integerNumber;

        // You can call this from other classes
        public void PublicFunction() { }

        // you cannot call this from other classes
        private void PrivateFunction() { }

        // returns an integer
        private int PrivateFunctionWithIntegerReturn()
        {
            return 0;
        }

        // you can get the return from above function inside here
        public void GetIntegerFromFunction()
        {
            var integer = PrivateFunctionWithIntegerReturn();
            Debug.Log("This is the integer from function: " + integer);
            PassThingsIntoFunctions(integer, "Some random text");
        }

        // you can pass things into functions, this is how u do it
        private void PassThingsIntoFunctions(int integer, string text)
        {
            Debug.Log("This is the integer from parameter: " + integer);
            Debug.Log("This is the text from a parameter: " + text);
        }
    }
}