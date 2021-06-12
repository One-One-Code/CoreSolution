using NUnit.Framework;
using OneOne.Utility4Core.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneOne.UnitTest
{
    [TestFixture]
    public class IdNumberHelperTest
    {
        [OneTimeSetUp]
        public void FixtureSetUp()
        {

        }

        /// <summary>
        /// 当身份证号码位数小于18的时候返回false
        /// </summary>
        [Test]
        public void CheckIdNumber_When_Number_Length_Less_Then_18_Return_Must_Be_False()
        {
            var testNumber = "36042819890803633";
            var result = IdNumberHelper.CheckIdNumber(testNumber);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 当身份证号码位数大于18的时候返回false
        /// </summary>
        [Test]
        public void CheckIdNumber_When_Number_Length_Grater_Then_18_Return_Must_Be_False()
        {
            var testNumber = "3604281989080363344";
            var result = IdNumberHelper.CheckIdNumber(testNumber);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 当身份证号码为空的时候，被测试方法返回false
        /// </summary>
        [Test]
        public void CheckIdNumber_When_Number_Is_Null_Return_Must_Be_False()
        {
            var result = IdNumberHelper.CheckIdNumber(null);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 当身份证号码为18个1的时候，被测试方法返回false
        /// </summary>
        [Test]
        public void CheckIdNumber_When_Number_Is_111111111111111111_Return_Must_Be_False()
        {
            var testNumber = "111111111111111111";
            var result = IdNumberHelper.CheckIdNumber(testNumber);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 当身份证号码为360428190808036111的时候，被测试方法返回true
        /// </summary>
        [Test]
        public void CheckIdNumber_When_Number_Is_360428190808036111_Return_Must_Be_True()
        {
            var testNumber = "360428190808036111";
            var result = IdNumberHelper.CheckIdNumber(testNumber);
            Assert.IsTrue(result);
        }

        /// <summary>
        /// 当身份证号码为360428190808036111且man参数为false的时候，被测试方法返回false
        /// </summary>
        [Test]
        public void CheckIdNumber_When_Number_Is_360428190808036111_And_Man_Is_False_Return_Must_Be_Fasle()
        {
            var testNumber = "360428190808036111";
            var result = IdNumberHelper.CheckIdNumber(testNumber,false);
            Assert.IsFalse(result);
        }

        /// <summary>
        /// 当身份证号码为360428190808036111且man参数为true的时候，被测试方法返回true
        /// </summary>
        [Test]
        public void CheckIdNumber_When_Number_Is_360428190808036111_And_Man_Is_True_Return_Must_Be_True()
        {
            var testNumber = "360428190808036111";
            var result = IdNumberHelper.CheckIdNumber(testNumber, true);
            Assert.IsTrue(result);
        }
    }
}
