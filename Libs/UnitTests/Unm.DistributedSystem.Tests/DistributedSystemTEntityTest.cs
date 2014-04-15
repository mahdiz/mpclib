// <copyright file="DistributedSystemTEntityTest.cs" company="Microsoft">Copyright © Microsoft 2012</copyright>
using System;
using Microsoft.Pex.Framework;
using Microsoft.Pex.Framework.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unm.DistributedSystem;

namespace Unm.DistributedSystem
{
    /// <summary>This class contains parameterized unit tests for DistributedSystem`1</summary>
    [PexClass(typeof(DistributedSystem<>))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(InvalidOperationException))]
    [PexAllowedExceptionFromTypeUnderTest(typeof(ArgumentException), AcceptExceptionSubtypes = true)]
    [TestClass]
    public partial class DistributedSystemTEntityTest
    {
    }
}
