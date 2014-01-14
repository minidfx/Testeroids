﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Testeroids" file="ITesteroidsMock.cs">
//   © 2012-2014 Testeroids. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Testeroids
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq.Expressions;

    using Moq;
    using Moq.Language;
    using Moq.Language.Flow;

    using Testeroids.Mocking;

    public interface ITesteroidsMock
    {
        #region Public Properties

        /// <summary>
        /// Gets the behavior of the mock, according to the value set in the constructor.
        /// </summary>
        MockBehavior Behavior { get; }

        /// <summary>
        /// Gets or sets a value indicating whether the base member virtual implementation will be called
        ///             for mocked classes if no setup is matched. Defaults to <see langword="false"/>.
        /// </summary>
        bool CallBase { get; set; }

        /// <summary>
        /// Gets or sets the behavior to use when returning default values for unexpected invocations on loose mocks.
        /// </summary>
        DefaultValue DefaultValue { get; set; }

        /// <summary>
        /// Gets the mocked object instance.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods", Justification = "The public Object property is the only one visible to Moq consumers. The protected member is for internal use only.")]
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Exposes the mocked object instance, so it's appropriate.", MessageId = "Object")]
        object Object { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Adds an interface implementation to the mock,
        ///             allowing setups to be specified for it.
        /// </summary>
        /// <remarks>
        /// This method can only be called before the first use
        ///             of the mock <see cref="ITesteroidsMock.Object"/> property, at which
        ///             point the runtime type has already been generated
        ///             and no more interfaces can be added to it.
        /// <para>
        /// Also, <typeparamref name="TInterface"/> must be an
        ///                 interface and not a class, which must be specified
        ///                 when creating the mock instead.
        /// </para>
        /// </remarks>
        /// <exception cref="T:System.InvalidOperationException">The mock type
        ///             has already been generated by accessing the <see cref="ITesteroidsMock.Object"/> property.
        /// </exception>
        /// <exception cref="T:System.ArgumentException">The <typeparamref name="TInterface"/> specified
        ///             is not an interface.
        /// </exception>
        /// <example>
        /// The following example creates a mock for the main interface
        ///             and later adds <see cref="T:System.IDisposable"/> to it to verify
        ///             it's called by the consumer code:
        /// <code>
        /// var mock = new Mock&lt;IProcessor&gt;();
        ///                 mock.Setup(x =&gt; x.Execute("ping"));
        /// 
        ///                 // add IDisposable interface
        ///                 var disposable = mock.As&lt;IDisposable&gt;();
        ///                 disposable.Setup(d =&gt; d.Dispose()).Verifiable();
        /// </code>
        /// </example>
        /// <typeparam name="TInterface">Type of interface to cast the mock to.</typeparam>
        /// <returns>
        /// A <see cref="ITesteroidsMock{T}"/> instance.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "We want the method called exactly as the keyword because that's what it does, it adds an implemented interface so that you can cast it later.", MessageId = "As")]
        ITesteroidsMock<TInterface> As<TInterface>() where TInterface : class;

        /// <summary>
        /// Reset the counts of all the method calls done previously.
        /// </summary>
        void ResetAllCallCounts();

        void SetReturnsDefault<TReturn>(TReturn value);

        /// <summary>
        /// Verifies that all verifiable expectations have been met.
        /// </summary>
        /// <example group="verification">
        /// This example sets up an expectation and marks it as verifiable. After
        ///             the mock is used, a <c>Verify()</c> call is issued on the mock
        ///             to ensure the method in the setup was invoked:
        /// <code>
        /// var mock = new Mock&lt;IWarehouse&gt;();
        ///                 this.Setup(x =&gt; x.HasInventory(TALISKER, 50)).Verifiable().Returns(true);
        ///                 ...
        ///                 // other test code
        ///                 ...
        ///                 // Will throw if the test code has didn't call HasInventory.
        ///                 this.Verify();
        /// 
        /// </code>
        /// </example>
        /// <exception cref="MockSetupMethodNeverUsedException">Not all verifiable expectations were met.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2200:RethrowToPreserveStackDetails", Justification = "We want to explicitly reset the stack trace here.")]
        void Verify();

        /// <summary>
        /// Verifies all expectations regardless of whether they have
        ///             been flagged as verifiable.
        /// </summary>
        /// <example group="verification">
        /// This example sets up an expectation without marking it as verifiable. After
        ///             the mock is used, a <see cref="ITesteroidsMock.VerifyAll"/> call is issued on the mock
        ///             to ensure that all expectations are met:
        /// <code>
        /// var mock = new Mock&lt;IWarehouse&gt;();
        ///                 this.Setup(x =&gt; x.HasInventory(TALISKER, 50)).Returns(true);
        ///                 ...
        ///                 // other test code
        ///                 ...
        ///                 // Will throw if the test code has didn't call HasInventory, even
        ///                 // that expectation was not marked as verifiable.
        ///                 this.VerifyMocks();
        /// 
        /// </code>
        /// </example>
        /// <exception cref="MockSetupMethodNeverUsedException">At least one expectation was not met.</exception>
        [SuppressMessage("Microsoft.Usage", "CA2200:RethrowToPreserveStackDetails", Justification = "We want to explicitly reset the stack trace here.")]
        void VerifyAll();

        #endregion
    }

    public interface ITesteroidsMock<T> : ITesteroidsMock
        where T : class
    {
        #region Public Properties

        new T Object { get; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Raises the event referenced in <paramref name="eventExpression"/> using
        ///             the given <paramref name="args"/> argument.
        /// </summary>
        /// <exception cref="T:System.ArgumentException">The <paramref name="args"/> argument is
        ///             invalid for the target event invocation, or the <paramref name="eventExpression"/> is
        ///             not an event attach or detach expression.
        /// </exception>
        /// <example>
        /// The following example shows how to raise a <see cref="E:System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> event:
        /// <code>
        /// var mock = new Mock&lt;IViewModel&gt;();
        /// 
        ///                 mock.Raise(x =&gt; x.PropertyChanged -= null, new PropertyChangedEventArgs("Name"));
        /// 
        /// </code>
        /// </example>
        /// <example>
        /// This example shows how to invoke an event with a custom event arguments
        ///             class in a view that will cause its corresponding presenter to
        ///             react by changing its state:
        /// <code>
        /// var mockView = new Mock&lt;IOrdersView&gt;();
        ///                 var presenter = new OrdersPresenter(mockView.Object);
        /// 
        ///                 // Check that the presenter has no selection by default
        ///                 Assert.Null(presenter.SelectedOrder);
        /// 
        ///                 // Raise the event with a specific arguments data
        ///                 mockView.Raise(v =&gt; v.SelectionChanged += null, new OrderEventArgs { Order = new Order("moq", 500) });
        /// 
        ///                 // Now the presenter reacted to the event, and we have a selected order
        ///                 Assert.NotNull(presenter.SelectedOrder);
        ///                 Assert.Equal("moq", presenter.SelectedOrder.ProductName);
        /// </code>
        /// </example>
        [SuppressMessage("Microsoft.Usage", "CA2200:RethrowToPreserveStackDetails", Justification = "We want to reset the stack trace to avoid Moq noise in it.")]
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Raises the event, rather than being one.")]
        void Raise(Action<T> eventExpression,
                   EventArgs args);

        /// <summary>
        /// Raises the event referenced in <paramref name="eventExpression"/> using
        ///             the given <paramref name="args"/> argument for a non-EventHandler typed event.
        /// </summary>
        /// <exception cref="T:System.ArgumentException">The <paramref name="args"/> arguments are
        ///             invalid for the target event invocation, or the <paramref name="eventExpression"/> is
        ///             not an event attach or detach expression.
        /// </exception>
        /// <example>
        /// The following example shows how to raise a custom event that does not adhere to
        ///             the standard <c>EventHandler</c>:
        /// <code>
        /// var mock = new Mock&lt;IViewModel&gt;();
        /// 
        ///                 mock.Raise(x =&gt; x.MyEvent -= null, "Name", bool, 25);
        /// </code>
        /// </example>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "Raises the event, rather than being one.")]
        [SuppressMessage("Microsoft.Usage", "CA2200:RethrowToPreserveStackDetails", Justification = "We want to reset the stack trace to avoid Moq noise in it.")]
        void Raise(Action<T> eventExpression,
                   params object[] args);

        /// <summary>
        /// Specifies a setup on the mocked type for a call
        ///             to a void method.
        /// </summary>
        /// <remarks>
        /// If more than one setup is specified for the same method or property,
        ///             the latest one wins and is the one that will be executed.
        /// </remarks>
        /// <param name="expression">Lambda expression that specifies the expected method invocation.</param>
        /// <example group="setups">
        /// <code>
        /// var mock = new Mock&lt;IProcessor&gt;();
        ///                 mock.Setup(x =&gt; x.Execute("ping"));
        /// </code>
        /// </example>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        ISetup<T> Setup(Expression<Action<T>> expression);

        /// <summary>
        /// Specifies a setup on the mocked type for a call
        ///             to a value returning method.
        /// </summary>
        /// <typeparam name="TResult">Type of the return value. Typically omitted as it can be inferred from the expression.</typeparam>
        /// <remarks>
        /// If more than one setup is specified for the same method or property,
        ///             the latest one wins and is the one that will be executed.
        /// </remarks>
        /// <param name="expression">Lambda expression that specifies the method invocation.</param>
        /// <example group="setups">
        /// <code>
        /// mock.Setup(x =&gt; x.HasInventory("Talisker", 50)).Returns(true);
        /// </code>
        /// </example>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        ISetup<T, TResult> Setup<TResult>(Expression<Func<T, TResult>> expression);

        /// <summary>
        /// Specifies a setup on the mocked type for a call
        ///             to a property getter.
        /// </summary>
        /// <remarks>
        /// If more than one setup is set for the same property getter,
        ///             the latest one wins and is the one that will be executed.
        /// </remarks>
        /// <typeparam name="TProperty">Type of the property. Typically omitted as it can be inferred from the expression.</typeparam><param name="expression">Lambda expression that specifies the property getter.</param>
        /// <example group="setups">
        /// <code>
        /// mock.SetupGet(x =&gt; x.Suspended)
        ///                      .Returns(true);
        /// </code>
        /// </example>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        ISetupGetter<T, TProperty> SetupGet<TProperty>(Expression<Func<T, TProperty>> expression);

        /// <summary>
        /// Specifies that the given property should have "property behavior",
        ///             meaning that setting its value will cause it to be saved and
        ///             later returned when the property is requested. (this is also
        ///             known as "stubbing").
        /// </summary>
        /// <typeparam name="TProperty">
        /// Type of the property, inferred from the property
        ///             expression (does not need to be specified).
        /// </typeparam>
        /// <param name="property">
        /// Property expression to stub.
        /// </param>
        /// <example>
        /// If you have an interface with an <see cref="int"/> property <c>Value</c>, you might
        ///             stub it using the following straightforward call:
        /// <code>
        /// var mock = new Mock&lt;IHaveValue&gt;();
        ///                 mock.Stub(v =&gt; v.Value);
        /// </code>
        /// After the <c>Stub</c> call has been issued, setting and
        ///             retrieving the object value will behave as expected:
        /// <code>
        /// IHaveValue v = mock.Object;
        /// 
        ///                 v.Value = 5;
        ///                 Assert.Equal(5, v.Value);
        /// </code>
        /// </example>
        /// <returns>
        /// The <see cref="ITesteroidsMock"/> itself, allowing for fluent composition.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "This sets properties, so it's appropriate.", MessageId = "Property")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        ITesteroidsMock<T> SetupProperty<TProperty>(Expression<Func<T, TProperty>> property);

        /// <summary>
        /// Specifies that the given property should have "property behavior",
        ///             meaning that setting its value will cause it to be saved and
        ///             later returned when the property is requested. This overload
        ///             allows setting the initial value for the property. (this is also
        ///             known as "stubbing").
        /// </summary>
        /// <typeparam name="TProperty">Type of the property, inferred from the property
        ///             expression (does not need to be specified).
        /// </typeparam>
        /// <param name="property">Property expression to stub.</param><param name="initialValue">Initial value for the property.</param>
        /// <example>
        /// If you have an interface with an <see cref="int"/> property <c>Value</c>, you might
        ///             stub it using the following straightforward call:
        /// <code>
        /// var mock = new Mock&lt;IHaveValue&gt;();
        ///                 mock.SetupProperty(v =&gt; v.Value, 5);
        /// 
        /// </code>
        ///             After the <c>SetupProperty</c> call has been issued, setting and
        ///             retrieving the object value will behave as expected:
        /// <code>
        /// IHaveValue v = mock.Object;
        ///                 // Initial value was stored
        ///                 Assert.Equal(5, v.Value);
        /// 
        ///                 // New value set which changes the initial value
        ///                 v.Value = 6;
        ///                 Assert.Equal(6, v.Value);
        /// </code>
        /// </example>
        /// <returns>
        /// The <see cref="ITesteroidsMock"/> itself, allowing for fluent composition.
        /// </returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "We're setting up a property, so it's appropriate.", MessageId = "Property")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        ITesteroidsMock<T> SetupProperty<TProperty>(Expression<Func<T, TProperty>> property,
                                                    TProperty initialValue);

        /// <summary>
        /// Specifies a setup on the mocked type for a call to
        ///             to a property setter.
        /// </summary>
        /// <remarks>
        /// If more than one setup is set for the same property setter,
        ///             the latest one wins and is the one that will be executed.
        /// <para>
        /// This overloads allows the use of a callback already
        ///                 typed for the property type.
        /// </para>
        /// </remarks>
        /// <typeparam name="TProperty">Type of the property. Typically omitted as it can be inferred from the expression.</typeparam><param name="setterExpression">The Lambda expression that sets a property to a value.</param>
        /// <example group="setups">
        /// <code>
        /// mock.SetupSet(x =&gt; x.Suspended = true);
        /// </code>
        /// </example>
        ISetupSetter<T, TProperty> SetupSet<TProperty>(Action<T> setterExpression);

        /// <summary>
        /// Specifies a setup on the mocked type for a call to
        ///             to a property setter.
        /// </summary>
        /// <remarks>
        /// If more than one setup is set for the same property setter,
        ///             the latest one wins and is the one that will be executed.
        /// </remarks>
        /// <param name="setterExpression">Lambda expression that sets a property to a value.</param>
        /// <example group="setups">
        /// <code>
        /// mock.SetupSet(x =&gt; x.Suspended = true);
        /// </code>
        /// </example>
        ISetup<T> SetupSet(Action<T> setterExpression);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the mock. Use
        ///             in conjunction with the default <see cref="F:Moq.MockBehavior.Loose"/>.
        /// </summary>
        /// <example group="verification">
        /// This example assumes that the mock has been used, and later we want to verify that a given
        ///             invocation with specific parameters was performed:
        /// <code>
        /// var mock = new Mock&lt;IProcessor&gt;();
        ///                 // exercise mock
        ///                 //...
        ///                 // Will throw if the test code didn't call Execute with a "ping" string argument.
        ///                 mock.Verify(proc =&gt; proc.Execute("ping"));
        /// </code>
        /// </example>
        /// <exception cref="T:Moq.MockException">The invocation was not performed on the mock.</exception><param name="expression">Expression to verify.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void Verify(Expression<Action<T>> expression);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the mock. Use
        ///             in conjunction with the default <see cref="F:Moq.MockBehavior.Loose"/>.
        /// </summary>
        /// <exception cref="T:Moq.MockException">The invocation was not call the times specified by
        ///             <paramref name="times"/>.
        /// </exception><param name="expression">Expression to verify.</param><param name="times">The number of times a method is allowed to be called.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void Verify(Expression<Action<T>> expression,
                    Times times);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the mock,
        ///             specifying a failure error message. Use in conjunction with the default
        ///             <see cref="F:Moq.MockBehavior.Loose"/>.
        /// </summary>
        /// <example group="verification">
        /// This example assumes that the mock has been used, and later we want to verify that a given
        ///             invocation with specific parameters was performed:
        /// <code>
        /// var mock = new Mock&lt;IProcessor&gt;();
        ///                 // exercise mock
        ///                 //...
        ///                 // Will throw if the test code didn't call Execute with a "ping" string argument.
        ///                 mock.Verify(proc =&gt; proc.Execute("ping"));
        /// </code>
        /// </example>
        /// <exception cref="T:Moq.MockException">The invocation was not performed on the mock.</exception><param name="expression">Expression to verify.</param><param name="failMessage">Message to show if verification fails.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void Verify(Expression<Action<T>> expression,
                    string failMessage);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the mock,
        ///             specifying a failure error message. Use in conjunction with the default
        ///             <see cref="F:Moq.MockBehavior.Loose"/>.
        /// </summary>
        /// <exception cref="T:Moq.MockException">The invocation was not call the times specified by
        ///             <paramref name="times"/>.
        /// </exception><param name="expression">Expression to verify.</param><param name="times">The number of times a method is allowed to be called.</param><param name="failMessage">Message to show if verification fails.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void Verify(Expression<Action<T>> expression,
                    Times times,
                    string failMessage);

        /// <summary>
        /// Verifies that a specific invocation matching the given expression was performed on the mock. Use
        ///             in conjunction with the default <see cref="F:Moq.MockBehavior.Loose"/>.
        /// </summary>
        /// <example group="verification">
        /// This example assumes that the mock has been used, and later we want to verify that a given
        ///             invocation with specific parameters was performed:
        /// <code>
        /// var mock = new Mock&lt;IWarehouse&gt;();
        ///                 // exercise mock
        ///                 //...
        ///                 // Will throw if the test code didn't call HasInventory.
        ///                 mock.Verify(warehouse =&gt; warehouse.HasInventory(TALISKER, 50));
        /// </code>
        /// </example>
        /// <exception cref="T:Moq.MockException">The invocation was not performed on the mock.</exception><param name="expression">Expression to verify.</param><typeparam name="TResult">Type of return value from the expression.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void Verify<TResult>(Expression<Func<T, TResult>> expression);

        /// <summary>
        /// Verifies that a specific invocation matching the given
        ///             expression was performed on the mock. Use in conjunction
        ///             with the default <see cref="F:Moq.MockBehavior.Loose"/>.
        /// </summary>
        /// <exception cref="T:Moq.MockException">The invocation was not call the times specified by
        ///             <paramref name="times"/>.
        /// </exception><param name="expression">Expression to verify.</param><param name="times">The number of times a method is allowed to be called.</param><typeparam name="TResult">Type of return value from the expression.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void Verify<TResult>(Expression<Func<T, TResult>> expression,
                             Times times);

        /// <summary>
        /// Verifies that a specific invocation matching the given
        ///             expression was performed on the mock, specifying a failure
        ///             error message.
        /// </summary>
        /// <example group="verification">
        /// This example assumes that the mock has been used,
        ///             and later we want to verify that a given invocation
        ///             with specific parameters was performed:
        /// <code>
        /// var mock = new Mock&lt;IWarehouse&gt;();
        ///                 // exercise mock
        ///                 //...
        ///                 // Will throw if the test code didn't call HasInventory.
        ///                 mock.Verify(warehouse =&gt; warehouse.HasInventory(TALISKER, 50), "When filling orders, inventory has to be checked");
        /// </code>
        /// </example>
        /// <exception cref="T:Moq.MockException">The invocation was not performed on the mock.</exception><param name="expression">Expression to verify.</param><param name="failMessage">Message to show if verification fails.</param><typeparam name="TResult">Type of return value from the expression.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void Verify<TResult>(Expression<Func<T, TResult>> expression,
                             string failMessage);

        /// <summary>
        /// Verifies that a specific invocation matching the given
        ///             expression was performed on the mock, specifying a failure
        ///             error message.
        /// </summary>
        /// <exception cref="T:Moq.MockException">The invocation was not call the times specified by
        ///             <paramref name="times"/>.
        /// </exception><param name="expression">Expression to verify.</param><param name="times">The number of times a method is allowed to be called.</param><param name="failMessage">Message to show if verification fails.</param><typeparam name="TResult">Type of return value from the expression.</typeparam>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void Verify<TResult>(Expression<Func<T, TResult>> expression,
                             Times times,
                             string failMessage);

        /// <summary>
        /// Verifies that a property was read on the mock.
        /// </summary>
        /// <example group="verification">
        /// This example assumes that the mock has been used,
        ///             and later we want to verify that a given property
        ///             was retrieved from it:
        /// <code>
        /// var mock = new Mock&lt;IWarehouse&gt;();
        ///                 // exercise mock
        ///                 //...
        ///                 // Will throw if the test code didn't retrieve the IsClosed property.
        ///                 mock.VerifyGet(warehouse =&gt; warehouse.IsClosed);
        /// 
        /// </code>
        /// </example>
        /// <exception cref="T:Moq.MockException">The invocation was not performed on the mock.</exception><param name="expression">Expression to verify.</param><typeparam name="TProperty">Type of the property to verify. Typically omitted as it can
        ///             be inferred from the expression's return type.
        /// </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void VerifyGet<TProperty>(Expression<Func<T, TProperty>> expression);

        /// <summary>
        /// Verifies that a property was read on the mock.
        /// </summary>
        /// <exception cref="T:Moq.MockException">The invocation was not call the times specified by
        ///             <paramref name="times"/>.
        /// </exception><param name="times">The number of times a method is allowed to be called.</param><param name="expression">Expression to verify.</param><typeparam name="TProperty">Type of the property to verify. Typically omitted as it can
        ///             be inferred from the expression's return type.
        /// </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void VerifyGet<TProperty>(Expression<Func<T, TProperty>> expression,
                                  Times times);

        /// <summary>
        /// Verifies that a property was read on the mock, specifying a failure
        ///             error message.
        /// </summary>
        /// <example group="verification">
        /// This example assumes that the mock has been used,
        ///             and later we want to verify that a given property
        ///             was retrieved from it:
        /// <code>
        /// var mock = new Mock&lt;IWarehouse&gt;();
        ///                 // exercise mock
        ///                 //...
        ///                 // Will throw if the test code didn't retrieve the IsClosed property.
        ///                 mock.VerifyGet(warehouse =&gt; warehouse.IsClosed);
        /// 
        /// </code>
        /// </example>
        /// <exception cref="T:Moq.MockException">The invocation was not performed on the mock.</exception><param name="expression">Expression to verify.</param><param name="failMessage">Message to show if verification fails.</param><typeparam name="TProperty">Type of the property to verify. Typically omitted as it can
        ///             be inferred from the expression's return type.
        /// </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void VerifyGet<TProperty>(Expression<Func<T, TProperty>> expression,
                                  string failMessage);

        /// <summary>
        /// Verifies that a property was read on the mock, specifying a failure
        ///             error message.
        /// </summary>
        /// <exception cref="T:Moq.MockException">The invocation was not call the times specified by
        ///             <paramref name="times"/>.
        /// </exception><param name="times">The number of times a method is allowed to be called.</param><param name="expression">Expression to verify.</param><param name="failMessage">Message to show if verification fails.</param><typeparam name="TProperty">Type of the property to verify. Typically omitted as it can
        ///             be inferred from the expression's return type.
        /// </typeparam>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification = "By design")]
        void VerifyGet<TProperty>(Expression<Func<T, TProperty>> expression,
                                  Times times,
                                  string failMessage);

        /// <summary>
        /// Verifies that a property was set on the mock.
        /// </summary>
        /// <example group="verification">
        /// This example assumes that the mock has been used,
        ///             and later we want to verify that a given property
        ///             was set on it:
        /// <code>
        /// var mock = new Mock&lt;IWarehouse&gt;();
        ///                 // exercise mock
        ///                 //...
        ///                 // Will throw if the test code didn't set the IsClosed property.
        ///                 mock.VerifySet(warehouse =&gt; warehouse.IsClosed = true);
        /// 
        /// </code>
        /// </example>
        /// <exception cref="T:Moq.MockException">The invocation was not performed on the mock.</exception><param name="setterExpression">Expression to verify.</param>
        void VerifySet(Action<T> setterExpression);

        /// <summary>
        /// Verifies that a property was set on the mock.
        /// </summary>
        /// <exception cref="T:Moq.MockException">The invocation was not call the times specified by
        /// <paramref name="times"/>.
        /// </exception><param name="times">The number of times a method is allowed to be called.</param><param name="setterExpression">Expression to verify.</param>
        void VerifySet(Action<T> setterExpression,
                       Times times);

        /// <summary>
        /// Verifies that a property was set on the mock, specifying a failure message.
        /// </summary>
        /// <example group="verification">
        /// This example assumes that the mock has been used,
        ///             and later we want to verify that a given property
        ///             was set on it:
        /// <code>
        /// var mock = new Mock&lt;IWarehouse&gt;();
        ///                 // exercise mock
        ///                 //...
        ///                 // Will throw if the test code didn't set the IsClosed property.
        ///                 mock.VerifySet(warehouse =&gt; warehouse.IsClosed = true, "Warehouse should always be closed after the action");
        /// </code>
        /// </example>
        /// <exception cref="T:Moq.MockException">The invocation was not performed on the mock.</exception><param name="setterExpression">Expression to verify.</param><param name="failMessage">Message to show if verification fails.</param>
        void VerifySet(Action<T> setterExpression,
                       string failMessage);

        /// <summary>
        /// Verifies that a property was set on the mock, specifying
        ///             a failure message.
        /// </summary>
        /// <exception cref="T:Moq.MockException">The invocation was not call the times specified by
        ///             <paramref name="times"/>.
        /// </exception><param name="times">The number of times a method is allowed to be called.</param><param name="setterExpression">Expression to verify.</param><param name="failMessage">Message to show if verification fails.</param>
        void VerifySet(Action<T> setterExpression,
                       Times times,
                       string failMessage);

        ISetupConditionResult<T> When(Func<bool> condition);

        #endregion
    }
}