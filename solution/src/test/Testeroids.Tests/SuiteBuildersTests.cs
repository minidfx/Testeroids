﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SuiteBuildersTests.cs" company="Testeroids">
//   © 2013 Testeroids. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Testeroids.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    using Combinatorics.Collections;

    using Moq;

    using NUnit.Core;
    using NUnit.Core.Extensibility;
    using NUnit.Framework;

    using Testeroids.Aspects.Attributes;

    public class SuiteBuildersTests
    {
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
            
            /// <summary>
            /// TODO: This class has 2 test fixtures which require a simpler triangulation mechanism.
            /// </summary>
            [TestFixture]
            [TriangulatedFixture]
            public class when_Sum_is_called : given_instantiated_Sut
            {
                #region Context

                private int Result { get; set; }

                private int ReturnedSum { get; set; }

                [TriangulationValues(10)]
                private int SpecifiedOperand1 { get; set; }

                [TriangulationValues(7, -7, 8)]
                private int SpecifiedOperand2 { get; set; }

                protected override void EstablishContext()
                {
                    base.EstablishContext();

                    this.ReturnedSum = int.MaxValue;

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
                public void then_SpecifiedOperand1_matches_10()
                {
                    Assert.AreEqual(this.SpecifiedOperand1, 10);
                }

                /// <summary>
                /// This one will fail in some cases because SpecifiedOperand2 is being triangulated with -7 and 7 !
                /// </summary>
                // [Test]
                // public void then_SpecifiedOperand2_matches_minus7()
                // {
                //     Assert.AreEqual(this.SpecifiedOperand2, -7);
                // }

                [Test] 
                public void then_Result_matches_ReturnedSum()
                {
                    Assert.AreEqual(this.ReturnedSum, this.Result);
                }
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    [NUnitAddin(Description = "Hello World Plugin")]
    public class TriangulatedFixture : Attribute, NUnit.Core.Extensibility.IAddin, ISuiteBuilder
    {
        public bool Install(IExtensionHost host)
        {
            IExtensionPoint testCaseBuilders = host.GetExtensionPoint("SuiteBuilders");

            testCaseBuilders.Install(this); //this implements both interfaces

            return true;

        }       

        public bool CanBuildFrom(Type type)
        {
            bool isOk;

            if (type.IsAbstract)
            {
                isOk =  false;
            }
            else
            {                
                isOk = NUnit.Core.Reflect.HasAttribute(type, "Testeroids.Tests.TriangulatedFixture", true);
            }

            return isOk;
        }

        public NUnit.Core.Test BuildFrom(Type type)
        {
            return new SuiteTestBuilder(type);
        }
    }

    public class SuiteTestBuilder : TestSuite
    {
        public SuiteTestBuilder(Type fixtureType)
            : base(fixtureType)
        {
            string myString = "";
            this.Parent = new TriangulatedTestMethodFixture(fixtureType);
            foreach (MethodInfo method in fixtureType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                if (method.Name.StartsWith("then_"))
                {                    
                    List<Tuple<PropertyInfo, object>> triangulationValues = null;

                    // TODO: Read this off the Triangulation attributes.
                    var triangulatedProperties = method.DeclaringType.FindMembers(MemberTypes.Property,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, 
                        (info, criteria) => info.IsDefined(typeof(TriangulationValuesAttribute), false),
                        null).Cast<PropertyInfo>();

                    Dictionary<PropertyInfo, TriangulatedValuesInformation> possibleValuesForProperties = new Dictionary<PropertyInfo, TriangulatedValuesInformation>();

                    foreach (var property in triangulatedProperties)
                    {
                        object[] values = property.GetCustomAttributes(typeof(TriangulationValuesAttribute), false).Cast<TriangulationValuesAttribute>().Single().TriangulationValues;                        
                        var valuesInfo = new TriangulatedValuesInformation(values);
                        possibleValuesForProperties.Add(property, valuesInfo);
                        // triangulationValues.Add(new Tuple<PropertyInfo, object>(property,values));
                    }

                    bool dontIncrementNextProperty = false;
                    bool finishedIterationForCurrentPropertyValues = false;
                    triangulationValues = new List<Tuple<PropertyInfo, object>>();
                    int indexOfLastPropertyIncremented = -1;

                    // dontIncrementNextProperty to true means that we are still not done iterating the current property. if it's false that means that we are done iterating the current property's triangulated values
                    while (!finishedIterationForCurrentPropertyValues && triangulationValues.Count <= possibleValuesForProperties.Count)
                    {
                        triangulationValues = new List<Tuple<PropertyInfo, object>>();

                        // make sure the order is deterministic (is it necessary ?)
                        var propertyInfos = possibleValuesForProperties.Keys.OrderBy(o => o.Name).ToList();

                        foreach (var property in propertyInfos)
                        {
                            var currentIndex = possibleValuesForProperties[property].CurrentlyProcessedValueIndex;
                            triangulationValues.Add(new Tuple<PropertyInfo, object>(property, possibleValuesForProperties[property].Values[currentIndex]));

                            var currentIndexOfProperty = propertyInfos.IndexOf(property);

                            // get the current index ready for next step.
                            if (currentIndex >= possibleValuesForProperties[property].Values.Length - 1)
                            {
                                possibleValuesForProperties[property].CurrentlyProcessedValueIndex = 0;
                                finishedIterationForCurrentPropertyValues = true;                                
                            }
                            else
                            {
                                var nextIndexToProcess = currentIndex;
                                if (!(indexOfLastPropertyIncremented == currentIndexOfProperty && finishedIterationForCurrentPropertyValues == false))
                                {
                                    nextIndexToProcess++;                                    
                                }

                                possibleValuesForProperties[property].CurrentlyProcessedValueIndex = nextIndexToProcess;
                                dontIncrementNextProperty = true;
                                finishedIterationForCurrentPropertyValues = false;
                            }

                            indexOfLastPropertyIncremented = currentIndexOfProperty;
                            // if index = maxindex we should set a flag that says "retenue"
                        }
                        var nUnitTestMethod = new TriangulatedTestMethod(method, triangulationValues);
                        this.Add(nUnitTestMethod);
                        myString += "\r\ntriangulationValues[0] - " + triangulationValues[0].Item2 + " triangulationValues[1] - " + triangulationValues[1].Item2;
                    }
                    
                    // triangulationValues.Add(new Tuple<PropertyInfo, object>(this.FixtureType.GetProperty("SpecifiedOperand1", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public), 10));
                    // triangulationValues.Add(new Tuple<PropertyInfo, object>(this.FixtureType.GetProperty("SpecifiedOperand2", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public), 7));
                    //var nUnitTestMethod = new TriangulatedTestMethod(method, triangulationValues);
                    //this.Add(nUnitTestMethod); 
                    
                    //triangulationValues = new List<Tuple<PropertyInfo, object>>();
                    //// TODO: Read this off the Triangulation attributes.
                    //triangulationValues.Add(new Tuple<PropertyInfo, object>(this.FixtureType.GetProperty("SpecifiedOperand1", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public), 10));
                    //triangulationValues.Add(new Tuple<PropertyInfo, object>(this.FixtureType.GetProperty("SpecifiedOperand2", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public), -7));
                    //nUnitTestMethod = new TriangulatedTestMethod(method, triangulationValues);
                    //this.Add(nUnitTestMethod);
                }
            }

            // throw new Exception(myString);
        }
    }

    public class TriangulatedValuesInformation
    {
        public TriangulatedValuesInformation(object[] values)
        {
            this.Values = values;
            this.CurrentlyProcessedValueIndex = 0;
        }

        public int CurrentlyProcessedValueIndex { get; set; }

        public object[] Values { get; private set; }
    }

    public class TriangulatedTestMethod : NUnitTestMethod
    {
        private readonly IEnumerable<Tuple<PropertyInfo, object>> triangulationValues;

        public TriangulatedTestMethod(MethodInfo methodInfo, IEnumerable<Tuple<PropertyInfo, object>> triangulationValues)
            : base(methodInfo)
        {
            this.triangulationValues = triangulationValues;
            var testFixture = new TriangulatedTestMethodFixture(Method.DeclaringType);
            this.Parent = testFixture;

            var triangulatedName = triangulationValues.Aggregate(TestName.Name + " - Triangulated : ", (s,
                                                                                                 tuple) => string.Format("{0} {1} = {2}", s, tuple.Item1.Name, tuple.Item2.ToString()));
            this.TestName.Name = triangulatedName;
            this.TestName.FullName = triangulationValues.Aggregate(TestName.FullName + "Triangulated", (s,
                                                                                                 tuple) => string.Format("{0}_{1}Is{2}", s, tuple.Item1.Name, tuple.Item2.ToString()));
        }

        public override TestResult RunTest()
        {
            foreach (var triangulationValue in triangulationValues)
            {
                triangulationValue.Item1.SetValue(this.Fixture, triangulationValue.Item2, null);
                // Find out when is this.Fixture instantiates, and by reflection: use triangulationValue.Item1 ( the PropertyInfo ) to override the property's value before running the tests.
            }
            var contextSpecificationBase = this.Fixture as ContextSpecificationBase;
            contextSpecificationBase.BaseSetUp();
            return base.RunTest();
        }
    }

    public class TriangulatedTestMethodFixture : NUnitTestFixture
    {
        public TriangulatedTestMethodFixture(Type declaringType)
            : base(declaringType)
        {
            var baseType = declaringType.BaseType;
            if (baseType != null)
            {
                this.Parent = new TriangulatedTestMethodFixture(baseType);
            }
        }
    }

    internal class TriangulationValuesAttribute : Attribute
    {
        public object[] TriangulationValues { get; private set; }

        public TriangulationValuesAttribute(params object[] triangulationValues)
        {
            this.TriangulationValues = triangulationValues;            
        }
    }
}