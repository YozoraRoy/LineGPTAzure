namespace FunctionTestProject
{
    [TestClass]
    public class TestSample
    {
        [TestMethod()]
        public void TestSampleMethod()
        {

            // Arrange
            int num1 = 5;
            int num2 = 10;           
            // Act
            int result = MyMath(num1, num2);

            // Assert
            int expectedResult = 15;
            Assert.AreEqual(expectedResult, result);

        }


        private int MyMath(int num1, int num2)
        {
           return num1 + num2;
        }
    }
}