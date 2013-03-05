﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestSpecs.cs" company="Testeroids">
//   © 2012-2013 Testeroids. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Testeroids.Tests
{
    using System;

    using Moq;

    using NUnit.Framework;

    using Testeroids.Aspects.Attributes;

    public abstract class TestSpecs
    {
        [TestFixture]
        public sealed class after_instantiating_Sut : SubjectInstantiationContextSpecification<Test>
        {
            #region Context

            private IMock<ICalculator> InjectedCalculatorMock { get; set; }

            protected override void EstablishContext()
            {
                base.EstablishContext();

                this.InjectedCalculatorMock = this.MockRepository.CreateMock<ICalculator>();
            }

            protected override Test BecauseSutIsCreated()
            {
                return new Test(this.InjectedCalculatorMock.Object);
            }

            #endregion

            [Test]
            public void then_Calculator_is_InjectedCalculatorMock()
            {
                Assert.AreSame(this.InjectedCalculatorMock.Object, this.Sut.Calculator);
            }
        }

        public abstract class given_instantiated_Sut : ContextSpecification<Test>
        {
            #region Context

            protected IMock<ICalculator> InjectedCalculatorMock { get; private set; }

            protected override void EstablishContext()
            {
                base.EstablishContext();

                this.InjectedCalculatorMock = this.MockRepository.CreateMock<ICalculator>();
            }

            protected override Test CreateSubjectUnderTest()
            {
                return new Test(this.InjectedCalculatorMock.Object);
            }

            #endregion

            public abstract class with_CheckAllSetupsVerified_setting_Base : given_instantiated_Sut
            {
                #region Context

                protected abstract bool EstablishCheckAllSetupsVerified();

                protected override void EstablishContext()
                {
                    base.EstablishContext();

                    this.CheckSetupsAreMatchedWithVerifyCalls = this.EstablishCheckAllSetupsVerified();

                    this.InjectedCalculatorMock
                        .Setup(o => o.Sum(It.IsAny<int>(), It.IsAny<int>()))
                        .Returns(0);
                }

                protected override void Because()
                {
                    this.Sut.Sum(1, 1);
                }

                #endregion

                [TestFixture]
                public class with_CheckAllSetupsVerified_turned_on : with_CheckAllSetupsVerified_setting_Base
                {
                    #region Context

                    protected override bool EstablishCheckAllSetupsVerified()
                    {
                        return true;
                    }

                    public override void BaseTestFixtureTearDown()
                    {
                        try
                        {
                            base.BaseTestFixtureTearDown();

                            throw new Exception("Mock setup/verify match checking is not working. Expected MockNotVerifiedException was not thrown.");
                        }
                        catch (MockNotVerifiedException)
                        {
                            // This exception is expected, since we did not call this.InjectedCalculatorMock.Verify(o => o.Sum(...));
                        }
                    }

                    #endregion

                    [Test]
                    public void then_MockNotVerifiedException_is_thrown_on_test_fixture_teardown()
                    {
                        Assert.Pass();
                    }
                }

                [TestFixture]
                public class with_CheckAllSetupsVerified_turned_off : with_CheckAllSetupsVerified_setting_Base
                {
                    #region Context

                    protected override bool EstablishCheckAllSetupsVerified()
                    {
                        return false;
                    }

                    public override void BaseTestFixtureTearDown()
                    {
                        try
                        {
                            base.BaseTestFixtureTearDown();
                        }
                        catch (MockNotVerifiedException)
                        {
                            // This exception is not expected, since this.CheckSetupsAreMatchedWithVerifyCalls = false;
                            throw new Exception("CheckSetupsAreMatchedWithVerifyCalls is not working properly. Unexpected MockNotVerifiedException was thrown.");
                        }
                    }

                    #endregion

                    [Test]
                    public void then_MockNotVerifiedException_is_not_thrown_on_test_fixture_teardown()
                    {
                        Assert.Pass();
                    }
                }
            }

            public abstract class with_AutoVerifyMocks_setting_Base : given_instantiated_Sut
            {
                #region Context

                private static void NoOp()
                {
                }

                protected abstract bool EstablishAutoVerifyMocks();

                protected override void EstablishContext()
                {
                    base.EstablishContext();

                    this.AutoVerifyMocks = this.EstablishAutoVerifyMocks();

                    this.InjectedCalculatorMock
                        .Setup(o => o.Sum(It.IsAny<int>(), It.IsAny<int>()))
                        .Returns(0)
                        .Verifiable();
                }

                protected override void Because()
                {
                    // Nothing to test in the Act part, since we are testing the framework behavior.
                    NoOp();
                }

                #endregion

                [TestFixture]
                public class with_AutoVerifyMocks_turned_on : with_AutoVerifyMocks_setting_Base
                {
                    #region Context

                    protected override bool EstablishAutoVerifyMocks()
                    {
                        return true;
                    }

                    public override void BaseTestFixtureTearDown()
                    {
                        try
                        {
                            base.BaseTestFixtureTearDown();

                            throw new Exception("Mock setup automatic verification is not working. Expected MockException was not thrown.");
                        }
                        catch (MockException)
                        {
                            // This exception is expected, since we did not call exercise the this.InjectedCalculatorMock.Setup(o => o.Sum(...));
                        }
                    }

                    protected override void EstablishContext()
                    {
                        base.EstablishContext();

                        this.AutoVerifyMocks = true;
                    }

                    #endregion

                    [Test]
                    public void then_MockException_is_thrown_on_test_fixture_teardown()
                    {
                        Assert.Pass();
                    }
                }

                [TestFixture]
                public class with_AutoVerifyMocks_turned_off : with_AutoVerifyMocks_setting_Base
                {
                    #region Context

                    protected override bool EstablishAutoVerifyMocks()
                    {
                        return false;
                    }

                    public override void BaseTestFixtureTearDown()
                    {
                        try
                        {
                            base.BaseTestFixtureTearDown();
                        }
                        catch (MockException)
                        {
                            // This exception is not expected, since this.AutoVerifyMocks = false;
                            throw new Exception("AutoVerifyMocks is not working properly. Unexpected MockException was thrown.");
                        }
                    }

                    #endregion

                    [Test]
                    public void then_MockException_is_not_thrown_on_test_fixture_teardown()
                    {
                        Assert.Pass();
                    }
                }
            }

            [AbstractTestFixture]
            public abstract class when_Sum_is_called : given_instantiated_Sut
            {
                #region Context

                private int Result { get; set; }

                private int ReturnedSum { get; set; }

                private int SpecifiedOperand1 { get; set; }

                private int SpecifiedOperand2 { get; set; }

                protected abstract int EstablishSpecifiedOperand1();

                protected abstract int EstablishSpecifiedOperand2();

                protected abstract int EstablishReturnedSum();

                protected override void EstablishContext()
                {
                    base.EstablishContext();

                    this.SpecifiedOperand1 = this.EstablishSpecifiedOperand1();
                    this.SpecifiedOperand2 = this.EstablishSpecifiedOperand2();

                    this.ReturnedSum = this.EstablishReturnedSum();

                    this.InjectedCalculatorMock
                        .Setup(o => o.Sum(It.IsAny<int>(), It.IsAny<int>()))
                        .Returns(this.ReturnedSum)
                        .Verifiable();
                }

                protected override sealed void Because()
                {
                    this.Result = this.Sut.Sum(this.SpecifiedOperand1, this.SpecifiedOperand2);
                }

                #endregion

                [TestFixture]
                public class with_SpecifiedOperand1_equal_to_10_and_SpecifiedOperand2_equal_to_7 : when_Sum_is_called
                {
                    #region Context

                    protected override int EstablishSpecifiedOperand1()
                    {
                        return 10;
                    }

                    protected override int EstablishSpecifiedOperand2()
                    {
                        return 7;
                    }

                    protected override int EstablishReturnedSum()
                    {
                        // Return an erroneous value, just to certify that we are returning the value which is handed out by the mock
                        return int.MaxValue;
                    }

                    #endregion
                }

                [TestFixture]
                public class with_SpecifiedOperand1_equal_to_10_and_SpecifiedOperand2_equal_to_minus_7 : when_Sum_is_called
                {
                    #region Context

                    protected override int EstablishSpecifiedOperand1()
                    {
                        return 10;
                    }

                    protected override int EstablishSpecifiedOperand2()
                    {
                        return -7;
                    }

                    protected override int EstablishReturnedSum()
                    {
                        return 3;
                    }

                    #endregion
                }

                [Test]
                public void then_Sum_is_called_once_on_InjectedTestMock()
                {
                    this.InjectedCalculatorMock.Verify(o => o.Sum(It.IsAny<int>(), It.IsAny<int>()), Times.Once());
                }

                [Test]
                public void then_Sum_is_called_once_on_InjectedTestMock_passing_SpecifiedOperand1()
                {
                    this.InjectedCalculatorMock.Verify(o => o.Sum(this.SpecifiedOperand1, It.IsAny<int>()), Times.Once());
                }

                [Test]
                public void then_Sum_is_called_once_on_InjectedTestMock_passing_SpecifiedOperand2()
                {
                    this.InjectedCalculatorMock.Verify(o => o.Sum(It.IsAny<int>(), this.SpecifiedOperand2), Times.Once());
                }

                [Test]
                public void then_Result_matches_ReturnedSum()
                {
                    Assert.AreEqual(this.ReturnedSum, this.Result);
                }
            }
        }
    }
}