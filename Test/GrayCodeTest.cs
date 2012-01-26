using Owl.Algorythms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Owl.Test
{
    
    
    /// <summary>
    ///Это класс теста для GrayCodeTest, в котором должны
    ///находиться все модульные тесты GrayCodeTest
    ///</summary>
    [TestClass]
    public class GrayCodeTest
    {


        private TestContext _testContextInstance;

        /// <summary>
        ///Получает или устанавливает контекст теста, в котором предоставляются
        ///сведения о текущем тестовом запуске и обеспечивается его функциональность.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

        #region Дополнительные атрибуты теста
        // 
        //При написании тестов можно использовать следующие дополнительные атрибуты:
        //
        //ClassInitialize используется для выполнения кода до запуска первого теста в классе
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //ClassCleanup используется для выполнения кода после завершения работы всех тестов в классе
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //TestInitialize используется для выполнения кода перед запуском каждого теста
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //TestCleanup используется для выполнения кода после завершения каждого теста
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///Тест для DecimalCode
        ///</summary>
        [TestMethod]
        public void ValueUintGrayCodeTest()
        {
            double value = 0.5;
            double min = -1.0; 
            double max = 1.0; 
            uint bitAccuracy = 8; 
            GrayCodeConfig config = new GrayCodeConfig(min,max,bitAccuracy);
            GrayCode target = new GrayCode(value, config); 
            uint expected = 224;
            uint actual = target.DecimalCode;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Тест для Value
        ///</summary>
        [TestMethod]
        public void ValueTest()
        {
            double value = 1.0; 
            double min = 0.5; 
            double max = 4;
            uint bitAccuracy = 8; 
            GrayCodeConfig config = new GrayCodeConfig(min, max, bitAccuracy);
            GrayCode target = new GrayCode(value, config); 
            double expected = value; 
            double actual;
            actual = target.Value;
            Assert.IsTrue(Math.Abs(actual - expected) <= 2/(Math.Pow(2,bitAccuracy)-1));
        }

        /// <summary>
        ///Тест для StringCode
        ///</summary>
        [TestMethod]
        public void BinaryGTest()
        {
            double value = 0.5; 
            double min = -1.0; 
            double max = 1.0;
            uint bitAccuracy = 8; 
            GrayCodeConfig config = new GrayCodeConfig(min, max, bitAccuracy);
            GrayCode target = new GrayCode(value, config); 
            string expected = "11100000"; 
            string actual;
            //target.StringCode = expected;
            actual = target.StringCode;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///Тест для Конструктор GrayCode
        ///</summary>
        [TestMethod]
        public void GrayCodeConstructorTest()
        {
            double value = 0.2; 
            double min = 0F; 
            double max = 2;
            uint bitAccuracy = 8;
            GrayCodeConfig config = new GrayCodeConfig(min, max, bitAccuracy);
            GrayCode target = new GrayCode(value, config); 
        }
    }
}
