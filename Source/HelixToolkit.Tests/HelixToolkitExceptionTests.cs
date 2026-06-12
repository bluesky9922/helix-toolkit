
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;

namespace HelixToolkit.Tests;

// ReSharper disable InconsistentNaming
[SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
[TestFixture]
public class HelixToolkitExceptionTests
{
    [Test]
    public void HelixToolkitException_Throw()
    {
        Assert.Throws<HelixToolkitException>((TestDelegate)(() => HelixToolkitException.Throw("message")));
    }

    [Test]
    public void HelixToolkitException_ThrowT()
    {
        Assert.Throws<HelixToolkitException>((TestDelegate)(() => HelixToolkitException.Throw<int>("message")));
    }
}
