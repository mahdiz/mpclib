using System.Reflection;
using NUnit.Framework;

// Disable warnings about invalid entry points
#pragma warning disable 0402
#pragma warning disable 0028

namespace MiscUtil.UnitTests
{
    [TestFixture]
    public class ApplicationChooserTest
    {
        [Test]
        public void NoMainMethodTypeIsRejected()
        {
            Assert.IsNull(ApplicationChooser.GetEntryPoint(typeof(NoMainMethod)));
        }

        [Test]
        public void BothMethodAndTypeCanBePrivate()
        {
            Assert.IsNotNull(ApplicationChooser.GetEntryPoint(typeof(PrivateClassAndPrivateMainMethod)));
        }
                
        [Test]
        public void BothMethodAndTypeCanBePublic()
        {
            Assert.IsNotNull(ApplicationChooser.GetEntryPoint(typeof(PublicClassAndPublicMainMethod)));
        }

        [Test]
        public void MainMethodCanHaveStringArrayParameter()
        {
            Assert.IsNotNull(ApplicationChooser.GetEntryPoint(typeof(MainMethodWithStringArrayParameter)));
        }

        [Test]
        public void MainMethodCannotHaveStringArrayParameterByRef()
        {
            Assert.IsNull(ApplicationChooser.GetEntryPoint(typeof(MainMethodWithStringArrayParameterByRef)));
        }

        [Test]
        public void TypeCannotBeOpenGeneric()
        {
            Assert.IsNull(ApplicationChooser.GetEntryPoint(typeof(GenericTypeWithMainMethod<>)));
        }

        [Test]
        public void TypeCannotBeClosedGeneric()
        {
            Assert.IsNull(ApplicationChooser.GetEntryPoint(typeof(GenericTypeWithMainMethod<int>)));
        }

        [Test]
        public void MethodCannotBeGeneric()
        {
            Assert.IsNull(ApplicationChooser.GetEntryPoint(typeof(TypeWithGenericMainMethod)));
        }

        [Test]
        public void GenericMethodsCanCoexistAndAreIgnored()
        {
            MethodBase method = ApplicationChooser.GetEntryPoint(typeof(TypeWithMainMethodsOverloadedByTypeParameterNumber));
            Assert.IsNotNull(method);
            Assert.IsFalse(method.IsGenericMethod);
            Assert.IsFalse(method.IsGenericMethodDefinition);
        }

        [Test]
        public void MethodCannotTakeOtherParameters()
        {
            Assert.IsNull(ApplicationChooser.GetEntryPoint(typeof(MainMethodWithInt32Parameter)));
        }

        [Test]
        public void MethodWithStringArrayParameterIsPreferredToMethodWithNoParameters()
        {
            MethodBase method = ApplicationChooser.GetEntryPoint(typeof(TypeHasMultipleMainMethods));
            Assert.IsNotNull(method);
            Assert.AreEqual(1, method.GetParameters().Length);
            Assert.AreEqual(typeof(string[]), method.GetParameters()[0].ParameterType);
        }

        class NoMainMethod
        {
        }

        class PrivateClassAndPrivateMainMethod
        {
            static void Main() { }
        }

        public class PublicClassAndPublicMainMethod
        {
            public static void Main() { } 
        }

        class MainMethodWithStringArrayParameter
        {
            static void Main(string[] args) { }
        }

        class MainMethodWithInt32Parameter
        {
            static void Main(int arg) { }
        }

        class MainMethodWithStringArrayParameterByRef
        {
            static void Main(ref string[] args) { }
        }

        class GenericTypeWithMainMethod<T>
        {
            static void Main() { }
        }

        class TypeWithGenericMainMethod
        {
            static void Main<T>() { } 
        }

        class TypeWithMainMethodsOverloadedByTypeParameterNumber
        {
            static void Main<T>() { }
            static void Main() { }
        }

        class TypeHasMultipleMainMethods
        {
            static void Main(int x, int y) { }
            static void Main() { }
            static void Main(string[] args) { }
        }
    }
}
